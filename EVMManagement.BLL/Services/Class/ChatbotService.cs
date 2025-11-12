using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Chatbot;
using EVMManagement.BLL.DTOs.Response.Chatbot;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Options;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace EVMManagement.BLL.Services.Class
{
    public class ChatbotService : IChatbotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly GeminiSettings _geminiSettings;
        private readonly HttpClient _httpClient;
        private const int CHAT_HISTORY_EXPIRATION_MINUTES = 5;

        public ChatbotService(
            IUnitOfWork unitOfWork,
            IDistributedCache cache,
            IOptions<GeminiSettings> geminiSettings,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _geminiSettings = geminiSettings.Value;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<ChatResponseDto> ProcessChatAsync(ChatRequestDto request)
        {
            if (!request.UserRole.HasValue)
            {
                throw new UnauthorizedAccessException("Không thể xác định quyền của người dùng");
            }

            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
            var chatHistory = await GetChatHistoryAsync(sessionId);

            chatHistory.Add(new GeminiMessage
            {
                Role = "user",
                Parts = new List<GeminiPart> { new GeminiPart { Text = request.Message } }
            });

            var functionsCalled = new List<string>();
            string finalResponse = string.Empty;
            int maxIterations = 5;
            int iteration = 0;

            while (iteration < maxIterations)
            {
                iteration++;

                var geminiRequest = new GeminiRequest
                {
                    Contents = chatHistory,
                    Tools = GetFunctionDeclarations(request.UserRole.Value),
                    SystemInstruction = new GeminiSystemInstruction
                    {
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart
                            {
                                Text = GetSystemInstruction(request.UserRole.Value)
                            }
                        }
                    }
                };

                var apiUrl = $"{_geminiSettings.BaseUrl}/v1beta/models/{_geminiSettings.Model}:generateContent?key={_geminiSettings.ApiKey}";
                var jsonRequest = JsonSerializer.Serialize(geminiRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                var httpResponse = await _httpClient.PostAsync(apiUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Lỗi gọi Gemini API: {responseContent}");
                }

                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Count == 0)
                {
                    throw new Exception("Không nhận được phản hồi từ Gemini API");
                }

                var candidate = geminiResponse.Candidates[0];
                chatHistory.Add(new GeminiMessage
                {
                    Role = "model",
                    Parts = candidate.Content.Parts
                });

                var functionCalls = candidate.Content.Parts
                    .Where(p => p.FunctionCall != null)
                    .Select(p => p.FunctionCall)
                    .ToList();

                if (functionCalls.Any())
                {
                    var functionResponses = new List<GeminiPart>();

                    foreach (var fc in functionCalls)
                    {
                        if (fc?.Name != null && !string.IsNullOrEmpty(fc.Name))
                        {
                            if (!ChatbotPermissions.CanAccessFunction(request.UserRole.Value, fc.Name))
                            {
                                functionResponses.Add(new GeminiPart
                                {
                                    FunctionResponse = new GeminiFunctionResponse
                                    {
                                        Name = fc.Name,
                                        Response = new Dictionary<string, object>
                                        {
                                            { "error", $"Bạn không có quyền sử dụng chức năng này. {ChatbotPermissions.GetPermissionDeniedMessage(request.UserRole.Value)}" }
                                        }
                                    }
                                });
                                continue;
                            }

                            functionsCalled.Add(fc.Name);
                        }
                        var functionResult = await ExecuteFunctionAsync(fc?.Name ?? string.Empty, fc?.Args ?? new Dictionary<string, object>(), request.DealerId);

                        functionResponses.Add(new GeminiPart
                        {
                            FunctionResponse = new GeminiFunctionResponse
                            {
                                Name = fc?.Name ?? string.Empty,
                                Response = new Dictionary<string, object>
                                {
                                    { "result", functionResult }
                                }
                            }
                        });
                    }

                    chatHistory.Add(new GeminiMessage
                    {
                        Role = "user",
                        Parts = functionResponses
                    });
                }
                else
                {
                    var textPart = candidate.Content.Parts.FirstOrDefault(p => !string.IsNullOrEmpty(p.Text));
                    if (textPart != null && !string.IsNullOrEmpty(textPart.Text))
                    {
                        finalResponse = textPart.Text;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(finalResponse))
            {
                finalResponse = "Xin lỗi, tôi không thể xử lý yêu cầu của bạn lúc này. Vui lòng thử lại.";
            }

            await SaveChatHistoryAsync(sessionId, chatHistory);

            return new ChatResponseDto
            {
                Response = finalResponse,
                SessionId = sessionId,
                FunctionsCalled = functionsCalled.Any() ? functionsCalled : null,
                Timestamp = DateTime.UtcNow
            };
        }

        private string GetSystemInstruction(AccountRole role)
        {
            var baseInstruction = @"Bạn là trợ lý AI chuyên về xe điện EVM. Nhiệm vụ của bạn:
1. Tư vấn khách hàng về các mẫu xe điện dựa trên nhu cầu (giá, quãng đường, tốc độ)
2. Cung cấp thông tin chính xác về xe từ database
3. Trả lời bằng tiếng Việt, thân thiện và chuyên nghiệp
4. Khi cần thông tin, hãy gọi các function tools có sẵn
5. Tổng hợp kết quả từ nhiều function calls để đưa ra câu trả lời đầy đủ";

            return role switch
            {
                AccountRole.DEALER_STAFF => baseInstruction + "\n\nLưu ý: Bạn chỉ có thể trả lời các câu hỏi về thông tin xe, so sánh các phiên bản xe. Không được cung cấp thông tin về doanh số và phân tích thị trường.",
                AccountRole.DEALER_MANAGER => baseInstruction + "\n\n6. Phân tích doanh số bán hàng và xu hướng của đại lý\n7. Cung cấp thống kê hiệu suất đại lý",
                AccountRole.EVM_STAFF => baseInstruction + "\n\n6. Phân tích doanh số bán hàng và xu hướng\n7. Cung cấp thống kê hiệu suất các đại lý",
                AccountRole.EVM_ADMIN => baseInstruction + "\n\n6. Phân tích doanh số bán hàng và xu hướng\n7. Đề xuất các mẫu xe mới để cạnh tranh thị trường\n8. Cung cấp phân tích chiến lược toàn diện",
                _ => baseInstruction
            };
        }

        private List<GeminiTool> GetFunctionDeclarations(AccountRole userRole)
        {
            var allowedFunctions = ChatbotPermissions.GetAllowedFunctions(userRole);
            var allDeclarations = new Dictionary<string, GeminiFunctionDeclaration>
            {
                {
                    "get_sales_statistics", new GeminiFunctionDeclaration
                    {
                        Name = "get_sales_statistics",
                        Description = "Lấy thống kê doanh số bán hàng theo thời gian, loại đơn hàng (B2C/B2B), mẫu xe. Dùng khi người dùng hỏi về doanh số, số lượng xe bán được, doanh thu.",
                        Parameters = new GeminiFunctionParameters
                        {
                            Type = "object",
                            Properties = new Dictionary<string, GeminiFunctionProperty>
                            {
                                {
                                    "startDate", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Ngày bắt đầu (yyyy-MM-dd). Nếu không có, lấy 30 ngày gần nhất"
                                    }
                                },
                                {
                                    "endDate", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Ngày kết thúc (yyyy-MM-dd). Nếu không có, lấy đến hiện tại"
                                    }
                                },
                                {
                                    "orderType", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Loại đơn hàng: B2C (Dealer-Customer) hoặc B2B (Dealer-EVM)",
                                        Enum = new List<string> { "B2C", "B2B" }
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "get_top_selling_models", new GeminiFunctionDeclaration
                    {
                        Name = "get_top_selling_models",
                        Description = "Lấy danh sách các mẫu xe bán chạy nhất theo số lượng hoặc doanh thu. Dùng khi hỏi về mẫu xe nào bán chạy, phổ biến.",
                        Parameters = new GeminiFunctionParameters
                        {
                            Type = "object",
                            Properties = new Dictionary<string, GeminiFunctionProperty>
                            {
                                {
                                    "topCount", new GeminiFunctionProperty
                                    {
                                        Type = "integer",
                                        Description = "Số lượng mẫu xe muốn lấy (mặc định 5)"
                                    }
                                },
                                {
                                    "startDate", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Ngày bắt đầu (yyyy-MM-dd)"
                                    }
                                },
                                {
                                    "endDate", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Ngày kết thúc (yyyy-MM-dd)"
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "search_vehicles_by_criteria", new GeminiFunctionDeclaration
                    {
                        Name = "search_vehicles_by_criteria",
                        Description = "Tìm kiếm xe theo tiêu chí: giá, quãng đường 1 lần sạc, tốc độ tối đa, màu sắc. Dùng khi khách hàng muốn tư vấn chọn xe phù hợp nhu cầu.",
                        Parameters = new GeminiFunctionParameters
                        {
                            Type = "object",
                            Properties = new Dictionary<string, GeminiFunctionProperty>
                            {
                                {
                                    "minPrice", new GeminiFunctionProperty
                                    {
                                        Type = "number",
                                        Description = "Giá tối thiểu (VND)"
                                    }
                                },
                                {
                                    "maxPrice", new GeminiFunctionProperty
                                    {
                                        Type = "number",
                                        Description = "Giá tối đa (VND)"
                                    }
                                },
                                {
                                    "minDistance", new GeminiFunctionProperty
                                    {
                                        Type = "number",
                                        Description = "Quãng đường tối thiểu 1 lần sạc (km)"
                                    }
                                },
                                {
                                    "minSpeed", new GeminiFunctionProperty
                                    {
                                        Type = "number",
                                        Description = "Tốc độ tối đa tối thiểu (km/h)"
                                    }
                                },
                                {
                                    "color", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Màu sắc xe"
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "get_vehicle_models_info", new GeminiFunctionDeclaration
                    {
                        Name = "get_vehicle_models_info",
                        Description = "Lấy thông tin chi tiết về các mẫu xe (tên, mô tả, thông số kỹ thuật). Dùng khi hỏi về thông tin chung các mẫu xe có sẵn.",
                        Parameters = new GeminiFunctionParameters
                        {
                            Type = "object",
                            Properties = new Dictionary<string, GeminiFunctionProperty>
                            {
                                {
                                    "modelName", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Tên mẫu xe cụ thể (Vento, Theon, Feliz...)"
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "get_customer_purchase_trends", new GeminiFunctionDeclaration
                    {
                        Name = "get_customer_purchase_trends",
                        Description = "Phân tích xu hướng mua hàng của khách hàng: mẫu xe ưa chuộng, giá trung bình, thời gian mua. Dùng cho phân tích thị trường.",
                        Parameters = new GeminiFunctionParameters
                        {
                            Type = "object",
                            Properties = new Dictionary<string, GeminiFunctionProperty>
                            {
                                {
                                    "startDate", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Ngày bắt đầu (yyyy-MM-dd)"
                                    }
                                },
                                {
                                    "endDate", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Ngày kết thúc (yyyy-MM-dd)"
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "compare_vehicle_variants", new GeminiFunctionDeclaration
                    {
                        Name = "compare_vehicle_variants",
                        Description = "So sánh các phiên bản xe theo model. Dùng khi khách hàng muốn so sánh các phiên bản của cùng 1 mẫu xe.",
                        Parameters = new GeminiFunctionParameters
                        {
                            Type = "object",
                            Properties = new Dictionary<string, GeminiFunctionProperty>
                            {
                                {
                                    "modelName", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Tên mẫu xe cần so sánh các phiên bản"
                                    }
                                }
                            },
                            Required = new List<string> { "modelName" }
                        }
                    }
                },
                {
                    "get_dealer_performance", new GeminiFunctionDeclaration
                    {
                        Name = "get_dealer_performance",
                        Description = "Lấy thống kê hiệu suất bán hàng của đại lý. Dùng khi hỏi về doanh số, hiệu quả của đại lý.",
                        Parameters = new GeminiFunctionParameters
                        {
                            Type = "object",
                            Properties = new Dictionary<string, GeminiFunctionProperty>
                            {
                                {
                                    "startDate", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Ngày bắt đầu (yyyy-MM-dd)"
                                    }
                                },
                                {
                                    "endDate", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Ngày kết thúc (yyyy-MM-dd)"
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "get_market_competition_analysis", new GeminiFunctionDeclaration
                    {
                        Name = "get_market_competition_analysis",
                        Description = "Phân tích thị trường xe điện, đề xuất mẫu xe mới để cạnh tranh. Dùng khi hỏi về chiến lược sản phẩm, cạnh tranh thị trường.",
                        Parameters = new GeminiFunctionParameters
                        {
                            Type = "object",
                            Properties = new Dictionary<string, GeminiFunctionProperty>
                            {
                                {
                                    "priceRange", new GeminiFunctionProperty
                                    {
                                        Type = "string",
                                        Description = "Phân khúc giá: low (dưới 20tr), medium (20-40tr), high (trên 40tr)"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var filteredDeclarations = allDeclarations
                .Where(kvp => allowedFunctions.Contains(kvp.Key))
                .Select(kvp => kvp.Value)
                .ToList();

            return new List<GeminiTool>
            {
                new GeminiTool
                {
                    FunctionDeclarations = filteredDeclarations
                }
            };
        }

        private async Task<object> ExecuteFunctionAsync(string functionName, Dictionary<string, object> args, Guid? dealerId)
        {
            try
            {
                return functionName switch
                {
                    "get_sales_statistics" => await GetSalesStatisticsAsync(args, dealerId),
                    "get_top_selling_models" => await GetTopSellingModelsAsync(args, dealerId),
                    "search_vehicles_by_criteria" => await SearchVehiclesByCriteriaAsync(args),
                    "get_vehicle_models_info" => await GetVehicleModelsInfoAsync(args),
                    "get_customer_purchase_trends" => await GetCustomerPurchaseTrendsAsync(args, dealerId),
                    "compare_vehicle_variants" => await CompareVehicleVariantsAsync(args),
                    "get_dealer_performance" => await GetDealerPerformanceAsync(args, dealerId),
                    "get_market_competition_analysis" => await GetMarketCompetitionAnalysisAsync(args),
                    _ => new { error = $"Function {functionName} không tồn tại" }
                };
            }
            catch (Exception ex)
            {
                return new { error = $"Lỗi thực thi function {functionName}: {ex.Message}" };
            }
        }

        private async Task<object> GetSalesStatisticsAsync(Dictionary<string, object> args, Guid? dealerId)
        {
            var startDate = args.ContainsKey("startDate") && args["startDate"] != null
                ? DateTime.Parse(args["startDate"].ToString()!)
                : DateTime.UtcNow.AddDays(-30);

            var endDate = args.ContainsKey("endDate") && args["endDate"] != null
                ? DateTime.Parse(args["endDate"].ToString()!)
                : DateTime.UtcNow;

            var query = _unitOfWork.Orders.GetQueryable()
                .Where(o => !o.IsDeleted && o.CreatedDate >= startDate && o.CreatedDate <= endDate);

            if (dealerId.HasValue)
            {
                query = query.Where(o => o.DealerId == dealerId.Value);
            }

            if (args.ContainsKey("orderType") && args["orderType"] != null)
            {
                var orderType = Enum.Parse<OrderType>(args["orderType"].ToString()!);
                query = query.Where(o => o.OrderType == orderType);
            }

            var statistics = await query
                .GroupBy(o => 1)
                .Select(g => new
                {
                    TotalOrders = g.Count(),
                    TotalRevenue = g.Sum(o => o.FinalAmount ?? 0),
                    CompletedOrders = g.Count(o => o.Status == OrderStatus.COMPLETED),
                    CanceledOrders = g.Count(o => o.Status == OrderStatus.CANCELED),
                    AverageOrderValue = g.Average(o => o.FinalAmount ?? 0)
                })
                .FirstOrDefaultAsync();

            var ordersByMonth = await query
                .GroupBy(o => new { o.CreatedDate.Year, o.CreatedDate.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.FinalAmount ?? 0)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return new
            {
                period = $"{startDate:yyyy-MM-dd} đến {endDate:yyyy-MM-dd}",
                summary = statistics ?? new
                {
                    TotalOrders = 0,
                    TotalRevenue = 0m,
                    CompletedOrders = 0,
                    CanceledOrders = 0,
                    AverageOrderValue = 0m
                },
                monthlyBreakdown = ordersByMonth
            };
        }

        private async Task<object> GetTopSellingModelsAsync(Dictionary<string, object> args, Guid? dealerId)
        {
            var topCount = args.ContainsKey("topCount") && args["topCount"] != null
                ? Convert.ToInt32(args["topCount"])
                : 5;

            var query = _unitOfWork.OrderDetails.GetQueryable()
                .Include(od => od.Order)
                .Include(od => od.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                .Where(od => !od.IsDeleted && !od.Order.IsDeleted);

            if (dealerId.HasValue)
            {
                query = query.Where(od => od.Order.DealerId == dealerId.Value);
            }

            if (args.ContainsKey("startDate") && args["startDate"] != null)
            {
                var startDate = DateTime.Parse(args["startDate"].ToString()!);
                query = query.Where(od => od.Order.CreatedDate >= startDate);
            }

            if (args.ContainsKey("endDate") && args["endDate"] != null)
            {
                var endDate = DateTime.Parse(args["endDate"].ToString()!);
                query = query.Where(od => od.Order.CreatedDate <= endDate);
            }

            var topModels = await query
                .GroupBy(od => new
                {
                    ModelId = od.VehicleVariant.ModelId,
                    ModelName = od.VehicleVariant.VehicleModel.Name,
                    ModelCode = od.VehicleVariant.VehicleModel.Code
                })
                .Select(g => new
                {
                    ModelName = g.Key.ModelName,
                    ModelCode = g.Key.ModelCode,
                    TotalQuantitySold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Quantity * od.UnitPrice),
                    OrderCount = g.Select(od => od.OrderId).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(topCount)
                .ToListAsync();

            return new
            {
                topCount = topCount,
                models = topModels
            };
        }

        private async Task<object> SearchVehiclesByCriteriaAsync(Dictionary<string, object> args)
        {
            var query = _unitOfWork.VehicleVariants.GetQueryable()
                .Include(vv => vv.VehicleModel)
                .Where(vv => !vv.IsDeleted && vv.VehicleModel.Status);

            if (args.ContainsKey("minPrice") && args["minPrice"] != null)
            {
                var minPrice = Convert.ToDecimal(args["minPrice"]);
                query = query.Where(vv => vv.Price >= minPrice);
            }

            if (args.ContainsKey("maxPrice") && args["maxPrice"] != null)
            {
                var maxPrice = Convert.ToDecimal(args["maxPrice"]);
                query = query.Where(vv => vv.Price <= maxPrice);
            }

            if (args.ContainsKey("minDistance") && args["minDistance"] != null)
            {
                var minDistance = args["minDistance"].ToString();
                query = query.Where(vv => vv.DistancePerCharge != null && 
                    EF.Functions.Like(vv.DistancePerCharge, $"%{minDistance}%"));
            }

            if (args.ContainsKey("minSpeed") && args["minSpeed"] != null)
            {
                var minSpeed = Convert.ToDecimal(args["minSpeed"]);
                query = query.Where(vv => vv.MaximumSpeed >= minSpeed);
            }

            if (args.ContainsKey("color") && args["color"] != null)
            {
                var color = args["color"].ToString()!.ToLower();
                query = query.Where(vv => vv.Color != null && vv.Color.ToLower().Contains(color));
            }

            var vehicles = await query
                .Select(vv => new
                {
                    ModelName = vv.VehicleModel.Name,
                    Color = vv.Color,
                    Price = vv.Price,
                    DistancePerCharge = vv.DistancePerCharge,
                    MaximumSpeed = vv.MaximumSpeed,
                    BatteryType = vv.BatteryType,
                    ChargingTime = vv.ChargingTime,
                    Engine = vv.Engine,
                    Description = vv.Description
                })
                .Take(10)
                .ToListAsync();

            return new
            {
                count = vehicles.Count,
                vehicles = vehicles
            };
        }

        private async Task<object> GetVehicleModelsInfoAsync(Dictionary<string, object> args)
        {
            var query = _unitOfWork.VehicleModels.GetQueryable()
                .Include(vm => vm.VehicleVariants)
                .Where(vm => !vm.IsDeleted && vm.Status);

            if (args.ContainsKey("modelName") && args["modelName"] != null)
            {
                var modelName = args["modelName"].ToString()!.ToLower();
                query = query.Where(vm => vm.Name.ToLower().Contains(modelName));
            }

            var models = await query
                .Select(vm => new
                {
                    Name = vm.Name,
                    Code = vm.Code,
                    Description = vm.Description,
                    LaunchDate = vm.LaunchDate,
                    ImageUrl = vm.ImageUrl,
                    Ranking = vm.Ranking,
                    VariantsCount = vm.VehicleVariants.Count(vv => !vv.IsDeleted),
                    PriceRange = new
                    {
                        Min = vm.VehicleVariants.Where(vv => !vv.IsDeleted).Min(vv => vv.Price),
                        Max = vm.VehicleVariants.Where(vv => !vv.IsDeleted).Max(vv => vv.Price)
                    }
                })
                .ToListAsync();

            return new
            {
                count = models.Count,
                models = models
            };
        }

        private async Task<object> GetCustomerPurchaseTrendsAsync(Dictionary<string, object> args, Guid? dealerId)
        {
            var startDate = args.ContainsKey("startDate") && args["startDate"] != null
                ? DateTime.Parse(args["startDate"].ToString()!)
                : DateTime.UtcNow.AddMonths(-6);

            var endDate = args.ContainsKey("endDate") && args["endDate"] != null
                ? DateTime.Parse(args["endDate"].ToString()!)
                : DateTime.UtcNow;

            var query = _unitOfWork.Orders.GetQueryable()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel)
                .Where(o => !o.IsDeleted && 
                    o.OrderType == OrderType.B2C && 
                    o.CreatedDate >= startDate && 
                    o.CreatedDate <= endDate);

            if (dealerId.HasValue)
            {
                query = query.Where(o => o.DealerId == dealerId.Value);
            }

            var trends = await query
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => od.VehicleVariant.VehicleModel.Name)
                .Select(g => new
                {
                    ModelName = g.Key,
                    Purchases = g.Count(),
                    AveragePrice = g.Average(od => od.UnitPrice)
                })
                .OrderByDescending(x => x.Purchases)
                .Take(10)
                .ToListAsync();

            var priceDistribution = await query
                .GroupBy(o => o.FinalAmount / 10000000 * 10000000)
                .Select(g => new
                {
                    PriceRange = $"{g.Key:N0} - {g.Key + 10000000:N0} VND",
                    Count = g.Count()
                })
                .OrderBy(x => x.PriceRange)
                .ToListAsync();

            return new
            {
                period = $"{startDate:yyyy-MM-dd} đến {endDate:yyyy-MM-dd}",
                popularModels = trends,
                priceDistribution = priceDistribution
            };
        }

        private async Task<object> CompareVehicleVariantsAsync(Dictionary<string, object> args)
        {
            if (!args.ContainsKey("modelName") || args["modelName"] == null)
            {
                return new { error = "Cần cung cấp tên mẫu xe để so sánh" };
            }

            var modelName = args["modelName"].ToString()!.ToLower();

            var variants = await _unitOfWork.VehicleVariants.GetQueryable()
                .Include(vv => vv.VehicleModel)
                .Where(vv => !vv.IsDeleted && 
                    vv.VehicleModel.Name.ToLower().Contains(modelName) &&
                    vv.VehicleModel.Status)
                .Select(vv => new
                {
                    ModelName = vv.VehicleModel.Name,
                    Color = vv.Color,
                    Price = vv.Price,
                    Engine = vv.Engine,
                    BatteryType = vv.BatteryType,
                    BatteryLife = vv.BatteryLife,
                    Capacity = vv.Capacity,
                    MaximumSpeed = vv.MaximumSpeed,
                    DistancePerCharge = vv.DistancePerCharge,
                    ChargingTime = vv.ChargingTime,
                    ChargingCapacity = vv.ChargingCapacity,
                    Weight = vv.Weight,
                    Dimensions = new
                    {
                        Length = vv.Length,
                        Width = vv.Width,
                        Height = vv.Height
                    },
                    Brakes = vv.Brakes,
                    ShockAbsorbers = vv.ShockAbsorbers,
                    GroundClearance = vv.GroundClearance
                })
                .ToListAsync();

            if (!variants.Any())
            {
                return new { error = $"Không tìm thấy phiên bản nào của mẫu xe {modelName}" };
            }

            return new
            {
                modelName = variants.First().ModelName,
                variantsCount = variants.Count,
                variants = variants
            };
        }

        private async Task<object> GetDealerPerformanceAsync(Dictionary<string, object> args, Guid? dealerId)
        {
            if (!dealerId.HasValue)
            {
                return new { error = "Cần cung cấp DealerId để xem hiệu suất đại lý" };
            }

            var startDate = args.ContainsKey("startDate") && args["startDate"] != null
                ? DateTime.Parse(args["startDate"].ToString()!)
                : DateTime.UtcNow.AddMonths(-3);

            var endDate = args.ContainsKey("endDate") && args["endDate"] != null
                ? DateTime.Parse(args["endDate"].ToString()!)
                : DateTime.UtcNow;

            var dealer = await _unitOfWork.Dealers.GetByIdAsync(dealerId.Value);
            if (dealer == null)
            {
                return new { error = "Không tìm thấy đại lý" };
            }

            var orders = await _unitOfWork.Orders.GetQueryable()
                .Where(o => !o.IsDeleted && 
                    o.DealerId == dealerId.Value && 
                    o.CreatedDate >= startDate && 
                    o.CreatedDate <= endDate)
                .ToListAsync();

            var b2cOrders = orders.Where(o => o.OrderType == OrderType.B2C).ToList();
            var b2bOrders = orders.Where(o => o.OrderType == OrderType.B2B).ToList();

            var customers = await _unitOfWork.Customers.GetQueryable()
                .Where(c => !c.IsDeleted && c.DealerId == dealerId.Value)
                .CountAsync();

            return new
            {
                dealerName = dealer.Name,
                period = $"{startDate:yyyy-MM-dd} đến {endDate:yyyy-MM-dd}",
                totalCustomers = customers,
                b2cSales = new
                {
                    orderCount = b2cOrders.Count,
                    totalRevenue = b2cOrders.Sum(o => o.FinalAmount ?? 0),
                    completedOrders = b2cOrders.Count(o => o.Status == OrderStatus.COMPLETED)
                },
                b2bSales = new
                {
                    orderCount = b2bOrders.Count,
                    totalRevenue = b2bOrders.Sum(o => o.FinalAmount ?? 0),
                    completedOrders = b2bOrders.Count(o => o.Status == OrderStatus.COMPLETED)
                },
                overall = new
                {
                    totalOrders = orders.Count,
                    totalRevenue = orders.Sum(o => o.FinalAmount ?? 0),
                    averageOrderValue = orders.Any() ? orders.Average(o => o.FinalAmount ?? 0) : 0
                }
            };
        }

        private async Task<object> GetMarketCompetitionAnalysisAsync(Dictionary<string, object> args)
        {
            var existingModels = await _unitOfWork.VehicleModels.GetQueryable()
                .Include(vm => vm.VehicleVariants)
                .Where(vm => !vm.IsDeleted && vm.Status)
                .Select(vm => new
                {
                    Name = vm.Name,
                    Description = vm.Description,
                    Ranking = vm.Ranking,
                    MinPrice = vm.VehicleVariants.Where(vv => !vv.IsDeleted).Min(vv => vv.Price),
                    MaxPrice = vm.VehicleVariants.Where(vv => !vv.IsDeleted).Max(vv => vv.Price),
                    LaunchDate = vm.LaunchDate
                })
                .ToListAsync();

            var salesData = await _unitOfWork.OrderDetails.GetQueryable()
                .Include(od => od.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                .Where(od => !od.IsDeleted && !od.Order.IsDeleted)
                .GroupBy(od => od.VehicleVariant.VehicleModel.Name)
                .Select(g => new
                {
                    ModelName = g.Key,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToListAsync();

            var priceRange = string.Empty;
            if (args.ContainsKey("priceRange") && args["priceRange"] != null)
            {
                priceRange = args["priceRange"].ToString()!;
            }

            var analysis = new
            {
                currentModels = existingModels,
                salesPerformance = salesData,
                marketInsights = new
                {
                    averagePrice = existingModels.Any() 
                        ? existingModels.Average(m => (m.MinPrice + m.MaxPrice) / 2) 
                        : 0,
                    priceRange = priceRange,
                    recommendation = GetMarketRecommendation(priceRange, existingModels, salesData)
                }
            };

            return analysis;
        }

        private string GetMarketRecommendation(string priceRange, dynamic existingModels, dynamic salesData)
        {
            var recommendations = new List<string>();

            if (priceRange == "low")
            {
                recommendations.Add("Phân khúc giá rẻ (dưới 20 triệu): Nên phát triển mẫu xe điện giá rẻ, thiết kế đơn giản, phù hợp sinh viên và người đi làm gần.");
                recommendations.Add("Tập trung vào pin bền, chi phí vận hành thấp, dễ bảo dưỡng.");
            }
            else if (priceRange == "medium")
            {
                recommendations.Add("Phân khúc trung cấp (20-40 triệu): Cần cải thiện thiết kế thẩm mỹ, tăng tính năng an toàn và công nghệ.");
                recommendations.Add("Quãng đường 1 lần sạc nên từ 80-120km, tốc độ tối đa 60-80km/h.");
            }
            else if (priceRange == "high")
            {
                recommendations.Add("Phân khúc cao cấp (trên 40 triệu): Tập trung vào công nghệ cao, thiết kế sang trọng, tính năng thông minh.");
                recommendations.Add("Pin dung lượng lớn, quãng đường 150km+, tích hợp IoT và app điều khiển.");
            }
            else
            {
                recommendations.Add("Phân tích chung: Thị trường xe điện Việt Nam đang phát triển mạnh.");
                recommendations.Add("Cần chú trọng vào chất lượng pin, hệ thống sạc, dịch vụ hậu mãi và giá cả cạnh tranh.");
            }

            return string.Join(" ", recommendations);
        }

        private async Task<List<GeminiMessage>> GetChatHistoryAsync(string sessionId)
        {
            var cacheKey = $"chat_history:{sessionId}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(cachedData))
            {
                return new List<GeminiMessage>();
            }

            return JsonSerializer.Deserialize<List<GeminiMessage>>(cachedData) ?? new List<GeminiMessage>();
        }

        private async Task SaveChatHistoryAsync(string sessionId, List<GeminiMessage> history)
        {
            var cacheKey = $"chat_history:{sessionId}";
            var jsonData = JsonSerializer.Serialize(history);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CHAT_HISTORY_EXPIRATION_MINUTES)
            };

            await _cache.SetStringAsync(cacheKey, jsonData, options);
        }
    }

    public class GeminiRequest
    {
        public List<GeminiMessage> Contents { get; set; } = new();
        public List<GeminiTool>? Tools { get; set; }
        public GeminiSystemInstruction? SystemInstruction { get; set; }
    }

    public class GeminiSystemInstruction
    {
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiMessage
    {
        public string Role { get; set; } = string.Empty;
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiPart
    {
        public string? Text { get; set; }
        public GeminiFunctionCall? FunctionCall { get; set; }
        public GeminiFunctionResponse? FunctionResponse { get; set; }
    }

    public class GeminiFunctionCall
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Args { get; set; } = new();
    }

    public class GeminiFunctionResponse
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Response { get; set; } = new();
    }

    public class GeminiTool
    {
        public List<GeminiFunctionDeclaration> FunctionDeclarations { get; set; } = new();
    }

    public class GeminiFunctionDeclaration
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GeminiFunctionParameters? Parameters { get; set; }
    }

    public class GeminiFunctionParameters
    {
        public string Type { get; set; } = "object";
        public Dictionary<string, GeminiFunctionProperty> Properties { get; set; } = new();
        public List<string>? Required { get; set; }
    }

    public class GeminiFunctionProperty
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string>? Enum { get; set; }
    }

    public class GeminiResponse
    {
        public List<GeminiCandidate> Candidates { get; set; } = new();
    }

    public class GeminiCandidate
    {
        public GeminiContent Content { get; set; } = new();
    }

    public class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; } = new();
        public string Role { get; set; } = string.Empty;
    }
}

using System.Collections.Generic;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Helpers
{
    public static class ChatbotPermissions
    {
        private static readonly Dictionary<AccountRole, HashSet<string>> RolePermissions = new()
        {
            {
                AccountRole.EVM_ADMIN, new HashSet<string>
                {
                    "get_sales_statistics",
                    "get_top_selling_models",
                    "search_vehicles_by_criteria",
                    "get_vehicle_models_info",
                    "get_customer_purchase_trends",
                    "compare_vehicle_variants",
                    "get_dealer_performance",
                    "get_market_competition_analysis"
                }
            },
            {
                AccountRole.EVM_STAFF, new HashSet<string>
                {
                    "get_sales_statistics",
                    "get_top_selling_models",
                    "search_vehicles_by_criteria",
                    "get_vehicle_models_info",
                    "get_customer_purchase_trends",
                    "compare_vehicle_variants",
                    "get_dealer_performance"
                }
            },
            {
                AccountRole.DEALER_MANAGER, new HashSet<string>
                {
                    "get_sales_statistics",
                    "get_top_selling_models",
                    "search_vehicles_by_criteria",
                    "get_vehicle_models_info",
                    "get_customer_purchase_trends",
                    "compare_vehicle_variants",
                    "get_dealer_performance"
                }
            },
            {
                AccountRole.DEALER_STAFF, new HashSet<string>
                {
                    "search_vehicles_by_criteria",
                    "get_vehicle_models_info",
                    "compare_vehicle_variants"
                }
            }
        };

        public static bool CanAccessFunction(AccountRole role, string functionName)
        {
            return RolePermissions.ContainsKey(role) && 
                   RolePermissions[role].Contains(functionName);
        }

        public static HashSet<string> GetAllowedFunctions(AccountRole role)
        {
            return RolePermissions.ContainsKey(role) 
                ? RolePermissions[role] 
                : new HashSet<string>();
        }

        public static string GetPermissionDeniedMessage(AccountRole role)
        {
            return role switch
            {
                AccountRole.DEALER_STAFF => "Bạn chỉ có quyền hỏi về thông tin xe và so sánh các phiên bản xe.",
                AccountRole.DEALER_MANAGER => "Bạn có quyền hỏi về thông tin xe, doanh số và hiệu suất đại lý của mình.",
                AccountRole.EVM_STAFF => "Bạn có quyền truy cập hầu hết thông tin ngoại trừ phân tích cạnh tranh thị trường.",
                AccountRole.EVM_ADMIN => "Bạn có quyền truy cập tất cả thông tin.",
                _ => "Bạn không có quyền truy cập chức năng này."
            };
        }
    }
}

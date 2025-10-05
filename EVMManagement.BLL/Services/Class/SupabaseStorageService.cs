using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Supabase;
using EVMManagement.BLL.Services.Interface;

namespace EVMManagement.BLL.Services.Class
{
    public class SupabaseStorageService : IStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly Supabase.Client _supabaseClient;
        private readonly string _bucketName;

        public SupabaseStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _bucketName = _configuration["SupabaseSettings:BucketName"] 
                ?? throw new InvalidOperationException("SupabaseSettings:BucketName is not configured.");
            
            var url = _configuration["SupabaseSettings:Url"] 
                ?? throw new InvalidOperationException("SupabaseSettings:Url is not configured.");
            var key = _configuration["SupabaseSettings:Key"] 
                ?? throw new InvalidOperationException("SupabaseSettings:Key is not configured.");
            
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            };

            _supabaseClient = new Supabase.Client(url, key, options);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder = "")
        {
            var path = string.IsNullOrEmpty(folder) ? fileName : $"{folder}/{fileName}";
            
            // Convert stream to byte array
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Upload to Supabase Storage
            var result = await _supabaseClient.Storage
                .From(_bucketName)
                .Upload(fileBytes, path);

            return GetPublicUrl(path);
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extract path from URL
                var path = ExtractPathFromUrl(fileUrl);
                
                await _supabaseClient.Storage
                    .From(_bucketName)
                    .Remove(path);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileUrl)
        {
            var path = ExtractPathFromUrl(fileUrl);
            
            var fileBytes = await _supabaseClient.Storage
                .From(_bucketName)
                .Download(path, null);

            return new MemoryStream(fileBytes);
        }

        public string GetPublicUrl(string filePath)
        {
            return _supabaseClient.Storage
                .From(_bucketName)
                .GetPublicUrl(filePath);
        }

        private string ExtractPathFromUrl(string fileUrl)
        {
            // Extract the file path from the full URL
            var uri = new Uri(fileUrl);
            var segments = uri.Segments;
            
            // Find the bucket name index and get everything after it
            var bucketIndex = Array.FindIndex(segments, s => s.Contains(_bucketName));
            if (bucketIndex >= 0 && bucketIndex < segments.Length - 1)
            {
                return string.Join("", segments[(bucketIndex + 1)..]).TrimStart('/');
            }

            return fileUrl;
        }
    }
}

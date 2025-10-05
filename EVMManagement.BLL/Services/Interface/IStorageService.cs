using System.IO;
using System.Threading.Tasks;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder = "");
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<Stream> DownloadFileAsync(string fileUrl);
        string GetPublicUrl(string filePath);
    }
}

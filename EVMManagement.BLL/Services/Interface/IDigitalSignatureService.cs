using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.DigitalSignature;
using EVMManagement.BLL.DTOs.Response.DigitalSignature;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IDigitalSignatureService
    {
        Task<OtpRequestResponse> RequestOtpAsync(RequestOtpDto dto, string ipAddress, string userAgent);
        Task<DigitalSignatureResponse> VerifyOtpAsync(VerifyOtpDto dto);
        Task<DigitalSignatureResponse> CompleteSignatureAsync(CompleteSignatureDto dto);
        Task<DigitalSignatureResponse?> GetByIdAsync(Guid id);
        Task<List<DigitalSignatureResponse>> GetByContractIdAsync(Guid contractId);
        Task<List<DigitalSignatureResponse>> GetByHandoverRecordIdAsync(Guid handoverRecordId);
        Task<List<DigitalSignatureResponse>> GetByDealerContractIdAsync(Guid dealerContractId);
    }
}

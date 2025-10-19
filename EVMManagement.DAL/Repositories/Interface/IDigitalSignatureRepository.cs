using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IDigitalSignatureRepository : IGenericRepository<DigitalSignature>
    {
        Task<List<DigitalSignature>> GetByContractIdAsync(Guid contractId);
        Task<List<DigitalSignature>> GetByHandoverRecordIdAsync(Guid handoverRecordId);
        Task<DigitalSignature?> GetPendingOtpAsync(string email, SignatureEntityType entityType, Guid? contractId, Guid? handoverRecordId);
    }
}

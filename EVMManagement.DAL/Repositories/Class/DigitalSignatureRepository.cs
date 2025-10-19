using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.DAL.Repositories.Class
{
    public class DigitalSignatureRepository : GenericRepository<DigitalSignature>, IDigitalSignatureRepository
    {
        public DigitalSignatureRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<DigitalSignature>> GetByContractIdAsync(Guid contractId)
        {
            return await _context.DigitalSignatures
                .Where(ds => ds.ContractId == contractId && !ds.IsDeleted)
                .OrderByDescending(ds => ds.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<DigitalSignature>> GetByHandoverRecordIdAsync(Guid handoverRecordId)
        {
            return await _context.DigitalSignatures
                .Where(ds => ds.HandoverRecordId == handoverRecordId && !ds.IsDeleted)
                .OrderByDescending(ds => ds.CreatedDate)
                .ToListAsync();
        }

        public async Task<DigitalSignature?> GetPendingOtpAsync(string email, SignatureEntityType entityType, Guid? contractId, Guid? handoverRecordId)
        {
            var query = _context.DigitalSignatures
                .Where(ds => ds.SignerEmail == email 
                    && ds.EntityType == entityType
                    && ds.Status == SignatureStatus.OTP_SENT
                    && ds.OtpExpiresAt > DateTime.UtcNow
                    && !ds.IsDeleted);

            if (contractId.HasValue)
            {
                query = query.Where(ds => ds.ContractId == contractId.Value);
            }

            if (handoverRecordId.HasValue)
            {
                query = query.Where(ds => ds.HandoverRecordId == handoverRecordId.Value);
            }

            return await query.OrderByDescending(ds => ds.CreatedDate).FirstOrDefaultAsync();
        }
    }
}

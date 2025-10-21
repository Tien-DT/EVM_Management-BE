using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EVMManagement.BLL.DTOs.Request.DigitalSignature;
using EVMManagement.BLL.DTOs.Response.DigitalSignature;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class DigitalSignatureService : IDigitalSignatureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public DigitalSignatureService(IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<OtpRequestResponse> RequestOtpAsync(RequestOtpDto dto, string ipAddress, string userAgent)
        {
            if (dto.EntityType == SignatureEntityType.CONTRACT && !dto.ContractId.HasValue)
            {
                throw new ArgumentException("ContractId is required for CONTRACT entity type");
            }

            if (dto.EntityType == SignatureEntityType.HANDOVER_RECORD && !dto.HandoverRecordId.HasValue)
            {
                throw new ArgumentException("HandoverRecordId is required for HANDOVER_RECORD entity type");
            }

            if (dto.EntityType == SignatureEntityType.DEALER_CONTRACT && !dto.DealerContractId.HasValue)
            {
                throw new ArgumentException("DealerContractId is required for DEALER_CONTRACT entity type");
            }

            var existingOtp = await _unitOfWork.DigitalSignatures.GetPendingOtpAsync(
                dto.SignerEmail, 
                dto.EntityType, 
                dto.ContractId, 
                dto.HandoverRecordId,
                dto.DealerContractId
            );

            if (existingOtp != null)
            {
                existingOtp.Status = SignatureStatus.EXPIRED;
                existingOtp.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.DigitalSignatures.Update(existingOtp);
            }

            var otpCode = GenerateOtpCode();
            var hashedOtp = HashOtp(otpCode);

            var signature = new DigitalSignature
            {
                SignerEmail = dto.SignerEmail,
                EntityType = dto.EntityType,
                ContractId = dto.ContractId,
                HandoverRecordId = dto.HandoverRecordId,
                DealerContractId = dto.DealerContractId,
                Status = SignatureStatus.OTP_SENT,
                OtpCode = hashedOtp,
                OtpExpiresAt = DateTime.UtcNow.AddMinutes(5),
                OtpAttemptCount = 0,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _unitOfWork.DigitalSignatures.AddAsync(signature);
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendOtpEmailAsync(dto.SignerEmail, otpCode, signature.OtpExpiresAt.Value);

            return new OtpRequestResponse
            {
                SignatureId = signature.Id,
                Message = $"OTP has been sent to {dto.SignerEmail}",
                ExpiresAt = signature.OtpExpiresAt.Value
            };
        }

        public async Task<DigitalSignatureResponse> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var signature = await _unitOfWork.DigitalSignatures.GetPendingOtpAsync(
                dto.SignerEmail, 
                dto.EntityType, 
                dto.ContractId, 
                dto.HandoverRecordId,
                dto.DealerContractId
            );

            if (signature == null)
            {
                throw new InvalidOperationException("No pending OTP found or OTP has expired");
            }

            if (signature.OtpAttemptCount >= 5)
            {
                signature.Status = SignatureStatus.EXPIRED;
                signature.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.DigitalSignatures.Update(signature);
                await _unitOfWork.SaveChangesAsync();
                throw new InvalidOperationException("Maximum OTP verification attempts exceeded");
            }

            var hashedInputOtp = HashOtp(dto.OtpCode);

            if (signature.OtpCode != hashedInputOtp)
            {
                signature.OtpAttemptCount++;
                signature.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.DigitalSignatures.Update(signature);
                await _unitOfWork.SaveChangesAsync();
                throw new InvalidOperationException($"Invalid OTP code. Attempts remaining: {5 - signature.OtpAttemptCount}");
            }

            signature.Status = SignatureStatus.OTP_VERIFIED;
            signature.VerificationCode = hashedInputOtp;
            signature.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.DigitalSignatures.Update(signature);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DigitalSignatureResponse>(signature);
        }

        public async Task<DigitalSignatureResponse> CompleteSignatureAsync(CompleteSignatureDto dto)
        {
            var signature = await _unitOfWork.DigitalSignatures.GetByIdAsync(dto.SignatureId);

            if (signature == null)
            {
                throw new InvalidOperationException("Signature not found");
            }

            if (signature.Status != SignatureStatus.OTP_VERIFIED)
            {
                throw new InvalidOperationException("OTP must be verified before completing signature");
            }

            signature.SignerName = dto.SignerName;
            signature.FileUrl = dto.FileUrl;
            signature.Notes = dto.Notes;
            signature.SignatureData = GenerateSignatureData(signature);
            signature.SignedAt = DateTime.UtcNow;
            signature.Status = SignatureStatus.SIGNED;
            signature.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.DigitalSignatures.Update(signature);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DigitalSignatureResponse>(signature);
        }

        public async Task<DigitalSignatureResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.DigitalSignatures.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<DigitalSignatureResponse>(entity);
        }

        public async Task<List<DigitalSignatureResponse>> GetByContractIdAsync(Guid contractId)
        {
            var signatures = await _unitOfWork.DigitalSignatures.GetByContractIdAsync(contractId);
            return signatures.Select(s => _mapper.Map<DigitalSignatureResponse>(s)).ToList();
        }

        public async Task<List<DigitalSignatureResponse>> GetByHandoverRecordIdAsync(Guid handoverRecordId)
        {
            var signatures = await _unitOfWork.DigitalSignatures.GetByHandoverRecordIdAsync(handoverRecordId);
            return signatures.Select(s => _mapper.Map<DigitalSignatureResponse>(s)).ToList();
        }

        public async Task<List<DigitalSignatureResponse>> GetByDealerContractIdAsync(Guid dealerContractId)
        {
            var signatures = await _unitOfWork.DigitalSignatures.GetByDealerContractIdAsync(dealerContractId);
            return signatures.Select(s => _mapper.Map<DigitalSignatureResponse>(s)).ToList();
        }

        private string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(otp);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GenerateSignatureData(DigitalSignature signature)
        {
            var data = $"{signature.SignerEmail}|{signature.SignerName}|{signature.SignedAt}|{signature.VerificationCode}";
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(data);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

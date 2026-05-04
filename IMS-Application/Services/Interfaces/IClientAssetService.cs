using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace IMS_Application.Services.Interfaces
{
    public interface IClientAssetService
    {
        Task<Result<object>> Add(CreateClientAssetDto dto, int userId);
        Task<Result<IEnumerable<ClientAsset>>> GetAll();
        Task<Result<ClientAsset?>> GetById(int id);
        Task<Result<bool>> QuickUpdate(int id, EditClientAssetQuickDto dto);
        Task<Result<bool>> FullUpdate(int id, EditClientAssetFullDto dto);
        Task<Result<bool>> Delete(int id);
        Task<IEnumerable<ClientAsset>> FilterAsync(ClientAssetFilterDto filter);
        
        Task<Result<AttachmentResponseDto>> UploadAttachmentAsync(int clientAssetId, IFormFile file, int userId);
        Task<Result<IEnumerable<AttachmentResponseDto>>> GetAttachmentsByAssetAsync(int assetId);
        Task<Result<AttachmentResponseDto?>> GetAttachmentByIdAsync(int id);
        Task<Result<bool>> DeleteAttachmentAsync(int id, int deletedBy);
        Task<Result<(byte[] FileBytes, string ContentType, string FileName)>> DownloadAttachmentAsync(int attachmentId);
        Task<Result<(byte[] FileBytes, string ContentType, string FileName)>> ViewAttachmentAsync(int attachmentId);
    }

}
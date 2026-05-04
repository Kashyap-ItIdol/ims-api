using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Extensions;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IMS_Application.Services
{
    public class ClientAssetService : IClientAssetService
    {
        private readonly IClientAssetRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientAssetService> _logger;

        public ClientAssetService(IClientAssetRepository repository, IMapper mapper, ILogger<ClientAssetService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<object>> Add(CreateClientAssetDto dto, int userId)
        {
            if (dto == null)
                return Result<object>.Failure(ErrorMessages.ClientAssetRequired, 400);

            try
            {
                // Use AutoMapper to map DTO to entity
                var entity = _mapper.Map<ClientAsset>(dto);
                entity.CreatedBy = userId;
                
                await _repository.AddAsync(entity);
                await _repository.SaveChangesAsync();
                
                var response = new { id = entity.Id, message = SuccessMessages.ClientAssetCreated };
                return Result<object>.Success(response, SuccessMessages.ClientAssetCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client asset");
                return Result<object>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<IEnumerable<ClientAsset>>> GetAll()
        {
            try
            {
                var assets = await _repository.GetAllAsync();
                return Result<IEnumerable<ClientAsset>>.Success(assets, SuccessMessages.ClientAssetsRetrieved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all client assets");
                return Result<IEnumerable<ClientAsset>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<ClientAsset?>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return Result<ClientAsset?>.Failure(ErrorMessages.InvalidClientAssetId, 400);

                var asset = await _repository.GetByIdAsync(id);
                if (asset == null)
                    return Result<ClientAsset?>.Failure(ErrorMessages.ClientAssetNotFound, 404);

                return Result<ClientAsset?>.Success(asset, SuccessMessages.ClientAssetRetrieved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client asset with ID: {Id}", id);
                return Result<ClientAsset?>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<bool>> QuickUpdate(int id, EditClientAssetQuickDto dto)
        {
            try
            {
                if (id <= 0)
                    return Result<bool>.Failure(ErrorMessages.InvalidClientAssetId, 400);

                var asset = await _repository.GetByIdAsync(id);
                if (asset == null)
                    return Result<bool>.Failure(ErrorMessages.ClientAssetNotFound, 404);

                if (dto.AssetName != null)
                    asset.AssetName = dto.AssetName;

                if (dto.SerialNumber != null)
                    asset.SerialNumber = dto.SerialNumber;

                asset.UpdatedAt = DateTime.UtcNow;

                var result = await _repository.UpdateAsync(asset);

                if (result)
                    return Result<bool>.Success(true, SuccessMessages.ClientAssetQuickUpdated);
                else
                    return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client asset");
                return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<bool>> FullUpdate(int id, EditClientAssetFullDto dto)
        {
            try
            {
                if (id <= 0)
                    return Result<bool>.Failure(ErrorMessages.InvalidClientAssetId, 400);

                var asset = await _repository.GetByIdAsync(id);
                if (asset == null)
                    return Result<bool>.Failure(ErrorMessages.ClientAssetNotFound, 404);

                _mapper.Map(dto, asset);
                asset.UpdatedAt = DateTime.UtcNow;

                var result = await _repository.UpdateAsync(asset);
                
                if (result)
                    return Result<bool>.Success(true, SuccessMessages.ClientAssetUpdated);
                else
                    return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client asset");
                return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<bool>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return Result<bool>.Failure(ErrorMessages.InvalidClientAssetId, 400);

                var exists = await _repository.ExistsAsync(id);
                if (!exists)
                    return Result<bool>.Failure(ErrorMessages.ClientAssetNotFound, 404);

                var result = await _repository.DeleteAsync(id);
                
                if (result)
                    return Result<bool>.Success(true, SuccessMessages.ClientAssetDeleted);
                else
                    return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client asset");
                return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<AttachmentResponseDto>> UploadAttachmentAsync(int clientAssetId, IFormFile file, int userId)
        {
            try
            {
                var clientAsset = await _repository.GetByIdAsync(clientAssetId);
                if (clientAsset == null)
                    return Result<AttachmentResponseDto>.Failure(ErrorMessages.ClientAssetNotFound, 404);

                // Generate unique file name and save file
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachment = new ClientAssetAttachment
                {
                    ClientAssetId = clientAssetId,
                    FileName = file.FileName,
                    FilePath = fileName
                };

                await _repository.AddAttachmentAsync(attachment);
                var responseDto = _mapper.Map<AttachmentResponseDto>(attachment);
                
                return Result<AttachmentResponseDto>.Success(responseDto, SuccessMessages.AttachmentUploaded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment for ClientAsset {ClientAssetId}", clientAssetId);
                return Result<AttachmentResponseDto>.Failure(ErrorMessages.AttachmentUploadFailed, 500);
            }
        }

        public async Task<Result<IEnumerable<AttachmentResponseDto>>> GetAttachmentsByAssetAsync(int assetId)
        {
            try
            {
                var attachments = await _repository.GetAttachmentsByAssetIdAsync(assetId);
                var responseDtos = _mapper.Map<IEnumerable<AttachmentResponseDto>>(attachments);
                return Result<IEnumerable<AttachmentResponseDto>>.Success(responseDtos, SuccessMessages.AttachmentsRetrieved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachments for asset {AssetId}", assetId);
                return Result<IEnumerable<AttachmentResponseDto>>.Failure(ErrorMessages.AttachmentDownloadFailed, 500);
            }
        }

        public async Task<Result<AttachmentResponseDto?>> GetAttachmentByIdAsync(int id)
        {
            try
            {
                var attachment = await _repository.GetAttachmentByIdAsync(id);
                if (attachment == null)
                    return Result<AttachmentResponseDto?>.Success(null, ErrorMessages.AttachmentNotFound);

                var responseDto = _mapper.Map<AttachmentResponseDto>(attachment);
                return Result<AttachmentResponseDto?>.Success(responseDto, SuccessMessages.AttachmentRetrieved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachment with ID: {Id}", id);
                return Result<AttachmentResponseDto?>.Failure(ErrorMessages.AttachmentDownloadFailed, 500);
            }
        }

        public async Task<Result<(byte[] FileBytes, string ContentType, string FileName)>> DownloadAttachmentAsync(int attachmentId)
        {
            try
            {
                var attachment = await _repository.GetAttachmentByIdAsync(attachmentId);
                if (attachment == null)
                    return Result<(byte[], string, string)>.Failure(ErrorMessages.AttachmentNotFound, 404);

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var filePath = Path.Combine(uploadsFolder, attachment.FilePath);
                
                if (!System.IO.File.Exists(filePath))
                    return Result<(byte[], string, string)>.Failure(ErrorMessages.FileNotFound, 404);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = FileHelperExtensions.GetContentType(attachment.FileName);

                return Result<(byte[], string, string)>.Success((fileBytes, contentType, attachment.FileName), SuccessMessages.FileDownloaded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment with ID: {Id}", attachmentId);
                return Result<(byte[], string, string)>.Failure(ErrorMessages.AttachmentDownloadFailed, 500);
            }
        }

        public async Task<Result<(byte[] FileBytes, string ContentType, string FileName)>> ViewAttachmentAsync(int attachmentId)
        {
            return await DownloadAttachmentAsync(attachmentId);
        }

        public async Task<Result<bool>> DeleteAttachmentAsync(int id, int deletedBy)
        {
            try
            {
                var attachment = await _repository.GetAttachmentByIdAsync(id);
                if (attachment == null)
                    return Result<bool>.Failure(ErrorMessages.AttachmentNotFound, 404);

                // Soft delete
                attachment.IsDeleted = true;
                attachment.DeletedBy = deletedBy;
                attachment.DeletedAt = DateTime.UtcNow;

                await _repository.DeleteAttachmentAsync(attachment);
                return Result<bool>.Success(true, SuccessMessages.AttachmentDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment with ID: {Id}", id);
                return Result<bool>.Failure(ErrorMessages.AttachmentDeleteFailed, 500);
            }
        }

        public async Task<IEnumerable<ClientAsset>> FilterAsync(ClientAssetFilterDto filter)
        {
            var allAssets = await _repository.GetAllAsync();
            var query = allAssets.Where(x => !x.IsDeleted).AsQueryable();

            if (filter.Status != null && filter.Status.Count > 0)
            {
                query = query.Where(x => filter.Status.Contains(x.Status));
            }
            
            if (filter.Brand != null && filter.Brand.Count > 0)
            {
                query = query.Where(x => filter.Brand.Contains(x.Brand));
            }
            
            if (filter.ClientProject != null && filter.ClientProject.Count > 0)
            {
                query = query.Where(x => filter.ClientProject.Contains(x.ClientName));
            }
            
            if (filter.AssignedTo != null && filter.AssignedTo.Count > 0)
            {
                if (filter.AssignedTo.Contains(0))
                {
                    query = query.Where(x => x.AssignedTo == null);
                }
                else
                {
                    query = query.Where(x => x.AssignedTo.HasValue && 
                                          filter.AssignedTo.Contains(x.AssignedTo.Value));
                }
            }
            
            if (filter.AssignedFrom.HasValue)
            {
                query = query.Where(x => x.AssignedDate.HasValue && 
                                      x.AssignedDate.Value >= filter.AssignedFrom.Value);
            }
            if (filter.AssignedToDate.HasValue)
            {
                query = query.Where(x => x.AssignedDate.HasValue && 
                                      x.AssignedDate.Value <= filter.AssignedToDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                query = query.Where(x => 
                    x.AssetName.ToLower().Contains(searchLower) ||
                    x.SerialNumber.ToLower().Contains(searchLower) ||
                    x.Model.ToLower().Contains(searchLower) ||
                    x.Brand.ToLower().Contains(searchLower));
            }
            
            return query.ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using IMS_Application.Interfaces;

using IMS_Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IMS_Application.Services
{
    public class AssignedAssetService : IAssignedAssetService
    {
        private readonly IAssignedAssetRepository _repository;
        private readonly IClientAssetRepository _clientAssetRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AssignedAssetService> _logger;

        public AssignedAssetService(
            IAssignedAssetRepository repository,
            IClientAssetRepository clientAssetRepository,
            IMapper mapper,
            ILogger<AssignedAssetService> logger)
        {
            _repository = repository;
            _clientAssetRepository = clientAssetRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ClientAssignedAssetDto>> CreateAsync(CreateClientAssignedAssetDto dto, int userId)
        {
            if (dto == null)
                return Result<ClientAssignedAssetDto>.Failure(ErrorMessages.InvalidInput, 400);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.ItemName))
                return Result<ClientAssignedAssetDto>.Failure("itemName is required", 400);

            if (string.IsNullOrWhiteSpace(dto.SerialNumber))
                return Result<ClientAssignedAssetDto>.Failure("serialNumber is required", 400);

            if (dto.AssignedDate == default)
                return Result<ClientAssignedAssetDto>.Failure("assignedDate is required", 400);

            try
            {
                var entity = _mapper.Map<AssignedAsset>(dto);

                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = userId;
                entity.UpdatedAt = null;
                entity.UpdatedBy = null;
                entity.DeletedAt = null;
                entity.DeletedBy = null;
                entity.IsDeleted = false;

                await _repository.AddAsync(entity);

                var persisted = await _repository.UpdateAsync(entity);
                if (!persisted)
                    return Result<ClientAssignedAssetDto>.Failure(ErrorMessages.UnexpectedError, 500);

                // Reload with navigation property for response
                var reloaded = await _repository.GetByIdAsync(entity.Id);
                if (reloaded != null)
                {
                    var response = _mapper.Map<ClientAssignedAssetDto>(reloaded);
                    return Result<ClientAssignedAssetDto>.Success(response, null);
                }

                var responseFallback = _mapper.Map<ClientAssignedAssetDto>(entity);
                return Result<ClientAssignedAssetDto>.Success(responseFallback, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assigned asset");
                return Result<ClientAssignedAssetDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<IEnumerable<ClientAssignedAssetDto>>> GetAllAsync()
        {
            try
            {
                var assets = await _repository.GetAllAsync();
                var response = _mapper.Map<IEnumerable<ClientAssignedAssetDto>>(assets).ToList();

                return Result<IEnumerable<ClientAssignedAssetDto>>.Success(response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assigned assets");
                return Result<IEnumerable<ClientAssignedAssetDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<ClientAssignedAssetDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return Result<ClientAssignedAssetDto>.Failure(ErrorMessages.InvalidClientAssetId, 400);

                var asset = await _repository.GetByIdAsync(id);
                if (asset == null)
                    return Result<ClientAssignedAssetDto>.Failure(ErrorMessages.AssetNotFound, 404);

                var response = _mapper.Map<ClientAssignedAssetDto>(asset);
                return Result<ClientAssignedAssetDto>.Success(response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assigned asset with id {Id}", id);
                return Result<ClientAssignedAssetDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<bool>> UpdateAsync(int id, UpdateClientAssignedAssetDto dto, int userId)
        {
            try
            {
                if (id <= 0)
                    return Result<bool>.Failure(ErrorMessages.InvalidClientAssetId, 400);

                var asset = await _repository.GetByIdAsync(id);
                if (asset == null)
                    return Result<bool>.Failure(ErrorMessages.AssetNotFound, 404);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.ItemName))
                    return Result<bool>.Failure("itemName is required", 400);

                if (string.IsNullOrWhiteSpace(dto.SerialNumber))
                    return Result<bool>.Failure("serialNumber is required", 400);

                if (dto.AssignedDate == default)
                    return Result<bool>.Failure("assignedDate is required", 400);

                _mapper.Map(dto, asset);

                asset.UpdatedAt = DateTime.UtcNow;
                asset.UpdatedBy = userId;

                var ok = await _repository.UpdateAsync(asset);
                if (!ok)
                    return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);

                return Result<bool>.Success(true, "Asset Updated Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assigned asset with id {Id}", id);
                return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, int deletedBy)
        {
            try
            {
                if (id <= 0)
                    return Result<bool>.Failure(ErrorMessages.InvalidClientAssetId, 400);

                var asset = await _repository.GetByIdAsync(id);
                if (asset == null)
                    return Result<bool>.Failure(ErrorMessages.AssetNotFound, 404);

                asset.IsDeleted = true;
                asset.DeletedAt = DateTime.UtcNow;
                asset.DeletedBy = deletedBy;

                var ok = await _repository.UpdateAsync(asset);
                if (!ok)
                    return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);

                return Result<bool>.Success(true, "Asset Deleted Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assigned asset with id {Id}", id);
                return Result<bool>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}
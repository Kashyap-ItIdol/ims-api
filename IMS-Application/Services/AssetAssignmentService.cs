using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Domain.Entities;
using IMS_Application.Common.Models;
using IMS_Application.Common.Constants;
using AutoMapper;
using IMS_Application.Services.Interfaces;

namespace IMS_Application.Services;

public class AssetAssignmentService : IAssetAssignmentService
{
    private readonly IAssetAssignmentRepository _repository;
    private readonly IAssetRepository _assetRepository;
    private readonly IMapper _mapper;

    public AssetAssignmentService(IAssetAssignmentRepository repository, IAssetRepository assetRepository, IMapper mapper)
    {
        _repository = repository;
        _assetRepository = assetRepository;
        _mapper = mapper;
    }

    public async Task<Result<AssetAssignmentResponseDto>> AssignAssetAsync(AssetAssignmentDto dto, int createdBy)
    {
        if (dto == null)
            return Result<AssetAssignmentResponseDto>.Failure(ErrorMessages.AssetAssignmentRequired, 400);

        // Use AutoMapper to map DTO to entity
        var entity = _mapper.Map<AssetAssignment>(dto);
        entity.CreatedBy = createdBy;

        var result = await _repository.AddAsync(entity);
        var responseDto = _mapper.Map<AssetAssignmentResponseDto>(result);

        return Result<AssetAssignmentResponseDto>.Success(responseDto, SuccessMessages.AssetAssignmentCreated);
    }

    public async Task<Result<List<AssetAssignmentResponseDto>>> GetAllAsync()
    {
        var data = await _repository.GetAllAsync();
        var responseDtos = _mapper.Map<List<AssetAssignmentResponseDto>>(data);
        return Result<List<AssetAssignmentResponseDto>>.Success(responseDtos, SuccessMessages.AssetAssignmentsRetrieved);
    }

    public async Task<Result<AssetAssignmentResponseDto>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return Result<AssetAssignmentResponseDto>.Failure(ErrorMessages.AssetAssignmentNotFound, 404);
        
        var responseDto = _mapper.Map<AssetAssignmentResponseDto>(entity);
        return Result<AssetAssignmentResponseDto>.Success(responseDto, SuccessMessages.AssetAssignmentRetrieved);
    }

    public async Task<Result<AssetAssignmentResponseDto>> UpdateAssetAsync(int id, AssetAssignmentDto dto, int updatedBy)
    {
        if (dto == null)
            return Result<AssetAssignmentResponseDto>.Failure(ErrorMessages.AssetAssignmentRequired, 400);

        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return Result<AssetAssignmentResponseDto>.Failure(ErrorMessages.AssetAssignmentNotFound, 404);

        // Use AutoMapper to map DTO to existing entity
        _mapper.Map(dto, entity);
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        var responseDto = _mapper.Map<AssetAssignmentResponseDto>(entity);
        
        return Result<AssetAssignmentResponseDto>.Success(responseDto, SuccessMessages.AssetAssignmentUpdated);
    }

    public async Task<Result<AssetAssignmentResponseDto>> ReturnAssetAsync(int id, DateTime returnDate, int updatedBy)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return Result<AssetAssignmentResponseDto>.Failure(ErrorMessages.AssetAssignmentNotFound, 404);

        entity.ActualReturnDate = returnDate;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        var responseDto = _mapper.Map<AssetAssignmentResponseDto>(entity);
        
        return Result<AssetAssignmentResponseDto>.Success(responseDto, SuccessMessages.AssetAssignmentReturned);
    }

    public async Task<Result<AssetAssignmentResponseDto>> DeleteAssetAsync(int id, int updatedBy)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return Result<AssetAssignmentResponseDto>.Failure(ErrorMessages.AssetAssignmentNotFound, 404);

        // Soft delete implementation
        entity.IsDeleted = true;
        entity.DeletedBy = updatedBy;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        var responseDto = _mapper.Map<AssetAssignmentResponseDto>(entity);
        
        return Result<AssetAssignmentResponseDto>.Success(responseDto, SuccessMessages.AssetAssignmentDeleted);
    }

    public async Task<Result<AssetAssignmentResponseDto>> CreateAndAssignAssetAsync(CreateAndAssignAssetDto dto, int createdBy)
    {
        if (dto == null)
            return Result<AssetAssignmentResponseDto>.Failure(ErrorMessages.AssetAssignmentRequired, 400);

        // Step 1: Create new asset using AutoMapper
        var newAsset = _mapper.Map<Asset>(dto);
        newAsset.CreatedBy = createdBy;

        // Save the new asset
        await _assetRepository.Add(newAsset);
        await _assetRepository.Save();

        // Step 2: Create assignment for the newly created asset using AutoMapper
        var assignment = _mapper.Map<AssetAssignment>(dto);
        assignment.AssetId = newAsset.Id;
        assignment.CreatedBy = createdBy;

        var result = await _repository.AddAsync(assignment);
        var responseDto = _mapper.Map<AssetAssignmentResponseDto>(result);
        
        return Result<AssetAssignmentResponseDto>.Success(responseDto, SuccessMessages.AssetCreatedAndAssigned);
    }
}

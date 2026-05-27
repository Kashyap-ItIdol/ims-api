using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Extentions;
using IMS_Application.Interfaces;
using IMS_Application.Interfaces; 

using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;


namespace IMS_Application.Services
{
    public class ClientAssetService : IClientAssetService
    {
        private readonly IClientAssetRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientAssetService> _logger;


        public ClientAssetService(IClientAssetRepository repository, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClientAssetService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<Result<object>> Add(CreateClientAssetDto dto, int userId)
        {
            if (dto == null)
                return Result<object>.Failure(ErrorMessages.ClientAssetRequired, 400);

            try
            {                
                var entity = _mapper.Map<ClientAsset>(dto);
                entity.CreatedBy = userId;
                await _repository.AddAsync(entity);
                await _repository.SaveChangesAsync();

 
                if (dto.AssignedTo.HasValue)
                {
                    var assignment = new AssetAssignment
                    {
                        AssetId = entity.Id,
                        EmployeeId = dto.AssignedTo.Value,
                        AssignedDate = dto.AssignedDate ?? DateTime.UtcNow,
                        ExpectedReturnDate = dto.ExpectedReturnDate,
                        ActualReturnDate = dto.ActualReturnDate,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _unitOfWork.AssetAssignments.AddAsync(assignment);
                    await _unitOfWork.SaveChangesAsync();
                }


                var response = new { id = entity.Id, message = SuccessMessages.ClientAssetCreated };
                return Result<object>.Success(response, SuccessMessages.ClientAssetCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client asset");
                return Result<object>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        public async Task<Result<IEnumerable<ClientAssetResponseDto>>> GetAll()
        {
            try
            {
                var assets = await _repository.GetAllAsync();
                var responseDtos = _mapper.Map<IEnumerable<ClientAssetResponseDto>>(assets).ToList();

                await PopulateAssignmentDatesAsync(responseDtos);

                return Result<IEnumerable<ClientAssetResponseDto>>.Success(responseDtos, SuccessMessages.ClientAssetsRetrieved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all client assets");
                return Result<IEnumerable<ClientAssetResponseDto>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }


        public async Task<Result<ClientAssetResponseDto>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return Result<ClientAssetResponseDto>.Failure(ErrorMessages.InvalidClientAssetId, 400);

                var asset = await _repository.GetByIdAsync(id);
                if (asset == null)
                    return Result<ClientAssetResponseDto>.Failure(ErrorMessages.ClientAssetNotFound, 404);

                var responseDto = _mapper.Map<ClientAssetResponseDto>(asset);

                await PopulateAssignmentDatesAsync(new List<ClientAssetResponseDto> { responseDto });

                return Result<ClientAssetResponseDto>.Success(responseDto, SuccessMessages.ClientAssetRetrieved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client asset with ID: {Id}", id);
                return Result<ClientAssetResponseDto>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }


        public async Task<Result<bool>> QuickUpdate(int id, EditClientAssetQuickDto dto, int userId)
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
                asset.UpdatedBy = userId;

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

        public async Task<Result<bool>> FullUpdate(int id, EditClientAssetFullDto dto, int userId)
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
                asset.UpdatedBy = userId;

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

                var asset = await _repository.GetByIdAsync(id);
                if (asset == null)
                    return Result<bool>.Failure(ErrorMessages.ClientAssetNotFound, 404);

               
                asset.IsDeleted = true;
                asset.DeletedAt = DateTime.UtcNow;

                var result = await _repository.UpdateAsync(asset);
                
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
                await _repository.SaveChangesAsync();
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
                _logger.LogInformation("Downloading attachment with ID: {AttachmentId}", attachmentId);
                
                var attachment = await _repository.GetAttachmentByIdAsync(attachmentId);
                if (attachment == null)
                {
                    _logger.LogWarning("Attachment not found with ID: {AttachmentId}", attachmentId);
                    return Result<(byte[], string, string)>.Failure(ErrorMessages.AttachmentNotFound, 404);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var filePath = Path.Combine(uploadsFolder, attachment.FilePath);
                
                _logger.LogInformation("Attempting to read file from path: {FilePath}", filePath);
                
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("File not found at path: {FilePath}", filePath);
                    return Result<(byte[], string, string)>.Failure(ErrorMessages.FileNotFound, 404);
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = FileHelperExtentions.GetContentType(attachment.FileName);

                _logger.LogInformation("Successfully downloaded attachment with ID: {AttachmentId}", attachmentId);
                return Result<(byte[], string, string)>.Success((fileBytes, contentType, attachment.FileName), SuccessMessages.FileDownloaded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment with ID: {AttachmentId}. Error: {ErrorMessage}", attachmentId, ex.Message);
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

        public async Task<Result<ClientAssetCsvImportResultDto>> ImportCsvAsync(ClientAssetCsvImportRequestDto dto, int userId)
        {
            try
            {
                if (dto == null || dto.File == null || dto.File.Length == 0)
                    return Result<ClientAssetCsvImportResultDto>.Failure(ErrorMessages.FileNotFound, 400);

                var result = new ClientAssetCsvImportResultDto();
                var errors = new List<CsvRowErrorDto>();

                var seenSerialNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var existingSerialNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var existingAssets = await _repository.GetAllAsync();
                foreach (var a in existingAssets.Where(x => !x.IsDeleted))

                {
                    if (!string.IsNullOrWhiteSpace(a.SerialNumber))
                        existingSerialNumbers.Add(a.SerialNumber.Trim());
                }

                _logger.LogInformation("Starting CSV import. HasHeaderRow={HasHeaderRow}", dto.HasHeaderRow);

                using var stream = dto.File.OpenReadStream();
                using var reader = new StreamReader(stream);

                string? line;
                int rowNumber = 0;
                bool isFirstLine = true;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    rowNumber++;

                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        if (dto.HasHeaderRow)
                        {
                            var expectedHeader = "AssetName,Status,Category,SubCategory,Brand,Model,SerialNumber,Condition,ClientName,ClientPOC,SalesPOC";

                            var normalizedHeader = line.Replace(" ", "");

                            if (!string.Equals(normalizedHeader, expectedHeader, StringComparison.OrdinalIgnoreCase))
                            {
                                var err = $"Invalid CSV header. Expected: {expectedHeader}";
                                errors.Add(new CsvRowErrorDto { RowNumber = rowNumber, ErrorMessage = err });
                                result.FailedRows++;
                                // Treat header mismatch as fatal for beginners: stop early.
                                break;
                            }

                            _logger.LogDebug("Validated header row {RowNumber}", rowNumber);
                            continue;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        _logger.LogDebug("Skipping empty line {RowNumber}", rowNumber);
                        continue;
                    }

                    result.TotalRows++;

                    _logger.LogDebug("Parsing CSV row {RowNumber}: {Line}", rowNumber, line);

                    var parts = line.Split(',');
                    if (parts.Length < 11)
                    {
                        var err = "Invalid column count. Expected 11 columns.";

                        _logger.LogWarning("Row {RowNumber} validation error: {Error}", rowNumber, err);
                        errors.Add(new CsvRowErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = err
                        });
                        result.FailedRows++;
                        continue;
                    }

                    var assetName = parts[0].Trim();
                    var statusName = parts[1].Trim();
                    var categoryName = parts[2].Trim();
                    var subCategoryName = parts[3].Trim();
                    var brand = parts[4].Trim();
                    var model = parts[5].Trim();
                    var serialNumber = parts[6].Trim();
                    var condition = parts[7].Trim();
                    var clientName = parts[8].Trim();
                    var clientPOC = parts[9].Trim();
                    var salesPOC = parts[10].Trim();

                    var missingField = new List<string>();
                    if (string.IsNullOrWhiteSpace(assetName)) missingField.Add("AssetName");
                    if (string.IsNullOrWhiteSpace(statusName)) missingField.Add("Status");
                    if (string.IsNullOrWhiteSpace(categoryName)) missingField.Add("Category");
                    if (string.IsNullOrWhiteSpace(subCategoryName)) missingField.Add("SubCategory");
                    if (string.IsNullOrWhiteSpace(serialNumber)) missingField.Add("SerialNumber");
                    if (string.IsNullOrWhiteSpace(condition)) missingField.Add("Condition");
                    if (string.IsNullOrWhiteSpace(clientName)) missingField.Add("ClientName");
                    if (string.IsNullOrWhiteSpace(clientPOC)) missingField.Add("ClientPOC");
                    if (string.IsNullOrWhiteSpace(salesPOC)) missingField.Add("SalesPOC");
                    if (string.IsNullOrWhiteSpace(brand)) missingField.Add("Brand");
                    if (string.IsNullOrWhiteSpace(model)) missingField.Add("Model");

                    if (missingField.Count > 0)
                    {
                        errors.Add(new CsvRowErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = $"Missing/empty required fields: {string.Join(", ", missingField)}"
                        });
                        result.FailedRows++;
                        continue;
                    }

                    var category = await _unitOfWork.Categories.GetByNameAsync(categoryName);

                    if (category == null || !category.IsActive)

                    {
                        errors.Add(new CsvRowErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = $"Category not found: {categoryName}"
                        });
                        result.FailedRows++;
                        continue;
                    }

                    var subCategory = await _unitOfWork.SubCategories.GetByNameAsync(subCategoryName);

                    if (subCategory == null || !subCategory.IsActive)
                    {
                        errors.Add(new CsvRowErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = $"SubCategory not found: {subCategoryName}"
                        });
                        result.FailedRows++;
                        continue;
                    }

                    if (subCategory.CategoryId != category.Id)
                    {
                        errors.Add(new CsvRowErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = $"SubCategory '{subCategoryName}' does not belong to Category '{categoryName}'"
                        });
                        result.FailedRows++;
                        continue;
                    }

                    // Debug logging (temporary)
                    _logger.LogInformation("[CSV Import] Row {RowNumber}: incoming Status='{StatusRaw}' (normalized='{StatusNormalized}')",
                        rowNumber,
                        statusName,
                        statusName.Trim().ToLower());

                    // Lookup from real AssetStatuses table with trim + lowercase comparison
                    var statusEntity = await _unitOfWork.AssetStatuses.GetByStatusNameAsync(statusName);

                    // Debug: verify matched lookup
                    if (statusEntity != null)
                    {
                        _logger.LogInformation("[CSV Import] Row {RowNumber}: matched AssetStatus.Id={Id}, Status='{Status}'",
                            rowNumber,
                            statusEntity.Id,
                            statusEntity.Status);
                    }
                    else
                    {
                        _logger.LogWarning("[CSV Import] Row {RowNumber}: AssetStatus not found for '{StatusRaw}'", rowNumber, statusName);
                    }

                    if (statusEntity == null)
                    {
                        errors.Add(new CsvRowErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = $"Status not found: {statusName}"
                        });
                        result.FailedRows++;
                        continue;
                    }

                    if (!seenSerialNumbers.Add(serialNumber))
                    {
                        var err = $"Serial number already exists in file: {serialNumber}";

                        _logger.LogWarning("Row {RowNumber} validation error: {Error}", rowNumber, err);
                        errors.Add(new CsvRowErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = err
                        });
                        result.FailedRows++;
                        continue;
                    }
                    if (existingSerialNumbers.Contains(serialNumber))
                    {
                        var err = $"Serial number already exists: {serialNumber}";
                        _logger.LogWarning("Row {RowNumber} validation error: {Error}", rowNumber, err);
                        errors.Add(new CsvRowErrorDto
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = err
                        });
                        result.FailedRows++;
                        continue;
                    }

                    _logger.LogDebug(
                        "Row {RowNumber} validated. Creating asset. Serial={SerialNumber}",
                        rowNumber, serialNumber);

                    try
                    {
                        var asset = new ClientAsset
                        {
                            AssetName = assetName,
                            SerialNumber = serialNumber,
                            Model = model,
                            Brand = brand,
                            Condition = condition,
                            ClientName = clientName,
                            ClientPOC = clientPOC,
                            SalesPOC = salesPOC,
                            Status = statusName,

                            CategoryId = category.Id,
                            SubCategoryId = subCategory.Id,

                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                        };

                        _logger.LogDebug("Preparing asset object for row {RowNumber} (Serial={SerialNumber})", rowNumber, serialNumber);
                        _logger.LogInformation(
                            "CSV import insert attempt row {RowNumber}: AssetName={AssetName}, Serial={SerialNumber}, Brand={Brand}, Model={Model}, Condition={Condition}, ClientName={ClientName}",
                            rowNumber,
                            asset.AssetName,
                            asset.SerialNumber,
                            asset.Brand,
                            asset.Model,
                            asset.Condition,
                            asset.ClientName);

                        _logger.LogDebug("Inserting asset for row {RowNumber}, Serial={SerialNumber}", rowNumber, serialNumber);
                        await _repository.AddAsync(asset);
                        _logger.LogDebug("Insert done for row {RowNumber}, Serial={SerialNumber}", rowNumber, serialNumber);

                        result.ImportedRows++;
                        existingSerialNumbers.Add(serialNumber);
                    }
                    catch (Exception rowEx)
                    {
                        _logger.LogError(rowEx, "Error importing client asset CSV row {RowNumber}", rowNumber);
                        errors.Add(new CsvRowErrorDto { RowNumber = rowNumber, ErrorMessage = rowEx.Message });
                        result.FailedRows++;
                    }
                }

                await _repository.SaveChangesAsync();
                result.Errors = errors;

                return Result<ClientAssetCsvImportResultDto>.Success(result, "CSV import completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing client assets CSV");

                var message = ex.InnerException?.Message ?? ex.Message;
                return Result<ClientAssetCsvImportResultDto>.Failure(message, 500);
            }
        }

        public async Task<Result<(byte[] FileBytes, string ContentType, string FileName)>> ExportCsvAsync()
        {
            try
            {
                var assets = await _repository.GetAllAsync();
                var activeAssets = assets.Where(x => !x.IsDeleted).ToList();

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("AssetName,Status,Category,SubCategory,Brand,Model,SerialNumber,Condition,ClientName,ClientPOC,SalesPOC");

                foreach (var a in activeAssets)
                {
                    sb.AppendLine(string.Join(",",
                        EscapeCsv(a.AssetName),
                        EscapeCsv(a.Status),
                        EscapeCsv(a.CategoryId.ToString()),
                        EscapeCsv(a.SubCategoryId.ToString()),
                        EscapeCsv(a.Brand),
                        EscapeCsv(a.Model),
                        EscapeCsv(a.SerialNumber),
                        EscapeCsv(a.Condition),
                        EscapeCsv(a.ClientName),
                        EscapeCsv(a.ClientPOC),
                        EscapeCsv(a.SalesPOC)
                    ));
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return Result<(byte[], string, string)>.Success(
                    (bytes, "text/csv", "client-assets.csv"),
                    "CSV export completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting client assets CSV");
                return Result<(byte[], string, string)>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }

        private static string EscapeCsv(string? input)
        {
            input ??= string.Empty;
            if (input.Contains(',') || input.Contains('"') || input.Contains('\n') || input.Contains('\r'))
            {
                return "\"" + input.Replace("\"", "\"\"") + "\"";
            }
            return input;
        }

        private async Task PopulateAssignmentDatesAsync(List<ClientAssetResponseDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return;

            var assetIds = dtos.Select(d => d.Id).Distinct().ToList();
            var assignments = await _unitOfWork.AssetAssignments.GetAllAsync();

            var latestByAssetId = assignments
                .Where(a => assetIds.Contains(a.AssetId))
                .GroupBy(a => a.AssetId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(a => a.AssignedDate).First());

            foreach (var dto in dtos)
            {
                if (latestByAssetId.TryGetValue(dto.Id, out var assignment))
                {
                    dto.ExpectedReturnDate = assignment.ExpectedReturnDate;
                    dto.ActualReturnDate = assignment.ActualReturnDate;
                }
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

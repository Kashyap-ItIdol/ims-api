using AutoMapper;
using IMS_Application.Common.Constants;
using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using IMS_Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IMS_Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DepartmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<Result<IEnumerable<Department>>> GetAllDepartmentsAsync()
        {
            try
            {
                var departments = await _unitOfWork.Departments.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<Department>>(departments);

                return Result<IEnumerable<Department>>.Success(dtos,SuccessMessages.RetrievedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving departments");
                return Result<IEnumerable<Department>>.Failure(ErrorMessages.UnexpectedError, 500);
            }
        }
    }
}

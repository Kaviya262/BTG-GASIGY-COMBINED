using BackEnd.Department;
using Core.Abstractions;
using Core.Master.Department;
using Core.Master.ErrorLog;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using UserPanel.Infrastructure.Data;
using static Core.Master.Department.DepartmentItem;

namespace Infrastructure.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public DepartmentRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository transactionLogRepo)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = transactionLogRepo;
        }
        #region GetAllDepartmentAsync
        public async Task<object> GetAllDepartmentAsync(int opt, int DepartId, string DepartCode, string DepartName)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //1
                param.Add("DeptId", DepartId);
                param.Add("DeptCode", string.IsNullOrEmpty(DepartCode) ? null : DepartCode);
                param.Add("DeptName", string.IsNullOrEmpty(DepartName) ? null : DepartName);

                var List = await _connection.QueryAsync<object>(Department.DepartmentProcedure,
                    param: param, commandType: CommandType.StoredProcedure);
                var modelList = List.ToList();

                return new ResponseModel()
                {
                    Data = modelList,
                    Message = "Success",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(DepartmentRepository),
                    Method_Function = nameof(GetAllDepartmentAsync),
                    UserId = 0,
                    ScreenName = "Department",
                    RequestData_Payload = JsonConvert.SerializeObject(new { opt, DepartId, DepartCode, DepartName })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = ex.Message,
                    Status = false
                };
            }
        }
        #endregion
        #region GetDepartmentByIdAsync
        public async Task<object> GetDepartmentByIdAsync(int opt, int departmentId, string dpCode, string dpName)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //2
                param.Add("DeptId", departmentId);
                param.Add("DeptCode", dpCode);
                param.Add("DeptName", dpName);

                var data = await _connection.QueryFirstOrDefaultAsync<object>(Department.DepartmentProcedure,
                    param: param, commandType: CommandType.StoredProcedure);

                if (data == null)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Department Id not found!",
                        Status = false
                    };
                }

                return new ResponseModel()
                {
                    Data = data,
                    Message = "Department found!",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(DepartmentRepository),
                    Method_Function = nameof(GetDepartmentByIdAsync),
                    UserId = 0,
                    ScreenName = "Department",
                    RequestData_Payload = JsonConvert.SerializeObject(new { opt, departmentId, dpCode, dpName })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Something went wrong: {ex.Message}",
                    Status = false
                };
            }
        }
        #endregion
        #region GetDepartmentByCodeAsync
        public async Task<object> GetDepartmentByCodeAsync(int opt, int dpId, string departCode, string dpName)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //3
                param.Add("DeptId", dpId);
                param.Add("DeptCode", departCode);
                param.Add("DeptName", dpName);

                var data = await _connection.QueryAsync<object>(Department.DepartmentProcedure,
                    param: param, commandType: CommandType.StoredProcedure);

                if (data == null)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "User Role not found!",
                        Status = false
                    };
                }

                return new ResponseModel()
                {
                    Data = data,
                    Message = "User Role found!",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(DepartmentRepository),
                    Method_Function = nameof(GetDepartmentByCodeAsync),
                    UserId = 0,
                    ScreenName = "Department",
                    RequestData_Payload = JsonConvert.SerializeObject(new { opt, dpId, departCode, dpName })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Something went wrong: {ex.Message}",
                    Status = false
                };
            }
        }
        #endregion
        #region GetDepartmentByNameAsync
        public async Task<object> GetDepartmentByNameAsync(int opt, int dpId, string dpCode, string departName)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //4
                param.Add("DeptId", dpId);
                param.Add("DeptCode", dpCode);
                param.Add("DeptName", departName);

                var data = await _connection.QueryAsync<object>(Department.DepartmentProcedure,
                    param: param, commandType: CommandType.StoredProcedure);

                if (data == null)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Department Name not found!",
                        Status = false
                    };
                }

                return new ResponseModel()
                {
                    Data = data,
                    Message = "Department Name found!",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(DepartmentRepository),
                    Method_Function = nameof(GetDepartmentByNameAsync),
                    UserId = 0,
                    ScreenName = "Department",
                    RequestData_Payload = JsonConvert.SerializeObject(new { opt, dpId, dpCode, departName })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Something went wrong: {ex.Message}",
                    Status = false
                };
            }
        }
        #endregion
        #region CreateDepartmentAsync
        public async Task<object> CreateDepartmentAsync(DepartmentItemMain obj)
        {
            try
            {
                var query = @"INSERT INTO master_department (
                   DepartmentCode, DepartmentName, DepartmentRemark, 
                   CreatedBy, CreatedDate, CreatedIP,                                   
                   IsActive, OrgId, BranchId)     
                  VALUES (
                   @DepartmentCode, @DepartmentName, @DepartmentRemark, 
                   @UserId, Now(), '',1, @OrgId, @BranchId
                    );
                    SELECT LAST_INSERT_ID();";
                var result = await _connection.ExecuteScalarAsync<int>(query, obj.Header);

                // Log transaction
                await LogTransactionAsync(
                    id: result,
                    branchId: obj.Header.BranchId,
                    orgId: obj.Header.OrgId,
                    actionType: "Insert",
                    actionDescription: "Added new Department",
                    oldValue: null,
                    newValue: obj,
                    tableName: "MasterDepartment",
                    userId: obj.Header.UserId
                );
                if (result > 0)
                    return new ResponseModel()
                    {
                        Data = result,
                        Message = "Department created successfully",
                        Status = true
                    };
                else
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Failed to Create Department",
                        Status = false
                    };

            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(DepartmentRepository),
                    Method_Function = nameof(CreateDepartmentAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "Department",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = ex.Message,
                    Status = false
                };
            }
        }
        #endregion
        #region UpdateDepartmentAsync
        public async Task<object> UpdateDepartmentAsync(DepartmentItemMain obj)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_department where DepartmentId = {obj.Header.DepartmentId}");
                var Updatequery = @"
                    UPDATE master_department
                    SET 
                        DepartmentCode = @DepartmentCode,
                        DepartmentName = @DepartmentName,
                        DepartmentRemark = @DepartmentRemark,                      
                        ModifiedBy = @UserId,
                        ModifiedDate = Now(),  
                        ModifiedIP = '',
                        IsActive = @IsActive                                              
                        WHERE DepartmentId = @DepartmentId;";
                var rowsAffected = await _connection.ExecuteAsync(Updatequery, obj.Header);

                // Log transaction
                await LogTransactionAsync(
                    id: obj.Header.DepartmentId,
                    branchId: obj.Header.BranchId,
                    orgId: obj.Header.OrgId,
                    actionType: "Update",
                    actionDescription: "Update Department",
                    oldValue: oldvalue,
                    newValue: obj,
                    tableName: "MasterDepartment",
                    userId: obj.Header.UserId
                );

                if (rowsAffected > 0)
                {
                    return new ResponseModel()
                    {
                        Data = rowsAffected,
                        Message = "Updated Successfully!!",
                        Status = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Update Failed!",
                        Status = false
                    };
                }

            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(DepartmentRepository),
                    Method_Function = nameof(UpdateDepartmentAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "Department",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = ex.Message,
                    Status = false
                };
            }
        }
        #endregion

        private async Task LogTransactionAsync(int id, int branchId, int orgId, string actionType, string actionDescription, object oldValue, object newValue, string tableName, int? userId = 0)
        {
            var log = new UserTransactionLogModel
            {
                TransactionId = id.ToString(),
                ModuleId = 1,
                ScreenId = 1,
                ModuleName = "Master",
                ScreenName = "Department",
                UserId = userId,
                ActionType = actionType,
                ActionDescription = actionDescription,
                TableName = tableName,
                OldValue = oldValue != null ? JsonConvert.SerializeObject(oldValue) : null,
                NewValue = newValue != null ? JsonConvert.SerializeObject(newValue) : null,
                CreatedBy = userId ?? 0,
                OrgId = orgId,
                BranchId = branchId,
            };

            await _transactionLogRepo.LogTransactionAsync(log);
        }
    }
}

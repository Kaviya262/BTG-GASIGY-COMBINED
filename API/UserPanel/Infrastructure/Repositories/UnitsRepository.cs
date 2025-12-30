using BackEnd.Units;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Master.Units;
using Core.Models;
using Dapper;
using Newtonsoft.Json;
using System.Data;
using UserPanel.Infrastructure.Data;
using static Core.Master.Units.UnitsItem;

namespace Infrastructure.Repositories
{
    public class UnitsRepository : IUnitsRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;
        public UnitsRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogRepo, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogRepo;
            _transactionLogRepo = userTransactionLogRepository;
        }

        #region GetAllUnitsAsync
        public async Task<object> GetAllUnitsAsync(int opt, int unitsId, string unitsCode)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //1
                param.Add("UnitId", unitsId); //0
                param.Add("UnitCode", unitsCode);

                var result = await _connection.QueryAsync<object>(Units.UnitsProcedure,
                    param: param, commandType: CommandType.StoredProcedure);
                var modelList = result.ToList();


                return new ResponseModel()
                {
                    Data = modelList,
                    Message = "Unit Items Listed!",
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
                    Source = nameof(UnitsRepository),
                    Method_Function = nameof(GetAllUnitsAsync),
                    UserId = 0,
                    ScreenName = "UOM",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        opt, unitsId, unitsCode
                    })
                });
                return new ResponseModel()
                {
                    Data = ex,
                    Message = "No Records Found!",
                    Status = false
                };
            }
        }
        #endregion
        #region GetUnitsByIdAsync
        public async Task<object> GetUnitsByIdAsync(int opt, int unitsId, string unitsCode)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //2
                param.Add("UnitId", unitsId);
                param.Add("UnitCode", unitsCode); //null
                var result = await _connection.QueryFirstOrDefaultAsync<object>(Units.UnitsProcedure,
                    param: param, commandType: CommandType.StoredProcedure);

                if (result == null)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "No Match Found!",
                        Status = false
                    };
                }
                return new ResponseModel()
                {
                    Data = result,
                    Message = "Id Match Found!",
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
                    Source = nameof(UnitsRepository),
                    Method_Function = nameof(GetUnitsByIdAsync),
                    UserId = 0,
                    ScreenName = "UOM",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        opt, unitsId, unitsCode
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "No Records Found!",
                    Status = false
                };
            }
        }
        #endregion
        #region GetUnitsByCodeAsync
        public async Task<object> GetUnitsByCodeAsync(int opt, int unitsId, string unitsCode)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //3
                param.Add("UnitId", unitsId); //null
                param.Add("UnitCode", unitsCode);

                var result = await _connection.QueryAsync<object>(Units.UnitsProcedure,
                    param: param, commandType: CommandType.StoredProcedure);

                if (result == null)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "No Records Found!",
                        Status = false
                    };
                }

                return new ResponseModel()
                {
                    Data = result,
                    Message = "Records Found!",
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
                    Source = nameof(UnitsRepository),
                    Method_Function = nameof(GetUnitsByCodeAsync),
                    UserId = 0,
                    ScreenName = "UOM",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        opt, unitsId, unitsCode
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "No Records Found!",
                    Status = false
                };
            }
        }
        #endregion

        #region CreateUnitsAsync
        public async Task<object> CreateUnitsAsync(UnitsItemMain obj)
        {
            try
            {
                var insertquery = @"INSERT INTO master_uom(UOM,Description,
                             IsActive, CreatedBy,CreatedIP,CreatedDate,OrgId,BranchId)
                             VALUES(@UOMCode, @UOMDescription,
                              1,@UserId, '',Now(),@OrgId, @BranchId);
                             SELECT LAST_INSERT_ID();";
                var newid = await _connection.ExecuteScalarAsync<int>(insertquery, obj.Header);

                // Log transaction
                await LogTransactionAsync(
                    id: newid,
                    branchId: obj.Header.BranchId,
                    orgId: obj.Header.OrgId,
                    actionType: "Insert",
                    actionDescription: "Added new UOM",
                    oldValue: null,
                    newValue: obj,
                    tableName: "master_uom",
                    userId: obj.Header.UserId
                );
                if (newid == 0)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Give Unique UnitCode!",
                        Status = false
                    };

                }
                return new ResponseModel()
                {
                    Data = newid,
                    Message = "New Record inserted!" + newid,
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
                    Source = nameof(UnitsRepository),
                    Method_Function = nameof(CreateUnitsAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "UOM",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel()
                {
                    Data = ex,
                    Message = "Give Unique UnitCode!",
                    Status = false
                };
            }
        }
        #endregion


        #region UpdateUnitsAsync
        public async Task<object> UpdateUnitsAsync(UnitsItemMain obj)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_uom where id = {obj.Header.UOMId}");

                var updatequery = @"UPDATE master_uom
                           SET UOM = @UOMCode,
                               Description = @UOMDescription,                                       
                               IsActive = @IsActive,
                               LastModifiedBy = @UserId,
                               LastModifiedIP='',
                               LastModifiedDate = Now()
                               WHERE Id = @UOMId;";

                var rowsAffected = await _connection.ExecuteAsync(updatequery, obj.Header);

                // Log transaction
                await LogTransactionAsync(
                    id: obj.Header.UOMId,
                    branchId: obj.Header.BranchId,
                    orgId: obj.Header.OrgId,
                    actionType: "Update",
                    actionDescription: "Update UOM",
                    oldValue: oldvalue,
                    newValue: obj,
                    tableName: "master_uom",
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
                    Source = nameof(UnitsRepository),
                    Method_Function = nameof(UpdateUnitsAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "UOM",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel()
                {
                    Data = ex,
                    Message = "No Records Found!",
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
                ScreenName = "UOM",
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

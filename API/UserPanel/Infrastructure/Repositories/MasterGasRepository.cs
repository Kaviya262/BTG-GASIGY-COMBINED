using BackEnd.Master;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.Gas;
using Core.Master.Transactionlog;
using Core.Models;
using Dapper;
using Newtonsoft.Json;
using System.Data;
using UserPanel.Infrastructure.Data;

namespace Infrastructure.Repositories.Master
{
    public class MasterGasRepository : IMasterGasRepository
    {
        static class SQLQuery
        {
            public const string GetLastInsertedIdSql = "SELECT LAST_INSERT_ID();";

            public const string insertMasterGas =
                "INSERT INTO master_gascode(GasCode, GasName, Volume, Pressure, CreatedBy, CreatedDate, CreatedIP, " +
                "IsActive, OrgId, BranchId, Descriptions, GasTypeId,VolumeId,PressureId) " +
                "VALUES (@GasCode, @GasName, @Volume, @Pressure, @UserId, now(), @UserIp, " +
                "@IsActive, @OrgId, @BranchId, @Descriptions, @GasTypeId,@Volumeid,@Pressureid); ";
               

            public const string updateMasterGas =
                "UPDATE master_gascode " +
                "SET GasCode = @GasCode, " +
                "GasName = @GasName, " +
                "Volume = @Volume, " +
                "Pressure = @Pressure, " +
                "LastModifiedBy = @UserId, " +
                "LastModifiedDate = now(), " +
                "LastModifiedIP = @UserIp, " +
                "IsActive = @IsActive, " +
                "OrgId = @OrgId, " +
                "BranchId = @BranchId, " +
                "Descriptions = @Descriptions, " +
                "GasTypeId = @GasTypeId, " +
                "VolumeId = @Volumeid, " +
                "PressureId = @Pressureid " +
                "WHERE Id = @Id; ";

            public const string toogleMasterGasStatus =
                "UPDATE master_gascode " +
                "SET IsActive = @IsActive " +
                "WHERE Id = @Id;";
        }

        private DynamicParameters GetDynamicParameters(int opt, string gas_name = "", string volume = "", string pressure = "", int gas_id = 0)
        {
            var param = new DynamicParameters();
            param.Add("@opt", opt);
            param.Add("@gas_Name", gas_name);

            param.Add("@volume_Name", volume);
            param.Add("@pressure_Name", pressure);

            param.Add("@gas_id", gas_id);

            return param;
        }

        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;
        public MasterGasRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = userTransactionLogRepository;
        }

        public async Task<object> AddAsync(MasterGas item)
        {
            var response = new ResponseModel() { Status = false };
       
            try
            {
                var result = 0;

                result = await _connection.ExecuteAsync(SQLQuery.insertMasterGas, item);

                var newId = await _connection.ExecuteScalarAsync<int>(SQLQuery.GetLastInsertedIdSql);
                // Log transaction
                await LogTransactionAsync(
                    id: newId,
                    branchId: item.BranchId,
                    orgId: item.OrgId,
                    actionType: "Insert",
                    actionDescription: "Added new Gascode",
                    oldValue: null,
                    newValue: item,
                    tableName: "master_gascode",
                    userId: item.UserId
                );

                if (result == 0)
                {
                    response = new ResponseModel() { Message = "Saving MasterGas failed 0 row", Status = false };
                    return response;
                }

                response = new ResponseModel() { Message = "Saved Successfully", Status = true };
                return response;
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(MasterGasRepository),
                    Method_Function = nameof(AddAsync),
                    UserId = item.UserId,
                    ScreenName = "Gas",
                    RequestData_Payload = JsonConvert.SerializeObject(item)
                });
                return new ResponseModel()
                {
                    Message = "Something went wrong - " + ex.Message + " - " + ex.InnerException?.Message,
                    Status = false
                };
            }
        }

        public async Task<object> GetAllAsync(string name, string volume, string pressure)
        {
            try
            {
                name = string.IsNullOrWhiteSpace(name) || name == "undefined" ? "" : name;
                volume = string.IsNullOrWhiteSpace(volume) ? "" : volume;
                pressure = string.IsNullOrWhiteSpace(pressure) ? "" : pressure;

                var param = GetDynamicParameters(1, name, volume, pressure);

                var result = await _connection.QueryAsync<dynamic>(
                    MasterGasMaster.MasterGasProcedure,
                    param,
                    commandType: CommandType.StoredProcedure
                );

                return new ResponseModel
                {
                    Status = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(MasterGasRepository),
                    Method_Function = nameof(GetAllAsync),
                    UserId = 0,
                    ScreenName = "Gas",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        name,
                        volume,
                        pressure
                    })
                });

                return new ResponseModel
                {
                    Message = $"Something went wrong - {ex.Message}",
                    Status = false
                };
            }
        }


        public async Task<object> GetByID(int id)
        {
            try
            {
                var param = GetDynamicParameters(2, gas_id: id);

                var result = await _connection.QueryAsync<MasterGas>(MasterGasMaster.MasterGasProcedure, param: param, commandType: CommandType.StoredProcedure);

                return new ResponseModel()
                {
                    Data = result,
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
                    Source = nameof(MasterGasRepository),
                    Method_Function = nameof(GetByID),
                    UserId = 0,
                    ScreenName = "Gas",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        id
                    })
                });
                return new ResponseModel()
                {
                    Message = "Something went wrong - " + ex.Message + " - " + ex.InnerException?.Message,
                    Status = false
                };
            }
        }

        public async Task<object> UpdateAsync(MasterGas item)
        {
            var response = new ResponseModel() { Status = false };            
            try
            {
                var result = 0;
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_gascode where id = {item.Id}");

                result = await _connection.ExecuteAsync(SQLQuery.updateMasterGas, item);

                // Log transaction
                await LogTransactionAsync(
                    id: item.Id,
                    branchId: item.BranchId,
                    orgId: item.OrgId,
                    actionType: "Update",
                    actionDescription: "Update Gascode",
                    oldValue: oldvalue,
                    newValue: item,
                    tableName: "master_gascode",
                    userId: item.UserId
                );
                if (result == 0)
                {
                    response = new ResponseModel() { Message = "Update MasterGas failed 0 row", Status = false };
                    return response;
                }

                response = new ResponseModel() { Message = "Updated Successfully", Status = true };
                return response;
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(MasterGasRepository),
                    Method_Function = nameof(UpdateAsync),
                    UserId = item.UserId,
                    ScreenName = "Gas",
                    RequestData_Payload = JsonConvert.SerializeObject(item)
                });
                return new ResponseModel()
                {
                    Message = "Something went wrong - " + ex.Message + " - " + ex.InnerException?.Message,
                    Status = false
                };
            }
        }

        public async Task<object> ToogleStatus(MasterGas item)
        {
            try
            {
                var result = await _connection.ExecuteAsync(SQLQuery.toogleMasterGasStatus, item);
                if (result == 0)
                {
                    return new ResponseModel() { Message = "Toogle status MasterGas failed 0 row", Status = false };
                }
                return new ResponseModel() { Message = "Toogle status MasterGas success", Status = true };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(MasterGasRepository),
                    Method_Function = nameof(ToogleStatus),
                    UserId = item.UserId,
                    ScreenName = "Gas",
                    RequestData_Payload = JsonConvert.SerializeObject(item)
                });
                return new ResponseModel()
                {
                    Message = "Something went wrong - " + ex.Message + " - " + ex.InnerException?.Message,
                    Status = false
                };
            }
        }

        public async Task<object> GetAllGasTypesAsync()
        {
            try
            {
                var param = GetDynamicParameters(3);

                var result = await _connection.QueryAsync<dynamic>(
                    MasterGasMaster.MasterGasProcedure,
                    param,
                    commandType: CommandType.StoredProcedure
                );

                return new ResponseModel
                {
                    Status = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(MasterGasRepository),
                    Method_Function = nameof(GetAllGasTypesAsync),
                    UserId = 0,
                    ScreenName = "Gas",
                    RequestData_Payload = JsonConvert.SerializeObject(0)
                });

                return new ResponseModel
                {
                    Message = $"Something went wrong - {ex.Message}",
                    Status = false
                };
            }
        }


        public Task<object> GetAllAsync(string name, string volume, string pressure, string from_date, string to_date)
        {
            throw new NotImplementedException();
        }

        private async Task LogTransactionAsync(int id, int? branchId, int? orgId, string actionType, string actionDescription, object oldValue, object newValue, string tableName, int? userId = 0)
        {
            var log = new UserTransactionLogModel
            {
                TransactionId = id.ToString(),
                ModuleId = 1,
                ScreenId = 1,
                ModuleName = "Master",
                ScreenName = "Gas",
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
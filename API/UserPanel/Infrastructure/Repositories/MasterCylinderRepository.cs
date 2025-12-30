using BackEnd.Master;
using Core.Abstractions;
using Core.Master.Cylinder;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System.Data;
using UserPanel.Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class MasterCylinderRepository : IMasterCylinderRepository
    {
        static class SQLQuery
        {
            public const string GetLastInsertedIdSql = "SELECT LAST_INSERT_ID();";

            public const string insertMasterCylinder =
                "INSERT INTO master_cylinder (" +
                "barcode, cylindername, cylindernumber, GasCodeId, cylindertypeid, ownershipid, testedon, nexttestdate, remarks, " +
                "CreatedDate, CreatedIP, IsActive, CreatedBy, BranchId, OrgId, StatusId, location, hscode, IsDelivered, manufacturer, " +
                "workingpressure, palletBarcode, palletRegNumber, docNumber, fileName, fileData, testedMonth, testedYear,gasdescription,cylindersize" +
                ") VALUES (" +
                "@barcode, @cylindercode, @cylindernumber, @GasCodeId, @cylindertypeid, @ownershipid, @testedon, @nexttestdate, @remarks, " +
                "now(), @UserIp, 1, @UserId, @BranchId, @OrgId, @StatusId, @location, @hscode, @IsDelivered, @manufacturer, " +
                "@workingpressure, @palletBarcode, @palletRegNumber, @docNumber, @fileName, @fileData, @testedMonth, @testedYear,@gasdescription,@cylindersize" +
                ");";
            public const string updateMasterCylinder =
    "UPDATE master_cylinder SET " +
    "barcode = @barcode, " +
    "gasdescription = @gasdescription, " +
    "cylindername = @cylindercode, " +
    "cylindernumber = @cylindernumber, " +
    "GasCodeId = @GasCodeId, " +
    "cylindersize = @CylinderSize, " +  // ✅ added missing comma
    "cylindertypeid = @cylindertypeid, " +
    "ownershipid = @ownershipid, " +
    "testedon = @testedon, " +
    "nexttestdate = @nexttestdate, " +
    "remarks = @remarks, " +
    "LastModifiedBy = @UserId, " +
    "LastModifiedDate = Now(), " +
    "LastModifiedIP = @UserIp, " +
    "IsActive = @IsActive, " +
    "BranchId = @BranchId, " +
    "OrgId = @OrgId, " +
    "StatusId = @StatusId, " +
    "location = @location, " +
    "hscode = @hscode, " +
    "IsDelivered = @IsDelivered, " +
    "manufacturer = @manufacturer, " +
    "workingpressure = @workingpressure, " +
    "palletBarcode = @palletBarcode, " +
    "palletRegNumber = @palletRegNumber, " +
    "docNumber = @docNumber, " +
    "fileName = @fileName, " +
    "fileData = @fileData, " +
    "testedMonth = @testedMonth, " +
    "testedYear = @testedYear " +
    "WHERE cylinderid = @cylinderid;";


            public const string toogleMasterCylinderStatus =
                "UPDATE master_cylinder " +
                "SET IsActive = @IsActive " +
                "WHERE cylinderid = @Cylinderid;";
        }


        private DynamicParameters GetDynamicParameters(int opt, string cylinder_name = "", string from_date = "", string to_date = "", int cylinder_id = 0)
        {
            var param = new DynamicParameters();
            param.Add("@opt", opt);
            param.Add("@cylinder_Name", cylinder_name);
            param.Add("@from_date", from_date);
            param.Add("@to_date", to_date);
            param.Add("@cylinder_id", cylinder_id);

            return param;
        }

        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;
        public MasterCylinderRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = userTransactionLogRepository;
        }

        public async Task<object> AddAsync(MasterCylinder item)
        {
            var response = new ResponseModel() { Status = false };
            
            try
            {
                var result = 0;

                if (item.Cylinderid > 0)
                {
                    var oldvalue = await _connection.QueryAsync<object>($"select * from master_cylinder where CylinderId = {item.Cylinderid}");
                    result = await _connection.ExecuteAsync(SQLQuery.updateMasterCylinder, item);

                    if (result == 0)
                    {
                        response = new ResponseModel() { Message = "Update MasterCylinder failed, 0 row affected", Status = false };
                        return response;
                    }

                    // Log transaction
                    await LogTransactionAsync(
                        id: item.Cylinderid,
                        branchId: item.BranchId,
                        orgId: item.OrgId,
                        actionType: "Update",
                        actionDescription: "Update Cylinder",
                        oldValue: null,
                        newValue: item,
                        tableName: "MasterCylinder",
                        userId: item.UserId
                    );

                    response = new ResponseModel() { Message = "Updated Successfully", Status = true };
                    return response;
                }
                else
                {

                    result = await _connection.ExecuteAsync(SQLQuery.insertMasterCylinder, item);

                    if (result == 0)
                    {
                        response = new ResponseModel() { Message = "Saving MasterCylinder failed, 0 row affected", Status = false };
                        return response;
                    }

                    var newId = await _connection.ExecuteScalarAsync<int>(SQLQuery.GetLastInsertedIdSql);
                    // Log transaction
                    await LogTransactionAsync(
                        id: newId,
                        branchId: item.BranchId,
                        orgId: item.OrgId,
                        actionType: "Insert",
                        actionDescription: "Added new Cylinder",
                        oldValue: null,
                        newValue: item,
                        tableName: "MasterCylinder",
                        userId: item.UserId
                    );
                    response = new ResponseModel() { Message = "Saved Successfully", Status = true };
                    return response;
                }

            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(MasterCylinderRepository),
                    Method_Function = nameof(AddAsync),
                    UserId = item.UserId,
                    ScreenName = "Cylinder",
                    RequestData_Payload = JsonConvert.SerializeObject(item)
                });
                return new ResponseModel()
                {
                    Message = "Something went wrong - " + ex.Message + " - " + ex.InnerException?.Message,
                    Status = false
                };
            }
        }

        public async Task<object> GetByID(int id)
        {
            try
            {
                var param = GetDynamicParameters(2, cylinder_id: id);

                var result = await _connection.QueryAsync<MasterCylinder>(MasterCylinderMaster.MasterCylinderProcedure, param: param, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(MasterCylinderRepository),
                    Method_Function = nameof(GetByID),
                    UserId = 0,
                    ScreenName = "Cylinder",
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

        public async Task<object> GetAllAsync(string name, string from_date, string to_date)
        {
            try
            {
                var param = GetDynamicParameters(1, name, from_date, to_date);

                var result = await _connection.QueryAsync<dynamic>(
                    MasterCylinderMaster.MasterCylinderProcedure,
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
                    Source = nameof(MasterCylinderRepository),
                    Method_Function = nameof(GetAllAsync),
                    UserId = 0,
                    ScreenName = "Cylinder",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        name,
                        from_date,
                        to_date
                    })
                });

                return new ResponseModel
                {
                    Message = $"Something went wrong - {ex.Message}",
                    Status = false
                };
            }
        }


        public async Task<object> UpdateAsync(MasterCylinder item)
        {
            var response = new ResponseModel() { Status = false };
            
            try
            {
                var result = 0;
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_cylinder where CylinderId = {item.Cylinderid}");

                result = await _connection.ExecuteAsync(SQLQuery.updateMasterCylinder, item);
                if (result == 0)
                {
                    response = new ResponseModel() { Message = "Update MasterCylinder failed 0 row", Status = false };
                    return response;
                }

                // Log transaction
                await LogTransactionAsync(
                    id: item.Cylinderid,
                    branchId: item.BranchId,
                    orgId: item.OrgId,
                    actionType: "Update",
                    actionDescription: "Update Cylinder",
                    oldValue: oldvalue,
                    newValue: item,
                    tableName: "MasterCylinder",
                    userId: item.UserId
                );

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
                    Source = nameof(MasterCylinderRepository),
                    Method_Function = nameof(UpdateAsync),
                    UserId = item.UserId,
                    ScreenName = "Cylinder",
                    RequestData_Payload = JsonConvert.SerializeObject(item)
                });
                return new ResponseModel()
                {
                    Message = "Something went wrong - " + ex.Message + " - " + ex.InnerException?.Message,
                    Status = false
                };
            }
        }

        public async Task<object> ToogleStatus(MasterCylinder item)
        {
            try
            {

                var result = await _connection.ExecuteAsync(SQLQuery.toogleMasterCylinderStatus, item);

                if (result == 0)
                {
                    return new ResponseModel() { Message = "Toggle status failed, 0 row affected", Status = false };
                }

                return new ResponseModel()
                {
                    Message = $"Status updated successfully to {(item.IsActive ? "Active" : "Inactive")}",
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
                    Source = nameof(MasterCylinderRepository),
                    Method_Function = nameof(ToogleStatus),
                    UserId = item.UserId,
                    ScreenName = "Cylinder",
                    RequestData_Payload = JsonConvert.SerializeObject(item)
                });
                return new ResponseModel()
                {
                    Message = "Something went wrong - " + ex.Message + " - " + ex.InnerException?.Message,
                    Status = false
                };

            }
        }

        private async Task LogTransactionAsync(int id, int? branchId, int? orgId, string actionType, string actionDescription, object oldValue, object newValue, string tableName, int? userId = 0)
        {
            var log = new UserTransactionLogModel
            {
                TransactionId = id.ToString(),
                ModuleId = 1,
                ScreenId = 1,
                ModuleName = "Master",
                ScreenName = "Cylinder",
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

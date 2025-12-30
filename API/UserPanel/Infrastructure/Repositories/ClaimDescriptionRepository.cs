using BackEnd.Master;
using Core.Abstractions;
using Core.Master.Claim;
using Core.Master.ErrorLog;
using Core.Master.Item;
using Core.Master.Transactionlog;
using Core.Models;
using Core.Procurement.InvoiceReceipt;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ClaimDescriptionRepository : IClaimDescriptionRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public ClaimDescriptionRepository(IUnitOfWorkDB3 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = userTransactionLogRepository;
        }
        public async Task<object> GetAllCategory(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 1);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("typeid", 0);
                parameters.Add("claimtypeid", 0);

                var result = await _connection.QueryAsync<dynamic>(ClaimDescriptionMaster.MasterClaimDescriptionProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ClaimDescriptionRepository),
                    Method_Function = nameof(GetAllCategory),
                    UserId = 0,
                    ScreenName = "ClaimDescription",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        public async Task<object> GetAllCategorytypes(int branchid, int orgid, int typeid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 2);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("typeid", typeid);
                parameters.Add("claimtypeid", 0);

                var result = await _connection.QueryAsync<dynamic>(ClaimDescriptionMaster.MasterClaimDescriptionProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ClaimDescriptionRepository),
                    Method_Function = nameof(GetAllCategorytypes),
                    UserId = 0,
                    ScreenName = "ClaimDescription",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid, typeid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        public async Task<object> AddAsync(ClaimDescriptionPayment Obj)
        {
            try
            {
                string Message = "";
                const string headerSql = @"
                      INSERT INTO master_claim_payment_desc
                        (PaymentCode,PaymentDescription,claimtypeid,CreatedBy,CreatedDate,CreatedIP,LastModifiedBy,LastModifiedDate,LastModifiedIP,IsActive,OrgId,BranchId)
                        VALUES
                        (@paymentCode,@paymentdescription,@claimtypeid,@createdby,NOW(),@CreatedIP,0,NOW(),'',1,@OrgId,@BranchId);
SELECT LAST_INSERT_ID();
                ";

                var result = await _connection.ExecuteScalarAsync<int>(headerSql, Obj.payment);

                // Log transaction
                await LogTransactionAsync(
                    id: result,
                    branchId: Obj.payment.BranchId,
                    orgId: Obj.payment.OrgId,
                    actionType: "Insert",
                    actionDescription: "Added new Claim & Payment Description",
                    oldValue: null,
                    newValue: Obj,
                    tableName: "master_claim_payment_desc",
                    userId: Obj.payment.createdby
                );
                return new ResponseModel()
                {
                    Message = "Saved Successfully" + Message,
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimDescriptionRepository),
                    Method_Function = nameof(AddAsync),
                    UserId = 0,
                    ScreenName = "ClaimDescription",
                    RequestData_Payload = JsonSerializer.Serialize(Obj)
                });
                return new ResponseModel()
                {
                    Message = $"Error: {ex.Message}",
                    Status = false
                };
            }
        }
        public async Task<object> UpdateAsync(ClaimDescriptionPayment Obj)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_claim_payment_desc where Id = {Obj.payment.paymentid}");
                const string headerSql = @"
                        UPDATE master_claim_payment_desc
                        SET
                        PaymentCode = @paymentCode,
                        PaymentDescription = @paymentdescription,
                        claimtypeid = @claimtypeid,
                        LastModifiedBy = @createdby,
                        LastModifiedDate = NOW(),
                        LastModifiedIP = '',
                        IsActive = 1,
                        OrgId = @OrgId,
                        BranchId = @BranchId
                        WHERE Id = @paymentid";

                await _connection.ExecuteAsync(headerSql, Obj.payment);

                // Log transaction
                await LogTransactionAsync(
                    id: Obj.payment.paymentid,
                    branchId: Obj.payment.BranchId,
                    orgId: Obj.payment.OrgId,
                    actionType: "Update",
                    actionDescription: "Update Claim & Payment Description",
                    oldValue: oldvalue,
                    newValue: Obj,
                    tableName: "master_claim_payment_desc",
                    userId: Obj.payment.createdby
                );

                return new ResponseModel
                {
                    Data = null,
                    Message = "Claim Description master updated successfully",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimDescriptionRepository),
                    Method_Function = nameof(UpdateAsync),
                    UserId = 0,
                    ScreenName = "ClaimDescription",
                    RequestData_Payload = JsonSerializer.Serialize(Obj)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Something went wrong: {ex.Message}",
                    Status = false
                };
            }
        }
        public async Task<object> GetClaimDescriptionByIdAsync(int opt, int typeid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //2
                param.Add("branchid", 1);
                param.Add("orgid", 1);
                param.Add("typeid", typeid);
                param.Add("claimtypeid", 0);

                var result = await _connection.QueryAsync<dynamic>(ClaimDescriptionMaster.MasterClaimDescriptionProcedure, param, commandType: CommandType.StoredProcedure);

                if (result == null)
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
                    Data = result,
                    Message = "Department found!",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimDescriptionRepository),
                    Method_Function = nameof(GetClaimDescriptionByIdAsync),
                    UserId = 0,
                    ScreenName = "ClaimDescription",
                    RequestData_Payload = JsonSerializer.Serialize(new { opt, typeid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Something went wrong: {ex.Message}",
                    Status = false
                };
            }
        }
        public async Task<object> GetPaymentDescriptionList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 4);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("typeid", 0);
                parameters.Add("claimtypeid", 0);

                var result = await _connection.QueryAsync<dynamic>(ClaimDescriptionMaster.MasterClaimDescriptionProcedure, parameters, commandType: CommandType.StoredProcedure);

                return new ResponseModel
                {
                    Status = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimDescriptionRepository),
                    Method_Function = nameof(GetPaymentDescriptionList),
                    UserId = 0,
                    ScreenName = "ClaimDescription",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        public async Task<object> DescriptionstatusChange(int paymentid)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_claim_payment_desc where id = {paymentid}");

                const string updateDetailsSql = @"UPDATE master_claim_payment_desc SET isActive = CASE WHEN isActive = 1 THEN 0 ELSE 1 END WHERE id = @paymentid;";
                await _connection.ExecuteAsync(updateDetailsSql, new { paymentid });

                var newValue = await _connection.QueryFirstOrDefaultAsync<object>("SELECT * FROM master_claim_payment_desc WHERE id = @paymentid", new { paymentid });

                // Log transaction
                await LogTransactionAsync(
                    id: paymentid,
                    branchId: 0,
                    orgId: 0,
                    actionType: "Update",
                    actionDescription: "Update Claim & Payment Description",
                    oldValue: oldvalue,
                    newValue: newValue,
                    tableName: "master_claim_payment_desc",
                    userId: 0
                );

                return new ResponseModel() { Data = null, Message = "Claim Payment Updated successfully", Status = true };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimDescriptionRepository),
                    Method_Function = nameof(DescriptionstatusChange),
                    UserId = 0,
                    ScreenName = "ClaimDescription",
                    RequestData_Payload = JsonSerializer.Serialize(new { paymentid })
                });
                return new ResponseModel() { Data = null, Message = $"Something went wrong: {ex.Message}", Status = false };
            }
        }
        public async Task<object> searchbyCategory(int branchid, int orgid, int categoryid, int claimtypeid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 5);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("typeid", categoryid);
                parameters.Add("claimtypeid", claimtypeid);

                var result = await _connection.QueryAsync<dynamic>(ClaimDescriptionMaster.MasterClaimDescriptionProcedure, parameters, commandType: CommandType.StoredProcedure);

                return new ResponseModel
                {
                    Status = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimDescriptionRepository),
                    Method_Function = nameof(searchbyCategory),
                    UserId = 0,
                    ScreenName = "ClaimDescription",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid, categoryid, claimtypeid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private async Task LogTransactionAsync(int id, int branchId, int orgId, string actionType, string actionDescription, object oldValue, object newValue, string tableName, int? userId = 0)
        {
            var log = new UserTransactionLogModel
            {
                TransactionId = id.ToString(),
                ModuleId = 1,
                ScreenId = 1,
                ModuleName = "Master",
                ScreenName = "Claim & Payment Description",
                UserId = userId,
                ActionType = actionType,
                ActionDescription = actionDescription,
                TableName = tableName,
                OldValue = oldValue != null ? Newtonsoft.Json.JsonConvert.SerializeObject(oldValue) : null,
                NewValue = newValue != null ? Newtonsoft.Json.JsonConvert.SerializeObject(newValue) : null,
                CreatedBy = userId ?? 0,
                OrgId = orgId,
                BranchId = branchId,
            };

            await _transactionLogRepo.LogTransactionAsync(log);
        }
    }
}

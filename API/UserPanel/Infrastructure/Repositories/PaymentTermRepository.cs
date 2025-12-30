using BackEnd.PaymentTerms;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.PaymentTerms;
using Core.Master.Transactionlog;
using Core.Models;
using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPanel.Infrastructure.Data;
using static Core.Master.PaymentTerms.PaymentTermItem;

namespace Infrastructure.Repositories
{
    public class PaymentTermRepository : IPaymentTermRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;
        public PaymentTermRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection ;
            _errorLogRepo = errorLogMasterRepository ;
            _transactionLogRepo = userTransactionLogRepository ;
        }
        #region GetAllPaymentTermAsync
        public async Task<object> GetAllPaymentTermAsync(int opt, int payTermId, string payTermCode)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //1
                param.Add("PayTmId", payTermId); //null
                param.Add("PayTmCode", payTermCode);

                var result = await _connection.QueryAsync<object>(PaymentTerms.PaymentTermProcedure,
                    param: param, commandType: CommandType.StoredProcedure);
                var modelList = result.ToList();


                return new ResponseModel()
                {
                    Data = modelList,
                    Message = "Payment Terms Listed!",
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
                    Source = nameof(PaymentTermRepository),
                    Method_Function = nameof(GetAllPaymentTermAsync),
                    UserId = 0,
                    ScreenName = "PaymentTerm",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                       opt, payTermId, payTermCode
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "No Records Found!" + ex.Message,
                    Status = false
                };
            }
        }
        #endregion

        #region GetPaymentTermByIdAsync
        public async Task<object> GetPaymentTermByIdAsync(int opt, int payTermId, string payTermCode)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //2
                param.Add("PayTmId", payTermId);
                param.Add("PayTmCode", payTermCode); //null
                var result = await _connection.QueryFirstOrDefaultAsync<object>(PaymentTerms.PaymentTermProcedure,
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
                    Source = nameof(PaymentTermRepository),
                    Method_Function = nameof(GetPaymentTermByIdAsync),
                    UserId = 0,
                    ScreenName = "PaymentTerm",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        opt, payTermId, payTermCode
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "No Records Found!" + ex.Message,
                    Status = false
                };
            }
        }
        #endregion

        #region GetPaymentTermByCodeAsync
        public async Task<object> GetPaymentTermByCodeAsync(int opt, int payTermId, string payTermCode)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //3
                param.Add("PayTmId", payTermId); //null
                param.Add("PayTmCode", payTermCode);

                var result = await _connection.QueryAsync<object>(PaymentTerms.PaymentTermProcedure,
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
                    Source = nameof(PaymentTermRepository),
                    Method_Function = nameof(GetPaymentTermByCodeAsync),
                    UserId = 0,
                    ScreenName = "PaymentTerm",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        opt, payTermId, payTermCode
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "No Records Found!" + ex.Message,
                    Status = false
                };
            }
        }
        #endregion

        #region CreatePaymentTermAsync
        public async Task<object> CreatePaymentTermAsync(PaymentTermMain obj)
        {
            try
            {
                var insertquery = @"INSERT INTO master_terms(TermName,Description,DueDays,
                                    IsActive, CreatedBy,CreatedIP,CreatedDate)
                                    VALUES(@PaymentTermCode, @PaymentTermDesc, @DueDays,
                                     1,@UserId, '',Now());
                                    SELECT LAST_INSERT_ID();";
                var newid = await _connection.ExecuteScalarAsync<int>(insertquery, obj.Header);

                // Log transaction
                await LogTransactionAsync(
                    id: newid,
                    branchId: 1,
                    orgId: 1,
                    actionType: "Insert",
                    actionDescription: "Added new Payment Terms",
                    oldValue: null,
                    newValue: obj,
                    tableName: "master_terms",
                    userId: obj.Header.UserId
                );
                if (newid == 0)
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
                    Data = newid,
                    //Message = "New Record Id inserted : " + newid,
                    Message = "Payment Terms Inserted Successfully",
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
                    Source = nameof(PaymentTermRepository),
                    Method_Function = nameof(CreatePaymentTermAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "PaymentTerm",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "No Records Found!" + ex.Message,
                    Status = false
                };
            }
        }
        #endregion

        #region UpdatePaymentTermAsync
        #region UpdatePaymentTermAsync
        public async Task<object> UpdatePaymentTermAsync(PaymentTermMain obj)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_terms where id = {obj.Header.PaymentTermId}");

                var updatequery = @"UPDATE master_terms
                           SET TermName = @PaymentTermCode,
                               Description = @PaymentTermDesc,
                               DueDays = @DueDays,
                               IsActive = @IsActive,
                               LastModifiedBy = @UserId,
                               LastModifiedIP = '',
                               LastModifiedDate = CURRENT_TIMESTAMP
                           WHERE Id = @PaymentTermId";

                var rowsAffected = await _connection.ExecuteAsync(updatequery, obj.Header);

                // Log transaction
                await LogTransactionAsync(
                    id: obj.Header.PaymentTermId,
                    branchId: 1,
                    orgId: 1,
                    actionType: "Update",
                    actionDescription: "Update Payment Terms",
                    oldValue: oldvalue,
                    newValue: obj,
                    tableName: "master_terms",
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
                    Source = nameof(PaymentTermRepository),
                    Method_Function = nameof(UpdatePaymentTermAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "PaymentTerm",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Update failed: {ex.Message}",
                    Status = false
                };
            }
        }

        public Task<object> UpdateStatus(PaymentTermMain command)
        {
            throw new NotImplementedException();
        }
        #endregion

        #endregion

        private async Task LogTransactionAsync(int id, int branchId, int orgId, string actionType, string actionDescription, object oldValue, object newValue, string tableName, int? userId = 0)
        {
            var log = new UserTransactionLogModel
            {
                TransactionId = id.ToString(),
                ModuleId = 1,
                ScreenId = 1,
                ModuleName = "Master",
                ScreenName = "Payment Terms",
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


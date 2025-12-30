using BackEnd.Finance.ClaimAndPayment;
using BackEnd.Procurement.PurchaseMemo;
using BackEnd.Procurement.PurchaseRequitision;
using Core.Abstractions;
using Core.Finance.Approval;
using Core.Master.ErrorLog;
using Core.Master.Supplier;
using Core.Master.Transactionlog;
using Core.Models;
using Core.OrderMng.Invoices;
using Core.Procurement.Approval;
using Core.Procurement.PurchaseMemo;
using Core.Procurement.PurchaseRequisition;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml.Office;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPanel.Infrastructure.Data;

namespace Infrastructure.Repositories
{
   public class RequisitionApprovalRepository : IRequisitionApprovalRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public RequisitionApprovalRepository(IUnitOfWorkDB2 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = userTransactionLogRepository;
        }
             

        public async Task<object> GetAllAsync(int Id, int branchId, Int32 orgid, int userid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 1);
                param.Add("@userid", userid);
                param.Add("@branchid", branchId);
                param.Add("@orgid", orgid);
                param.Add("@id", 0);
                param.Add("@fromdate", "");
                param.Add("@todate", "");
                param.Add("@prid", 0);

                var list = await _connection.QueryAsync(PurchaseRequitisionBackEnd.PurchaseRequitionApproval, param, commandType: CommandType.StoredProcedure);

                return new ResponseModel
                {
                    Data = list,
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
                    Source = nameof(RequisitionApprovalRepository),
                    Method_Function = nameof(GetAllAsync),
                    UserId = 0,
                    ScreenName = "Requisition Approval",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                       Id, branchId, orgid, userid
                    })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Error retrieving Requisitions.",
                    Status = false
                };
            }
        }


        public async Task<object> ApproveAsync(RequisitionApprovalHdr obj)
        {
            try
            {
                var oldDetails = new List<object>();

                foreach (var d in obj.approve)
                {
                    if (d.prid > 0)
                    {
                        var oldvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_PurchaseRequisition_Header where PRId = @PRId", new { PRId = d.prid });

                        oldDetails.Add(oldvalue);
                    }
                    else
                    {
                        oldDetails.Add(null);
                    }
                }
                string updatedetails = @"
                    UPDATE tbl_PurchaseRequisition_Header
                    SET 
                         ModifiedBY=@userid ,
                          pr_discussed_count = CASE 
                                WHEN (@isdiscussedone = 1 AND IFNULL(pr_gm_isdiscussed, 0) = 0)
                                  OR (@isdiscussedtwo = 1 AND IFNULL(pr_director_isdiscussed, 0) = 0)
                                THEN IFNULL(pr_discussed_count, 0) + 1 
                                ELSE IFNULL(pr_discussed_count, 0)
                            END,

                            pr_gm_isapproved = case when ifnull(pr_gm_isapproved,0)=1 and  @isdiscussedtwo=1 then 0 else @isapprovedone end ,                
                        pr_gm_isdiscussed = @isdiscussedone,
                        pr_director_isapproved = @isapprovedtwo,
                        pr_director_isdiscussed =@isdiscussedtwo,                
                        pr_comment = @remarks
                    WHERE PRId = @prid ;";

                foreach (var item in obj.approve)
                {
                    item.userid = obj.UserId;
                    await _connection.ExecuteAsync(updatedetails, item);
                }

                string updatedetailsstatus = @"
                    UPDATE tbl_PurchaseRequisition_Header
                    SET 
                          
    IsSubmitted = case when @isdiscussedone = 1
              
        THEN 0
        ELSE IsSubmitted 
          END,

                pr_director_isdiscussed = case when ifnull(pr_gm_isapproved,0)=1   then 0 else pr_director_isdiscussed end  
               
                    WHERE PRId = @prid ;";

                foreach (var item in obj.approve)
                {
                    item.userid = obj.UserId;
                    await _connection.ExecuteAsync(updatedetailsstatus, item);
                }

                foreach (var item in obj.approve)
                {
                    await LogTransactionAsync(
            id: item.prid,
            branchId: 1,
            orgId: 1,
            actionType: "Update",
            actionDescription: "Updated Purchase Requisition Approve",
            oldValue: oldDetails,
            newValue: obj,
            tableName: "tbl_PurchaseRequisition_Header",
            userId: 1
        );
                }

                return new ResponseModel
                {
                    Data = 1,
                    Message = "Updated successfully.",
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
                    Source = nameof(RequisitionApprovalRepository),
                    Method_Function = nameof(ApproveAsync),
                    UserId = obj.UserId,
                    ScreenName = "Requisition Approval",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Something went wrong while updating: " + ex.Message,
                    Status = false
                };
            }
        }


        public async Task<object> GetRemarksList(int prid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 4);
                param.Add("@userid", 0);
                param.Add("@branchid", 1);
                param.Add("@orgid", 1);
                param.Add("@id", 0);
                param.Add("@fromdate", "");
                param.Add("@todate", "");
                param.Add("@prid", prid);
                var list = await _connection.QueryAsync(PurchaseRequitisionBackEnd.PurchaseRequitionApproval, param, commandType: CommandType.StoredProcedure);

                return new ResponseModel
                {
                    Data = list,
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
                    Source = nameof(RequisitionApprovalRepository),
                    Method_Function = nameof(GetRemarksList),
                    UserId = 0,
                    ScreenName = "Requisition Approval",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        prid
                    })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Error retrieving Requisition.",
                    Status = false
                };
            }
        }


        public async Task<object> GetHistoryAsync(int id, int userid, int branchId, Int32 orgid, string fromdate, string todate)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@opt", 2);
                param.Add("@userid", userid);
                param.Add("@branchid", branchId);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@fromdate", fromdate);
                param.Add("@todate", todate);             
                param.Add("@prid", 0);

                var list = await _connection.QueryAsync(PurchaseRequitisionBackEnd.PurchaseRequitionApproval, param, commandType: CommandType.StoredProcedure);

                return new ResponseModel
                {
                    Data = list,
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
                    Source = nameof(RequisitionApprovalRepository),
                    Method_Function = nameof(GetHistoryAsync),
                    UserId = 0,
                    ScreenName = "Requisition Approval",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        id, userid, branchId, orgid, fromdate, todate
                    })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Error retrieving Purchase requisition by ID.",
                    Status = false
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
                ModuleName = "Procurement",
                ScreenName = "Purchase Requisition Approval",
                UserId = userId,
                ActionType = actionType,
                ActionDescription = actionDescription,
                TableName = tableName,
                OldValue = oldValue != null ? Newtonsoft.Json.JsonConvert.SerializeObject(oldValue) : null,
                NewValue = newValue != null ? Newtonsoft.Json.JsonConvert.SerializeObject(newValue) : null,
                CreatedBy = userId ?? 0,
                OrgId = orgId,
                BranchId = branchId,
                DbLog = 3
            };

            await _transactionLogRepo.LogTransactionAsync(log);
        }
    }
}

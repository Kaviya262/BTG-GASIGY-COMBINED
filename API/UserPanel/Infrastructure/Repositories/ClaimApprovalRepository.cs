using BackEnd.Finance.ClaimAndPayment;
using Core.Abstractions;
using Core.Finance.Approval;
using Core.Finance.ClaimAndPayment;
using Core.Finance.PaymentPlan;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Dapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
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
    public class ClaimApprovalRepository : IClaimApprovalRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public ClaimApprovalRepository(IUnitOfWorkDB3 financedb, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository transactionLogRepo)
        {
            _connection = financedb.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = transactionLogRepo;
        }

        //public async Task<object> ApproveAsync(ClaimApprovalHdr obj)
        //{
        //    try
        //    {
        //        const string updatedetails = @"
        //        UPDATE tbl_claimAndpayment_header
        //        SET 
        //        claim_gm_isapproved = @isapprovedone,
        //        claim_gm_isdiscussed = @isdiscussedone,
        //        claim_director_isapproved = @isapprovedtwo,
        //        claim_director_isdiscussed = @isdiscussedtwo,
        //        ppp_gm_approvalone = @ppp_gm_approvalone,
        //        ppp_director_approvalone = @ppp_director_approvalone,
        //        ppp_gm_discussed = @ppp_gm_discussed,

        //        ppp_commissioner_approvalone = @ppp_commissioner_approvalone,
        //        ppp_commissioner_approvalone = @ppp_commissioner_approvalone,


        //        GmComment =@GmComment,
        //        ppp_director_discussed = @ppp_director_discussed,
        //        ppp_pv_Commissioner_discussedone = @ppp_pv_Commissioner_discussedone,
        //        PPP_PV_Commissioner_approveone = @PPP_PV_Commissioner_approveone,
        //        claim_comment = @remarks
        //        WHERE Claim_ID = @claimid and ifnull(ppp_IsRejected,0)=0";
        //        foreach (var item in obj.approve)
        //        {
        //            await _connection.ExecuteAsync(updatedetails, item);
        //        }

        //        return new ResponseModel
        //        {
        //            Data = 1,
        //            Message = "Updated successfully.",
        //            Status = true
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel
        //        {
        //            Data = null,
        //            Message = "Something went wrong while updating: " + ex.Message,
        //            Status = false
        //        };
        //    }
        //}

        public async Task<object> ApproveAsync(ClaimApprovalHdr obj)
        {
            var oldDetails = new List<object>();

            foreach (var d in obj.approve)
            {
                if (d.claimid > 0)
                {
                    var oldvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_claimAndpayment_header where Claim_Id = @ClaimId", new { ClaimId = d.claimid });

                    oldDetails.Add(oldvalue);
                }
                else
                {
                    oldDetails.Add(null);
                }
            }
            try
            {
                if (obj.isppp_pv == 0)
                {

                    string updatedetails = @"
            UPDATE tbl_claimAndpayment_header
            SET 
                 LastModifiedBY=@userid ,  

  Claim_Discussed_Count = CASE 
        WHEN (@isdiscussedeight = 1 AND IFNULL(claim_hod_isdiscussed, 0) = 0) or (@isdiscussedone = 1 AND IFNULL(claim_gm_isdiscussed, 0) = 0)
          OR (@isdiscussedtwo = 1 AND IFNULL(claim_director_isdiscussed, 0) = 0)
        THEN IFNULL(Claim_Discussed_Count, 0) + 1 
        ELSE IFNULL(Claim_Discussed_Count, 0)
    END,

 

claim_hod_isapproved = case when ifnull(claim_hod_isapproved,0)=1 and  @isdiscussedone=1 then 0 else @isapprovedeight end ,
claim_hod_isdiscussed= @isdiscussedeight,
               
                
                claim_gm_isapproved = case when ifnull(claim_gm_isapproved,0)=1 and  @isdiscussedtwo=1 then 0 else @isapprovedone end ,

                claim_gm_isdiscussed = @isdiscussedone,

                claim_director_isapproved = @isapprovedtwo,

                claim_director_isdiscussed =@isdiscussedtwo,




PPP_Discussed_Count = CASE 
        WHEN (@ppp_gm_discussed = 1 AND IFNULL(ppp_gm_discussed, 0) = 0)
          OR (@ppp_director_discussed = 1 AND IFNULL(ppp_director_discussed, 0) = 0)
          OR (@ppp_commissioner_discussed = 1 AND IFNULL(ppp_commissioner_discussedone, 0) = 0   )
        THEN IFNULL(PPP_Discussed_Count, 0) + 1 
        ELSE IFNULL(PPP_Discussed_Count, 0)
    END,


  

 

                ppp_gm_approvalone = @ppp_gm_approvalone,

                ppp_gm_discussed = @ppp_gm_discussed,
               
                ppp_director_approvalone =@ppp_director_approvalone,
             ppp_commissioner_discussedone = CASE 
        WHEN IFNULL(voucherid,0) > 0 THEN @ppp_commissioner_discussed
        ELSE @ppp_commissioner_discussed
    END,
                

                ppp_director_discussed =@ppp_director_discussed,

                ppp_commissioner_approvalone = @ppp_commissioner_approvalone,
                
                claim_comment = @remarks,
                GmComment = @GmComment, PPP_temp_GM_status=0,PPP_temp_Director_status=0,PPP_temp_CEO_status=0
            WHERE Claim_ID = @claimid AND IFNULL(ppp_IsRejected, 0) = 0;";

                    foreach (var item in obj.approve)
                    {
                        item.userid = obj.UserId;
                        await _connection.ExecuteAsync(updatedetails, item);
                    }


                    const string statusupdatedetails = @"
            UPDATE tbl_claimAndpayment_header
            SET 
                


 IsPaymentgenerated = CASE
        WHEN (
            IFNULL(PPP_Discussed_Count, 0)
            
        ) >2 
        THEN 0
        ELSE IsPaymentgenerated
    END ,

        IsSubmitted = CASE
        WHEN ((IFNULL(Claim_Discussed_Count, 0) 
              ) > 2 or @isdiscussedeight = 1 or (ifnull(is_Hod_created,0)=0 and ifnull(@isdiscussedone,0)=1)
              )
        THEN 0
        ELSE IsSubmitted 
          END,

   
 
                claim_gm_isdiscussed = case when ifnull(claim_hod_isapproved,0)=1   then 0 else claim_gm_isdiscussed end  ,

                claim_director_isdiscussed = case when ifnull(claim_gm_isapproved,0)=1   then 0 else claim_director_isdiscussed end  
               
            WHERE Claim_ID = @claimid AND IFNULL(ppp_IsRejected, 0) = 0;";


                    foreach (var item in obj.approve)
                    {
                        await _connection.ExecuteAsync(statusupdatedetails, item);
                    }


                }
                else
                {

                    string updateSql = string.Empty;

                    if (obj.type == 1)
                    {
                        if (obj.operation == 1)
                        {
                            updateSql = @"
                UPDATE tbl_claimAndpayment_header
                SET PPP_PV_Director_approve = 1
                WHERE SummaryId = @SummaryId
                  AND IFNULL(PPP_PV_Director_approve, 0) = 0 and ifnull(ppp_IsRejected,0)=0 and ifnull(ppp_pv_IsRejected,0)=0;";
                        }
                        else if (obj.operation == 2)
                        {
                            updateSql = @"
                UPDATE tbl_claimAndpayment_header
                SET ppp_pv_Director_discussed = 1  ,claim_comment= @remarks
                WHERE SummaryId = @SummaryId
                  AND IFNULL(PPP_PV_Director_approve, 0) = 0 and ifnull(ppp_IsRejected,0)=0 and ifnull(ppp_pv_IsRejected,0)=0;";
                        }
                    }
                    else if (obj.type == 2)
                    {
                        if (obj.operation == 1)
                        {
                            updateSql = @"
                UPDATE tbl_claimAndpayment_header
                SET PPP_PV_Commissioner_approveone = 1,ppp_commissioner_approvalone=1 
                WHERE SummaryId = @SummaryId
                  AND IFNULL(PPP_PV_Commissioner_approveone, 0) = 0 and ifnull(ppp_IsRejected,0)=0 and ifnull(ppp_pv_IsRejected,0)=0;";
                        }
                        else if (obj.operation == 2)
                        {
                            updateSql = @"
                UPDATE tbl_claimAndpayment_header
                SET ppp_pv_Commissioner_discussedone = 1 ,claim_comment= @remarks
                WHERE SummaryId = @SummaryId
                  AND IFNULL(PPP_PV_Commissioner_approveone, 0) = 0 and ifnull(ppp_IsRejected,0)=0 and ifnull(ppp_pv_IsRejected,0)=0;";
                        }

                        }
                    else
                    {
                        return new ResponseModel
                        {
                            Data = null,
                            Message = "Invalid type value.",
                            Status = false
                        };
                    }

                    int affectedRows = await _connection.ExecuteAsync(updateSql, new { SummaryId = obj.summaryid, remarks=obj.remarks });


                

                }

                string updatedetailsdis = "";
                //updatedetailsdis += @"update tbl_claimAndpayment_header set  isdiscussionaccepted=" + 1 + "  where  ifnull(claim_gm_isdiscussed,0)=1;";
                //updatedetailsdis += @"update tbl_claimAndpayment_header set  isdiscussionaccepted=" + 1 + "  where   ifnull(claim_director_isdiscussed,0)=1;";
                updatedetailsdis += @"update tbl_claimAndpayment_header set  isdiscussionaccepted=" + 1 + "  where   ifnull(ppp_gm_discussed,0)=1;";
                updatedetailsdis += @"update tbl_claimAndpayment_header set  isdiscussionaccepted=" + 1 + "  where   ifnull(ppp_director_discussed,0)=1;";
                updatedetailsdis += @"update tbl_claimAndpayment_header set  isdiscussionaccepted=" + 1 + "  where   ifnull(ppp_commissioner_discussedone,0)=1;";

                updatedetailsdis += @"update tbl_claimAndpayment_header set  isdiscussionaccepted=" + 1 + "  where    ifnull(ppp_pv_Commissioner_discussedone,0)=1;";
                updatedetailsdis += @"update tbl_claimAndpayment_header set  isdiscussionaccepted=" + 1 + "  where ifnull(ppp_pv_Director_discussed,0)=1;";
                await _connection.ExecuteAsync(updatedetailsdis);

                foreach (var item in obj.approve)
                {

                    await LogTransactionAsync(
            id: item.claimid,
            branchId: 1,
            orgId: 1,
            actionType: "Update",
            actionDescription: "Updated Claim Approvels",
            oldValue: oldDetails,
            newValue: obj,
            tableName: "tbl_claimAndpayment_header",
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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(ApproveAsync),
                    UserId = 0,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(obj)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Something went wrong while updating: {ex.Message}",
                    Status = false
                };
            }
        }

 public async Task<object> PPPApproveAsync(PPPApproval obj)
        {
            try
            {  
                var oldvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_claimAndpayment_header where Claim_Id = @ClaimId", new { ClaimId = obj.claimid });
                   
                string updatedetailsdis = "";

                if (obj.level == 1)
                {
                    if (obj.isapproved == true)
                    {
                        updatedetailsdis += @"update tbl_claimAndpayment_header set  PPP_temp_GM_status=1  where   ifnull(Claim_ID,0)=@claimid and ifnull(ppp_gm_approvalone,0)=0;";
                    }
                    else
                    {
                        updatedetailsdis += @"update tbl_claimAndpayment_header set  PPP_temp_GM_status=2, claim_comment=@GmComment  where   ifnull(Claim_ID,0)=@claimid  and ifnull(ppp_gm_discussed,0)=0;";
                    }
                }
                else if (obj.level == 2)
                {
                    if (obj.isapproved == true)
                    {
                        updatedetailsdis += @"update tbl_claimAndpayment_header set  PPP_temp_Director_status=1  where   ifnull(Claim_ID,0)=@claimid  and ifnull(ppp_director_approvalone,0)=0;";
                    }
                    else
                    {
                        updatedetailsdis += @"update tbl_claimAndpayment_header set  PPP_temp_Director_status=2, claim_comment=@GmComment  where   ifnull(Claim_ID,0)=@claimid  and ifnull(ppp_director_discussed,0)=0;";
                    }   
                }
                else if (obj.level == 3)
                {
                    if (obj.isapproved == true)
                    {
                        updatedetailsdis += @"update tbl_claimAndpayment_header set  PPP_temp_CEO_status=1  where   ifnull(Claim_ID,0)=@claimid  and ifnull(ppp_commissioner_approvalone,0)=0;";
                    }
                    else
                    {
                        updatedetailsdis += @"update tbl_claimAndpayment_header set  PPP_temp_CEO_status=2, claim_comment=@GmComment  where   ifnull(Claim_ID,0)=@claimid  and ifnull(ppp_commissioner_discussedone,0)=0;";
                    }
                }

                await _connection.ExecuteAsync(updatedetailsdis, new
                {
                    claimid=obj.claimid,
                    GmComment=obj.GmComment

                });

                await LogTransactionAsync(
                id: obj.claimid,
                branchId: 1,
                orgId: 1,
                actionType: "Update",
                actionDescription: "Updated PPP Approval",
                oldValue: oldvalue,
                newValue: obj,
                tableName: "tbl_claimAndpayment_header",
                userId: 1
                );

                return new ResponseModel
                {
                    Data = 1,
                    Message = "Updated successfully.",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Data = null,
                    Message = "Something went wrong while updating: " + ex.Message,
                    Status = false
                };
            }
        }

        public async Task<object> GetHistoryAsync(int id, int userid, int branchId, Int32 orgid, string fromdate, string todate)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@opt", 1);
                param.Add("@fromdate", fromdate);
                param.Add("@todate", todate);
                param.Add("@orgid", orgid);
                param.Add("@branchid", branchId);
               

                var list = await _connection.QueryAsync(ClaimAndPaymentDB.ClaimAndPaymentApprovalHistory, param, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(GetHistoryAsync),
                    UserId = userid,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(new { id, userid, branchId, orgid, fromdate, todate })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving claim and payment by ID: {ex.Message}",
                    Status = false
                };
            }
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

                param.Add("@bankid", 0);
                param.Add("@mopid", 0);
                param.Add("@applicantid", 0);
                param.Add("@SupplierID", 0);

                param.Add("@isDirector", 0);
                param.Add("@PVPaymentId", 0);
                param.Add("@claimidlog", 0);
                var list = await _connection.QueryAsync(ClaimAndPaymentDB.ClaimAndPaymentApproval, param, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(GetAllAsync),
                    UserId = userid,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(new { Id, branchId, orgid, userid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving claims: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> GetAllAsync(int bankId, Int32 MODId, int SupplierId, int ApplicantId, int userid, int isDirector, int PVPaymentId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 3);
                param.Add("@userid", userid);
                param.Add("@branchid", 1);
                param.Add("@orgid", 1);
                param.Add("@id", 0);
                param.Add("@fromdate", "");
                param.Add("@todate", "");


                param.Add("@bankid", bankId);
                param.Add("@mopid", MODId);
                param.Add("@applicantid", ApplicantId);
                param.Add("@SupplierID", SupplierId);
                param.Add("@isDirector", isDirector);
                param.Add("@PVPaymentId", PVPaymentId);
                param.Add("@claimidlog", 0);
                var list = await _connection.QueryAsync(ClaimAndPaymentDB.ClaimAndPaymentApproval, param, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(GetAllAsync),
                    UserId = userid,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(new { bankId, MODId, SupplierId, ApplicantId, userid, isDirector, PVPaymentId })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving claims: {ex.Message}",
                    Status = false
                };
            }
        }


        public async Task<object> GetRemarksList(int claimid)
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


                param.Add("@bankid", 0);
                param.Add("@mopid", 0);
                param.Add("@applicantid", 0);
                param.Add("@SupplierID", 0);
                param.Add("@isDirector", 0);
                param.Add("@PVPaymentId", 0);
                param.Add("@claimidlog", claimid);
                var list = await _connection.QueryAsync(ClaimAndPaymentDB.ClaimAndPaymentApproval, param, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(GetRemarksList),
                    UserId = 0,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(new { claimid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving claims: {ex.Message}",
                    Status = false
                };
            }
        }



        public async Task<object> GetAllPaymentPlanAsync(int Id, int branchId, Int32 orgid, int userid)
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
                var list = await _connection.QueryMultipleAsync(ClaimAndPaymentDB.PaymentPlan, param, commandType: CommandType.StoredProcedure);

                var details = list.Read().ToList();
                var header = list.ReadFirstOrDefault();


                return new ResponseModel
                {
                    Data = new { Header = header, Details = details },
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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(GetAllPaymentPlanAsync),
                    UserId = userid,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(new { Id, branchId, orgid, userid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving claims: {ex.Message}",
                    Status = false
                };
            }
        }
        public async Task<object> SavePaymentPlanAsync(PaymentPlanHdr obj)
        {
            try
            {
                var oldvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_PaymentSummary_header where SummaryId = @SummaryId", new { SummaryId = obj.summary.header.PaymentId });
                var oldDetails = new List<object>();

                foreach (var d in obj.approve)
                {
                    if (d.claimid > 0)
                    {
                        var oldClaimvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_claimAndpayment_header where Claim_Id = @ClaimId", new { ClaimId = d.claimid });

                        oldDetails.Add(oldClaimvalue);
                    }
                    else
                    {
                        oldDetails.Add(null);
                    }
                }
                

                obj.summary.header.InHand_CNY = obj.summary.header.CashInHands.CNY;
                obj.summary.header.InHand_USD = obj.summary.header.CashInHands.USD;
                obj.summary.header.InHand_SGD = obj.summary.header.CashInHands.SGD;
                obj.summary.header.InHand_IDR = obj.summary.header.CashInHands.IDR;
                obj.summary.header.InHand_MYR = obj.summary.header.CashInHands.MYR;

                obj.summary.header.Sales_CNY = obj.summary.header.CashFromSales.CNY;
                obj.summary.header.Sales_USD = obj.summary.header.CashFromSales.USD;
                obj.summary.header.Sales_SGD = obj.summary.header.CashFromSales.SGD;
                obj.summary.header.Sales_IDR = obj.summary.header.CashFromSales.IDR;
                obj.summary.header.Sales_MYR = obj.summary.header.CashFromSales.MYR;
                var insertedHeaderId = 0;
                if (obj.summary.header.PaymentId == 0)
                {
                    string InsertSummaryHeader = @"INSERT INTO `tbl_PaymentSummary_header`(`IsSubmitted`,`PaymentNo`,`Sales_CNY`,`Sales_USD`,`Sales_SGD`,`Sales_IDR`,`Sales_MYR`,`InHand_CNY`,`InHand_USD`,`InHand_SGD`,`InHand_IDR`,`InHand_MYR`,`FromDate`,`TotalInHandCash`,`ToDate`,`CashInHand`,`CashFromSalesAtFactory`,`CashNeeded`,`CreatedBy`,`CreatedDate`,`CreatedIP`,`IsActive`,`OrgId`,`BranchId`)";
                    InsertSummaryHeader += @"values(@IsSubmitted,@seqno,@Sales_CNY,@Sales_USD,@Sales_SGD,@Sales_IDR,@Sales_MYR,@InHand_CNY,@InHand_USD,@InHand_SGD,@InHand_IDR,@InHand_MYR,@FromDate,@TotalInHandCash,@ToDate,@CashInHand,@CashFromSalesAtFactory,@CashNeeded,@UserId,now(),'',1,@orgid,@branchid);";
                    var insertedSummaryHeaderId = await _connection.ExecuteAsync(InsertSummaryHeader, obj.summary.header);
                    const string getLastInsertedIdSql = "SELECT LAST_INSERT_ID();";
                    insertedHeaderId = await _connection.QuerySingleAsync<int>(getLastInsertedIdSql);

                    // Log transaction
                    await LogTransactionAsync(
                        id: insertedHeaderId,
                        branchId: obj.summary.header.branchid,
                        orgId: obj.summary.header.orgid,
                        actionType: "Insert",
                        actionDescription: "Added new payment Summary",
                        oldValue: null,
                        newValue: obj.summary.header,
                        tableName: "tbl_PaymentSummary_header",
                        userId: obj.summary.header.UserId
                    );
                }
                else
                {
                    string InsertSummaryHeader = @"update tbl_PaymentSummary_header set IsSubmitted=@IsSubmitted, Sales_CNY=@Sales_CNY,Sales_USD=@Sales_USD,Sales_SGD=@Sales_SGD,Sales_IDR=@Sales_IDR,Sales_MYR=@Sales_MYR,InHand_CNY=@InHand_CNY,InHand_USD=@InHand_USD,InHand_SGD=@InHand_SGD,InHand_IDR=@InHand_IDR,InHand_MYR=@InHand_MYR,FromDate=@FromDate,TotalInHandCash=@TotalInHandCash,ToDate=@ToDate,CashInHand=@CashInHand,CashFromSalesAtFactory=@CashFromSalesAtFactory,CashNeeded=@CashNeeded where SummaryId=@PaymentId;";
                    var insertedSummaryHeaderId = await _connection.ExecuteAsync(InsertSummaryHeader, obj.summary.header);
                    insertedHeaderId = obj.summary.header.PaymentId;

                    await LogTransactionAsync(
                    id: obj.summary.header.PaymentId,
                    branchId: obj.summary.header.branchid,
                    orgId: obj.summary.header.orgid,
                    actionType: "Update",
                    actionDescription: "Updated payment Summary",
                    oldValue: oldvalue,
                    newValue: obj.summary.header,
                    tableName: "tbl_PaymentSummary_header",
                    userId: obj.summary.header.UserId
                    );
                }


                string updatedetails = "update tbl_claimAndpayment_header set SummaryId=0,IsPaymentPlanSaved=0,IsPaymentgenerated=0 where SummaryId=" + insertedHeaderId + "; ";
                await _connection.ExecuteAsync(updatedetails, obj.approve);
                updatedetails = "";
                if (obj.summary.header.IsSubmitted == 1)
                {
                    updatedetails = @"update tbl_claimAndpayment_header set SummaryId=" + insertedHeaderId + " ,IsPaymentPlanSaved=0, IsPaymentgenerated=@ispaymentgenerated,ppp_gm_discussed=0,ppp_director_discussed=0,ppp_commissioner_discussedone=0,ppp_IsRejected=0,ppp_gm_approvalone=0,ppp_director_approvalone=0,ppp_commissioner_approvalone=0,ppp_pv_IsRejected=0,BankId=0  where Claim_ID=@claimid; ";
                }
                else
                {
                    updatedetails = @"update tbl_claimAndpayment_header set SummaryId=" + insertedHeaderId + " ,IsPaymentPlanSaved=1, IsPaymentgenerated=0  where Claim_ID=@claimid; ";
                }
                await _connection.ExecuteAsync(updatedetails, obj.approve);

                foreach (var item in obj.approve)
                {

                    await LogTransactionAsync(
                        id: item.claimid,
                        branchId: 1,
                        orgId: 1,
                        actionType: "Update",
                        actionDescription: "Updated Claim Header",
                        oldValue: oldDetails,
                        newValue: obj.approve,
                        tableName: "tbl_claimAndpayment_header",
                        userId: 1
                    );
                }

                string InsertSummaryDetails = @"INSERT INTO `tbl_PaymentSummary_detail`(`SummaryId`,`TypeId`,`Category`,`CurrencyId`,`Conversion`,`ConvertedToIDR`,`Currency` ,
                                               `IsActive`,`Amount`)";
                InsertSummaryDetails += @"values(" + insertedHeaderId + ",@TypeId,@Category,@CurrencyId,@Conversion,@ConvertedToIDR,@Currency,1,@Amount);";

                await _connection.ExecuteAsync(InsertSummaryDetails, obj.summary.details);

                int insertDetailsId = await _connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID();");

                // Log transaction
                await LogTransactionAsync(
                    id: insertDetailsId,
                    branchId: obj.summary.header.branchid,
                    orgId: obj.summary.header.orgid,
                    actionType: "Insert",
                    actionDescription: "Added new payment Summary details",
                    oldValue: null,
                    newValue: obj.summary.header,
                    tableName: "tbl_PaymentSummary_detail",
                    userId: obj.summary.header.UserId
                );

                if (obj.summary.header.PaymentId == 0)
                {
                    var UpdateSeq = "update master_documentnumber set Doc_Number=Doc_Number+1 where Doc_Type=3 and unit=" + obj.summary.header.branchid + "; ";
                    var Result = await _connection.ExecuteAsync(UpdateSeq, obj.summary.header.branchid);
                }


                return new ResponseModel
                {
                    Data = 1,
                    Message = "updated Successfully",
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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(SavePaymentPlanAsync),
                    UserId = 0,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(obj)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Something went wrong while updating: {ex.Message}",
                    Status = false
                };
            }
        }


        public async Task<object> RejectClaims(RejectDetails claims)
        {
            try
            {
                var oldHeader = new List<object>();

                foreach (var d in claims.Reject)
                {
                    if (d.Id > 0)
                    {
                        var oldvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_claimAndpayment_header where Claim_Id = @ClaimId", new { ClaimId = d.Id });

                        oldHeader.Add(oldvalue);
                    }
                    else
                    {
                        oldHeader.Add(null);
                    }
                }
                if (claims.IsPPP == 1)
                {
                    string updatedetails = @"update tbl_claimAndpayment_header set voucherid=0,voucherno='',ppp_IsRejected=" + 1 + ",ppp_IsRejected_By=" + claims.UserId + "  where Claim_ID=@Id";
                    await _connection.ExecuteAsync(updatedetails, claims.Reject);
                }
                else
                {
                    string updatedetails = @"update tbl_claimAndpayment_header set voucherid=0,voucherno='',PPP_PV_Commissioner_approveone=0,PPP_PV_Director_approve=0,ppp_pv_Commissioner_discussedone=0,ppp_pv_Director_discussed=0,ppp_pv_IsRejected=" + 1 + ",ppp_pv_IsRejected_By=" + claims.UserId + "  where Claim_ID=@Id";
                    await _connection.ExecuteAsync(updatedetails, claims.Reject);
                }

                foreach (var item in claims.Reject)
                {

                    await LogTransactionAsync(
                        id: item.Id,
                        branchId: 1,
                        orgId: 1,
                        actionType: "Update",
                        actionDescription: "Updated claim Header",
                        oldValue: oldHeader,
                        newValue: claims,
                        tableName: "tbl_claimAndpayment_header",
                        userId: 1
                    );
                }
                return new ResponseModel
                {
                    Data = 1,
                    Message = "Rejected Successfully",
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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(RejectClaims),
                    UserId = 0,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(claims)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Something went wrong while rejecting: {ex.Message}",
                    Status = false
                };
            }
        }


        public async Task<object> GetPaymentSummarySeqNoAsync(int userid, int branchId, Int32 orgid)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@opt", 2);
                param.Add("@userid", userid);
                param.Add("@branchid", branchId);
                param.Add("@orgid", orgid);
                param.Add("@id", 0);
                param.Add("@fromdate", "");
                param.Add("@todate", "");

                param.Add("@bankid", 0);
                param.Add("@mopid", 0);
                param.Add("@applicantid", 0);
                param.Add("@SupplierID", 0);
                param.Add("@isDirector", 0);
                param.Add("@PVPaymentId", 0);
                param.Add("@claimidlog", 0);
                var list = await _connection.QueryAsync(ClaimAndPaymentDB.ClaimAndPaymentApproval, param, commandType: CommandType.StoredProcedure);
                var data = list.FirstOrDefault();
                return new ResponseModel
                {
                    Data = data,
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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(GetPaymentSummarySeqNoAsync),
                    UserId = userid,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(new { userid, branchId, orgid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving claim and payment by ID: {ex.Message}",
                    Status = false
                };
            }

        }


        public async Task<object> GetDiscussionList(int userid, int branchId, Int32 orgid)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@opt", 5);
                param.Add("@userid", userid);
                param.Add("@branchid", branchId);
                param.Add("@orgid", orgid);
                param.Add("@id", 0);
                param.Add("@fromdate", "");
                param.Add("@todate", "");

                param.Add("@bankid", 0);
                param.Add("@mopid", 0);
                param.Add("@applicantid", 0);
                param.Add("@SupplierID", 0);
                param.Add("@isDirector", 0);
                param.Add("@PVPaymentId", 0);
                param.Add("@claimidlog", 0);
                var list = await _connection.QueryAsync(ClaimAndPaymentDB.ClaimAndPaymentApproval, param, commandType: CommandType.StoredProcedure);
                var data = list;
                return new ResponseModel
                {
                    Data = data,
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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(GetDiscussionList),
                    UserId = userid,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(new { userid, branchId, orgid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving claim and payment by ID: {ex.Message}",
                    Status = false
                };
            }

        }

        public async Task<object> AcceptDiscussion(int claimid,string Comment,int Type,int isclaimant)
        {
            try
            {
                var oldClaimvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_claimAndpayment_header where Claim_Id = @ClaimId", new { ClaimId = claimid });
                if (claimid >  0)
                {
                    if (isclaimant == 1)
                    {

                        string updatedetails = @"update tbl_claimAndpayment_header set isclaimant_discussed=0,IsSubmitted=0,claim_comment='" + Comment + "', isdiscussionaccepted=" + 0 + "  where Claim_ID=@claimid and isdiscussionaccepted=1;";
                         
                        await _connection.ExecuteAsync(updatedetails, new { Comment, ClaimId = claimid });
                         
                    }
                    else
                    {
                        if (Type == 1)
                        {
                            string updatedetails = @"update tbl_claimAndpayment_header set claim_comment='" + Comment + "', isdiscussionaccepted=" + 0 + "  where Claim_ID=@claimid;";
                            updatedetails += @"update tbl_claimAndpayment_header set  claim_comment='" + Comment + "', claim_hod_isdiscussed=" + 0 + "  where Claim_ID=@claimid and ifnull(claim_hod_isdiscussed,0)=1;";
                            updatedetails += @"update tbl_claimAndpayment_header set  claim_comment='" + Comment + "', claim_gm_isdiscussed=" + 0 + "  where Claim_ID=@claimid and ifnull(claim_gm_isdiscussed,0)=1;";
                            updatedetails += @"update tbl_claimAndpayment_header set  claim_comment='" + Comment + "', claim_director_isdiscussed=" + 0 + "  where Claim_ID=@claimid and ifnull(claim_director_isdiscussed,0)=1;";

                            await _connection.ExecuteAsync(updatedetails, new { Comment, ClaimId = claimid });
                        }
                        else
                        {
                            string updatedetails = @"update tbl_claimAndpayment_header set claim_comment='" + Comment + "', isdiscussionaccepted=" + 0 + "  where summaryid=@claimid;";

                            updatedetails += @"update tbl_claimAndpayment_header set  claim_comment='" + Comment + "', ppp_commissioner_discussedone=" + 0 + "  where summaryid=@claimid and ifnull(ppp_commissioner_discussedone,0)=1;";

                            updatedetails += @"update tbl_claimAndpayment_header set  claim_comment='" + Comment + "', ppp_gm_discussed=" + 0 + "  where summaryid=@claimid and ifnull(ppp_gm_discussed,0)=1;";
                            updatedetails += @"update tbl_claimAndpayment_header set  claim_comment='" + Comment + "', ppp_director_discussed=" + 0 + "  where summaryid=@claimid and ifnull(ppp_director_discussed,0)=1;";

                            updatedetails += @"update tbl_claimAndpayment_header set  claim_comment='" + Comment + "', ppp_pv_Commissioner_discussedone=" + 0 + "  where summaryid=@claimid and ifnull(ppp_pv_Commissioner_discussedone,0)=1;";
                            updatedetails += @"update tbl_claimAndpayment_header set  claim_comment='" + Comment + "', ppp_pv_Director_discussed=" + 0 + "  where summaryid=@claimid and ifnull(ppp_pv_Director_discussed,0)=1;";
                            await _connection.ExecuteAsync(updatedetails, new { Comment, ClaimId = claimid });

                        }
                    }
                    await LogTransactionAsync(
                        id: claimid,
                        branchId: 1,
                        orgId: 1,
                        actionType: "Update",
                        actionDescription: "Updated Claim Header",
                        oldValue: oldClaimvalue,
                        newValue: new {claimid = claimid, Comment = Comment, Type = Type, isclaimant = isclaimant },
                        tableName: "tbl_PurchaseRequisition_Header",
                        userId: 1
                    );
                    return new ResponseModel
                        {
                            Data = 1,
                            Message = "Discussion Accepted Successfully",
                            Status = true
                        };
                }
                else
                {
                    return new ResponseModel
                    {
                        Data = 0,
                        Message = "Claim not available",
                        Status = true
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
                    Source = nameof(ClaimApprovalRepository),
                    Method_Function = nameof(AcceptDiscussion),
                    UserId = 0,
                    ScreenName = "ClaimApproval",
                    RequestData_Payload = JsonSerializer.Serialize(new { claimid, Comment, Type, isclaimant })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Something went wrong while accepting claim: {ex.Message}",
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
                ModuleName = "Claim",
                ScreenName = "Claim Approval",
                UserId = userId,
                ActionType = actionType,
                ActionDescription = actionDescription,
                TableName = tableName,
                OldValue = oldValue != null ? Newtonsoft.Json.JsonConvert.SerializeObject(oldValue) : null,
                NewValue = newValue != null ? Newtonsoft.Json.JsonConvert.SerializeObject(newValue) : null,
                CreatedBy = userId ?? 0,
                OrgId = orgId,
                BranchId = branchId,
                DbLog = 4
            };

            await _transactionLogRepo.LogTransactionAsync(log);
        }
    }
}

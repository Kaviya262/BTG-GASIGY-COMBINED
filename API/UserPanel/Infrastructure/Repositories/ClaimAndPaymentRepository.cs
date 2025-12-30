using Application.Procurement.InvoiceReceipt.getIRNDetails;
using BackEnd.Shared;
using Core.Abstractions;
using Core.Finance.ClaimAndPayment;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Dapper;
using System.Data;
using System.Security.Claims;

namespace Infrastructure.Repositories
{
    public class ClaimAndPaymentRepository : IClaimAndPaymentRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public ClaimAndPaymentRepository(IUnitOfWorkDB3 financedb, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository transactionLogRepo)
        {
            _connection = financedb.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = transactionLogRepo;
        }

        public async Task<SharedModelWithResponse> GetSeqNumber(int id, string text, int type, int unit, int orgid)
        {

            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 1);
                param.Add("@id", id);
                param.Add("@text", text);
                param.Add("@type", type);
                param.Add("@branchid", unit);
                param.Add("@orgid", orgid);


                var data = await _connection.QueryFirstOrDefaultAsync<SharedModel>(Shared.SharedProcedure, param: param, commandType: CommandType.StoredProcedure);



                return new SharedModelWithResponse()
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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(GetSeqNumber),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, text, type, unit, orgid })
                });
                return new SharedModelWithResponse()
                {
                    Data = null,
                    Message = "Something went wrong",
                    Status = false
                };
            }


        }

        public async Task<object> AddAsync(ClaimAndPaymentModel obj)
        {
            try
            {
                int IsValidated = 0;
                string Message = "";
                 
                var response = await GetSeqNumber(0, obj.Header.ApplicationNo, 1, obj.Header.BranchId, obj.Header.OrgId);
                if (response.Status == true)
                {
                    if (response.Data.result == 1)
                    {
                        IsValidated = 1;
                        Message = " - The current claim number " + obj.Header.ApplicationNo + " is taken for another Claim so the new claim number (" + response.Data.text + ") has been generated for this claim";
                        obj.Header.ApplicationNo = response.Data.text;
                    }
                }

                const string insertHeaderSql = @"
            INSERT INTO tbl_claimAndpayment_header (
                ClaimCategoryId, ApplicationDate, ApplicationNo, DepartmentId, ApplicantId,
                JobTitle, HOD, TransactionCurrencyId,ModeOfPaymentId, AttachmentName, AttachmentPath,
                CostCenterId, ClaimAmountInTC, TotalAmountInIDR, Remarks,CreatedBy,
                IsActive, IsSubmitted, OrgId, BranchId,SupplierId,PONo,docType
            )
            VALUES (
                @ClaimCategoryId, @ApplicationDate, @ApplicationNo, @DepartmentId, @ApplicantId,
                @JobTitle, @HOD, @TransactionCurrencyId,@ModeOfPaymentId, @AttachmentName, @AttachmentPath,
                @CostCenterId, @ClaimAmountInTC, @TotalAmountInIDR, @Remarks,@UserId,
                1, @IsSubmitted, @OrgId, @BranchId,@SupplierId,@PONo,@docType
            );";

                await _connection.ExecuteAsync(insertHeaderSql, obj.Header);

                var claimId = await _connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID();");

                // Log transaction
                await LogTransactionAsync(
                    id: claimId,
                    branchId: obj.Header.BranchId,
                    orgId: obj.Header.OrgId,
                    actionType: "Insert",
                    actionDescription: "Added new Claim And Payment",
                    oldValue: null,
                    newValue: obj.Header,
                    tableName: "tbl_claimAndpayment_header",
                    userId: obj.Header.UserId
                );

                const string insertDetailSql = @"
            INSERT INTO tbl_claimAndpayment_Details (
                Claim_ID, ClaimTypeId, Amount, TaxRate, TotalAmount,
                ExpenseDate, Purpose, IsActive, PaymentId,IsTaxCalType,TaxPerc,docReference,taxid,vatid,VatRate,VatPerc,poid
            )
            VALUES (
                @ClaimId, @ClaimTypeId, @Amount, @TaxRate, @TotalAmount,
                @ExpenseDate, @Purpose, 1, @PaymentId,@IsTaxCalType,@TaxPerc,@docReference,@taxid,@vatid,@VatRate,@VatPerc,@poid
            );";

                foreach (var detail in obj.Details)
                {
                    detail.ClaimId = claimId;
                }

                await _connection.ExecuteAsync(insertDetailSql, obj.Details);

                int insertDetailsId = await _connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID();");

                foreach (var details in obj.Details) {
                    // Log transaction
                    await LogTransactionAsync(
                        id: insertDetailsId,
                        branchId: obj.Header.BranchId,
                        orgId: obj.Header.OrgId,
                        actionType: "Insert",
                        actionDescription: "Added new Claim & payment Details",
                        oldValue: null,
                        newValue: details,
                        tableName: "tbl_claimAndpayment_Details",
                        userId: obj.Header.UserId
                    );
                }


                var UpdateSeq = "update master_documentnumber set Doc_Number=Doc_Number+1 where Doc_Type=1 and unit=" + obj.Header.BranchId + "; call proc_claimhodapproval("+ claimId + "); ";
                var Result = await _connection.ExecuteAsync(UpdateSeq, obj.Header.BranchId);
                 

                return new ResponseModel
                {
                    Data = claimId,
                    Message = obj.Header.IsSubmitted == 1 ? "Posted Successfully" : "Saved Successfully " + Message,
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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(AddAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Something went wrong while saving claim and payment. : "+ex.Message,
                    Status = false
                };
            }
        }

        public async Task<object> UpdateAsync(ClaimAndPaymentModel obj)
        {
            try
            {
                var oldvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_claimAndpayment_header where Claim_ID = @Claim_ID", new { Claim_ID = obj.Header.ClaimId });
                var oldDetails = new List<object>();

                foreach (var d in obj.Details)
                {
                    if (d.ClaimDtlId > 0)
                    {
                        var oldDetailsvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_claimAndpayment_Details where Claim_Dtl_ID = @ClaimDtlId", new { ClaimDtlId = d.ClaimDtlId });

                        oldDetails.Add(oldDetailsvalue);
                    }
                    else
                    {
                        oldDetails.Add(null);
                    }
                }

                var claimcount = await _connection.QuerySingleAsync<int>("SELECT  count(*) from tbl_claimAndpayment_header where Claim_ID=@ClaimId and ifnull(ppp_gm_approvalone,0)=0 ", new { ClaimId = obj.Header.ClaimId });

                if (claimcount == 0)
                {
                    return new ResponseModel
                    {
                        Data = 0,
                        Message = "Cannot edit this claim. please check the approval status.",
                        Status = false
                    };
                }
                const string updateHeaderSql = @"
            UPDATE tbl_claimAndpayment_header
            SET 
                ClaimCategoryId = @ClaimCategoryId,
                ApplicationDate = @ApplicationDate,
                ApplicationNo = @ApplicationNo,
                DepartmentId = @DepartmentId,
                ApplicantId = @ApplicantId,
                JobTitle = @JobTitle,
                HOD = @HOD,
                TransactionCurrencyId = @TransactionCurrencyId,
                ModeOfPaymentId = @ModeOfPaymentId,
                ClaimAmountInTC = @ClaimAmountInTC,
                TotalAmountInIDR = @TotalAmountInIDR,
                Remarks = @Remarks,
                CreatedBy = @UserId,
                LastModifiedDate = NOW(),
                LastModifiedIP = '',
                IsSubmitted = case when(IsSubmitted=1) then IsSubmitted else @IsSubmitted end,
                SupplierId=@SupplierId,
                PONo=@PONo,
                docType=@docType,claim_gm_isdiscussed= case when (@IsSubmitted=1) then 0 else claim_gm_isdiscussed end,claim_hod_isdiscussed= case when (@IsSubmitted=1) then 0  else claim_hod_isdiscussed end,claim_director_isdiscussed= case when (@IsSubmitted=1) then 0 else claim_director_isdiscussed end 
            WHERE Claim_ID = @ClaimId;";

                await _connection.ExecuteAsync(updateHeaderSql, obj.Header);

                await LogTransactionAsync(
                id: obj.Header.ClaimId,
                branchId: obj.Header.BranchId,
                orgId: obj.Header.OrgId,
                actionType: "Update",
                actionDescription: "Updated Claim $ payment Header",
                oldValue: oldvalue,
                newValue: obj,
                tableName: "tbl_claimAndpayment_header",
                userId: obj.Header.UserId
                );

                const string deactivateDetailsSql = @"
            UPDATE tbl_claimAndpayment_Details 
            SET IsActive = 0 
            WHERE Claim_ID = @ClaimId;";
                await _connection.ExecuteAsync(deactivateDetailsSql, new { ClaimId = obj.Header.ClaimId });

                foreach (var detail in obj.Details)
                {
                    detail.ClaimId = obj.Header.ClaimId;

                    if (detail.ClaimDtlId == 0)
                    {
                        const string insertDetailSql = @"
                    INSERT INTO tbl_claimAndpayment_Details (
                        Claim_ID, ClaimTypeId, ClaimAndPaymentDesc, Amount, TaxRate, TotalAmount,
                        ExpenseDate, Purpose, IsActive, PaymentId ,IsTaxCalType,TaxPerc,docReference,taxid,vatid,VatRate,VatPerc,poid
                    )
                    VALUES (
                        @ClaimId, @ClaimTypeId, @ClaimAndPaymentDesc, @Amount, @TaxRate, @TotalAmount,
                        @ExpenseDate, @Purpose, 1, @PaymentId ,@IsTaxCalType,@TaxPerc,@docReference,@taxid,@vatid,@VatRate,@VatPerc,@poid
                    );";

                        await _connection.ExecuteAsync(insertDetailSql, detail);
                    }
                    else
                    {
                        const string reactivateDetailSql = @"
                    UPDATE tbl_claimAndpayment_Details 
                    SET poid=@poid,VatRate=@VatRate,VatPerc=@VatPerc,taxid=@taxid,vatid=@vatid, docReference=@docReference,TaxPerc=@TaxPerc , IsTaxCalType=@IsTaxCalType,IsActive = 1 ,ClaimTypeId=@ClaimTypeId,PaymentId=@PaymentId,Amount=@Amount,TaxRate=@TaxRate,TotalAmount=@TotalAmount,ExpenseDate=@ExpenseDate,
                    Purpose=@Purpose
                    WHERE Claim_Dtl_ID = @ClaimDtlId;";

                        await _connection.ExecuteAsync(reactivateDetailSql, detail);
                    }
                }

                foreach (var item in obj.Details)
                {

                    await LogTransactionAsync(
                        id: item.ClaimDtlId,
                        branchId: obj.Header.BranchId,
                        orgId: obj.Header.OrgId,
                        actionType: "Update",
                        actionDescription: "Updated Claim & Payment Details",
                        oldValue: oldDetails,
                        newValue: obj,
                        tableName: "tbl_PurchaseRequisition_Header",
                        userId: 1
                    );
                }

                var UpdateSeq = "call proc_claimhodapproval(" + obj.Header.ClaimId + "); ";
                var Result = await _connection.ExecuteAsync(UpdateSeq, obj.Header.BranchId);


                return new ResponseModel
                {
                    Data = obj.Header.ClaimId,
                    Message = obj.Header.IsSubmitted == 1 ? "Posted Successfully" : "Updated Successfully",
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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(UpdateAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel
                {
                    Data = 0,
                    Message = $"Error updating claim and payment: {ex.Message}",
                    Status = false
                };
            }
        }


        public async Task<object> DeleteClaim(InActiveClaim obj)
        {
            try
            {
                var oldvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_claimAndpayment_header where Claim_Id = @ClaimId", new { ClaimId = obj.ClaimId });
                string updateHeaderSql = @"
            UPDATE tbl_claimAndpayment_header SET isactive=0, InActiveBy = "+obj.InActiveBy+", InActiveDate = now(), InActiveIP = '" + obj.ClaimId + "'   WHERE ifnull(claim_gm_isapproved,0)=0 and Claim_ID = " + obj.ClaimId + ";";

                await _connection.ExecuteAsync(updateHeaderSql);

                await LogTransactionAsync(
                    id: obj.ClaimId,
                    branchId: 1,
                    orgId: 1,
                    actionType: "Update",
                    actionDescription: "Updated Claim & Payment Header",
                    oldValue: oldvalue,
                    newValue: obj,
                    tableName: "tbl_claimAndpayment_header",
                    userId: 1
                );
                var UpdateSeq = "call proc_claimhodapproval(" + obj.ClaimId + "); ";
                var Result = await _connection.ExecuteAsync(UpdateSeq, obj.ClaimId);


                return new ResponseModel
                {
                    Data = 0,
                    Message ="Deleted Successfully",
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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(DeleteClaim),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel
                {
                    Data = 0,
                    Message = $"Error deleting claim: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> DiscussClaim(DiscussClaim obj)
        {
            try
            {
                var oldvalue = await _connection.QueryFirstOrDefaultAsync<object>("select * from tbl_claimAndpayment_header where ClaimId = @ClaimId", new { ClaimId = obj.ClaimId });
                //    string updateHeaderSql = @"
                //UPDATE tbl_claimAndpayment_header SET isdiscussionaccepted=1,isclaimant_discussed=1, claim_comment = '" + obj.Comment + "'   WHERE Claim_ID = " + obj.ClaimId + " and ifnull(isclaimant_discussed,0)=0 and ifnull(claim_gm_isdiscussed,0)=0 and ifnull(claim_gm_isapproved,0)=0;";

                string updateHeaderSql = @"
            UPDATE tbl_claimAndpayment_header SET claim_gm_isdiscussed=0,issubmitted=1,claim_comment = '" + obj.Comment + "', LastModifiedBY="+obj.DiscussedBy+"   WHERE Claim_ID = " + obj.ClaimId + " and   ifnull(claim_gm_isdiscussed,0)=1 and ifnull(claim_gm_isapproved,0)=0 and isactive=1;";

                await _connection.ExecuteAsync(updateHeaderSql);

                await LogTransactionAsync(
                id: obj.ClaimId,
                branchId: 1,
                orgId: 1,
                actionType: "Update",
                actionDescription: "Updated Claim & payment",
                oldValue: oldvalue,
                newValue: obj,
                tableName: "tbl_claimAndpayment_header",
                userId: 1
                );

                return new ResponseModel
                {
                    Data = 0,
                    Message = "Discussed Successfully",
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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(DiscussClaim),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel
                {
                    Data = 0,
                    Message = $"Error discussing claim: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<ResponseModel> UploadDO(int Id, string Path,string FileName)
        {
            int Result = 0;
            try
            {
              
                var Updatepacking = "update tbl_claimAndpayment_header set AttachmentName='"+ FileName + "',AttachmentPath='" + Path + "'  where Claim_ID=" + Id + ";";
               
                Result = await _connection.ExecuteAsync(Updatepacking);
                
                 
                return new ResponseModel()
                {
                    Data = null,
                    Message = "File Uploaded Successfully",
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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(UploadDO),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { Id, Path, FileName })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error uploading file: {ex.Message}",
                    Status = false
                };
            }

        }
        public async Task<object> GetByIdAsync(int id,int orgid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 2);
                param.Add("@claimid", id);
                param.Add("@branchid", 1);
                param.Add("@orgid", orgid);
                param.Add("@departmentid", 0);
                param.Add("@categorytypeid", 0);
                param.Add("@currencyid", 0);
                param.Add("@user_id", 0);
                param.Add("@claimtypeid", 0);

                var result = await _connection.QueryMultipleAsync("proc_claimAndpayment", param, commandType: CommandType.StoredProcedure);

                var header = result.ReadFirstOrDefault();
                var details = result.Read().ToList();

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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(GetByIdAsync),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, orgid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving claim and payment by ID: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> GetAllAsync(int requesterId, int branchId,Int32 orgid, Int32 departmentid, Int32 categoryid,Int32 currencyid,Int32 user_id,Int32 claimtypeid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 1);
                param.Add("@claimid", 0);
                param.Add("@branchid", 1);
                param.Add("@orgid", orgid);
                param.Add("@departmentid", departmentid);
                param.Add("@categorytypeid", categoryid);
                 param.Add("@currencyid", currencyid);
                param.Add("@user_id", user_id);
                param.Add("@claimtypeid", claimtypeid);
                
                var list = await _connection.QueryAsync("proc_claimAndpayment", param, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(GetAllAsync),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { requesterId, branchId, orgid, departmentid, categoryid, currencyid, user_id, claimtypeid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving claims: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> GetSequencesNo(int branchId,int orgid, int userid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 3);
                param.Add("@claimid", 0);
                param.Add("@branchid", branchId);  // use passed value
                param.Add("@orgid", orgid);
                param.Add("@departmentid", 0);
                param.Add("@categorytypeid", 0);
                param.Add("@currencyid", 0);
                param.Add("@user_id", userid);
                param.Add("@claimtypeid", 0);
                var result = await _connection.QueryFirstOrDefaultAsync<object>(
                    "proc_claimAndpayment",
                    param,
                    commandType: CommandType.StoredProcedure);

                return new ResponseModel
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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(GetSequencesNo),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { branchId, orgid, userid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving sequence number: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> GetClaimHistory(string fromdate, string todate, int branchid, int orgid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 1);
                param.Add("@fromdate", fromdate);
                param.Add("@todate", todate);
                param.Add("@branchid", branchid);
                param.Add("@orgid", orgid);
                var result = await _connection.QueryAsync<object>("proc_claim_gmanddirector_history", param, commandType: CommandType.StoredProcedure);

                return new ResponseModel
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
                    Source = nameof(ClaimAndPaymentRepository),
                    Method_Function = nameof(GetClaimHistory),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { fromdate, todate, branchid, orgid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = $"Error retrieving Claim History: {ex.Message}",
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
                ScreenName = "Cliam & payment",
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

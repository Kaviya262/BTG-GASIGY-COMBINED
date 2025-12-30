using BackEnd.Quotation;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Core.OrderMng.Quotation;
using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Infrastructure.Repositories;
using Mysqlx.Crud;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using UserPanel.Infrastructure.Data;

public class QuotationRepository : IQuotationRepository
{

    private readonly IDbConnection _connection;
    string IPAddress = "";
    private readonly IErrorLogMasterRepository _errorLogRepo;
    private readonly IUserTransactionLogRepository _transactionLogRepo;

    public QuotationRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogRepo, IUserTransactionLogRepository userTransactionLogRepository)
    {
        _connection = unitOfWork.Connection;
        _errorLogRepo = errorLogRepo;
        _transactionLogRepo = userTransactionLogRepository;
    }
    public async Task<object> AddAsync(QuotationItemsMain Obj)
    {
        try
        {

            int IsValidated = 0;
            string Message = "";
            Int32 Result = 0;
            string checkSql = "SELECT count(*) FROM tbl_salesquatation_header WHERE SQ_Nbr = @SQ_Nbr";
            var isUsed = await _connection.QueryFirstOrDefaultAsync<int>(checkSql, new { SQ_Nbr = Obj.Header.SQ_Nbr });
            if (isUsed > 0)
            {
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Sales Quototation {Obj.Header.SQ_Nbr} is already Exist. Please Refresh The Page",
                    Status = false
                };
            }
            SharedRepository SR = new SharedRepository(_connection);

            var response = await SR.GetSeqNumber(0, Obj.Header.SQ_Nbr, 1, Obj.Header.BranchId, Obj.Header.OrgId);
            if (response.Status == true)
            {
                if (response.Data.result == 1)
                {
                    IsValidated = 1;
                    Message = " - The current order number " + Obj.Header.SQ_Nbr + " is taken for another order so the new order number (" + response.Data.text + ") has been generated for this order";
                    Obj.Header.SQ_Nbr = response.Data.text;
                }
            }



            const string headerSql = @"
            INSERT INTO tbl_salesquatation_header 
            (SQ_Nbr, Sys_SQ_Nbr, SQ_Type, SQ_Date, OrderedBy, Subject, CustomerId, MainAddress, DeliveryAddress, 
             PhoneNumber, FaxNo, Email, CustomerAttention, Validity, DeliveryTerms, PaymentTerms, 
             PaymentMethod, SalesPerson, SalesPersonContact, SalesPersonEmail, CreatedBy, CreatedDate, createdip,
               IsActive, OrgId, BranchId,DeliveryAddressId,CustomerContactId,IsSubmitted,TermsAndCond,EffectiveFromDate,IsReadyToPost,IsSavedByDSO,Qtn_Day,Qtn_Month,TBA,IsSalesOrderSaved,IsWithCustomer)
            VALUES 
            (@SQ_Nbr, @Sys_SQ_Nbr, @SQ_Type, @SQ_Date, @OrderedBy, @Subject, @CustomerId, @MainAddress, @DeliveryAddress,
             @PhoneNumber, @FaxNo, @Email, @CustomerAttention, @Validity, @DeliveryTerms, @PaymentTerms, 
             @PaymentMethod, @SalesPerson, @SalesPersonContact, @SalesPersonEmail, @UserId, now(), 
             '',   1, @OrgId, @BranchId,@DeliveryAddressId,@CustomerContactId,@IsSubmit,@TermsAndCond,@EffectiveFromDate,@IsReadyToPost,@IsSavedByDSO,@Qtn_Day,@Qtn_Month,@TBA,@IsSalesOrderSaved,@IsWithCustomer);               
        ";

            await _connection.ExecuteAsync(headerSql, Obj.Header);

            const string getLastInsertedIdSql = "SELECT LAST_INSERT_ID();";
            var insertedHeaderId = await _connection.QuerySingleAsync<int>(getLastInsertedIdSql);

            // Log transaction
            await LogTransactionAsync(
                id: insertedHeaderId,
                branchId: Obj.Header.BranchId,
                orgId: Obj.Header.OrgId,
                actionType: "Insert",
                actionDescription: "Added new Quatation",
                oldValue: null,
                newValue: Obj,
                tableName: "tbl_salesquatation_header",
                userId: Obj.Header.UserId
            );

            Result = insertedHeaderId;
            string ContactOperationQuery = "";

            if (Obj.operation != null)
            {
                foreach (var row in Obj.operation)
                {
                    ContactOperationQuery += "INSERT INTO  `tbl_salesquatation_Cont_Operation`( `SQ_ID`,`ContactId`,`IsActive`,`CreatedDate`,`CreatedIP`)" +
                        "select " + insertedHeaderId + "," + row.CustomerContactId + ",1,now(),'192'; ";
                }
                if (Obj.operation.Count > 0)
                {
                    Result = await _connection.ExecuteAsync(ContactOperationQuery);
                }

                var contactOpLastId = await _connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID();");

                await LogTransactionAsync(
                    id: contactOpLastId,
                    branchId: Obj.Header.BranchId,
                    orgId: Obj.Header.OrgId,
                    actionType: "Insert",
                    actionDescription: "Added new contact operation",
                    oldValue: null,
                    newValue: Obj.operation,
                    tableName: "tbl_salesquatation_Cont_Operation",
                    userId: Obj.Header.UserId
                );
            }
            foreach (var row in Obj.Details)
            {
                row.SQ_ID = insertedHeaderId;
                const string detailsql = @"
                INSERT INTO tbl_salesquatation_detail 
                (SQ_ID, GasCodeId, GasDescription, Volume, Po_No, Pressure, Qty, UOM, CurrencyId, UnitPrice, TotalPrice, 
                 ConvertedPrice, ConvertedCurrencyId, IsActive,Exchangerate, IsSoRaised)
                VALUES 
                ( @SQ_ID  , @GasCodeId, @GasDescription, @Volume, @Po_No, @Pressure, @Qty, @UOM, @CurrencyId, @UnitPrice,  @TotalPrice, @ConvertedPrice, @ConvertedCurrencyId, 1,@Exchangerate, 0);";
                Result = await _connection.ExecuteAsync(detailsql, row);

                var detailLastId = await _connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID();");

                await LogTransactionAsync(
                    id: detailLastId,
                    branchId: Obj.Header.BranchId,
                    orgId: Obj.Header.OrgId,
                    actionType: "Insert",
                    actionDescription: "Added new quotation detail",
                    oldValue: null,
                    newValue: row,
                    tableName: "tbl_salesquatation_detail",
                    userId: Obj.Header.UserId
                );
            }


            int BranchId = Obj.Header.BranchId;
            var UpdateSeq = "update master_documentnumber set Doc_Number=Doc_Number+1 where Doc_Type=1 and unit=" + BranchId;
            Result = await _connection.ExecuteAsync(UpdateSeq, BranchId);

            var docNo = await _connection.QuerySingleAsync<int>("SELECT Doc_Number FROM master_documentnumber WHERE Doc_Type = 1 AND unit = @Unit", new { Unit = BranchId });
            await LogTransactionAsync(
    id: docNo,
    branchId: Obj.Header.BranchId,
    orgId: Obj.Header.OrgId,
    actionType: "Update",
    actionDescription: "Updated document sequence number",
    oldValue: null,
    newValue: new { Doc_Number = docNo + 1 },
    tableName: "master_documentnumber",
    userId: Obj.Header.UserId
);
            if (Obj.Header.IsSubmit == 1)
            {
                string RateUpdate = "update master_gascode as a inner join tbl_salesquatation_detail as b on a.id = b.gascodeid and b.isactive = 1 inner join tbl_salesquatation_header as c on c.id = b.sq_id and c.isactive = 1 and ifnull(c.IsSubmitted,0)= 1 set a.LastUpdatedPrice = b.unitprice where b.sq_id = " + insertedHeaderId + ";";
                Result = await _connection.ExecuteAsync(RateUpdate);

                await LogTransactionAsync(
    id: insertedHeaderId,
    branchId: Obj.Header.BranchId,
    orgId: Obj.Header.OrgId,
    actionType: "Update",
    actionDescription: "Updated gas last updated price",
    oldValue: null,
    newValue: new { SQ_ID = insertedHeaderId },
    tableName: "master_gascode",
    userId: Obj.Header.UserId
);
            }
            if (Result == 0)
            {
                return new ResponseModel()
                {
                    Data = null,
                    Message = "Saved failed",
                    Status = false
                };
            }
            else
            {
                if (Obj.Header.IsSubmit == 0)
                {
                    return new ResponseModel()
                    {
                        Data = insertedHeaderId,
                        Message = "Saved Successfully" + Message,
                        Status = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        Data = insertedHeaderId,
                        Message = "Posted Successfully" + Message,
                        Status = true
                    };
                }
            }
        }
        catch (Exception Ex)
        {
            //Logger.Instance.Error("Exception:", Ex);
            await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
            {
                ErrorMessage = Ex.Message,
                ErrorType = Ex.GetType().Name,
                StackTrace = Ex.StackTrace,
                Source = nameof(QuotationRepository),
                Method_Function = nameof(AddAsync),
                UserId = Obj.Header.UserId,
                ScreenName = "Quotation",
                RequestData_Payload = JsonConvert.SerializeObject(Obj)
            });
            return new ResponseModel()
            {
                Data = null,
                Message = "Something went wrong",
                Status = false
            };
        }
    }


    public async Task<object> UpdateAsync(QuotationItemsMain obj)
    {
        try
        {


            if (obj.Header.isalreadypost == 1)
            {
                string GetInsertedData = "SELECT Id from tbl_salesquatation_header where Id=" + obj.Header.Id + " and ((IsSubmitted=1 and ifnull(IsSoTaken,0)=0) OR (IsSubmitted=0 and ifnull(IsSoTaken,0)=1) );";
                var PricechangeId = await _connection.QuerySingleOrDefaultAsync<int>(GetInsertedData);

                if (obj.Header.Id == PricechangeId)
                {


                    const string priceSql = @"
        UPDATE tbl_salesquatation_detail
        SET
            UnitPrice = @UnitPrice,
            TotalPrice = @TotalPrice,
            ConvertedPrice = @ConvertedPrice,
            Exchangerate=@Exchangerate
        WHERE Id = @Id ;";




                    foreach (var row in obj.Details)
                    {
                        //row.SQ_ID = header.Id;
                        if (row.Id > 0)
                        {
                            await _connection.ExecuteAsync(priceSql, row);

                            await LogTransactionAsync(
                            id: row.Id,
                            branchId: obj.Header.BranchId,
                            orgId: obj.Header.OrgId,
                            actionType: "Update",
                            actionDescription: "Updated quotation price",
                            oldValue: null,
                            newValue: row,
                            tableName: "tbl_salesquatation_detail",
                            userId: obj.Header.UserId
                        );
                        }

                    }
                    string RateUpdate = "update master_gascode as a inner join tbl_salesquatation_detail as b on a.id = b.gascodeid and b.isactive = 1 inner join tbl_salesquatation_header as c on c.id = b.sq_id and c.isactive = 1 and ifnull(c.IsSubmitted,0)= 1 set a.LastUpdatedPrice = b.unitprice where b.sq_id = " + obj.Header.Id + ";";
                    var Results_data = await _connection.ExecuteAsync(RateUpdate);

                    await LogTransactionAsync(
                    id: obj.Header.Id,
                    branchId: obj.Header.BranchId,
                    orgId: obj.Header.OrgId,
                    actionType: "Update",
                    actionDescription: "Updated gas last updated price",
                    oldValue: null,
                    newValue: new { SQ_ID = obj.Header.Id },
                    tableName: "master_gascode",
                    userId: obj.Header.UserId
                );
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Price changed Successfully",
                        Status = true
                    };

                }
                else
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Price cannot be edited for this SQ.",
                        Status = false
                    };
                }
            }
            else
            {
                var oldHeader = await _connection.QueryFirstOrDefaultAsync<object>(
                "SELECT * FROM tbl_salesquatation_header WHERE Id = @Id",
                new { Id = obj.Header.Id }
            );

                Int32 Result = 0;
                const string headerSql = @"
        UPDATE tbl_salesquatation_header
        SET
            SQ_Nbr = @SQ_Nbr,
            Sys_SQ_Nbr = @Sys_SQ_Nbr,
            SQ_Type = @SQ_Type,
            SQ_Date = @SQ_Date,
            OrderedBy = @OrderedBy,
            Subject = @Subject,
            CustomerId = @CustomerId,
            MainAddress = @MainAddress,
            DeliveryAddress = @DeliveryAddress,
            PhoneNumber = @PhoneNumber,
            FaxNo = @FaxNo,
            Email = @Email,
            CustomerAttention = @CustomerAttention,
            Validity = @Validity,
            DeliveryTerms = @DeliveryTerms,
            SalesPersonContact = @SalesPersonContact,
            PaymentTerms = @PaymentTerms,
            PaymentMethod = @PaymentMethod,
            SalesPerson = @SalesPerson,
            SalesPersonEmail = @SalesPersonEmail,
         
            LastModifiedBy = @UserId,
            LastModifiedDate = now(),
            LastModifiedIP ='',
            IsActive = 1,
            OrgId = @OrgId,
            BranchId = @BranchId,
            IsSubmitted=@IsSubmit,
            DeliveryAddressId=@DeliveryAddressId,
            CustomerContactId=@CustomerContactId,
            TermsAndCond=@TermsAndCond,
            EffectiveFromDate=@EffectiveFromDate,
            IsReadyToPost=@IsReadyToPost,
IsSavedByDSO =@IsSavedByDSO,
            Qtn_Day=@Qtn_Day,
            Qtn_Month=@Qtn_Month,
            TBA=@TBA,
            IsSalesOrderSaved =@IsSalesOrderSaved,
            IsWithCustomer=@IsWithCustomer
            where ID =@Id;
            
            ";


                await _connection.ExecuteAsync(headerSql, obj.Header);

                await LogTransactionAsync(
                id: obj.Header.Id,
                branchId: obj.Header.BranchId,
                orgId: obj.Header.OrgId,
                actionType: "Update",
                actionDescription: "Updated Quotation Header",
                oldValue: oldHeader,
                newValue: obj.Header,
                tableName: "tbl_salesquatation_header",
                userId: obj.Header.UserId
            );


                int HeaderId = obj.Header.Id;
                var UpdateSeq = "update tbl_salesquatation_detail set isactive=0 where sq_id=" + HeaderId + ";update tbl_salesquatation_Cont_Operation set isactive=0 where SQ_ID=" + HeaderId + ";";
                Result = await _connection.ExecuteAsync(UpdateSeq, HeaderId);


                string ContactOperationQuery = "";
                if (obj.operation != null)
                {
                    foreach (var row in obj.operation)
                    {
                        if (row.Id == 0)
                        {
                            ContactOperationQuery += "INSERT INTO  `tbl_salesquatation_Cont_Operation`( `SQ_ID`,`ContactId`,`IsActive`,`CreatedDate`,`CreatedIP`)" +
                                "select " + HeaderId + "," + row.CustomerContactId + ",1,now(),'192'; ";
                        }
                        else
                        {
                            ContactOperationQuery += "update tbl_salesquatation_Cont_Operation set isactive=1 where id=" + row.Id + ";";
                        }
                    }
                    if (obj.operation.Count > 0)
                    {
                        Result = await _connection.ExecuteAsync(ContactOperationQuery);

                        foreach (var row in obj.operation)
                        {
                            object oldOperation = null;
                            if (row.Id > 0)
                            {
                                oldOperation = await _connection.QueryFirstOrDefaultAsync<object>(
                                    "SELECT * FROM tbl_salesquatation_Cont_Operation WHERE Id = @Id",
                                    new { Id = row.Id }
                                );
                            }

                            await LogTransactionAsync(
                                id: row.Id,
                                branchId: HeaderId,
                                orgId: obj.Header.OrgId,
                                actionType: row.Id == 0 ? "Insert" : "Update",
                                actionDescription: row.Id == 0 ? "Inserted Quotation Contact Operation" : "Updated Quotation Contact Operation",
                                oldValue: oldOperation,
                                newValue: row,
                                tableName: "tbl_salesquatation_Cont_Operation",
                                userId: obj.Header.UserId
                            );
                        }
                    }
                }



                const string detailSql = @"
        UPDATE tbl_salesquatation_detail
        SET
            GasCodeId = @GasCodeId,
            GasDescription = @GasDescription,
            Volume = @Volume,
            Po_No = @Po_No,
            Pressure = @Pressure,
            Qty = @Qty,
            UOM = @UOM,
            CurrencyId = @CurrencyId,
            UnitPrice = @UnitPrice,
            TotalPrice = @TotalPrice,
            ConvertedPrice = @ConvertedPrice,
            ConvertedCurrencyId = @ConvertedCurrencyId,
            IsActive = 1,
            Exchangerate=@Exchangerate
        WHERE Id = @Id ;";


                const string Insertsql = @"
                INSERT INTO tbl_salesquatation_detail 
                (SQ_ID, GasCodeId, GasDescription, Volume, Po_No, Pressure, Qty, UOM, CurrencyId, UnitPrice, TotalPrice, 
                 ConvertedPrice, ConvertedCurrencyId, IsActive,Exchangerate)
                VALUES 
                ( @SQ_ID  , @GasCodeId, @GasDescription, @Volume, @Po_No, @Pressure, @Qty, @UOM, @CurrencyId, @UnitPrice,  @TotalPrice, @ConvertedPrice, @ConvertedCurrencyId, 1,@Exchangerate);";


                foreach (var row in obj.Details)
                {
                    //row.SQ_ID = header.Id;
                    if (row.Id > 0)
                    {
                        var oldDetail = await _connection.QueryFirstOrDefaultAsync<object>(
                        "SELECT * FROM tbl_salesquatation_detail WHERE Id = @Id",
                        new { Id = row.Id }
                    );
                        await _connection.ExecuteAsync(detailSql, row);

                        await LogTransactionAsync(
                        id: row.Id,
                        branchId: HeaderId,
                        orgId: obj.Header.OrgId,
                        actionType: "Update",
                        actionDescription: "Updated Quotation Detail",
                        oldValue: oldDetail,
                        newValue: row,
                        tableName: "tbl_salesquatation_detail",
                        userId: obj.Header.UserId
                    );
                    }
                    else if (row.Id == 0)
                    {
                        row.SQ_ID = HeaderId;
                        await _connection.ExecuteAsync(Insertsql, row);

                        var detailLastId = await _connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID();");

                        await LogTransactionAsync(
                        id: detailLastId, 
                        branchId: HeaderId,
                        orgId: obj.Header.OrgId,
                        actionType: "Insert",
                        actionDescription: "Inserted Quotation Detail",
                        oldValue: null,
                        newValue: row,
                        tableName: "tbl_salesquatation_detail",
                        userId: obj.Header.UserId
                    );
                    }
                }

                if (obj.Header.IsSubmit == 1)
                {
                    string RateUpdate = "update master_gascode as a inner join tbl_salesquatation_detail as b on a.id = b.gascodeid and b.isactive = 1 inner join tbl_salesquatation_header as c on c.id = b.sq_id and c.isactive = 1 and ifnull(c.IsSubmitted,0)= 1 set a.LastUpdatedPrice = b.unitprice where b.sq_id = " + HeaderId + ";";
                    Result = await _connection.ExecuteAsync(RateUpdate);
                }

                Result = 1;

                if (Result == 0)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "",
                        Status = false
                    };
                }
                else
                {
                    if (obj.Header.IsSubmit == 0)
                    {
                        return new ResponseModel()
                        {
                            Data = null,
                            Message = "Update Successfully",
                            Status = true
                        };
                    }
                    else
                    {
                        return new ResponseModel()
                        {
                            Data = null,
                            Message = "Posted Successfully",
                            Status = true
                        };
                    }


                }
            }

        }
        catch (Exception Ex)
        {
            //  Logger.Instance.Error("Exception:", Ex);
            await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
            {
                ErrorMessage = Ex.Message,
                ErrorType = Ex.GetType().Name,
                StackTrace = Ex.StackTrace,
                Source = nameof(QuotationRepository),
                Method_Function = nameof(UpdateAsync),
                UserId = obj.Header.UserId,
                ScreenName = "Quotation",
                RequestData_Payload = JsonConvert.SerializeObject(obj)
            });
            return new ResponseModel()
            {
                Data = null,
                Message = "Something went wrong",
                Status = false
            };
        }



    }







    public async Task<object> GetAllAsync(Int32 sys_sqnbr, string from_date, string to_date, Int32 BranchId)
    {
        try
        {
            var param = new DynamicParameters();
            param.Add("opt", 1);
            param.Add("quotation_id", 0);
            param.Add("orgid", 0);
            param.Add("branchid", BranchId);
            param.Add("sys_sqnbr", sys_sqnbr);
            param.Add("from_date", from_date);
            param.Add("to_date", to_date);
            param.Add("userid", 0);
            param.Add("IPAddress", "");
            param.Add("Customer_Name", "");


            var List = await _connection.QueryAsync(Quotation.QuotationProcedure, param: param, commandType: CommandType.StoredProcedure);
            var Modellist = List.ToList();


            return new ResponseModel()
            {
                Data = Modellist,
                Message = "Success",
                Status = true
            };


        }
        catch (Exception Ex)
        {
            await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
            {
                ErrorMessage = Ex.Message,
                ErrorType = Ex.GetType().Name,
                StackTrace = Ex.StackTrace,
                Source = nameof(QuotationRepository),
                Method_Function = nameof(GetAllAsync),
                UserId = 0,
                ScreenName = "Quotation",                 
                RequestData_Payload = JsonConvert.SerializeObject(new
                {
                    sys_sqnbr, from_date, to_date, BranchId
                })
            });

            return new ResponseModel()
            {
                Data = null,
                Message = "Something went wrong",
                Status = false
            };
        }
    }



    public async Task<object> GetByIdAsync(int Id)
    {
        try
        {
            var param = new DynamicParameters();
            param.Add("@Opt", 2);
            param.Add("@quotation_id", Id);
            param.Add("@orgid", 0);
            param.Add("@branchid", 0);

            param.Add("@sys_sqnbr", 0);
            param.Add("@from_date", "");
            param.Add("@to_date", "");
            param.Add("@userid", 0);
            param.Add("@IPAddress", "");
            param.Add("@Customer_Name", "");


            var List = await _connection.QueryMultipleAsync(Quotation.QuotationProcedure, param: param, commandType: CommandType.StoredProcedure);
            dynamic Modellist = new List<object>();

            while (!List.IsConsumed)
            {
                dynamic nl = List.Read();
                Modellist.Add(nl);
            }


            return new ResponseModel()
            {
                Data = Modellist,
                Message = "Success",
                Status = true
            };


        }
        catch (Exception Ex)
        {
            await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
            {
                ErrorMessage = Ex.Message,
                ErrorType = Ex.GetType().Name,
                StackTrace = Ex.StackTrace,
                Source = nameof(QuotationRepository),
                Method_Function = nameof(GetByIdAsync),
                UserId = 0,
                ScreenName = "Quotation",
                RequestData_Payload = JsonConvert.SerializeObject(new
                {
                    Id
                })
            });
            return new ResponseModel()
            {
                Data = null,
                Message = "Something went wrong",
                Status = false
            };
        }
    }


    public async Task<object> GetBySqNoAsync(int unit)
    {
        try
        {
            var param = new DynamicParameters();
            param.Add("@Opt", 3);
            param.Add("@branchid", unit);
            param.Add("@orgid", 0);
            param.Add("@quotation_id", 0);

            param.Add("@sys_sqnbr", 0);
            param.Add("@from_date", "");
            param.Add("@to_date", "");
            param.Add("@userid", 0);
            param.Add("@IPAddress", "");
            param.Add("@Customer_Name", "");

            var data = await _connection.QueryFirstOrDefaultAsync(Quotation.QuotationProcedure, param: param, commandType: CommandType.StoredProcedure);

            return new ResponseModel()
            {
                Data = data,
                Message = "Success",
                Status = true
            };


        }
        catch (Exception Ex)
        {
            await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
            {
                ErrorMessage = Ex.Message,
                ErrorType = Ex.GetType().Name,
                StackTrace = Ex.StackTrace,
                Source = nameof(QuotationRepository),
                Method_Function = nameof(GetBySqNoAsync),
                UserId = 0,
                ScreenName = "Quotation",
                RequestData_Payload = JsonConvert.SerializeObject(new
                {
                    unit
                })
            });
            return new ResponseModel()
            {
                Data = null,
                Message = "Something went wrong",
                Status = false
            };
        }
    }
    public async Task<object> CopyAsync(int Id)
    {
        try
        {
            var param = new DynamicParameters();
            param.Add("@Opt", 4);
            param.Add("@quotation_id", Id);
            param.Add("@orgid", 0);
            param.Add("@branchid", 0);


            param.Add("@sys_sqnbr", 0);
            param.Add("@from_date", "");
            param.Add("@to_date", "");
            param.Add("@userid", 0);
            param.Add("@IPAddress", "");
            param.Add("@Customer_Name", "");

            var List = await _connection.QueryMultipleAsync(Quotation.QuotationProcedure, param: param, commandType: CommandType.StoredProcedure);
            dynamic Modellist = new List<object>();

            while (!List.IsConsumed)
            {
                dynamic nl = List.Read();
                Modellist.Add(nl);
            }


            return new ResponseModel()
            {
                Data = Modellist,
                Message = "Success",
                Status = true
            };


        }
        catch (Exception Ex)
        {

            await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
            {
                ErrorMessage = Ex.Message,
                ErrorType = Ex.GetType().Name,
                StackTrace = Ex.StackTrace,
                Source = nameof(QuotationRepository),
                Method_Function = nameof(CopyAsync),
                UserId = 0,
                ScreenName = "Quotation",
                RequestData_Payload = JsonConvert.SerializeObject(new
                {
                    Id
                })
            });
            return new ResponseModel()
            {
                Data = null,
                Message = "Something went wrong",
                Status = false
            };
        }
    }

    public async Task<object> DeleteAsync(int Id, int IsActive, int userid)
    {
        try
        {
            var oldvalue = await _connection.QueryAsync<object>($"select * from tbl_salesquatation_header where id = {Id}");
            string Message = "";
            var param = new DynamicParameters();
            param.Add("@Opt", IsActive == 0 ? 5 : 6);
            param.Add("@quotation_id", Id);
            param.Add("@orgid", 0);
            param.Add("@branchid", 0);


            param.Add("@sys_sqnbr", 0);
            param.Add("@from_date", "");
            param.Add("@to_date", "");
            param.Add("@userid", userid);
            param.Add("@IPAddress", "");
            param.Add("@Customer_Name", "");


            var Response = await _connection.QueryFirstOrDefaultAsync<int>(Quotation.QuotationProcedure, param: param, commandType: CommandType.StoredProcedure);

            var newHeader = await _connection.QueryFirstOrDefaultAsync<object>("SELECT * FROM tbl_salesquatation_header WHERE id = @Id", new { Id });

            await LogTransactionAsync(
               id: Id,
               branchId: 1,
               orgId: 1,
               actionType: "Update",
               actionDescription: "Updated Quotation Header",
               oldValue: oldvalue,
               newValue: newHeader,
               tableName: "tbl_salesquatation_header",
               userId: userid
           );
            if (IsActive == 0)
            {
                if (Response == 1)
                {
                    Message = "SQ was inactivated successfully";
                }
                else if (Response == 2)
                {
                    Message = "This SQ is already inactivated";
                }
                else if (Response == 3)
                {
                    Message = "SO is raised for this SQ";
                }

            }
            else
            {
                if (Response == 1)
                {
                    Message = "SQ was activated successfully";
                }
                else if (Response == 2)
                {
                    Message = "This SQ is already activated";
                }
                else if (Response == 3)
                {
                    Message = "SO is raised for this SQ";
                }

            }
            return new ResponseModel()
            {
                Data = Message,
                Message = "Success",
                Status = true
            };


        }
        catch (Exception Ex)
        {
            await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
            {
                ErrorMessage = Ex.Message,
                ErrorType = Ex.GetType().Name,
                StackTrace = Ex.StackTrace,
                Source = nameof(QuotationRepository),
                Method_Function = nameof(DeleteAsync),
                UserId = 0,
                ScreenName = "Quotation",
                RequestData_Payload = JsonConvert.SerializeObject(new
                {
                    Id, IsActive, userid
                })
            });
            return new ResponseModel()
            {
                Data = null,
                Message = "Something went wrong",
                Status = false
            };
        }
    }

    public async Task<object> Createcustomer(string CustomerName, int OrgId,int BranchId, int userid)
    {
        try
        {
            string Message = "";
            var param = new DynamicParameters();
            param.Add("@Opt", 7);
            param.Add("@quotation_id", 0);
            param.Add("@orgid", OrgId);
            param.Add("@branchid", BranchId);
            param.Add("@sys_sqnbr", 0);
            param.Add("@from_date", "");
            param.Add("@to_date", "");
            param.Add("@userid", userid);
            param.Add("@IPAddress", "");
            param.Add("@Customer_Name", CustomerName);


            var Response = await _connection.QueryFirstOrDefaultAsync<int>(Quotation.QuotationProcedure, param: param, commandType: CommandType.StoredProcedure);

            // Log transaction
            await LogTransactionAsync(
                id: Response,
                branchId: BranchId,
                orgId: OrgId,
                actionType: "Insert",
                actionDescription: "Added new Customer",
                oldValue: null,
                newValue: new { CustomerName = CustomerName, OrgId = OrgId, BranchId = BranchId, CreatedBy = userid },
                tableName: "tbl_salesquatation_header",
                userId: userid
            );

            if (Response > 0)
                {
                    Message = "Customer created successfully";
                }
                else if (Response == 0)
                {
                    Message = "The Customer is already exists";
                }
            return new ResponseModel()
            {
                Data = Response,
                Message = Message,
                Status = true
            };


        }
        catch (Exception Ex)
        {
            await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
            {
                ErrorMessage = Ex.Message,
                ErrorType = Ex.GetType().Name,
                StackTrace = Ex.StackTrace,
                Source = nameof(QuotationRepository),
                Method_Function = nameof(Createcustomer),
                UserId = 0,
                ScreenName = "Quotation",
                RequestData_Payload = JsonConvert.SerializeObject(new
                {
                    CustomerName, OrgId, BranchId, userid
                })
            });
            return new ResponseModel()
            {
                Data = null,
                Message = "Something went wrong",
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
            ModuleName = "Sales",
            ScreenName = "Quatation",
            UserId = userId,
            ActionType = actionType,
            ActionDescription = actionDescription,
            TableName = tableName,
            OldValue = oldValue != null ? JsonConvert.SerializeObject(oldValue) : null,
            NewValue = newValue != null ? JsonConvert.SerializeObject(newValue) : null,
            CreatedBy = userId ?? 0,
            OrgId = orgId,
            BranchId = branchId,
            DbLog = 2
        };

        await _transactionLogRepo.LogTransactionAsync(log);
    }
}

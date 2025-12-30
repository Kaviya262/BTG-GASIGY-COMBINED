using BackEnd.Country;
using BackEnd.Master;
using ClosedXML.Excel;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.Supplier;
using Core.Master.Transactionlog;
using Core.Models;
using Core.Procurement.PurchaseRequisition;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Mysqlx.Crud;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Infrastructure.Repositories
{
    public class SupplierMasterRepository : ISupplierMasterRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public SupplierMasterRepository(IUnitOfWorkDB4 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = userTransactionLogRepository;
        }

        public async Task<object> GetAllAsync(int branchid, int orgid, int supplierid, int cityid, int stateid, int suppliercategoryid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 1);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", supplierid);
                parameters.Add("city", cityid);
                parameters.Add("state", stateid);
                parameters.Add("suppliercategoryid", suppliercategoryid);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters,commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetAllAsync),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid, stateid, suppliercategoryid, cityid, supplierid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> AddAsync(SupplierMaster Obj)
        {
            try
            {
              
                string Message = "";
                Int32 Result = 0;               

                const string headerSql = @"
                       INSERT INTO master_supplier 
                       (SupplierCode, SupplierName, ShortName, SupplierCategoryId, Email, PhoneNo, WebSite, UENNO,
                        Bank1, Bank1_Code, Bank1_AccountNumber, Bank2, Bank2_Code, Bank2_AccountNumber, PajakPph_Perc, UEN_Number,
                        CreditLimit, SupplierBlockId, CountryId,  PostalCode, Address, CreatedBy, CreatedDate, CreatedIP,
                        IsActive, OrgId, BranchId,taxid,stateid,cityid,peymenttermid ,deliverytermid )
                        VALUES
                        (@SupplierCode, @SupplierName, @ShortName, @SupplierCategoryId, @Email, @PhoneNo, @WebSite, @UENNO,
                        @Bank1, @Bank1_Code, @Bank1_AccountNumber, @Bank2, @Bank2_Code, @Bank2_AccountNumber, @PajakPph_Perc, @UEN_Number,
                        @CreditLimit, @SupplierBlockId, @CountryId,  @PostalCode, @Address, @userid, now(),
                        '',@IsActive, @OrgId, @BranchId,@taxid,@stateid,@cityid, @paymenttermid, @deliverytermid);
                ";

                await _connection.ExecuteAsync(headerSql, Obj.Master);

                const string getLastInsertedIdSql = "SELECT LAST_INSERT_ID();";
                var insertedHeaderId = await _connection.QuerySingleAsync<int>(getLastInsertedIdSql);

                // Log transaction
                await LogTransactionAsync(
                    id: insertedHeaderId,
                    branchId: Obj.Master.BranchId,
                    orgId: Obj.Master.OrgId,
                    actionType: "Insert",
                    actionDescription: "Added new Supplier",
                    oldValue: null,
                    newValue: Obj,
                    tableName: "master_supplier",
                    userId: Obj.Master.userid
                );

                string detailsql = "";
                foreach (var row in Obj.Currency)
                {
                    row.SupplierId = insertedHeaderId;
                    detailsql += @"
                                INSERT INTO master_suppliercurrency (SupplierId, CurrencyId, CreatedBy, CreatedDate, CreatedIP, IsActive, OrgId, BranchId)
                                select " + row.SupplierId + "," + row.CurrencyId + "," + row.userid + ",NOW(),'',"+row.IsActive+"," + row.OrgId + "," + row.BranchId + "; ";

                    Result = await _connection.ExecuteAsync(detailsql);

                    string getCurrencyLastInsertedIdSql = "SELECT LAST_INSERT_ID();";
                    var insertedCurrencyId = await _connection.QuerySingleAsync<int>(getCurrencyLastInsertedIdSql);
                    // Log transaction
                    await LogTransactionAsync(
                        id: insertedCurrencyId,
                        branchId: row.BranchId,
                        orgId: row.OrgId,
                        actionType: "Insert",
                        actionDescription: "Added new Supplier Currency",
                        oldValue: null,
                        newValue: row,
                        tableName: "master_suppliercurrency",
                        userId: row.userid
                    );
                }


                return new ResponseModel()
                {                   
                    Message = "Saved Successfully" + Message,
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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(AddAsync),
                    UserId = Obj.Master.userid,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(Obj)
                });
                return new ResponseModel()
                {                    
                    Message = $"Error: {Ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> UpdateAsync(SupplierMaster Obj)
        {
            try
            {
                var oldHeader = await _connection.QueryAsync<object>($"select * from master_supplier where supplierid = {Obj.Master.SupplierId}");

                var oldCurrency = await _connection.QueryAsync<object>("SELECT * FROM master_suppliercurrency WHERE SupplierId = @SupplierId",new { Obj.Master.SupplierId });

                var oldvalue = new
                {
                    Header = oldHeader,
                    Currency = oldCurrency
                };

                const string headerSql = @"
                        UPDATE master_supplier
                SET
               
                SupplierCode = @SupplierCode,
                SupplierName = @SupplierName,
                ShortName = @ShortName,
                SupplierCategoryId = @SupplierCategoryId,
                Email = @Email,
                PhoneNo = @PhoneNo,
                WebSite = @WebSite,
                UENNO = @UENNO,
                Bank1 = @Bank1,
                Bank1_Code = @Bank1_Code,
                Bank1_AccountNumber = @Bank1_AccountNumber,
                Bank2 = @Bank2,
                Bank2_Code = @Bank2_Code,
                Bank2_AccountNumber = @Bank2_AccountNumber,
                PajakPph_Perc = @PajakPph_Perc,
                UEN_Number = @UEN_Number,
                CreditLimit = @CreditLimit,
                SupplierBlockId = @SupplierBlockId,
                CountryId = @CountryId,
                
                PostalCode = @PostalCode,
                Address = @Address,             
                LastModifiedBy = @userid,
                LastModifiedDate = NOW(),
                LastModifiedIP = '',
                IsActive = @IsActive,
                OrgId = @OrgId,
                BranchId = @BranchId,
                taxid = @taxid,
                cityid = @cityid,
                stateid = @stateid,
                peymenttermid  = @paymenttermid,
                deliverytermid  = @deliverytermid
                WHERE SupplierId = @SupplierId";


                await _connection.ExecuteAsync(headerSql, Obj.Master);

                // 2. Soft delete old detail rows
                const string softDeleteDetailsSql = @"UPDATE master_suppliercurrency SET IsActive = 0 WHERE SupplierId = @SupplierId;";
                await _connection.ExecuteAsync(softDeleteDetailsSql, new { SupplierId = Obj.Master.SupplierId });



                const string insertSql = @"
    INSERT INTO master_suppliercurrency
        (SupplierId, CurrencyId, CreatedBy, CreatedDate, CreatedIP,
         IsActive, OrgId, BranchId)
    SELECT @SupplierId, @CurrencyId, @CreatedBy, NOW(), '',
           @IsActive, @OrgId, @BranchId
    WHERE NOT EXISTS (
        SELECT 1
        FROM   master_suppliercurrency
        WHERE  SupplierId = @SupplierId
          AND  CurrencyId = @CurrencyId
    );
";

                // Always update the row if it already exists
                const string updateSql = @"
    UPDATE master_suppliercurrency
    SET    LastModifiedBy   = @ModifiedBy,
           LastModifiedDate = NOW(),
           IsActive         = @IsActive,
           OrgId            = @OrgId,
           BranchId         = @BranchId
    WHERE  SupplierId = @SupplierId
      AND  CurrencyId = @CurrencyId;
";

                // --- 2. Execute inside a loop -----------------------------------------

                int totalInserted = 0;
                int totalUpdated = 0;

                foreach (var row in Obj.Currency)
                {
                    var parameters = new
                    {
                        SupplierId = row.SupplierId,
                        CurrencyId = row.CurrencyId,
                        CreatedBy = row.userid,
                        ModifiedBy = row.userid,
                        IsActive = 1,
                        OrgId = row.OrgId,
                        BranchId = row.BranchId
                    };

                    // First try to insert (if not exists)
                    totalInserted += await _connection.ExecuteAsync(insertSql, parameters);

                    // Then update regardless
                    totalUpdated += await _connection.ExecuteAsync(updateSql, parameters);
                }
                //string updatequery = "";
                //foreach (var row in Obj.Currency)
                //{

                //    if (row.Id == 0)
                //    {
                //        updatequery += @" INSERT INTO master_suppliercurrency (SupplierId, CurrencyId, CreatedBy, CreatedDate, CreatedIP, IsActive, OrgId, BranchId)
                //                select " + row.SupplierId + "," + row.CurrencyId + "," + row.userid + ",NOW(),'',"+row.IsActive+"," + row.OrgId + "," + row.BranchId + "; ";
                //    }
                //    else
                //    {
                //        updatequery += @"update master_suppliercurrency set SupplierId = "+row.SupplierId+ " , CurrencyId = "+row.CurrencyId+ ",  LastModifiedBy = "+row.userid+ ",LastModifiedDate = NOW(),IsActive = 1 where CurrencyId="+ row.CurrencyId + " and SupplierId = " + row.SupplierId + " ;";
                //    }
                //}

                //Int32 Result = 0;
                //Result = await _connection.ExecuteAsync(updatequery);

                var newHeader = await _connection.QueryFirstOrDefaultAsync<object>(
           "SELECT * FROM master_supplier WHERE SupplierId = @SupplierId",
           new { Obj.Master.SupplierId });

                var newCurrency = await _connection.QueryAsync<object>(
                    "SELECT * FROM master_suppliercurrency WHERE SupplierId = @SupplierId",
                    new { Obj.Master.SupplierId });

                var newvalue = new
                {
                    Header = newHeader,
                    Currency = newCurrency
                };

                // Log transaction
                await LogTransactionAsync(
                    id: Obj.Master.SupplierId,
                    branchId: Obj.Master.BranchId,
                    orgId: Obj.Master.OrgId,
                    actionType: "Update",
                    actionDescription: "Update Supplier",
                    oldValue: oldvalue,
                    newValue: newvalue,
                    tableName: "master_supplier, master_suppliercurrency",
                    userId: Obj.Master.userid
                );

                return new ResponseModel
                {
                    Data = null,
                    Message = "Suppleir master updated successfully",                       
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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(UpdateAsync),
                    UserId = Obj.Master.userid,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(Obj)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
                    Status = false
                };
            }
            
        }


        public async Task<object> GetCountryList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 6);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state",  0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetCountryList),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid,
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetStateList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 4);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetStateList),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetCityList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 3);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetCityList),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetSupplierBlockList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 7);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetSupplierBlockList),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetSupplierCategoryList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 5);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetSupplierCategoryList),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid,orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetCurrencyList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 8);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetCurrencyList),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetSupplierList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 2);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetSupplierList),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }


        public async Task<object> GetAllTaxList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 9);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetAllTaxList),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid,
                        orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetAllVatList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 12);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetAllVatList),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetAllPaymentTerms(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 10);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetAllPaymentTerms),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetAllDeliveryTerms(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 11);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("supplierid", 0);
                parameters.Add("city", 0);
                parameters.Add("state", 0);
                parameters.Add("suppliercategoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterSupplierMaster.MasterSupplierProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(GetAllDeliveryTerms),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> UpdateSupplierStatus(int branchid, int orgid, int supplierid, bool isactive, int userid)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_supplier where supplierid = {supplierid}");

                const string headerSql = @"
                        UPDATE master_supplier
                        SET                      
                        IsActive = @isactive,
                        LastModifiedIP = '',
                        LastModifiedBy = @userid,
                        LastModifiedDate = NOW()                   
                        WHERE supplierid = @supplierid";

                var parameters = new
                {
                    isactive,
                    userid,
                    supplierid,
                    orgid,
                    branchid
                };
                await _connection.ExecuteAsync(headerSql, parameters);

                var newValue = await _connection.QueryFirstOrDefaultAsync<object>("SELECT * FROM master_supplier WHERE supplierid = @supplierid", new { supplierid });

                // Log transaction
                await LogTransactionAsync(
                    id: supplierid,
                    branchId: branchid,
                    orgId: orgid,
                    actionType: "Update",
                    actionDescription: "Update Supplier",
                    oldValue: oldvalue,
                    newValue: newValue,
                    tableName: "master_supplier",
                    userId: userid
                );

                return new ResponseModel
                {
                    Data = null,
                    Message = "supplier master status updated successfully",
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
                    Source = nameof(SupplierMasterRepository),
                    Method_Function = nameof(UpdateSupplierStatus),
                    UserId = 0,
                    ScreenName = "Supplier",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, orgid, supplierid, isactive, userid
                    })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
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
                ModuleName = "Master",
                ScreenName = "Supplier",
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


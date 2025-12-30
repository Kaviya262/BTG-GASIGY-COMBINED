using BackEnd.ProductionOrder;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Core.OrderMng.ProductionOrder;
 
using Dapper;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Mysqlx.Crud;
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
    public class ProductionRepository : IProductionRepository
    {

        private readonly IDbConnection _connection;
        string IPAddress = "";
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public ProductionRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = userTransactionLogRepository;
        }
            public async Task<object> AddAsync(ProductionItems Obj)
            {
                try
                {
                    int IsValidated = 0;
                    string Message = "";
                    Int32 Result = 0;
                    SharedRepository SR = new SharedRepository(_connection);

                    var response=await SR.GetSeqNumber(0, Obj.Header.ProdNo, 3, Obj.Header.BranchId, Obj.Header.OrgId);
                    if (response.Status==true)
                    {
                        if (response.Data.result == 1)
                        {
                            IsValidated = 1;
                            Message = " - The current order number " + Obj.Header.ProdNo + " is taken for another order so the new order number ("+response.Data.text+") has been generated for this order";
                            Obj.Header.ProdNo = response.Data.text;
                        }
                    }

                    const string headerSql = @"
                INSERT INTO tbl_productionorder_header(ProdNo,GasTypeId,GasCodeId,CreatedDate,CreatedIP,IsActive,IsSubmitted,CreatedBy,BranchId,OrgId,ProdDate)
                VALUES 
                (@ProdNo, @GasTypeId, @GasCodeId,  now(), '',1,@IsSubmitted, @UserId,@BranchId, @OrgId,@ProdDate);";

                    await _connection.ExecuteAsync(headerSql, Obj.Header);

                    const string getLastInsertedIdSql = "SELECT LAST_INSERT_ID();";
                    var insertedHeaderId = await _connection.QuerySingleAsync<int>(getLastInsertedIdSql);

                    // Log transaction
                    await LogTransactionAsync(
                        id: insertedHeaderId,
                        branchId: Obj.Header.BranchId,
                        orgId: Obj.Header.OrgId,
                        actionType: "Insert",
                        actionDescription: "Added new Production Order",
                        oldValue: null,
                        newValue: Obj.Header,
                        tableName: "tbl_productionorder_header",
                        userId: Obj.Header.UserId
                    );

                    Result = insertedHeaderId;

                    foreach (var row in Obj.Details)
                    {
                        row.Prod_ID = insertedHeaderId;
                        const string detailsql = @"INSERT INTO tbl_productionorder_details(Prod_ID,CylinderId,CylinderName,OwnershipId,GasCodeId,CylinderTypeId,TestedOn,NextTestDate,IsActive)
                    VALUES 
                    ( @Prod_ID  , @cylinderid, @cylindername, @ownershipid, @gascodeid, @cylindertypeid, @testedon, @nexttestdate, 1);";
                        Result = await _connection.ExecuteAsync(detailsql, row);

                        int productionOrderDetailsLastId = await _connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID();");

                    // Log transaction
                    await LogTransactionAsync(
                        id: productionOrderDetailsLastId,
                        branchId: Obj.Header.BranchId,
                        orgId: Obj.Header.OrgId,
                        actionType: "Insert",
                        actionDescription: "Added new Production Order Details",
                        oldValue: null,
                        newValue: Obj.Details,
                        tableName: "tbl_productionorder_details",
                        userId: Obj.Header.UserId
                    );

                        var oldCylinder = await _connection.QueryFirstOrDefaultAsync<object>("SELECT * FROM master_cylinder WHERE cylinderid = @cid",new { cid = row.cylinderid });

                    var UpdateCylinder = "update master_cylinder set statusid=1 where cylinderid=" + row.cylinderid;
                        Result = await _connection.ExecuteAsync(UpdateCylinder);

                    await LogTransactionAsync(
                        id: row.cylinderid,     
                        branchId: Obj.Header.BranchId,
                        orgId: Obj.Header.OrgId,
                        actionType: "Update",
                        actionDescription: "Updated Cylinder Status after Production",
                        oldValue: oldCylinder,
                        newValue: new { statusid = 1 },
                        tableName: "master_cylinder",
                        userId: Obj.Header.UserId
                    );

                }
                        int BranchId = Obj.Header.BranchId;
                        var UpdateSeq = "update master_documentnumber set Doc_Number= Doc_Number + 1 where Doc_Type= 3 and unit= @BranchId;";
                        Result = await _connection.ExecuteAsync(UpdateSeq, new { BranchId });
                
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
                        if (Obj.Header.IsSubmitted == 0)
                        {
                            return new ResponseModel()
                            {
                                Data = null,
                                Message = "Saved Successfully"+ Message,
                                Status = true
                            };
                        }
                        else
                        {
                            return new ResponseModel()
                            {
                                Data = null,
                                Message = "Posted Successfully"+ Message,
                                Status = true
                            };
                        }
                    }
                }
                catch (Exception Ex)
                {
                    await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                    {
                        ErrorMessage = Ex.Message,
                        ErrorType = Ex.GetType().Name,
                        StackTrace = Ex.StackTrace,
                        Source = nameof(ProductionRepository),
                        Method_Function = nameof(AddAsync),
                        UserId = Obj.Header.UserId,
                        ScreenName = "Production",
                        RequestData_Payload = JsonConvert.SerializeObject(Obj)
                    });
                    //Logger.Instance.Error("Exception:", Ex);
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Something went wrong",
                        Status = false
                    };
                }
            }

        public async Task<object> UpdateAsync(ProductionItems obj)
        {
            try
            {
                Int32 Result = 0;

                var oldHeader = await _connection.QueryFirstOrDefaultAsync<object>("SELECT * FROM tbl_productionorder_header WHERE Prod_ID = @Prod_ID", new { Prod_ID = obj.Header.Prod_ID });

                var oldDetails = new List<object>();

                foreach (var d in obj.Details)
                {
                    if (d.Prod_dtl_Id > 0)
                    {
                        var oldRow = await _connection.QueryFirstOrDefaultAsync<object>("SELECT * FROM tbl_productionorder_details WHERE Prod_dtl_Id = @DtlId", new { DtlId = d.Prod_dtl_Id });
                        oldDetails.Add(oldRow);
                    }
                    else
                    {
                        oldDetails.Add(null); 
                    }
                }

                const string headerSql = @"
        UPDATE tbl_productionorder_header
        SET
            GasTypeId = @GasTypeId,
            GasCodeId = @GasCodeId,
            ProdDate = @ProdDate,                     
            LastModifiedBY = @UserId,
            LastModifiedDate = now(),
            LastMOdifiedIP ='',
            IsSubmitted=@IsSubmitted
            where Prod_ID =@Prod_ID;
            ";

                await _connection.ExecuteAsync(headerSql, obj.Header);

                int HeaderId = obj.Header.Prod_ID;
                var UpdateSeq = "update tbl_productionorder_details set isactive=0 where Prod_ID=" + HeaderId;
                Result = await _connection.ExecuteAsync(UpdateSeq, HeaderId);

                const string detailSql = @"
                UPDATE tbl_productionorder_details
                SET
                CylinderId = @cylinderid,
                CylinderName = @cylindername,
                OwnershipId = @ownershipid,
                GasCodeId = @gascodeid,
                CylinderTypeId = @cylindertypeid,
                TestedOn = @testedon,
                NextTestDate = @nexttestdate,
                IsActive = 1
                 
                WHERE Prod_dtl_Id = @Prod_dtl_Id ;";


                const string Insertsql = @"INSERT INTO tbl_productionorder_details(Prod_ID, CylinderId, CylinderName, OwnershipId, GasCodeId, CylinderTypeId, TestedOn, NextTestDate, IsActive)
                VALUES
                (@Prod_ID, @cylinderid, @cylindername, @ownershipid, @gascodeid, @cylindertypeid, @testedon, @nexttestdate, 1); ";

                foreach (var row in obj.Details)
                {
                    //row.SQ_ID = header.Id;
                    if (row.Prod_dtl_Id > 0)
                    {
                        await _connection.ExecuteAsync(detailSql, row);
                    }
                    else if (row.Prod_dtl_Id == 0)
                    {
                        row.Prod_ID = HeaderId;
                        await _connection.ExecuteAsync(Insertsql, row);
                    }
                Result = 1;
                }

                const string updateCylinderSql = @"
        UPDATE master_cylinder AS mc
        INNER JOIN tbl_productionorder_details AS d
            ON mc.CylinderId = d.CylinderId
        SET mc.StatusId = 1
        WHERE d.Prod_ID = @Prod_ID AND d.IsActive = 1 AND d.CylinderId = @CylinderId;
        ";

                foreach (var row in obj.Details)
                {
                    if (row.cylinderid > 0)
                    {
                        await _connection.ExecuteAsync(updateCylinderSql, new
                        {
                            Prod_ID = HeaderId,
                            CylinderId = row.cylinderid
                        });
                    }
                }

                //var UpdateCylinder = "UPDATE master_cylinder AS b INNER JOIN tbl_productionorder_details AS g ON g.isactive=0  and g.prod_id=" + obj.Header.Prod_ID + " and b.cylinderid = g.CylinderId SET b.statusid = 1;";
                //Result = await _connection.ExecuteAsync(UpdateCylinder);
                //Result = 1;

                await LogTransactionAsync(
            id: HeaderId,
            branchId: obj.Header.BranchId,
            orgId: obj.Header.OrgId,
            actionType: "Update",
            actionDescription: "Updated Production Order",
            oldValue: new { Header = oldHeader, Details = oldDetails },
            newValue: obj,
            tableName: "tbl_productionorder_header / tbl_productionorder_details",
            userId: obj.Header.UserId
        );

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
                    if (obj.Header.IsSubmitted == 0)
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
            catch (Exception Ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = Ex.Message,
                    ErrorType = Ex.GetType().Name,
                    StackTrace = Ex.StackTrace,
                    Source = nameof(ProductionRepository),
                    Method_Function = nameof(UpdateAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "Production",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                //  Logger.Instance.Error("Exception:", Ex);
                return new ResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong",
                    Status = false
                };
            }
        }

        public async Task<object> GetAllAsync(Int32 ProdId, string from_date, string to_date, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@Opt", 2);
                param.Add("@ProdId", ProdId);
                param.Add("@orgid", 0);
                param.Add("@branchid", BranchId);                
                param.Add("@from_date", from_date);
                param.Add("@to_date", to_date);

                var List = await _connection.QueryAsync(ProductionOrder.ProductionProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(ProductionRepository),
                    Method_Function = nameof(GetAllAsync),
                    UserId = 0,
                    ScreenName = "Production",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        ProdId, from_date, to_date, BranchId
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
                param.Add("@Opt", 1);
                param.Add("@ProdId", Id);
                param.Add("@orgid", 0);
                param.Add("@branchid", 0);
                param.Add("@from_date", "");
                param.Add("@to_date", "");

                var List = await _connection.QueryMultipleAsync(ProductionOrder.ProductionProcedure, param: param, commandType: CommandType.StoredProcedure);

                dynamic Modellist = new ExpandoObject();
                int I = 0;
                while (!List.IsConsumed)
                {
                    dynamic nl = List.Read();

                    if (I == 0)
                    {
                        int count = nl.Count;
                        if (count == 0)
                        {
                            Modellist.Header = new object();
                        }
                        else
                        {
                            Modellist.Header = nl[0];
                        }
                    }
                    else if (I == 1)
                    {
                        Modellist.Detail = nl;
                    }
                    I++;
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
                    Source = nameof(ProductionRepository),
                    Method_Function = nameof(GetByIdAsync),
                    UserId = 0,
                    ScreenName = "Production",
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

        public async Task<object> GetByProductionOrderNoAsync(int unit)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@Opt", 3);
                param.Add("@ProdId", 0);
                param.Add("@orgid", 0);
                param.Add("@branchid", unit);
                param.Add("@from_date", "");
                param.Add("@to_date", "");

                var data = await _connection.QueryFirstOrDefaultAsync(ProductionOrder.ProductionProcedure, param: param, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ProductionRepository),
                    Method_Function = nameof(GetByProductionOrderNoAsync),
                    UserId = 0,
                    ScreenName = "Production",
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

        private async Task LogTransactionAsync(int id, int branchId, int orgId, string actionType, string actionDescription, object oldValue, object newValue, string tableName, int? userId = 0)
        {
            var log = new UserTransactionLogModel
            {
                TransactionId = id.ToString(),
                ModuleId = 1,
                ScreenId = 1,
                ModuleName = "Sales",
                ScreenName = "Production Order",
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
}

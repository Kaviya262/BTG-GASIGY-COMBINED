using BackEnd.Master;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.Item;
using Core.Master.Supplier;
using Core.Master.Transactionlog;
using Core.Models;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ItemMasterRepository : IItemMasterRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public ItemMasterRepository(IUnitOfWorkDB4 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = userTransactionLogRepository;
        }
        public async Task<object> GetAllAsync(int branchid, int orgid, int itemid, string itemcode, string itemname, int groupid, int categoryid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 1);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("itemid", itemid);
                parameters.Add("itemcode", itemcode);
                parameters.Add("itemname", itemname);
                parameters.Add("groupid", groupid);
                parameters.Add("categoryid", categoryid);

                var result = await _connection.QueryAsync<dynamic>(MasterItemMaster.MasterItemProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(GetAllAsync),
                    UserId = 0,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid, itemid, itemcode, itemname, groupid, categoryid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> AddAsync(ItemMaster Obj)
        {
            try
            {

                string Message = "";
               

                const string headerSql = @"
                      INSERT INTO master_item
                        (itemid,itemcode,itemname,description,categoryid,groupid,UOMID,locationid,IsActive,CreatedBy,CreatedIP,CreatedDate,orgid,branchid,TaxPerc,unitprice,VAT,sellingitemname)
                        VALUES
                        (@itemid,@itemcode,@itemname,@description,@categoryid,@groupid,@UOMID,@locationid,1,@userid,@CreatedIP,NOW(),@orgid,@branchid,@TaxPerc,@unitprice,@VAT,
                        @sellingitemname);
SELECT LAST_INSERT_ID();
                ";

               var result = await _connection.ExecuteScalarAsync<int>(headerSql, Obj.Master);

                // Log transaction
                await LogTransactionAsync(
                    id: result,
                    branchId: Obj.Master.BranchId,
                    orgId: Obj.Master.OrgId,
                    actionType: "Insert",
                    actionDescription: "Added new Item",
                    oldValue: null,
                    newValue: Obj,
                    tableName: "MasterItem",
                    userId: Obj.Master.userid
                );

                return new ResponseModel()
                {
                    Message = "Saved Successfully" + Message,
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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(AddAsync),
                    UserId = Obj.Master.userid,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(Obj)
                });
                return new ResponseModel()
                {
                    Message = $"Error: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> UpdateAsync(ItemMaster Obj)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_item where itemid = {Obj.Master.ItemId}");

                const string headerSql = @"
                        UPDATE master_item
                        SET
                        itemid = @itemid,
                        itemcode = @itemcode,
                        itemname = @itemname,
                        description = @description,
                        categoryid = @categoryid,
                        groupid = @groupid,
                        UOMID = @UOMID,
                        locationid = @locationid,
                        
                        LastModifiedIP = @LastModifiedIP,
                        LastModifiedBy = @userid,
                        LastModifiedDate = NOW(),
                        orgid = @orgid,
                        branchid = @branchid,
                        TaxPerc = @TaxPerc,
                        unitprice = @unitprice,
                        VAT = @VAT,
                        sellingitemname = @sellingitemname
                        WHERE itemid = @itemid";


                await _connection.ExecuteAsync(headerSql, Obj.Master);

                // Log transaction
                await LogTransactionAsync(
                    id: Obj.Master.ItemId,
                    branchId: Obj.Master.BranchId,
                    orgId: Obj.Master.OrgId,
                    actionType: "Update",
                    actionDescription: "Update Item",
                    oldValue: oldvalue,
                    newValue: Obj,
                    tableName: "MasterItem",
                    userId: Obj.Master.userid
                );


                return new ResponseModel
                {
                    Data = null,
                    Message = "Item master updated successfully",
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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(UpdateAsync),
                    UserId = Obj.Master.userid,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(Obj)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
                    Status = false
                };
            }

        }


        public async Task<object> GetItemCategoryList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 2);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("itemid", 0);
                parameters.Add("itemcode", null, DbType.String);
                parameters.Add("itemname", null, DbType.String);
                parameters.Add("groupid", 0);
                parameters.Add("categoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterItemMaster.MasterItemProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(GetItemCategoryList),
                    UserId = 0,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetItemGroupList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 3);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("itemid", 0);
                parameters.Add("itemcode", null, DbType.String);
                parameters.Add("itemname", null, DbType.String);
                parameters.Add("groupid", 0);
                parameters.Add("categoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterItemMaster.MasterItemProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(GetItemGroupList),
                    UserId = 0,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetUOMList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 4);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("itemid", 0);
                parameters.Add("itemcode", null, DbType.String);
                parameters.Add("itemname", null, DbType.String);
                parameters.Add("groupid", 0);
                parameters.Add("categoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterItemMaster.MasterItemProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(GetUOMList),
                    UserId = 0,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetItemCodeList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 5);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("itemid", 0);
                parameters.Add("itemcode", null, DbType.String);
                parameters.Add("itemname", null, DbType.String);
                parameters.Add("groupid", 0);
                parameters.Add("categoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterItemMaster.MasterItemProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(GetItemCodeList),
                    UserId = 0,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<object> GetItemNameList(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 6);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("itemid", 0);
                parameters.Add("itemcode", null, DbType.String);
                parameters.Add("itemname", null, DbType.String);
                parameters.Add("groupid", 0);
                parameters.Add("categoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterItemMaster.MasterItemProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(GetItemNameList),
                    UserId = 0,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }


        public async Task<object> UpdateItemStatus(int branchid,int orgid,int itemid,bool isactive,int userid)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_item where itemid = {itemid}");

                const string headerSql = @"
                        UPDATE master_item
                        SET                      
                        IsActive = @isactive,
                        LastModifiedIP = '',
                        LastModifiedBy = @userid,
                        LastModifiedDate = NOW()                                       
                        WHERE itemid = @itemid";

                var parameters = new
                {
                    isactive,                    
                    userid,
                    itemid,
                    orgid,
                    branchid
                };
                await _connection.ExecuteAsync(headerSql, parameters);

                    var newValue = await _connection.QueryFirstOrDefaultAsync<object>("SELECT * FROM master_item WHERE itemid = @itemid", new { itemid });

                // Log transaction
                await LogTransactionAsync(
                    id: itemid,
                    branchId: branchid,
                    orgId: orgid,
                    actionType: "Update",
                    actionDescription: "Update Item",
                    oldValue: oldvalue,
                    newValue: newValue,
                    tableName: "Masteritem",
                    userId: userid
                );

                return new ResponseModel
                {
                    Data = null,
                    Message = "Item master status updated successfully",
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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(UpdateItemStatus),
                    UserId = userid,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid, itemid, isactive, userid })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
                    Status = false
                };
            }

        }
        public async Task<object> UpdateActiveateAsync(int itemid, bool isactive)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_item where itemid = {itemid}");

                const string headerSql = @"
                UPDATE master_item
                SET
                IsActive = @isactive
                WHERE itemid = @itemid";

                var parameters = new
                {
                    isactive,
                    itemid
                };
                await _connection.ExecuteAsync(headerSql, parameters);

                var newValue = await _connection.QueryFirstOrDefaultAsync<object>("SELECT * FROM master_item WHERE itemid = @itemid", new { itemid });

                // Log transaction
                await LogTransactionAsync(
                    id: itemid,
                    branchId: 0,
                    orgId: 0,
                    actionType: "Update",
                    actionDescription: "Update Item",
                    oldValue: oldvalue,
                    newValue: newValue,
                    tableName: "Masteritem",
                    userId: 0
                );

                return new ResponseModel
                {
                    Data = null,
                    Message = "Item master updated successfully",
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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(UpdateActiveateAsync),
                    UserId = 0,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(new { itemid, isactive })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Something went wrong: " + ex.Message,
                    Status = false
                };
            }
        }

        public async Task<object> GetItemcodeSeqId(int branchid, int orgid)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("opt", 7);
                parameters.Add("branchid", branchid);
                parameters.Add("orgid", orgid);
                parameters.Add("itemid", 0);
                parameters.Add("itemcode", null, DbType.String);
                parameters.Add("itemname", null, DbType.String);
                parameters.Add("groupid", 0);
                parameters.Add("categoryid", 0);

                var result = await _connection.QueryAsync<dynamic>(MasterItemMaster.MasterItemProcedure, parameters, commandType: CommandType.StoredProcedure);

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
                    Source = nameof(ItemMasterRepository),
                    Method_Function = nameof(GetItemcodeSeqId),
                    UserId = 0,
                    ScreenName = "ItemMaster",
                    RequestData_Payload = JsonSerializer.Serialize(new { branchid, orgid })
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
                ScreenName = "Item",
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

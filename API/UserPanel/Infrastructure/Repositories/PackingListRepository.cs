using BackEnd.Master;
using BackEnd.PackingAndDO;
using BackEnd.Shared;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Core.OrderMng.Distribution.MasterSalesOrders;
using Core.OrderMng.Distribution.PackingList;
using Core.OrderMng.PackingAndDO;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Mysqlx.Crud;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PackingListRepository : IPackingListRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public PackingListRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = userTransactionLogRepository;
        }

        public async Task<object> GetAll(int? searchBy, int? customerId, int? gasCodeId, int? branchId)
        {
            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("p_searchBy", searchBy == 0 ? (int?)null : searchBy);
                parameters.Add("p_customerId", customerId == 0 ? (int?)null : customerId);
                parameters.Add("p_gasCodeId", gasCodeId == 0 ? (int?)null : gasCodeId);
                parameters.Add("p_branchId", branchId == 0 ? (int?)null : branchId);


                var list = await _connection.QueryAsync(
                    "GetPDLDetailsList",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var modelList = list.ToList();

                return new ResponseModel
                {
                    Data = modelList,
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
                    Source = nameof(PackingListRepository),
                    Method_Function = nameof(GetAll),
                    UserId = 0,
                    ScreenName = "PackingList",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        searchBy, customerId, gasCodeId, branchId,
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
        public async Task<object> AddAsync(string barcodes, int packingDetailsId,int packingId, int rackId, bool isSubmitted,int userId,string packNo)
        {
            try
            {
                var oldHeader = await _connection.QueryFirstOrDefaultAsync<object>(
            "SELECT * FROM tbl_packing_header WHERE id = @PackingId",
            new { PackingId = packingId }
        );

                var oldDeliveryDetails = await _connection.QueryAsync<object>(
                    "SELECT * FROM tbl_packing_deliverydetail WHERE PakingDetailsId = @PackingDetailsId",
                    new { PackingDetailsId = packingDetailsId }
                );

                var parameters = new DynamicParameters();
                parameters.Add("pBarcodes", barcodes, DbType.String);
                parameters.Add("pPackingDetailsId", packingDetailsId, DbType.Int32);
                parameters.Add("pPackingId", packingId, DbType.Int32);
                parameters.Add("pRackId", rackId, DbType.Int32);
                parameters.Add("pIsBarCodeAllocated", isSubmitted ? 1 : 0, DbType.Boolean);

                await _connection.ExecuteAsync(
                    "proc_SavePackingBarcodes",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var newHeader = await _connection.QueryFirstOrDefaultAsync<object>(
           "SELECT * FROM tbl_packing_header WHERE id = @PackingId",
           new { PackingId = packingId }
       );

                var newDeliveryDetails = await _connection.QueryAsync<object>(
                    "SELECT * FROM tbl_packing_deliverydetail WHERE PakingDetailsId = @PackingDetailsId",
                    new { PackingDetailsId = packingDetailsId }
                );

                await LogTransactionAsync(
             id: packingId,
             branchId: 1, 
             orgId: 1,    
             actionType: "Update",
             actionDescription: "Updated RackId for packing header",
             oldValue: oldHeader,
             newValue: newHeader,
             tableName: "tbl_packing_header",
             userId: userId
         );

                // 🔹 Log each delivery detail change
                foreach (var oldDetail in oldDeliveryDetails)
                {
                    var newDetail = newDeliveryDetails.FirstOrDefault(d => ((dynamic)d).id == ((dynamic)oldDetail).id);
                    if (newDetail != null)
                    {
                        await LogTransactionAsync(
                            id: Convert.ToInt32(((dynamic)newDetail).id),
                            branchId: 1,
                            orgId: 1,
                            actionType: "Update",
                            actionDescription: "Updated Barcode allocation",
                            oldValue: oldDetail,
                            newValue: newDetail,
                            tableName: "tbl_packing_deliverydetail",
                            userId: userId
                        );
                    }
                }

                string message = "Barcodes saved successfully.";
                object downloadResult = null;

                if (isSubmitted)
                {
                    var result = await DownloadDO(packingId);

                    if (result is ResponseModel res && res.Status)
                    {
                        downloadResult = res.Data;
                        message = "Barcodes saved and DO downloaded successfully.";
                    }
                    else
                    {
                        message = "Barcodes saved, but DO download failed.";
                    }
                }

                return new ResponseModel
                {
                    Status = true,
                    Message = message,
                    Data = downloadResult
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(PackingListRepository),
                    Method_Function = nameof(AddAsync),
                    UserId = userId,
                    ScreenName = "PackingList",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        barcodes, packingDetailsId, packingId, rackId, isSubmitted, userId, packNo
                    })
                });
                return new ResponseModel
                {
                    Status = false,
                    Message = "Something went wrong: " + ex.Message,
                    Data = null
                };
            }
        }


        public async Task<object> GetAllExportAsync(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 6);
                param.Add("@branchid", BranchId);
                var List = await _connection.QueryAsync(PackingAndDO.PackingAndDOProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(PackingListRepository),
                    Method_Function = nameof(GetAllExportAsync),
                    UserId = 0,
                    ScreenName = "PackingList",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId,
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

        public async Task<object> DownloadDO(int Id)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 2);
                param.Add("@packing_id", Id);
                param.Add("@userid", 0);
                param.Add("@branchid", 0);

                param.Add("@sys_packnbr", 0);
                param.Add("@from_date", "");
                param.Add("@to_date", "");
                param.Add("@DOID", "");
                param.Add("@FilterGasCodeId", 0);

                param.Add("@filter_customerid", 0);
                param.Add("@filter_esttime", "0");
                param.Add("@filter_packerid", 0);


                var List = await _connection.QueryMultipleAsync(PackingAndDO.PackingAndDOProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(PackingListRepository),
                    Method_Function = nameof(DownloadDO),
                    UserId = 0,
                    ScreenName = "PackingList",
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

        public async Task<object> GetByIdBarcode(int BranchId, int PackingId, int PackingDetailsId)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("BranchId", BranchId);
                parameters.Add("PackingId", PackingId);
                parameters.Add("PackingDetailsId", PackingDetailsId);

                var list = await _connection.QueryAsync(
                    "proc_GetBarcodePrefillDetails",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var modelList = list.ToList();

                return new ResponseModel
                {
                    Data = modelList,
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
                    Source = nameof(PackingListRepository),
                    Method_Function = nameof(AddAsync),
                    UserId = 0,
                    ScreenName = "PackingList",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, PackingId, PackingDetailsId
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
                ModuleName = "Sales",
                ScreenName = "Packing List",
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

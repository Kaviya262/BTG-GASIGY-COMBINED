using BackEnd.Master;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Models;
using Core.OrderMngMaster.Common;
using Dapper;
using Newtonsoft.Json;
using System.Data;
using System.Dynamic;

namespace Infrastructure.Repositories
{
    public class OrderMngMasterRepository : IOrderMngMasterRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbConnection _masterDBConnection;
        private readonly IErrorLogMasterRepository _errorLogRepo;

        public OrderMngMasterRepository(IUnitOfWorkDB1 unitOfWork, IUnitOfWorkDB4 masterDBConnection, IErrorLogMasterRepository errorLogMasterRepository)
        {
            _connection = unitOfWork.Connection;
            _masterDBConnection = masterDBConnection.Connection;
            _errorLogRepo = errorLogMasterRepository;
        }


        public async Task<object> GetQuotationTypeAsync(Int32 branchid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 1);
                param.Add("@branchid", branchid);


                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetQuotationTypeAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid
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
        public async Task<object> GetCustomerContactAsync(Int32 customerid, int Sqid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 2);
                param.Add("@customerid", customerid);

                param.Add("@BranchId", 0);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);

                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", Sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCustomerContactAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        customerid, Sqid
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
        public async Task<object> GetCustomerAddressAsync(Int32 contactid, int Sqid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 3);
                param.Add("@contactid", contactid);

                param.Add("@BranchId", 0);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@sqid", Sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCustomerAddressAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        contactid, Sqid
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
        public async Task<object> GetCustomerAsync(Int32 branch, int Sqid, string searchtext)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 4);
                param.Add("@branchid", branch);
                param.Add("@searchtext", searchtext);


                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", Sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCustomerAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branch, Sqid, searchtext
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

        public async Task<object> GetPaymentTermAsync(Int32 branch, int Sqid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 5);
                param.Add("@branchid", branch);


                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", Sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPaymentTermAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branch, Sqid
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

        public async Task<object> GetGasCodeAsync(Int32 branch, int Sqid, string searchtext)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 6);
                param.Add("@branchid", branch);
                param.Add("@searchtext", searchtext);


                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", Sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetGasCodeAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branch, searchtext, Sqid
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

        public async Task<object> GetUOMAsync(Int32 branch, int Sqid, string searchtext)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 7);
                param.Add("@branchid", branch);
                param.Add("@searchtext", searchtext);


                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", Sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetUOMAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branch, searchtext, Sqid
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
        public async Task<object> GetCurrencyAsync(Int32 branch, int Sqid, string searchtext)
        
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 8);
                param.Add("@branchid", branch);
                param.Add("@searchtext", searchtext);


                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", Sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCurrencyAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branch, searchtext, Sqid
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
        public async Task<object> GetGasCodeDetailsAsync(Int32 GasCodeId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 9);
                param.Add("@GasCodeId", GasCodeId);

                param.Add("@BranchId", 0);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetGasCodeDetailsAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        GasCodeId
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
        public async Task<object> GetCurrencyConversionAsync(Int32 currencyid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 10);
                param.Add("@currencyid", currencyid);
                param.Add("@BranchId", 0);
                param.Add("@searchtext", "");
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCurrencyConversionAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        currencyid
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
        public async Task<object> GetSalesPersonAsync(Int32 BranchId, int Sqid, string SearchText)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 11);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", Sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetSalesPersonAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, SearchText, Sqid
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
        public async Task<object> GetPaymentMethodAsync(Int32 BranchId, int Sqid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 12);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", Sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPaymentMethodAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, Sqid
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
        public async Task<object> GetSQNumberAsync(Int32 BranchId, string SearchText)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 13);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0); param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetSQNumberAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, SearchText
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
        public async Task<object> GetSOTypeAsync(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 14);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetSOTypeAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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
        public async Task<object> GetSQCustomerAsync(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 15);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetSQCustomerAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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
        public async Task<object> GetCustomerSQAsync(Int32 customerid, Int32 BranchId, int soid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 16);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", customerid);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", soid);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCustomerSQAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        customerid, BranchId, soid
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
        public async Task<object> GetSQGasCodeAsync(Int32 sqid, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 17);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetSQGasCodeAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        sqid, BranchId
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
        public async Task<object> GetCustomerGasCodeAsync(Int32 customerid, Int32 GasCodeId, Int32 BranchId, Int32 SOID)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 18);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", GasCodeId);
                param.Add("@customerid", customerid);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", SOID);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCustomerGasCodeAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, GasCodeId, customerid, SOID
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



        public async Task<object> GetCustomerGasCodeDetailAsync(Int32 customerid, Int32 BranchId, Int32 soid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 19);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", customerid);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", soid);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCustomerGasCodeDetailAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        customerid, BranchId, soid
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
        public async Task<object> GetPackingCustomerIdAsync(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 20);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPackingCustomerIdAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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


        public async Task<object> GetCustomerPackingIdAsync(Int32 customerid, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 21);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", customerid);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPackingCustomerIdAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        customerid, BranchId
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

        public async Task<object> GetGasTypes(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 22);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetGasTypes),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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

        public async Task<object> GetGasCodeAgGasTypes(Int32 GasTypeId, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 23);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", GasTypeId);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetGasCodeAgGasTypes),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        GasTypeId, BranchId
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

        public async Task<object> GetCylinderDetails(string SearchText, Int32 BranchId, int GasCodeId, int ProductionId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 24);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", GasCodeId);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", ProductionId);

                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCylinderDetails),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        SearchText, BranchId, GasCodeId, ProductionId
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
        public async Task<object> GetProductionNo(string SearchText, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 25);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetProductionNo),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        SearchText, BranchId
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
        public async Task<object> GetPackingDetails(int packingid, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 26);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", packingid);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPackingDetails),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        packingid, BranchId
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

        public async Task<object> GetsaleordercustomerId(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 27);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetsaleordercustomerId),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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




        public async Task<object> GetPackersName(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 28);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPackersName),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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

        public async Task<object> GetDriversName(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 50);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetDriversName),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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


        public async Task<object> GetPackingSO(Int32 customerid, Int32 BranchId, Int32 Packingid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 29);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", customerid);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", Packingid);

                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPackingSO),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        customerid, BranchId, Packingid
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
        public async Task<object> GetPackingSODetail(Int32 soid, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 30);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);

                param.Add("@soid", soid);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPackingSODetail),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        soid, BranchId
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
        public async Task<object> GetReturnOrderCylinderDetails(string SearchText, Int32 BranchId, int GasCodeId, int ProductionId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 31);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", GasCodeId);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", ProductionId);

                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetReturnOrderCylinderDetails),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        SearchText, GasCodeId, BranchId, ProductionId
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
        public async Task<object> GetReturnOrderCustomerPackingIdAsync(Int32 customerid, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 32);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", customerid);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetReturnOrderCustomerPackingIdAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, customerid
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

        public async Task<object> GetDeliveryAgGasDetails(Int32 GasCodeId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 33);
                param.Add("@BranchId", 0);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", GasCodeId);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetDeliveryAgGasDetails),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        GasCodeId
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
        public async Task<object> GetDeliveryAgDODetails(Int32 PackingId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 34);
                param.Add("@BranchId", 0);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", PackingId);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetDeliveryAgDODetails),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        PackingId
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
        public async Task<object> GetReturnOrderGasCode(Int32 BranchId, string SearchText, int CustomerId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 35);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", CustomerId);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetReturnOrderGasCode),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        CustomerId, SearchText, BranchId
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
        public async Task<object> GetSoHistory(Int32 BranchId, Int32 sqid, Int32 soid, Int32 GasCodeId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 36);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", GasCodeId);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", sqid);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", soid);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetSoHistory),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, GasCodeId, sqid, soid
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

        public async Task<object> GetBarcodeDetails(string Barcode, Int32 PackingId, Int32 doid, Int32 UomId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 1);
                param.Add("@barcode", Barcode);
                param.Add("@packingid", PackingId);

                param.Add("@BarcodeDtlid", 0);
                param.Add("@UserId", 0);
                param.Add("@doid", doid);
                param.Add("@rackid", 0);
                param.Add("@uomid", UomId);

                var List = await _connection.QueryAsync(OrderMngMaster.BarcodeProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetBarcodeDetails),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        Barcode, PackingId, doid
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

        public async Task<object> GetPressure(string SearchText, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 37);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPressure),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        SearchText, BranchId
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

        public async Task<object> GetVolume(string SearchText, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@opt", 38);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetVolume),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, SearchText
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
        public async Task<object> IsAdminUser(string UserId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@userid", UserId);
                param.Add("@opt", 1);


                var List = await _connection.QueryFirstOrDefaultAsync(OrderMngMaster.AutentiationProedure, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List;
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(IsAdminUser),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        UserId
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
        #region GetCylinderSize
        public async Task<object> GetCylinderSize(int BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@BranchId", BranchId);
                var List = await _connection.QueryAsync(OrderMngMaster.GetCylinderSizeByBranch, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCylinderSize),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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

        #endregion

        #region GetContactName
        public async Task<object> GetContactName(int CustomerId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@in_CustomerId", CustomerId);
                var List = await _connection.QueryAsync(OrderMngMaster.GetCustomerContactsLookup, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetContactName),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        CustomerId
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

        #endregion

        #region GetGasCode
        public async Task<object> GetGasCode(int BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@BranchId", BranchId);
                var List = await _connection.QueryAsync(OrderMngMaster.GetGasCodeByBranch, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetGasCode),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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

        #endregion

        #region GetPalletType
        public async Task<object> GetPalletType(int brachId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@in_branchid", brachId);
                var List = await _connection.QueryAsync(OrderMngMaster.MastergetPalletType, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPalletType),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        brachId
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

        #endregion
        public async Task<object> GetSOGasCodeDetails(Int32 SOID)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@opt", 41);
                param.Add("@BranchId", 0);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", SOID);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetSOGasCodeDetails),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        SOID
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

        public async Task<object> GetStagedata(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@opt", 42);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetStagedata),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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
        public async Task<object> GetgasCodeData(Int32 BranchId, string SearchText)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@opt", 43);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetgasCodeData),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, SearchText
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
        public async Task<object> GetRackDetails(Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@opt", 44);
                param.Add("@BranchId", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetRackDetails),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId
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

        public async Task<object> Getgascodeagainstcustomer(Int32 CustomerId, Int32 BranchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 46);
                param.Add("@branchid", BranchId);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", CustomerId);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryMultipleAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
                dynamic Modellist = new ExpandoObject();

                int I = 0;
                while (!List.IsConsumed)
                {
                    dynamic nl = List.Read();

                    if (I == 0)
                    {
                        Modellist.Gas = nl;
                    }
                    else if (I == 1)
                    {
                        Modellist.SO = nl;
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(Getgascodeagainstcustomer),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, CustomerId
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
        public async Task<object> GetSOagainstGas(Int32 GasCodeId, Int32 branchid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 47);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", "");
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", GasCodeId);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetSOagainstGas),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        branchid, GasCodeId
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

        public async Task<object> GetCustomerFilter(Int32 BranchId, string SearchText)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 51);
                param.Add("@branchid", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetCustomerFilter),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, SearchText
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

        public async Task<object> GetPackerList(Int32 BranchId, string SearchText)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 52);
                param.Add("@branchid", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetPackerList),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, SearchText
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
        public async Task<object> GetTruckName(Int32 BranchId, string SearchText)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 53);
                param.Add("@branchid", BranchId);
                param.Add("@searchtext", SearchText);
                param.Add("@CurrencyId", 0);
                param.Add("@GasCodeId", 0);
                param.Add("@customerid", 0);
                param.Add("@orgid", 0);
                param.Add("@contactid", 0);
                param.Add("@sqid", 0);
                param.Add("@gastypeid", 0);
                param.Add("@packingid", 0);
                param.Add("@soid", 0);
                param.Add("@Productionid", 0);
                var List = await _connection.QueryAsync(OrderMngMaster.OrderManagementMasterProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetTruckName),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        BranchId, SearchText
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

        public async Task<object> GetGasCodePalletAsync(int gasCodeId, int palletTypeId, int branchId, int palletId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@p_GasCodeId", gasCodeId);
                param.Add("@p_PalletTypeId", palletTypeId);
                param.Add("@p_BranchId", branchId);
                param.Add("@p_PalletId", palletId);

                var result = await _connection.QueryAsync("proc_gascodepallet", param, commandType: CommandType.StoredProcedure);

                return new ResponseModel
                {
                    Data = result.ToList(),
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetGasCodePalletAsync),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        gasCodeId, palletTypeId, branchId, palletId
                    })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Something went wrong",
                    Status = false
                };
            }
        }

        public async Task<object> GetBarcodePackingList(string Barcode, int packingId, string userId, int branchId, int? packingDetailsId, int? deliveryDetailRefId, int? packerId,  int? customerId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@barcodes", Barcode);
                param.Add("@PackingId", packingId);
                param.Add("@UserId", userId);
                param.Add("@BranchId", branchId);
                param.Add("@PackingDetailsId", packingDetailsId);
                param.Add("@PackerId", packerId);

                var list = await _connection.QueryAsync(OrderMngMaster.BarcodePacking, param: param, commandType: CommandType.StoredProcedure);
                var modelList = list;

                return new ResponseModel()
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetBarcodePackingList),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        Barcode, packingId, userId, branchId, packingDetailsId, deliveryDetailRefId, packerId, customerId
                    })
                });
                // Log ex if possible
                return new ResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong",
                    Status = false
                };
            }
        }


        public async Task<object> GetBank(string userId, int branchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@p_userid", userId);
                param.Add("@p_branchid", branchId);
                param.Add("@p_orgid", branchId);

                var list = await _masterDBConnection.QueryAsync(OrderMngMaster.GetBank, param: param, commandType: CommandType.StoredProcedure);
                var modelList = list.ToList();

                return new ResponseModel()
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
                    Source = nameof(OrderMngMasterRepository),
                    Method_Function = nameof(GetBank),
                    UserId = 0,
                    ScreenName = "OrderMngMaster",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        userId, branchId
                    })
                });
                // Log ex if possible
                return new ResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong",
                    Status = false
                };
            }
        }

    }

}


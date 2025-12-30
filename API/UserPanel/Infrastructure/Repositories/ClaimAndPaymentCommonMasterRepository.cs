using Application.Procurement.Master.Common;
using BackEnd.Finance;
using BackEnd.Master;
using BackEnd.Procurement;
using Core.Abstractions;
using Core.Finance.Master;
using Core.Master.ErrorLog;
using Core.Models;
using Core.Procurement.Master;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ClaimAndPaymentCommonMasterRepository : IClaimAndPaymentCommonMasterRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;

        public ClaimAndPaymentCommonMasterRepository(IUnitOfWorkDB3 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
        }

        public async Task<object> GetCategoryDetails(Int32 id, Int32 branchid, string Searchtext, Int32 orgid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 4);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", Searchtext);
                param.Add("@pmid", 0);
                param.Add("@prid", 0);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@categoryid", 0);
                param.Add("@claimtype_id", 0);
                param.Add("@supplier_id", 0);
                var List = await _connection.QueryAsync(ClaimAndPaymentMasterDB.ClaimAndPayment, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List.ToList();
                return new ResponseModel()
                {
                    Data = Modellist,
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
                    Source = nameof(ClaimAndPaymentCommonMasterRepository),
                    Method_Function = nameof(GetCategoryDetails),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, branchid, Searchtext, orgid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error retrieving category details: {ex.Message}",
                    Status = false
                };
            }
        }
        public async Task<object> GetDepartMentDetails(Int32 id, Int32 branchid, string Searchtext, Int32 orgid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 2);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", Searchtext);
                param.Add("@pmid", 0);
                param.Add("@prid", 0);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@categoryid", 0);
                param.Add("@claimtype_id", 0);
                param.Add("@supplier_id", 0);
                var List = await _connection.QueryAsync(ClaimAndPaymentMasterDB.ClaimAndPayment, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List.ToList();
                return new ResponseModel()
                {
                    Data = Modellist,
                    Message = "Success",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimAndPaymentCommonMasterRepository),
                    Method_Function = nameof(GetDepartMentDetails),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, branchid, Searchtext, orgid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error retrieving department details: {ex.Message}",
                    Status = false
                };
            }
        }
        public async Task<object> GetApplicantDetails(Int32 id, Int32 branchid, string Searchtext, Int32 orgid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 1);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", Searchtext);
                param.Add("@pmid", 0);
                param.Add("@prid", 0);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@categoryid", 0);
                param.Add("@claimtype_id", 0);
                param.Add("@supplier_id", 0);
                var List = await _connection.QueryAsync(ClaimAndPaymentMasterDB.ClaimAndPayment, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List.ToList();
                return new ResponseModel()
                {
                    Data = Modellist,
                    Message = "Success",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimAndPaymentCommonMasterRepository),
                    Method_Function = nameof(GetApplicantDetails),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, branchid, Searchtext, orgid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error retrieving applicant details: {ex.Message}",
                    Status = false
                };
            }
        }
        public async Task<object> GetTransactionCurrency(Int32 id, Int32 branchid, string Searchtext, Int32 orgid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 3);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", Searchtext);
                param.Add("@pmid", 0);
                param.Add("@prid", 0);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@categoryid", 0);
                param.Add("@claimtype_id", 0);
                param.Add("@supplier_id", 0);
                var List = await _connection.QueryAsync(ClaimAndPaymentMasterDB.ClaimAndPayment, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List.ToList();
                return new ResponseModel()
                {
                    Data = Modellist,
                    Message = "Success",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimAndPaymentCommonMasterRepository),
                    Method_Function = nameof(GetTransactionCurrency),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, branchid, Searchtext, orgid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error retrieving transaction currency: {ex.Message}",
                    Status = false
                };
            }
        }
        public async Task<object> GetClaimType(Int32 id, Int32 branchid, string Searchtext, Int32 orgid, Int32 categoryid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 5);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", Searchtext);
                param.Add("@pmid", 0);
                param.Add("@prid", 0);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@categoryid", categoryid);
                param.Add("@claimtype_id", 0);
                param.Add("@supplier_id", 0);
                var List = await _connection.QueryAsync(ClaimAndPaymentMasterDB.ClaimAndPayment, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List.ToList();
                return new ResponseModel()
                {
                    Data = Modellist,
                    Message = "Success",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimAndPaymentCommonMasterRepository),
                    Method_Function = nameof(GetClaimType),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, branchid, Searchtext, orgid, categoryid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error retrieving claim type: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> GetPaymentDescription(Int32 id, Int32 branchid, string Searchtext, Int32 orgid, Int32 claimtypeid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 6);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", Searchtext);
                param.Add("@pmid", 0);
                param.Add("@prid", 0);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@categoryid", 0);
                param.Add("@claimtype_id", claimtypeid);
                param.Add("@supplier_id", 0);
                var List = await _connection.QueryAsync(ClaimAndPaymentMasterDB.ClaimAndPayment, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List.ToList();
                return new ResponseModel()
                {
                    Data = Modellist,
                    Message = "Success",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimAndPaymentCommonMasterRepository),
                    Method_Function = nameof(GetPaymentDescription),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, branchid, Searchtext, orgid, claimtypeid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error retrieving payment description: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> GetSupplierList(Int32 id, Int32 branchid, string Searchtext, Int32 orgid, Int32 claimtypeid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 7);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", Searchtext);
                param.Add("@pmid", 0);
                param.Add("@prid", 0);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@categoryid", 0);
                param.Add("@claimtype_id", claimtypeid);
                param.Add("@supplier_id", 0);
                var List = await _connection.QueryAsync(ClaimAndPaymentMasterDB.ClaimAndPayment, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List.ToList();
                return new ResponseModel()
                {
                    Data = Modellist,
                    Message = "Success",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimAndPaymentCommonMasterRepository),
                    Method_Function = nameof(GetSupplierList),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, branchid, Searchtext, orgid, claimtypeid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error retrieving supplier list: {ex.Message}",
                    Status = false
                };
            }
        }
        public async Task<object> GetAllClaimList(Int32 id, Int32 branchid, string Searchtext, Int32 orgid, Int32 claimtypeid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 8);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", Searchtext);
                param.Add("@pmid", 0);
                param.Add("@prid", 0);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@categoryid", 0);
                param.Add("@claimtype_id", 0);
                param.Add("@supplier_id", 0);
                

                var List = await _connection.QueryAsync(ClaimAndPaymentMasterDB.ClaimAndPayment, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List.ToList();
                return new ResponseModel()
                {
                    Data = Modellist,
                    Message = "Success",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimAndPaymentCommonMasterRepository),
                    Method_Function = nameof(GetAllClaimList),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, branchid, Searchtext, orgid, claimtypeid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error retrieving claim list: {ex.Message}",
                    Status = false
                };
            }
        }

        public async Task<object> GetPOList(Int32 id, Int32 branchid, string Searchtext, Int32 orgid, Int32 supplierid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 9);
                param.Add("@branchid", branchid);
                param.Add("@searchtext", Searchtext);
                param.Add("@pmid", 0);
                param.Add("@prid", 0);
                param.Add("@orgid", orgid);
                param.Add("@id", id);
                param.Add("@categoryid", 0);
                param.Add("@claimtype_id", 0);
                param.Add("@supplier_id", supplierid);


                var List = await _connection.QueryAsync(ClaimAndPaymentMasterDB.ClaimAndPayment, param: param, commandType: CommandType.StoredProcedure);
                var Modellist = List.ToList();
                return new ResponseModel()
                {
                    Data = Modellist,
                    Message = "Success",
                    Status = true
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new Core.Master.ErrorLog.ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(ClaimAndPaymentCommonMasterRepository),
                    Method_Function = nameof(GetPOList),
                    UserId = 0,
                    ScreenName = "ClaimAndPayment",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { id, branchid, Searchtext, orgid, supplierid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error retrieving PO list: {ex.Message}",
                    Status = false
                };
            }
        }
    }
}

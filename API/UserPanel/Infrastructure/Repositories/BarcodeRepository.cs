using BackEnd.Master;
using Core.Abstractions;
using Core.Master.Country;
using Core.Models;
using Core.OrderMng.Barcode;
using Core.Master.ErrorLog;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPanel.Infrastructure.Data;

namespace Infrastructure.Repositories
{

        public class BarcodeRepository : IBarcodeRepository
        {
            private readonly IDbConnection _connection;
            private readonly IErrorLogMasterRepository _errorLogRepo;

            public BarcodeRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository)
            {
                _connection = unitOfWork.Connection;
                _errorLogRepo = errorLogMasterRepository;
            }

        #region OptBarcodeScan
        public async Task<object> OptBarcodeScan(Int32 PackingId)
        {
            try
            {
                var rowsAffected = await _connection.ExecuteAsync(OrderMngMaster.BarcodeProcedure, new
                {
                    Opt = 3,
                    packingid = PackingId,
                    barcode = "",
                    BarcodeDtlid = 0,
                    UserId = 0,
                    doid = 0,
                    rackid = 0
                });
                if (rowsAffected > 0)
                {
                    return new ResponseModel()
                    {
                        Data = rowsAffected,
                        Message = "Updated Successfully!!",
                        Status = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Update Failed!",
                        Status = false
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
                    Source = nameof(BarcodeRepository),
                    Method_Function = nameof(OptBarcodeScan),
                    UserId = 0,
                    ScreenName = "Barcode",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { PackingId })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error updating barcode: {ex.Message}",
                    Status = false
                };
            }
        }
        #endregion


        #region SaveBarcodeScan
        public async Task<object> SaveBarcodeScan(Int32 PackingId, Int32 rackid)
        {
            try
            {
                var rowsAffected = await _connection.ExecuteAsync(OrderMngMaster.BarcodeProcedure, new
                {
                    Opt = 4,
                    packingid = PackingId,
                    barcode = "",
                    BarcodeDtlid = 0,
                    UserId = 0,
                    doid = 0,
                    rackid = rackid
                });
                if (rowsAffected > 0)
                {
                    return new ResponseModel()
                    {
                        Data = rowsAffected,
                        Message = "Updated Successfully!!",
                        Status = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Update Failed!",
                        Status = false
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
                    Source = nameof(BarcodeRepository),
                    Method_Function = nameof(SaveBarcodeScan),
                    UserId = 0,
                    ScreenName = "Barcode",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { PackingId, rackid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error updating barcode: {ex.Message}",
                    Status = false
                };
            }
        }
        #endregion

        #region DeleteBarcode
        public async Task<object> DeleteBarcode(Int32 PackingId, Int32 BarcodeDtlid)
        {
            try
            {
                var rowsAffected = await _connection.ExecuteAsync(OrderMngMaster.BarcodeProcedure, new
                {
                    Opt = 2,
                    packingid = PackingId,
                    barcode = "",
                    BarcodeDtlid = BarcodeDtlid,
                    UserId = 0,
                    doid = 0,
                    rackid = 0
                });
                if (rowsAffected > 0)
                {
                    return new ResponseModel()
                    {
                        Data = rowsAffected,
                        Message = "Removed Successfully!!",
                        Status = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Remove Failed!",
                        Status = false
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
                    Source = nameof(BarcodeRepository),
                    Method_Function = nameof(DeleteBarcode),
                    UserId = 0,
                    ScreenName = "Barcode",
                    RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { PackingId, BarcodeDtlid })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error removing barcode: {ex.Message}",
                    Status = false
                };
            }
        }
        #endregion
    }
}

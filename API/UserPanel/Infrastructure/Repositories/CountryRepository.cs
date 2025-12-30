using Application.Master.CountryItem.CreateCountryItem;
using Application.Master.CountryItem.GetAllCountryItem;
using Application.Master.CountryItem.GetCountryItemById;
using Application.Master.CountryItem.UpdateCountryItem;
using BackEnd.Country;
using BackEnd.Invoices;
using Core.Abstractions;
using Core.Master.Country;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using MediatR;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPanel.Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class CountryRepository : ICountryRepository
    { //mysql queries
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public CountryRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogRepo, IUserTransactionLogRepository transactionLogRepo)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogRepo;
            _transactionLogRepo = transactionLogRepo;
        }
        #region GetAllCountriesAsync
        public async Task<object> GetAllCountriesAsync(int opt, int Id, string countryCode = "", string countryName = "")
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //1
                param.Add("ConId", Id); // 0
                param.Add("conCode", string.IsNullOrEmpty(countryCode) ? null : countryCode);
                param.Add("conName", string.IsNullOrEmpty(countryName) ? null : countryName);

                var List = await _connection.QueryAsync<object>(Country.CountryProcedure,
                    param: param, commandType: CommandType.StoredProcedure);
                var modelList = List.ToList();

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
                    Source = nameof(CountryRepository),
                    Method_Function = nameof(GetAllCountriesAsync),
                    UserId = 0,
                    ScreenName = "Country",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        opt, Id, countryCode, countryName
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = ex.Message,
                    Status = false
                };
            }
        }
        #endregion
        #region GetCountryByIdAsync
        public async Task<object> GetCountryByIdAsync(int opt, int countryId, string contCode, string contName)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); // get by id 2
                param.Add("ConId", countryId);
                param.Add("conCode", contCode); //null
                param.Add("conName", contName); //null

                var data = await _connection.QueryFirstOrDefaultAsync<object>(Country.CountryProcedure,
                    param: param, commandType: CommandType.StoredProcedure);

                if (data == null)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Country Id not found!",
                        Status = false
                    };
                }

                return new ResponseModel()
                {
                    Data = data,
                    Message = "Country found!",
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
                    Source = nameof(CountryRepository),
                    Method_Function = nameof(GetCountryByIdAsync),
                    UserId = 0,
                    ScreenName = "Country",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        opt, countryId, contCode, contName   
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong:" + ex.Message,
                    Status = false
                };
            }
        }
        #endregion
        #region GetCountryByCodeAsync 
        public async Task<object> GetCountryByCodeAsync(int opt, int Id, string contCode, string contName)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); //get by code 3
                param.Add("ConId", Id); //0
                param.Add("conCode", contCode);
                param.Add("conName", contName);
                var data = await _connection.QueryAsync<object>(Country.CountryProcedure,
                    param: param, commandType: CommandType.StoredProcedure);
                if (data == null)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Not Found!",
                        Status = false
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Data = data,
                        Message = "Listed Codes!",
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
                    Source = nameof(CountryRepository),
                    Method_Function = nameof(GetCountryByCodeAsync),
                    UserId = 0,
                    ScreenName = "Country",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                       opt, Id, contCode, contName
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "Something went wrong :" + ex.Message,
                    Status = false
                };

            }
        }
        #endregion
        #region GetCountryByNameAsync
        public async Task<object> GetCountryByNameAsync(int opt, int Id, string contCode, string contName)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Opt", opt); // get by name 4
                param.Add("ConId", Id); //0
                param.Add("conCode", contCode);
                param.Add("conName", contName);
                var data = await _connection.QueryAsync<object>(Country.CountryProcedure,
                param: param, commandType: CommandType.StoredProcedure);
                if (data == null)
                {
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Not Found!",
                        Status = false
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        Data = data,
                        Message = "Names Listed!",
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
                    Source = nameof(CountryRepository),
                    Method_Function = nameof(GetCountryByNameAsync),
                    UserId = 0,
                    ScreenName = "Country",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        opt, Id, contCode, contName
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "Something went Wrong : " + ex.Message,
                    Status = false
                };
            }

        }
        #endregion
        #region CreateCountryAsync
        public async Task<object> CreateCountryAsync(CountryItemMain obj)
        {
            try
            {
                var query = @"INSERT INTO master_country (CountryCode, CountryName, IsActive, CreatedBy, 
                                          CreaetedIP, CreatedDate) 
                                          VALUES (@CountryCode, @CountryName, @IsActive, @UserId,
                                          '', Now());
                                            SELECT LAST_INSERT_ID();";

                var result = await _connection.ExecuteScalarAsync<int>(query, obj.Header);

                int countryId = result;

                // Log transaction
                await LogTransactionAsync(
                    id: countryId,
                    branchId: 0,
                    orgId: 0,
                    actionType: "Insert",
                    actionDescription: "Added new Country",
                    oldValue: null,
                    newValue: obj,
                    tableName: "MasterCountry",
                    userId: obj.Header.UserId
                );

                if (result > 0)
                    return new ResponseModel()
                    {
                        Data = result,
                        Message = "Country created successfully",
                        Status = true
                    };
                else
                    return new ResponseModel()
                    {
                        Data = null,
                        Message = "Failed to create country",
                        Status = false
                    };

            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(CountryRepository),
                    Method_Function = nameof(CreateCountryAsync),
                    UserId = obj.Header.UserId,
                    ScreenName = "Country",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });

                return new ResponseModel()
                {
                    Data = null,
                    Message = ex.Message,
                    Status = false
                };
            }

        }
        #endregion
        #region UpdateCountryAsync
        public async Task<object> UpdateCountryAsync(CountryItemMain Obj)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from master_country where CurrencyId = {Obj.Header.CountryId}");
                var query = @"
                    UPDATE master_country 
                    SET 
                        CountryCode = @CountryCode,
                        CountryName = @CountryName,
                        IsActive = @IsActive,  
                        ModifiedBy = @UserId,
                        ModifiedIP = '',
                        ModifiedDate = Now()
                    WHERE CountryId = @countryId";
                var rowsAffected = await _connection.ExecuteAsync(query, new
                {
                    CountryCode = Obj.Header.CountryCode,
                    CountryName = Obj.Header.CountryName,
                    IsActive = Obj.Header.IsActive,
                    UserId = Obj.Header.UserId,
                    CountryId = Obj.Header.CountryId
                });

                // Log transaction
                await LogTransactionAsync(
                    id: Obj.Header.CountryId,
                    branchId: 0,
                    orgId: 0,
                    actionType: "Update",
                    actionDescription: "Update Country",
                    oldValue: null,
                    newValue: Obj,
                    tableName: "MasterCountry",
                    userId: Obj.Header.UserId
                );

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
                    Source = nameof(CountryRepository),
                    Method_Function = nameof(UpdateCountryAsync),
                    UserId = Obj.Header.UserId,
                    ScreenName = "Country",
                    RequestData_Payload = JsonConvert.SerializeObject(Obj)
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = ex.Message,
                    Status = false
                };
            }
        }
        #endregion

        private async Task LogTransactionAsync(int id, int branchId, int orgId, string actionType, string actionDescription, object oldValue, object newValue, string tableName, int? userId = 0)
        {
            var log = new UserTransactionLogModel
            {
                TransactionId = id.ToString(),
                ModuleId = 1,
                ScreenId = 1,
                ModuleName = "Master",
                ScreenName = "Country",
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

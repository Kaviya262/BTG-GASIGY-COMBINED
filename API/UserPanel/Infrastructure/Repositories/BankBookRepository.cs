using Core.Abstractions;
using Core.FinanceModule.Report;
using Core.Master.ErrorLog;
using Core.Models;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Mysqlx.Session;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using System.Data;

namespace Infrastructure.Repositories
{
    public class BankBookRepository : IBankBookRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;

        public BankBookRepository(IUnitOfWorkDB3 financedb, IErrorLogMasterRepository errorLogMasterRepository)
        {
            _connection = financedb.Connection;
            _errorLogRepo = errorLogMasterRepository;
        }

        public async Task<object> GetListBankBookAsync(DateTime? fromDate, DateTime? toDate, int branchId, int orgId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@p_fromDate", fromDate);
                param.Add("@p_toDate", toDate);
                param.Add("@p_branchId", branchId);
                param.Add("@p_orgId", orgId);
                param.Add("@p_openingBalance", 50000); //for Test

                var result = await _connection.QueryAsync<object>("proc_finance_BankBook", param,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(BankBookRepository),
                    Method_Function = nameof(GetListBankBookAsync),
                    UserId = 0,
                    ScreenName = "Approval",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        fromDate,
                        toDate,
                        branchId,
                        orgId,

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



        }
}
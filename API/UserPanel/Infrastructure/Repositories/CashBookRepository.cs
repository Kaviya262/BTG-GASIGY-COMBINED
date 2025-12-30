using Core.Abstractions;
using Core.FinanceModule.Report;
using Core.Master.ErrorLog;
using Core.Models;
using Dapper;
using System.Data;

public class CashBookRepository : ICashBookRepository
{
    private readonly IDbConnection _connection;
    private readonly IErrorLogMasterRepository _errorLogRepo;

    public CashBookRepository(IUnitOfWorkDB3 finacedb, IErrorLogMasterRepository errorLogMasterRepository)
    {
        _connection = finacedb.Connection;
        _errorLogRepo = errorLogMasterRepository;
    }

    public async Task<object> GetListCashBookAsync(DateTime? fromDate, DateTime? toDate, int branchId, int orgId)
    {
        try
        {
            var param = new DynamicParameters();
            param.Add("@p_fromDate", fromDate);
            param.Add("@p_toDate", toDate);
            param.Add("@p_branchId", branchId);
            param.Add("@p_orgId", orgId);
            param.Add("@p_openingBalance", 50000); //for Test

            var sql = "proc_finance_CashBook";

            var result = await _connection.QueryAsync<object>(
                sql,
                param,
                commandType: CommandType.StoredProcedure
            );

            return new {
                Data = result,
                Status = true,
                Message = "Success"
            };
        }
        catch (Exception ex)
        {
            await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
            {
                ErrorMessage = ex.Message,
                ErrorType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = nameof(CashBookRepository),
                Method_Function = nameof(GetListCashBookAsync),
                UserId = 0,
                ScreenName = "CashBook",
                RequestData_Payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { fromDate, toDate, branchId, orgId })
            });
            return new ResponseModel()
            {
                Data = null,
                Status = false,
                Message = $"Error retrieving cash book: {ex.Message}"
            };
        }
    }
}
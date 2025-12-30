using BackEnd.Master;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Core.OrderMngMaster.Users;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System.Data;
using UserPanel.Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class MasterUsersRepository : IMasterUsersRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public MasterUsersRepository(IUnitOfWorkDB1 unitOfWork, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository userTransactionLogRepository)
        {
            _connection = unitOfWork.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = userTransactionLogRepository;
        }

        public Task<object> GetAllAsync(int opt)
        {
            throw new NotImplementedException();
        }

        #region Get All Users
        public async Task<object> GetAllUser(int? prodId, string fromDate, string toDate, int? branchId, string UserName, string keyword, int? pageNumber, int? pageSize)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@searchUsername", string.IsNullOrEmpty(UserName) ? null : UserName, DbType.String);
                param.Add("@fromDate", string.IsNullOrEmpty(fromDate) ? (object)DBNull.Value : DateTime.Parse(fromDate).ToString("yyyy-MM-dd"), DbType.String);
                param.Add("@toDate", string.IsNullOrEmpty(toDate) ? (object)DBNull.Value : DateTime.Parse(toDate).ToString("yyyy-MM-dd"), DbType.String);
                param.Add("@keyword", string.IsNullOrEmpty(keyword) ? null : keyword, DbType.String);
                param.Add("@branchId", branchId, DbType.Int32);
                param.Add("@pageNumber", pageNumber, DbType.Int32);
                param.Add("@pageSize", pageSize, DbType.Int32);

                var list = await _connection.QueryAsync(MasterUsersMaster.GetAllMasterUserProcedure, param: param, commandType: CommandType.StoredProcedure);
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
                    Source = nameof(MasterUsersRepository),
                    Method_Function = nameof(GetAllUser),
                    UserId = 0,
                    ScreenName = "User",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        prodId, fromDate, toDate, branchId, pageNumber, pageSize, UserName, keyword
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Something went wrong: {ex.Message}",
                    Status = false
                };
            }
        }

        #endregion

        #region Toggle User Active Status
        public async Task<object> ToggleUserActiveStatusAsync(MasterUsersCommand userStatus)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from users where id = {userStatus.MasterUser.Id}");

                var param = new DynamicParameters();
                param.Add("@in_Id", userStatus.MasterUser.Id);
                param.Add("@in_IsActive", userStatus.MasterUser.IsActive);
                param.Add("@in_Remarks", userStatus.MasterUser.Remark); 
                param.Add("@in_BranchId", userStatus.MasterUser.BranchId); 

                var responseCode = await _connection.QueryFirstOrDefaultAsync<int>(
                    MasterUsersMaster.UpdateUserStatus,
                    param,
                    commandType: CommandType.StoredProcedure
                );

                // Log transaction
                await LogTransactionAsync(
                    id: userStatus.MasterUser.Id,
                    branchId: userStatus.MasterUser.BranchId,
                    orgId: 1,
                    actionType: "Update",
                    actionDescription: "Update users",
                    oldValue: oldvalue,
                    newValue: userStatus,
                    tableName: "MasterCylinder",
                    userId: 1
                );

                return new ResponseModel()
                {
                    Data = null,
                    Message = responseCode == 0 ? "User Active Status Toggled Successfully" : "Failed to Toggle Active Status",
                    Status = responseCode == 0
                };

            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(MasterUsersRepository),
                    Method_Function = nameof(ToggleUserActiveStatusAsync),
                    UserId = 0,
                    ScreenName = "User",
                    RequestData_Payload = JsonConvert.SerializeObject(userStatus)
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error: {ex.Message}",
                    Status = false
                };
            }
        }
        #endregion



        public async  Task<object> UpdateUserPasswordAsync(MasterUsersCommandpwd createOrUpdateUser)
        {
            try
            {
                var oldvalue = await _connection.QueryAsync<object>($"select * from users where id = {createOrUpdateUser.MasterUserspwd.Id}");

                var param = new DynamicParameters();
                param.Add("opt",1);

                param.Add("u_userid", createOrUpdateUser.MasterUserspwd.Id);
                param.Add("u_password", createOrUpdateUser.MasterUserspwd.Password);
                param.Add("u_old_password", createOrUpdateUser.MasterUserspwd.oldpassword);
                var result = await _connection.QueryAsync<int>(
                    "proc_updateusercredential",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                // Log transaction
                await LogTransactionAsync(
                    id: createOrUpdateUser.MasterUserspwd.Id,
                    branchId: createOrUpdateUser.MasterUserspwd.BranchId,
                    orgId: 1,
                    actionType: "Update",
                    actionDescription: "Update User",
                    oldValue: oldvalue,
                    newValue: createOrUpdateUser,
                    tableName: "users",
                    userId: 1
                );
                var user = result.FirstOrDefault();
                if (user == 1)
                {
                    return new ResponseModel
                    {
                        Data = null,
                        Message = "Password updated",
                        Status = true,
                        StatusCode = 200
                    };
                }
                else
                { 
                        return new ResponseModel
                        {
                            Data = null,
                            Message = "Old password not matched",
                            Status = false,
                            StatusCode = 400
                        };
                    
                }

            }
            catch(Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(MasterUsersRepository),
                    Method_Function = nameof(UpdateUserPasswordAsync),
                    UserId = 0,
                    ScreenName = "User",
                    RequestData_Payload = JsonConvert.SerializeObject(createOrUpdateUser)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "User not found",
                    Status = false,
                    StatusCode=500
                };
            }

           

        }

        #region Create or Update User
        public async Task<object> CreateOrUpdateUserAsync(MasterUsersCommand createOrUpdateUser)
        {
            try
            {
                object oldValue = null;
                bool isUpdate = createOrUpdateUser.MasterUser.Id > 0;

                if (isUpdate)
                {
                    oldValue = await _connection.QuerySingleOrDefaultAsync<object>("SELECT * FROM users WHERE Id = @Id", new { Id = createOrUpdateUser.MasterUser.Id }
                    );
                }
                var param = new DynamicParameters();

                // Input parameters
                param.Add("in_Id", createOrUpdateUser.MasterUser.Id == 0 ? 0 : createOrUpdateUser.MasterUser.Id, DbType.Int32);
                param.Add("in_FirstName", createOrUpdateUser.MasterUser.FirstName);
                param.Add("in_MiddleName", createOrUpdateUser.MasterUser.MiddleName);
                param.Add("in_LastName", createOrUpdateUser.MasterUser.LastName);
                param.Add("in_UserName", createOrUpdateUser.MasterUser.UserName);
                param.Add("in_Password", createOrUpdateUser.MasterUser.Password);
                param.Add("in_Role", createOrUpdateUser.MasterUser.Role);
                param.Add("in_Department", createOrUpdateUser.MasterUser.Department);
                param.Add("in_MobileNo", createOrUpdateUser.MasterUser.MobileNo);
                param.Add("in_EmailID", createOrUpdateUser.MasterUser.EmailID);
                param.Add("in_FromDate", createOrUpdateUser.MasterUser.FromDate);
                param.Add("in_ToDate", createOrUpdateUser.MasterUser.ToDate);
                param.Add("in_Remarks", createOrUpdateUser.MasterUser.Remark);
                param.Add("in_BranchId", createOrUpdateUser.MasterUser.BranchId);
                param.Add("in_CreatedBy", createOrUpdateUser.MasterUser.CreatedBy);

                // Output parameters
                param.Add("out_NewUserId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                param.Add("out_ResponseCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                param.Add("out_Message", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

                // Execute stored procedure
                await _connection.ExecuteAsync(
                    MasterUsersMaster.CreateOrUpdateUser,
                    param,
                    commandType: CommandType.StoredProcedure
                );

                // Get output values
                int newUserId = param.Get<int?>("out_NewUserId") ?? 0;
                int result = param.Get<int>("out_ResponseCode");
                string messageFromDb = param.Get<string>("out_Message");

                string message;
                bool status;
                int statusCode;
                int data;

                string actionType = isUpdate ? "Update" : "Insert";
                string actionDescription = isUpdate ? "Updated User" : "Created New User";

                // Log transaction
                await LogTransactionAsync(
                    id: isUpdate ? createOrUpdateUser.MasterUser.Id : newUserId,
                    branchId: createOrUpdateUser.MasterUser.BranchId,
                    orgId: 1,
                    actionType: "Insert",
                    actionDescription: "Added new User",
                    oldValue: isUpdate ? oldValue : null,
                    newValue: createOrUpdateUser,
                    tableName: "user",
                    userId: 1
                );

                switch (result)
                {
                    case 0:
                        data = newUserId;
                        message = "User Updated Successfully";
                        status = true;
                        statusCode = 200;
                        break;
                    case 1:
                        data = newUserId;
                        message = "User Created Successfully";
                        status = true;
                        statusCode = 200;
                        break;
                    case 2:
                        data = newUserId;
                        message = "Username already exists";
                        status = false;
                        statusCode = 400;
                        break;
                    case 3:
                        data = newUserId;
                        message = "Email already exists";
                        status = false;
                        statusCode = 400;
                        break;
                    case 4:
                        data = newUserId;
                        message = "User not found for update";
                        status = false;
                        statusCode = 404;
                        break;
                    default:
                        data = newUserId;
                        message = "Failed to Create/Update User";
                        status = false;
                        statusCode = 500;
                        break;
                }

                return new ResponseModel()
                {
                    Data = data,
                    Message = messageFromDb ?? message,
                    Status = status,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                await _errorLogRepo.LogErrorAsync(new ErrorLogMasterModel
                {
                    ErrorMessage = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Source = nameof(MasterUsersRepository),
                    Method_Function = nameof(CreateOrUpdateUserAsync),
                    UserId = 0,
                    ScreenName = "User",
                    RequestData_Payload = JsonConvert.SerializeObject(createOrUpdateUser)
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = $"Error: {ex.Message}",
                    Status = false,
                    StatusCode = 500
                };
            }
        }
        #endregion


        #region Get User by ID and Branch ID
        public async Task<object> GetUserByIdAsync(int userId,int branchId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("in_UserID", userId);
                param.Add("in_BranchId", branchId);

                var result = await _connection.QueryAsync<MasterUsers>(
                    "GetUserById",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                var user = result.FirstOrDefault();

                if (user == null)
                {
                    return new ResponseModel
                    {
                        Data = null,
                        Message = "User not found",
                        Status = false
                    };
                }

                var command = new MasterUsersCommand
                {
                    MasterUser = user
                };

                return new ResponseModel
                {
                    Data = command,
                    Message = "User found",
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
                    Source = nameof(MasterUsersRepository),
                    Method_Function = nameof(GetUserByIdAsync),
                    UserId = 0,
                    ScreenName = "User",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        userId, branchId
                    })
                });
                return new ResponseModel()
                {
                    Data = null,
                    Message = "User not found" + Ex.Message,
                    Status = false
                };
            }
        }

        #endregion

        private async Task LogTransactionAsync(int? id, int? branchId, int? orgId, string actionType, string actionDescription, object oldValue, object newValue, string tableName, int? userId = 0)
        {
            var log = new UserTransactionLogModel
            {
                TransactionId = id.ToString(),
                ModuleId = 1,
                ScreenId = 1,
                ModuleName = "Master",
                ScreenName = "User",
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

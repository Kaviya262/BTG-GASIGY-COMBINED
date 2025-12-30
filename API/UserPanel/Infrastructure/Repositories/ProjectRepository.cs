using BackEnd.Procurement.Master;
using Core.Abstractions;
using Core.Master.ErrorLog;
using Core.Master.Transactionlog;
using Core.Models;
using Core.Procurement.Master;
using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using Mysqlx.Crud;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IDbConnection _connection;
        private readonly IErrorLogMasterRepository _errorLogRepo;
        private readonly IUserTransactionLogRepository _transactionLogRepo;

        public ProjectRepository(IUnitOfWorkDB4 masterDb, IErrorLogMasterRepository errorLogMasterRepository, IUserTransactionLogRepository transactionLogRepo)
        {
            _connection = masterDb.Connection;
            _errorLogRepo = errorLogMasterRepository;
            _transactionLogRepo = transactionLogRepo;
        }

        public async Task<object> AddAsync(MasterProject obj)
        {
            try
            {

                var param = new DynamicParameters();
                param.Add("@opt", 1); // Add
                param.Add("@project_id", 0);
                param.Add("@project_Code", "");
                param.Add("@project_Name", "");
                param.Add("@user_id", 0);
                param.Add("@org_id", obj.orgid);
                param.Add("@branch_id", obj.branchid);
                param.Add("@json_project", JsonConvert.SerializeObject(obj));

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                    ProjectBackEnd.ProjectsProcedure, param, commandType: CommandType.StoredProcedure);

                int projectId = (int?)result?.new_project_id ?? 0;

                // Log transaction
                await LogTransactionAsync(
                    id: projectId,
                    branchId: obj.branchid,
                    orgId: obj.orgid,
                    actionType: "Insert",
                    actionDescription: "Added new project",
                    oldValue: null,
                    newValue: obj,
                    tableName: "MasterProject",
                    userId: obj.userid
                );

                return new ResponseModel
                {
                    Data = result,
                    Message = "Project saved successfully",
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
                    Source = nameof(ProjectRepository),
                    Method_Function = nameof(AddAsync),
                    UserId = obj.userid,
                    ScreenName = "Project",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Error saving project: " + ex.Message,
                    Status = false
                };
            }
        }

        public async Task<object> UpdateAsync(MasterProject obj)
        {
            try
            {
                var oldValue = await GetProjectByIdAsync(obj.projectid, obj.branchid, obj.orgid);

                var param = new DynamicParameters();
                param.Add("@opt", 2); // Update
                param.Add("@project_id", obj.projectid);
                param.Add("@project_Code", "");
                param.Add("@project_Name", "");
                param.Add("@user_id", 0);
                param.Add("@org_id", obj.orgid);
                param.Add("@branch_id", obj.branchid);
                param.Add("@json_project", JsonConvert.SerializeObject(obj));

                var result = await _connection.QueryFirstOrDefaultAsync<object>(
                    ProjectBackEnd.ProjectsProcedure, param, commandType: CommandType.StoredProcedure);

                // Log transaction
                await LogTransactionAsync(
                    id: obj.projectid,
                    actionType: "Update",
                    actionDescription: "Updated project",
                    oldValue: oldValue,
                    newValue: obj,
                    tableName: "MasterProject",
                    userId: obj.userid,
                    branchId: obj.branchid,
                    orgId: obj.orgid
                );

                return new ResponseModel
                {
                    Data = result,
                    Message = "Project updated successfully",
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
                    Source = nameof(ProjectRepository),
                    Method_Function = nameof(UpdateAsync),
                    UserId = obj.userid,
                    ScreenName = "Project",
                    RequestData_Payload = JsonConvert.SerializeObject(obj)
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Error updating project: " + ex.Message,
                    Status = false
                };
            }
        }

        public async Task<object> GetProjectByIdAsync(int projectid, int branchid, int orgid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 3); // Get by ID
                param.Add("@project_id", projectid);
                param.Add("@project_Code", "");
                param.Add("@project_Name", "");
                param.Add("@user_id", 0);
                param.Add("@org_id", orgid);
                param.Add("@branch_id", branchid);
                param.Add("@json_project", "{}");

                var result = await _connection.QueryMultipleAsync(
                    ProjectBackEnd.ProjectsProcedure, param, commandType: CommandType.StoredProcedure);

                var header = await result.ReadFirstOrDefaultAsync<object>();

                return new ResponseModel
                {
                    Data = new { Header = header },
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
                    Source = nameof(ProjectRepository),
                    Method_Function = nameof(GetProjectByIdAsync),
                    UserId = 0,
                    ScreenName = "Project",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                       projectid, branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Error retrieving project: " + ex.Message,
                    Status = false
                };
            }
        }

        public async Task<object> GetListProjectAsync(string projectcode,string projectname,int userid,int branchid, int orgid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@opt", 4); // List
                param.Add("@project_id", 0);
                param.Add("@project_Code", projectcode);
                param.Add("@project_Name", projectname);
                param.Add("@user_id", userid);
                param.Add("@org_id", orgid);
                param.Add("@branch_id", branchid);
                param.Add("@json_project", "{}");


                var result = await _connection.QueryAsync<object>(
                    ProjectBackEnd.ProjectsProcedure, param, commandType: CommandType.StoredProcedure);

                return new ResponseModel
                {
                    Data = result,
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
                    Source = nameof(ProjectRepository),
                    Method_Function = nameof(GetListProjectAsync),
                    UserId = userid,
                    ScreenName = "Project",
                    RequestData_Payload = JsonConvert.SerializeObject(new
                    {
                        projectcode, projectname, userid, branchid, orgid
                    })
                });
                return new ResponseModel
                {
                    Data = null,
                    Message = "Error retrieving project list: " + ex.Message,
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
                ModuleName = "Master",
                ScreenName = "Project",
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

using Application.Master.Customer.CreateCustomer;
using Application.Master.Customer.GetAllCustomer;
using Application.Master.Customer.GetCustomerByID;
using Application.Master.Customer.ToogleActiveStatus;
using Application.Master.Customer.UpdateCustomer;
using Application.Master.DepartmentItem.GetDepartmentItemById;
using Application.PackingAndDO.UploadPackingAndDO;
using Core.Models;
using MediatR;
using Core.OrderMngMaster.Customerdoc;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg;

namespace UserPanel.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterCustomerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MasterCustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] CreateCustomerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }


        [HttpPost("Upload-doc")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDO([FromForm] UploadCustomerDoc model)
        {
            if (model.CustomerId <= 0)
                return BadRequest("CustomerId is required.");

            if (model.BranchId <= 0 || model.UserId <= 0)
                return BadRequest("BranchId and UserId are required.");

            var files = Request.Form.Files;

            if (files == null || files.Count == 0)
                return BadRequest("At least one file is required.");

            try
            {
                var baseDir = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "UploadedFiles",
                    "CustomerDoc",
                    model.CustomerId.ToString()
                );

                if (!Directory.Exists(baseDir))
                    Directory.CreateDirectory(baseDir);

                foreach (var file in files)
                {
                    var filePath = Path.Combine(baseDir, file.FileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    // match frontend FormData keys
                    if (file.Name == "legalFile")
                        model.LegalDocumentPath = filePath;

                    if (file.Name == "customerReviewFile")
                        model.CustomerReviewFormPath = filePath;
                }

                if (model.LegalDocumentPath == null &&
                    model.CustomerReviewFormPath == null)
                    return BadRequest("At least one file is required.");

                await _mediator.Send(new UploadDocQuery
                {
                    Id = model.CustomerId,
                    LegalPath = model.LegalDocumentPath,
                    ReviewPath = model.CustomerReviewFormPath,
                    UserId = model.UserId,
                    BranchId = model.BranchId
                });

                return Ok(new ResponseModel
                {
                    Status = true,
                    Message = "Files uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    Status = false,
                    Message = "Upload failed due to server error."
                });
            }
        }




        [HttpGet("get-list-tab")]
        public async Task<IActionResult> GetListALL([FromQuery] int tabId, [FromQuery] int customerId, [FromQuery] int contactId, [FromQuery] int addressId)
        {
            var query = new GetAllCustomerQuery
            {
                TabId = tabId,
                CustomerId = customerId,
                ContactId = contactId,
                AddressId = addressId,
                BranchId = 1,
                UserId = 1
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("GetByID")]
        public async Task<IActionResult> GetByID(int customerID, int tabId, int branchId)
        {
            var result = await _mediator.Send(new GetCustomerByIDQuery() { CustomerId = customerID, TabId = tabId, BranchId = branchId });
            return Ok(result);
        }


        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateCustomerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("toggle-actve-status")]
        public async Task<IActionResult> ToogleActiveStatus([FromBody] ToogleActiveStatusCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("get-list-tab-customer")]
        public async Task<IActionResult> GetAllCustomerAsync(string customerName = "", int tabId = 0, int customerId = 0, int contactId = 0, int addressId = 0, int branchId = 0, int userId = 0)
        {
            var query = new GetAllCustomerListQuery
            {
                CustomerName = customerName,
                BranchId = branchId,
                UserId = userId,
                CustomerId = customerId,
                ContactId = contactId,
                AddressId = addressId,

            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        #region GetDepartmentByCode
        [HttpGet("name/{DepartmentName}")]
        public async Task<IActionResult> GetDepartmentByName(string DepartmentName)
        {
            var departnames = await _mediator.Send(new GetDepartmentItemByIdQuery { DepartmentName = DepartmentName });
            return Ok(departnames);
        }
        #endregion
    }
}

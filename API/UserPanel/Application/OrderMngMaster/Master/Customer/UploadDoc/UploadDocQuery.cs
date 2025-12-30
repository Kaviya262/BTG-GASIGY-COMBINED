using Core.Models;
using MediatR;

namespace Application.PackingAndDO.UploadPackingAndDO
{
    public class UploadDocQuery : IRequest<ResponseModel>
    {
        public int Id { get; set; }

 
        public string? LegalPath { get; set; }
        public string? ReviewPath { get; set; }

        public int UserId { get; set; }
        public int BranchId { get; set; }
    }
}

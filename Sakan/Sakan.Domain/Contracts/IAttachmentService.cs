using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sakan.Domain.Contracts
{
    public interface IAttachmentService
    {
        public string? UploadAttachment(IFormFile file, string folderName);  // returns filepath in database, file will be stored as asset in [wwwroot]
        void DeleteAttachment(string fileName, string foldername);
    }
}

using Microsoft.AspNetCore.Http;
using Sakan.Domain.Contracts;

namespace Sakan.Application.Services
{
    public class AttachmentService : IAttachmentService
    {
        readonly List<string> AllowedExtensions = [".png", ".jpeg", ".jpg"];
        const int MaxSize = 2097152; //2mb
        public string? UploadAttachment(IFormFile file, string folderName)
        {
            if (file == null) return null;  
            //1.Check Extension
            string extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension)) return null;

            //2.Check Size
            if (file.Length > MaxSize) return null;
            //3.Get Located Folder Path
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", folderName);
            //4.Make Attachment Name Unique-- GUID
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            //5.Get File Path
            var filePath = Path.Combine(folderPath, fileName);

            //6.Create File Stream To Copy File[Unmanaged]
            using FileStream fs = new FileStream(filePath, FileMode.Create);
            //7.Use Stream To Copy File
            file.CopyTo(fs);
            //8.Return FileName To Store In Database
            return fileName;

        }
        public void DeleteAttachment(string fileName, string folderName)
        {
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", folderName, fileName);
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

    }
}
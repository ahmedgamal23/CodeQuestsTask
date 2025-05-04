using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Application.Services
{
    public class SaveMetaData
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public SaveMetaData(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> Save(IFormFile formFile , MetaDataType MetaDataType)
        {
            string imageName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
            if (!File.Exists(MetaDataType.ToString()))
                Directory.CreateDirectory(MetaDataType.ToString());
            string imagePath = Path.Combine(MetaDataType.ToString(), imageName);
            string imageFullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath);
            using (FileStream fileStream = new FileStream(imageFullPath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }
            return imagePath;
        }

        public DeleteBehavior Delete(string path)
        {
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, path);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return DeleteBehavior.Deleted;
            }
            return DeleteBehavior.NotFound;
        }
        
        public enum DeleteBehavior
        {
            Deleted,
            NotFound
        }

        public enum MetaDataType
        {
            Images,
            Videos 
        }

    }
}

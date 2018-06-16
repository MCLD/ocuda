using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ops.Service
{
    public class FileService
    {
        public IEnumerable<File> GetFiles()
        {
            return new List<File>
            {
                new File
                {
                    CreatedAt = DateTime.Parse("2018-05-01"),
                    IsFeatured = true,
                    FilePath = "/file.txt",
                    Name = "Important File!",
                    Icon = "fa-file-word alert-primary"
                },
                new File
                {
                    CreatedAt = DateTime.Parse("2018-06-04"),
                    FilePath = "/file.txt",
                    Name = "New File 2",
                    Icon = "fa-file-excel alert-success"
                },
                new File
                {
                    CreatedAt = DateTime.Parse("2018-05-20"),
                    FilePath = "/file.txt",
                    Name = "New File 1",
                    Icon = "fa-file-pdf alert-danger"
                }
            };
        }

        public File GetFileById(int id)
        {
            return new File
            {
                Id = id,
                CreatedAt = DateTime.Parse("2018-05-01"),
                CreatedBy = 1,
                IsFeatured = true,
                FilePath = "/file.txt",
                Name = "Important File!",
                Icon = "fa-file-word alert-primary"
            };
        }

        public IEnumerable<FileCategory> GetFileCategories()
        {
            return new List<FileCategory>
            {
                new FileCategory
                {
                    Id = 1,
                    Name = "File Category 1",
                },
                new FileCategory
                {
                    Id = 2,
                    Name = "File Category 2",
                },
                new FileCategory
                {
                    Id = 3,
                    Name = "File Category 3",
                },
            };
        }

        public FileCategory GetFileCategoryById(int id)
        {
            return new FileCategory
            {
                Id = id,
                Name = $"Category {id}",
            };
        }

        public async Task<File> CreateFileAsync(File file)
        {
            // call create method from repository
            return file;
        }

        public async Task<File> EditFileAsync(File file)
        {
            // get existing item and update properties that changed
            // call edit method on existing post
            return file;
        }

        public async Task DeleteFileAsync(int id)
        {
            // call delete method from repository
        }

        public async Task<FileCategory> CreateFileCategoryAsync(FileCategory fileCategory)
        {
            // call create method from repository
            return fileCategory;
        }

        public async Task<FileCategory> EditFileCategoryAsync(FileCategory fileCategory)
        {
            // get existing item and update properties that changed
            // call edit method on existing post
            return fileCategory;
        }

        public async Task DeleteFileCategoryAsync(int id)
        {
            // call delete method from repository
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Xunit;

namespace Ocuda.test.Ops.Service.Test
{
    public class FileService_Tests
    {
        private const string TestDate = "2018-07-20";

        [Theory]
        [InlineData(true, 1, 1, ".txt", 1, "Name", null, null, 1)]      //Valid CategoryId
        [InlineData(true, null, 1, ".txt", 1, "Name", 1, null, 1)]      //Valid PageId
        [InlineData(true, null, 1, ".txt", 1, "Name", null, 1, 1)]      //Valid PostId
        [InlineData(false, 1, -1, ".txt", 1, "Name", null, null, 1)]    //Invalid CreatedBy
        [InlineData(false, -1, 1, ".txt", 1, "Name", null, null, 1)]    //Invalid CategoryId
        [InlineData(false, 1, 1, ".fail", 1, "Name", null, null, 1)]    //Invalid Extension
        [InlineData(false, 1, 1, ".txt", -1, "Name", null, null, 1)]    //Invalid FileTypeId
        [InlineData(false, null, 1, ".txt", 1, "Name", -1, null, 1)]    //Invalid PageId
        [InlineData(false, null, 1, ".txt", 1, "Name", null, -1, 1)]    //Invalid PostId
        [InlineData(false, 1, 1, ".txt", 1, "Name", null, null, -1)]    //Invalid SectionId
        [InlineData(false, null, 1, ".txt", 1, "Name", null, null, 1)]  //Invalid Null Category/Page/Post
        [InlineData(false, 1, 1, null, 1, "Name", null, null, 1)]       //Invalid Null Extension
        [InlineData(false, 1, 1, ".txt", 1, null, null, null, 1)]       //Invalid Null Name
        public async Task ValidateFile_ThrowsOcudaExceptions(
            bool isValidInput,
            int? categoryId,
            int createdBy,
            string extension,
            int fileTypeId,
            string name,
            int? pageId,
            int? postId,
            int sectionId)
        {
            var file = new File
            {
                CategoryId = categoryId,
                CreatedAt = DateTime.Parse(TestDate),
                CreatedBy = createdBy,
                Description = "Description",
                Extension = extension,
                FileTypeId = fileTypeId,
                Id = 1,
                IsFeatured = false,
                Name = name,
                PageId = pageId,
                PostId = postId,
                SectionId = sectionId
            };

            var logger = new Mock<ILogger<FileService>>();

            var categoryRepository = new Mock<ICategoryRepository>();
            categoryRepository.Setup(
                m => m.GetCategoryAndFileTypesByCategoryIdAsync(1))
                    .ReturnsAsync(new Category
                    {
                        Id = 1,
                        Name = "Test Category 1",
                        CategoryFileTypes = new List<CategoryFileType>
                        {
                            new CategoryFileType{
                                FileTypeId = 1,
                                FileType = new FileType
                                {
                                    Extension = ".txt"
                                }
                            }
                        }
                    });

            var fileRepository = new Mock<IFileRepository>();
            fileRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new File
                    {
                        Id = 1,
                        Name = "Test File 1"
                    });

            var pageRepository = new Mock<IPageRepository>();
            pageRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new Page
                    {
                        Id = 1,
                        Title = "Test Page 1"
                    });

            var postRepository = new Mock<IPostRepository>();
            postRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new Post
                    {
                        Id = 1,
                        Title = "Test Post 1"
                    });

            var sectionRepository = new Mock<ISectionRepository>();
            sectionRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new Section
                    {
                        Id = 1,
                        Name = "Test Section 1"
                    });

            var thumbnailRepository = new Mock<IThumbnailRepository>();

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new User
                    {
                        Id = 1,
                        Name = "Test User 1"
                    });

            var fileTypeService = new Mock<IFileTypeService>();
            fileTypeService.Setup(
                m => m.GetByIdAsync(1))
                    .ReturnsAsync(new FileType
                    {
                        Id = 1,
                        Extension = ".test"
                    });

            var thumbnailService = new Mock<IThumbnailService>();
            var pathResolver = new Mock<IPathResolverService>();

            var fileService = new FileService(
                logger.Object,
                categoryRepository.Object,
                fileRepository.Object,
                pageRepository.Object,
                postRepository.Object,
                sectionRepository.Object,
                thumbnailRepository.Object,
                userRepository.Object,
                fileTypeService.Object,
                thumbnailService.Object,
                pathResolver.Object);

            //Act
            var ex = await Record.ExceptionAsync(() => fileService.ValidateFileAsync(file));

            //Assert
            if (isValidInput)
            {
                Assert.Null(ex);
            }
            else
            {
                Assert.NotNull(ex);
                Assert.IsType<OcudaException>(ex);
            }
        }
    }
}

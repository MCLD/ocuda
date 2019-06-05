using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Ocuda.Ops.Models.Entities;
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
        [InlineData(true, 1, 1, 1, "Name", null, null)]        //Valid FileLibraryId
        [InlineData(true, 1, 1, null, "Name", 1, null)]      //Valid PageId
        [InlineData(true, 1, 1, null, "Name", null, 1)]      //Valid PostId
        [InlineData(false, -1, 1, 1, "Name", null, null)]    //Invalid CreatedBy
        [InlineData(false, 1, -1, 1, "Name", null, null)]    //Invalid FileTypeId
        [InlineData(false, 1, 1, -1, "Name", 1, null)]         //Invalid FileLibraryId
        [InlineData(false, 1, 1, null, "Name", -1, null)]    //Invalid PageId
        [InlineData(false, 1, 1, null, "Name", null, -1)]    //Invalid PostId
        [InlineData(false, 1, 1, null, "Name", null, null)]  //Invalid Null Library/Page/Post
        [InlineData(false, 1, 1, 1, null, null, null)]       //Invalid Null Name
        public void ValidateFile_ThrowsOcudaExceptions(
            bool isValidInput,
            int createdBy,
            int fileTypeId,
            int? fileLibraryId,
            string name,
            int? pageId,
            int? postId)
        {
            var file = new File
            {
                CreatedAt = DateTime.Parse(TestDate),
                CreatedBy = createdBy,
                Description = "Description",
                FileLibraryId = fileLibraryId,
                FileTypeId = fileTypeId,
                Id = 1,
                Name = name,
                PageId = pageId,
                PostId = postId
            };

            var logger = new Mock<ILogger<FileService>>();

            var fileLibraryRepository = new Mock<IFileLibraryRepository>();
            fileLibraryRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new FileLibrary
                    {
                        Id = 1,
                        Name = "Test Library 1"
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
                fileLibraryRepository.Object,
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
            var ex = Record.Exception(() => fileService.ValidateFile(file));

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

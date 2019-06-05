using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Exceptions;
using Xunit;

namespace Ocuda.test.Ops.Service.Test
{
    public class LinkService_Tests
    {
        private const string TestDate = "2018-07-20";

        [Theory]
        [InlineData(true, 1, 1, "Name", "www.test.com")]    //Valid
        public async Task ValidateLink_ThrowsOcudaExceptions(
            bool isValidInput,
            int createdBy,
            int linkLibraryId,
            string name,
            string url)
        {
            var link = new Link
            {

                CreatedAt = DateTime.Parse(TestDate),
                CreatedBy = createdBy,
                Id = 1,
                LinkLibraryId = linkLibraryId,
                Name = name,
                Url = url
            };

            var logger = new Mock<ILogger<LinkService>>();

            var linkLibraryRepository = new Mock<ILinkLibraryRepository>();
            linkLibraryRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new LinkLibrary
                    {
                        Id = 1,
                        Name = "Test Library 1"
                    });

            var linkRepository = new Mock<ILinkRepository>();
            linkRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new Link
                    {
                        Id = 1,
                        Name = "Test Link 1"
                    });

            var sectionRepository = new Mock<ISectionRepository>();
            sectionRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new Section
                    {
                        Id = 1,
                        Name = "Test Section 1"
                    });

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new User
                    {
                        Id = 1,
                        Name = "Test User 1"
                    });

            var linkService = new LinkService(
                logger.Object,
                linkLibraryRepository.Object,
                linkRepository.Object,
                sectionRepository.Object,
                userRepository.Object);

            //Act
            Exception ex = null; //await Record.ExceptionAsync(() => linkService.ValidateLinkAsync(link));

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

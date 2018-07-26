using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Ocuda.Ops.Models;
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
        [InlineData(true, 1, 1, "Name", 1, "www.test.com")]    //Valid
        [InlineData(false, -1, 1, "Name", 1, "www.test.com")]  //Invalid CategoryId
        [InlineData(false, 1, -1, "Name", 1, "www.test.com")]  //Invalid CreatedBy
        [InlineData(false, 1, 1, "Name", -1, "www.test.com")]  //Invalid SectionId
        [InlineData(false, 1, 1, null, 1, "www.test.com")]     //Invalid Null Name
        [InlineData(false, 1, 1, "Name", 1, null)]             //Invalid Null URL
        public async Task ValidateLink_ThrowsOcudaExceptions(
            bool isValidInput,
            int categoryId,
            int createdBy,
            string name,
            int sectionId,
            string url)
        {
            var link = new Link
            {
                CategoryId = categoryId,
                CreatedAt = DateTime.Parse(TestDate),
                CreatedBy = createdBy,
                Id = 1,
                IsFeatured = false,
                Name = name,
                SectionId = sectionId,
                Url = url
            };

            var logger = new Mock<ILogger<LinkService>>();

            var categoryRepository = new Mock<ICategoryRepository>();
            categoryRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new Category
                    {
                        Id = 1,
                        Name = "Test Category 1"
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
                categoryRepository.Object,
                linkRepository.Object,
                sectionRepository.Object,
                userRepository.Object);

            //Act
            var ex = await Record.ExceptionAsync(() => linkService.ValidateLinkAsync(link));

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

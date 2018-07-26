using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Exceptions;
using Xunit;

namespace Ocuda.test.Ops.Service.Test
{
    public class SiteSettingService_Tests
    {
        private const string TestDate = "2018-07-20";

        [Theory]
        [InlineData(true, false, 1, "key", "Name", 0, "true")]      //Valid bool
        [InlineData(true, false, 1, "key", "Name", 1, "1234")]      //Valid int
        [InlineData(true, false, 1, "key", "Name", 2, "string")]    //Valid string
        [InlineData(false, false, 1, "key", "Name", 0, "string")]   //Invalid bool value
        [InlineData(false, false, 1, "key", "Name", 1, "string")]   //Invalid int value
        [InlineData(false, false, 1, "key", "Name", 0, null)]       //Invalid null bool value
        [InlineData(false, false, 1, "key", "Name", 1, null)]       //Invalid null int value
        [InlineData(false, false, 1, "key", "Name", 2, null)]       //Invalid null string value
        [InlineData(false, false, 1, null, "Name", 2, "string")]    //Invalid null key
        [InlineData(false, false, 1, "key", null, 2, "string")]     //Invalid null name
        [InlineData(false, true, 1, "key", "Name", 2, "string")]    //Invalid duplicate key
        [InlineData(false, false, -1, "key", "Name", 2, "string")]  //Invalid CreatedBy
        [InlineData(false, false, 1, "key", "Name", -1, "string")]  //Invalid SiteSettingType
        public async Task ValidateSiteSetting_ThrowsOcudaExceptions(
            bool isValidInput,
            bool isDuplicateKey,
            int createdBy,
            string key,
            string name,
            int type,
            string value)
        {
            var siteSetting = new SiteSetting
            {
                Category = "Category",
                CreatedAt = DateTime.Parse(TestDate),
                CreatedBy = createdBy,
                Description = "Description",
                Id = 1,
                Key = key,
                Name = name,
                Type = (SiteSettingType)type,
                Value = value
            };

            var logger = new Mock<ILogger<SiteSettingService>>();
            var cache = new Mock<IDistributedCache>();
            var config = new Mock<IConfiguration>();
            var siteSettingRepository = new Mock<ISiteSettingRepository>();
            siteSettingRepository.Setup(m => m.IsDuplicateKey(siteSetting.Key)).ReturnsAsync(isDuplicateKey);

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new User
                    {
                        Id = 1,
                        Name = "Test User 1"
                    });

            var siteSettingService = new SiteSettingService(
                logger.Object,
                cache.Object,
                config.Object,
                siteSettingRepository.Object,
                userRepository.Object);

            //Act
            var ex = await Record.ExceptionAsync(() => 
                            siteSettingService.ValidateSiteSettingAsync(siteSetting));

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

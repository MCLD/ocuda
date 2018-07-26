using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Xunit;

namespace Ocuda.test.Ops.Service.Test
{
    public class SectionService_Tests
    {
        private const string TestDate = "2018-07-20";

        [Fact]
        public async Task EditAsync_SetCorrectFields()
        {
            var currentSection = new Section()
            {
                Id = 5,
                CreatedAt = DateTime.Parse("2018-06-24"),
                CreatedBy = 1,
                Name = "Test",
                Icon = "fa-test",
                Path = "Test"
            };
            var editedSection = new Section()
            {
                Id = 5,
                CreatedAt = DateTime.Parse("2016-11-22"),
                CreatedBy = 20,
                Name = "New Name",
                Icon = "fa-test2",
                Path = "TestPath"
            };
            var expectedSection = new Section()
            {
                Id = 5,
                CreatedAt = DateTime.Parse("2018-06-24"),
                CreatedBy = 1,
                Name = "New Name",
                Icon = "fa-test2",
                Path = "testpath"
            };

            var sectionRepository = new Mock<ISectionRepository>();
            sectionRepository.Setup(_ => _.FindAsync(5)).Returns(Task.FromResult(currentSection));

            var categoryLogger = new Mock<ILogger<CategoryService>>();
            var sectionLogger = new Mock<ILogger<SectionService>>();
            var categoryRepository = new Mock<ICategoryRepository>();
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new User
                    {
                        Id = 1,
                        Name = "Test User 1"
                    });

            var categoryService = new CategoryService(
                categoryLogger.Object, 
                categoryRepository.Object, 
                sectionRepository.Object,
                userRepository.Object);

            var service = new SectionService(
                sectionLogger.Object, 
                sectionRepository.Object, 
                userRepository.Object,
                categoryService);

            editedSection = await service.EditAsync(editedSection);

            Assert.Equal(expectedSection.Id, editedSection.Id);
            Assert.Equal(expectedSection.CreatedAt, editedSection.CreatedAt);
            Assert.Equal(expectedSection.CreatedBy, editedSection.CreatedBy);
            Assert.Equal(expectedSection.Name, editedSection.Name);
            Assert.Equal(expectedSection.Icon, editedSection.Icon);
            Assert.Equal(expectedSection.Path, editedSection.Path);
        }

        [Theory]
        [InlineData(true, false, false, false, 1, 2, "Name", "path")]    //Valid Section
        [InlineData(true, false, false, true, 1, 2, "Name", "path")]     //Valid Section (No Existing Default)
        [InlineData(true, false, false, false, 1, 1, "Name", null)]      //Valid Default Section
        [InlineData(true, false, false, true, 1, 1, "Name", null)]       //Valid Default Section (No Existing Default)
        [InlineData(false, false, false, false, -1, 2, "Name", "path")]  //Invalid CreatedBy
        [InlineData(false, false, false, false, 1, 2, null, "path")]     //Invalid Null Name
        [InlineData(false, false, false, false, 1, 2, "Name", null)]     //Invalid Null Path
        [InlineData(false, true, false, false, 1, 2, "Name", "path")]    //Invalid Duplicate Name
        [InlineData(false, false, true, false, 1, 2, "Name", "path")]    //Invalid Duplicate Path
        public async Task ValidateSection_ThrowsOcudaExceptions(
            bool isValidInput,
            bool isDuplicateName,
            bool isDuplicatePath,
            bool noExistingDefault,
            int createdBy,
            int id,
            string name,
            string path)
        {
            var section = new Section
            {
                CreatedAt = DateTime.Parse(TestDate),
                CreatedBy = createdBy,
                Icon = "icon",
                Id = id,
                Name = name,
                Path = path,
                SortOrder = 1
            };

            var logger = new Mock<ILogger<SectionService>>();

            var sectionRepository = new Mock<ISectionRepository>();

            if (noExistingDefault)
            {
                sectionRepository.Setup(m => m.GetDefaultSectionAsync()).ReturnsAsync(null, TimeSpan.FromMilliseconds(1));
            }
            else
            {
                sectionRepository.Setup(
                    m => m.GetDefaultSectionAsync())
                        .ReturnsAsync(new Section
                        {
                            Id = 1,
                            Name = "Test Default Section",
                            Path = null
                        });
            }

            sectionRepository.Setup(m => m.IsDuplicateNameAsync(section.Name)).ReturnsAsync(isDuplicateName);
            sectionRepository.Setup(m => m.IsDuplicatePathAsync(section.Path)).ReturnsAsync(isDuplicatePath);

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new User
                    {
                        Id = 1,
                        Name = "Test User 1"
                    });

            var categoryService = new Mock<ICategoryService>();

            var sectionService = new SectionService(
                logger.Object,
                sectionRepository.Object,
                userRepository.Object,
                categoryService.Object);

            //Act
            var ex = await Record.ExceptionAsync(() => sectionService.ValidateSectionAsync(section));

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

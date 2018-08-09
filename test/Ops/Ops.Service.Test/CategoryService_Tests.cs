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
    public class CategoryService_Tests
    {
        private const string TestDate = "2018-07-20";

        [Theory]
        [InlineData(true, false, 0, 1, false, "Test", 1)]        // Valid File
        [InlineData(true, false, 1, 1, false, "Test", 1)]        // Valid Link
        [InlineData(true, false, 0, 1, true, "Test", 1)]         // Valid Default File
        [InlineData(true, false, 1, 1, true, "Test", 1)]         // Valid Default Link
        [InlineData(false, false, -1, 1, false, "Test", 1)]      // Invalid CategoryType
        [InlineData(false, false, 1, -1, false, "Test", 1)]      // Invalid CreatedBy
        [InlineData(false, false, 0, 1, false, "Test", -1)]      // Invalid File SectionId
        [InlineData(false, false, 0, 1, false, null, 1)]         // Invalid File Null Name
        [InlineData(false, true, 0, 1, false, "Test", 1)]        // Invalid File Duplicate Name
        [InlineData(false, false, 1, 1, false, "Test", -1)]      // Invalid Link SectionId
        [InlineData(false, false, 1, 1, false, null, 1)]         // Invalid Link Null Name
        [InlineData(false, true, 1, 1, false, "Test", 1)]        // Invalid Link Duplicate Name
        public async Task ValidateCategory_ThrowsOcudaExceptions(
            bool isValidInput,
            bool isDuplicate,
            int categoryType,
            int createdBy, 
            bool isDefault, 
            string name, 
            int sectionId)
        {
            //Arrange
            var category = new Category
            {
                CategoryType = (CategoryType)categoryType,
                CreatedAt = DateTime.Parse(TestDate),
                CreatedBy = createdBy,
                Id = 1,
                IsDefault = isDefault,
                Name = name,
                SectionId = sectionId
            };

            var logger = new Mock<ILogger<CategoryService>>();

            var categoryRepository = new Mock<ICategoryRepository>();
            categoryRepository.Setup(m => m.IsDuplicateAsync(category)).ReturnsAsync(isDuplicate);

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

            var categoryService = new CategoryService(
                logger.Object, 
                categoryRepository.Object, 
                sectionRepository.Object,
                userRepository.Object);

            //Act
            var ex = await Record.ExceptionAsync(() => categoryService.ValidateCategoryAsync(category));

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

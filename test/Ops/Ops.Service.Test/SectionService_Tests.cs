using System;
using System.Threading.Tasks;
using Moq;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Xunit;

namespace Ocuda.test.Ops.Service.Test
{
    public class SectionService_Tests
    {
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
                Path = "TestPath"
            };

            var sectionRepository = new Mock<ISectionRepository>();
            sectionRepository.Setup(_ => _.FindAsync(5)).Returns(Task.FromResult(currentSection));

            var categoryRepository = new Mock<ICategoryRepository>();
            var categoryService = new CategoryService(categoryRepository.Object);

            var service = new SectionService(sectionRepository.Object, categoryService);

            editedSection = await service.EditAsync(editedSection);

            Assert.Equal(expectedSection.Id, editedSection.Id);
            Assert.Equal(expectedSection.CreatedAt, editedSection.CreatedAt);
            Assert.Equal(expectedSection.CreatedBy, editedSection.CreatedBy);
            Assert.Equal(expectedSection.Name, editedSection.Name);
            Assert.Equal(expectedSection.Icon, editedSection.Icon);
            Assert.Equal(expectedSection.Path, editedSection.Path);
        }
    }
}

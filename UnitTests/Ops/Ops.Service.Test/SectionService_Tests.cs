using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops;
using Xunit;

namespace Ocuda.UnitTests.Ops.Service.Test
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

            var service = new SectionService(sectionRepository.Object);

            editedSection = await service.EditAsync(editedSection);

            Assert.Equal(editedSection.Id, expectedSection.Id);
            Assert.Equal(editedSection.CreatedAt, expectedSection.CreatedAt);
            Assert.Equal(editedSection.CreatedBy, expectedSection.CreatedBy);
            Assert.Equal(editedSection.Name, expectedSection.Name);
            Assert.Equal(editedSection.Icon, expectedSection.Icon);
            Assert.Equal(editedSection.Path, expectedSection.Path);
        }
    }
}

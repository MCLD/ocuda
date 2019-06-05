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
    public class PostService_Tests
    {
        private const string TestDate = "2018-07-20";

        [Theory]
        [InlineData(true, false, 1, 1, null, "test-stub", "Test Title")]       //Valid Draft
        [InlineData(true, true, 1, 1, null, "test-stub", "Test Title")]        //Valid Draft, StubInUse
        [InlineData(true, false, 1, 1, TestDate, "test-stub", "Test Title")]      //Valid Non-Draft
        [InlineData(false, true, 1, 1, TestDate, "test-stub", "Test Title")]      //Invalid StubInUse, Non-Draft
        public async Task ValidatePost_ThrowsOcudaExceptions(
            bool isValidInput,
            bool stubInUse,
            int createdBy,
            int postCategoryId,
            string publishedAt,
            string stub,
            string title)
        {
            var post = new Post
            {
                Content = "Content",
                CreatedAt = DateTime.Parse(TestDate),
                CreatedBy = createdBy,
                Id = 1,
                PostCategoryId = postCategoryId,
                PublishedAt = !string.IsNullOrWhiteSpace(publishedAt) ? DateTime.Parse(publishedAt) : default(DateTime?),
                Stub = stub,
                Title = title
            };

            var logger = new Mock<ILogger<PostService>>();

            var postCategoryRepository = new Mock<IPostCategoryRepository>();
            postCategoryRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new PostCategory
                    {
                        Id = 1,
                        Name = "Test Category 1"
                    });

            var postRepository = new Mock<IPostRepository>();
            postRepository.Setup(m => m.StubInUseAsync(post)).ReturnsAsync(stubInUse);

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

            var postService = new PostService(
                logger.Object,
                postCategoryRepository.Object,
                postRepository.Object,
                sectionRepository.Object,
                userRepository.Object);

            //Act
            var ex = await Record.ExceptionAsync(() => postService.ValidatePostAsync(post));

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

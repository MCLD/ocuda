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
    public class UserService_Tests
    {
        private const string TestDate = "2018-07-20";

        [Theory]
        [InlineData(true, false, false, 1, "test@email.com", "Name", 2, "Username")]    //Valid with SupervisorId
        [InlineData(true, false, false, 1, "test@email.com", "Name", null, "Username")] //Valid null SupervisorId
        [InlineData(false, false, false, 1, "test@email.com", "Name", 2, null)]         //Invalid null Username
        [InlineData(false, false, false, 1, "test@email.com", "Name", -1, "Username")]  //Invalid SupervisorId
        [InlineData(false, false, true, 1, "test@email.com", "Name", 2, "Username")]    //Invalid duplicate Email
        [InlineData(false, true, false, 1, "test@email.com", "Name", 2, "Username")]    //Invalid duplicate Username
        public async Task ValidateUser_ThrowsOcudaExceptions(
            bool isValidInput,
            bool isDuplicateUsername,
            bool isDuplicateEmail,
            int createdBy,
            string email,
            string name,
            int? supervisorId,
            string username)
        {
            //TODO Create rules for validating other fields
            var user = new User
            {
                CreatedAt = DateTime.Parse(TestDate),
                CreatedBy = createdBy,
                Email = email,
                Id = 3,
                IsSysadmin = false,
                LastLdapUpdate = DateTime.Parse(TestDate),
                LastRosterUpdate = DateTime.Parse(TestDate),
                LastSeen = DateTime.Parse(TestDate),
                Name = name,
                Nickname = "Nickname",
                Phone = "123-456-7890",
                ReauthenticateUser = false,
                SupervisorId = supervisorId,
                Title = "Title",
                Username = username
            };

            var logger = new Mock<ILogger<UserService>>();
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(m => m.IsDuplicateUsername(user)).ReturnsAsync(isDuplicateUsername);
            userRepository.Setup(m => m.IsDuplicateEmail(user)).ReturnsAsync(isDuplicateEmail);
            userRepository.Setup(
                m => m.FindAsync(1))
                    .ReturnsAsync(new User
                    {
                        Id = 1,
                        Name = "Administrator"
                    });
            userRepository.Setup(
                m => m.FindAsync(2))
                    .ReturnsAsync(new User
                    {
                        Id = 2,
                        Name = "Supervisor"
                    });

            var userService = new UserService(
                logger.Object,
                userRepository.Object);

            //Act
            var ex = await Record.ExceptionAsync(() => userService.ValidateUserAsync(user));

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

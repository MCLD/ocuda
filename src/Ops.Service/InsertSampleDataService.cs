using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class InsertSampleDataService : IInsertSampleDataService
    {
        private readonly IFileTypeRepository _fileTypeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserMetadataTypeRepository _userMetadataTypeRepository;

        public InsertSampleDataService(IFileTypeRepository fileTypeRepository,
            IUserMetadataTypeRepository userMetadataTypeRepository,
            IUserRepository userRepository)
        {
            _fileTypeRepository = fileTypeRepository
                ?? throw new ArgumentNullException(nameof(fileTypeRepository));
            _userMetadataTypeRepository = userMetadataTypeRepository
                ?? throw new ArgumentNullException(nameof(userMetadataTypeRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        private User _systemAdministrator;

        private User SystemAdministrator
        {
            get
            {
                return _systemAdministrator
                    ?? (_systemAdministrator = _userRepository.GetSystemAdministratorAsync().Result);
            }
        }

        public async Task InsertDataAsync()
        {
            await InsertUserMetadataTypesAsync();
            await InsertUsersAsync();
            await InsertFileTypesAsync();
        }

        public async Task InsertUserMetadataTypesAsync()
        {
            await _userMetadataTypeRepository.AddAsync(new UserMetadataType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                IsPublic = false,
                Name = "Barcode"
            });

            await _userMetadataTypeRepository.AddAsync(new UserMetadataType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                IsPublic = true,
                Name = "Favorite Book"
            });
        }

        public async Task InsertUsersAsync()
        {
            var user1 = new User
            {
                Username = "dumbledore",
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                IsSysadmin = false,
                Name = "Albus Dumbledore",
                Nickname = null,
                Title = "Headmaster",
                Email = "dumbledore@hogwarts.edu",
                Phone = "(123) 456-7890",
                SupervisorId = null
            };

            var user2 = new User
            {
                Username = "snape",
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                IsSysadmin = false,
                Name = "Severus Snape",
                Nickname = "Snape",
                Title = "Potions Master",
                Email = "snape@hogwarts.edu",
                Phone = "(456) 789-0123",
                Supervisor = user1
            };

            var user3 = new User
            {
                Username = "potter",
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                IsSysadmin = false,
                Name = "Harry Potter",
                Nickname = "Harry",
                Title = "Student",
                Email = "potter@hogwarts.edu",
                Phone = "(890) 123-4567",
                Supervisor = user2
            };

            await _userRepository.AddAsync(user1);
            await _userRepository.AddAsync(user2);
            await _userRepository.AddAsync(user3);

            await _userRepository.SaveAsync();
        }

        public async Task<ICollection<FileType>> InsertFileTypesAsync()
        {
            var fileTypes = new List<FileType>
            {
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".zip",
                    Icon = "fa-regular fa-file-zipper"
                    },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".mp3",
                    Icon = "fa-regular fa-file-audio"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".cs",
                    Icon = "fa-regular fa-file-code"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".jpg",
                    Icon = "fa-regular fa-file-image"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".xlsx",
                    Icon = "fa-regular fa-file-excel alert-success"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".pptx",
                    Icon = "fa-regular fa-file-powerpoint"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".docx",
                    Icon = "fa-regular fa-file-word alert-primary"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".pdf",
                    Icon = "fa-regular fa-file-pdf alert-danger"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".txt",
                    Icon = "fa-regular fa-file-lines"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".mp4",
                    Icon = "fa-regular fa-file-video"
                }
            };

            await _fileTypeRepository.AddRangeAsync(fileTypes);
            await _fileTypeRepository.SaveAsync();

            return fileTypes;
        }
    }
}

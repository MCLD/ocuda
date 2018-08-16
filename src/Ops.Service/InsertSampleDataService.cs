using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class InsertSampleDataService : IInsertSampleDataService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IFileTypeRepository _fileTypeRepository;
        private readonly ILinkRepository _linkRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IPostRepository _postRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISiteSettingRepository _siteSettingRepository;
        private readonly ICategoryService _categoryService;

        public InsertSampleDataService(ICategoryRepository categoryRepository,
            IFileRepository fileRepository,
            IFileTypeRepository fileTypeRepository,
            ILinkRepository linkRepository,
            IPageRepository pageRepository,
            IPostRepository postRepository,
            ISectionRepository sectionRepository,
            IUserRepository userRepository,
            ISiteSettingRepository siteSettingRepository,
            ICategoryService categoryService)
        {
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
            _fileTypeRepository = fileTypeRepository
                ?? throw new ArgumentNullException(nameof(fileTypeRepository));
            _linkRepository = linkRepository
                ?? throw new ArgumentNullException(nameof(linkRepository));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _postRepository = postRepository
                ?? throw new ArgumentNullException(nameof(postRepository));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _siteSettingRepository = siteSettingRepository
                ?? throw new ArgumentNullException(nameof(siteSettingRepository));
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
        }

        private User _systemAdministrator;
        private User SystemAdministrator
        {
            get
            {
                if (_systemAdministrator == null)
                {
                    _systemAdministrator = _userRepository.GetSystemAdministratorAsync().Result;
                }
                return _systemAdministrator;
            }
        }

        public async Task InsertDataAsync()
        {
            var sections = await InsertSectionsAsync();
            var defaultSection = await _sectionRepository.GetDefaultSectionAsync();
            sections.Add(defaultSection);

            await InsertFileTypesAsync();

            foreach (var section in sections)
            {
                await InsertFileCategoriesAsync(section);
                await InsertLinkCategoriesAsync(section);
                await InsertPostsAsync(section.Id);
                await InsertLinksAsync(section.Id);
                await InsertPagesAsync(section.Id);
            }

            await InsertUsersAsync();
        }

        public async Task<ICollection<Section>> InsertSectionsAsync()
        {
            // seed data
            var sections = new List<Section>
            {
                new Section
                {
                    Icon = "fa-smile",
                    Name = "Human Resources",
                    Path = "HR",
                    SortOrder = 1,
                    CreatedBy = SystemAdministrator.Id,
                    CreatedAt = DateTime.Now
                },
                new Section
                {
                    Icon = "fa-users",
                    Name = "Operations",
                    Path = "Operations",
                    SortOrder = 2,
                    CreatedBy = SystemAdministrator.Id,
                    CreatedAt = DateTime.Now
                },
                new Section
                {
                    Icon = "fa-comments",
                    Name = "Communications",
                    Path = "Communications",
                    SortOrder = 3,
                    CreatedBy = SystemAdministrator.Id,
                    CreatedAt = DateTime.Now
                },
                new Section
                {
                    Icon = "fa-thumbs-up",
                    Name = "Services",
                    Path = "Services",
                    SortOrder = 4,
                    CreatedBy = SystemAdministrator.Id,
                    CreatedAt = DateTime.Now
                },
                new Section
                {
                    Icon = "fa-laptop",
                    Name = "IT",
                    Path = "IT",
                    SortOrder = 5,
                    CreatedBy = SystemAdministrator.Id,
                    CreatedAt = DateTime.Now
                }
            };

            foreach (var section in sections)
            {
                await _sectionRepository.AddAsync(section);
                await _categoryService.CreateDefaultCategories(SystemAdministrator.Id, section);
                await _sectionRepository.SaveAsync();
            }

            return sections;
        }

        public async Task InsertPostsAsync(int sectionId)
        {
            // seed data
            await _postRepository.AddAsync(new Post
            {
                Content = "Sed semper, sapien quis luctus semper, nibh eros sollicitudin tellus, at tincidunt arcu odio a est. Nam nec nulla ex. Nullam et maximus ex, at porttitor velit. Sed ac justo ligula. Morbi sed lectus turpis. Aenean suscipit tellus nec risus aliquam, et dignissim urna mollis. Aliquam erat volutpat. Curabitur risus tellus, facilisis a tempus eu, hendrerit ut elit. Phasellus ut quam consequat, molestie mauris non, faucibus felis. Pellentesque finibus lobortis arcu, a tincidunt erat pulvinar vel. Proin in egestas magna, nec feugiat velit.",
                CreatedAt = DateTime.Parse("2018-06-04 12:15"),
                IsPinned = false,
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Post 1",
                Stub = "test-post-1",
                SectionId = sectionId
            });
            await _postRepository.AddAsync(new Post
            {
                Content = "Pellentesque sit amet risus eu lorem elementum porttitor. Ut pretium facilisis finibus. Suspendisse potenti. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis vestibulum tortor blandit, imperdiet sem et, venenatis lectus. Sed id metus magna. Etiam vel congue sapien, nec blandit lectus. Etiam feugiat est ornare quam sollicitudin, et luctus neque efficitur. Nullam quis pretium ipsum. Pellentesque eleifend quam vitae laoreet varius. Integer fringilla velit in metus finibus, a aliquet lectus tincidunt. Pellentesque varius eleifend est, id maximus nunc tristique ac.",
                CreatedAt = DateTime.Parse("2018-06-04 13:30"),
                IsPinned = false,
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Post 2",
                Stub = "test-post-2",
                SectionId = sectionId
            });
            await _postRepository.AddAsync(new Post
            {
                Content = "Nam dignissim porta leo vitae sodales. Morbi mollis, libero vitae sagittis tincidunt, mi dui luctus metus, elementum tincidunt nisl nisl at elit. Nulla tellus elit, aliquam in pulvinar id, interdum in velit. Suspendisse non aliquam dolor, vestibulum lacinia est. Pellentesque placerat nibh blandit, gravida nulla sed, tristique nisi. Nunc volutpat ultrices augue et congue. In in lacus condimentum dui finibus sagittis nec ut neque. Aenean accumsan nisl quis convallis rutrum. Cras vel imperdiet est. Curabitur et sagittis dui.",
                CreatedAt = DateTime.Parse("2018-06-04 15:00"),
                IsPinned = true,
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Post 3",
                Stub = "test-post-3",
                SectionId = sectionId
            });
            await _postRepository.AddAsync(new Post
            {
                Content = "Ut auctor risus diam, sed aliquam quam iaculis ac. Sed rutrum tortor eget ante consequat, ac malesuada ligula dictum. Phasellus non urna interdum, vehicula augue ac, egestas orci. Sed ut nisl ipsum. Donec hendrerit, nisl vitae interdum pretium, ligula lorem varius nisi, non cursus libero libero eu dolor. Fusce bibendum, lorem sed tempor condimentum, enim ante sollicitudin dolor, faucibus viverra quam erat ac neque. Integer sagittis magna eu augue eleifend, at pellentesque diam malesuada.",
                CreatedAt = DateTime.Parse("2018-06-04 17:25"),
                IsPinned = false,
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Post 4",
                Stub = "test-post-4",
                SectionId = sectionId
            });

            await _postRepository.SaveAsync();
        }


        public async Task<ICollection<Category>> InsertLinkCategoriesAsync(Section section)
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Name = $"{section.Name} Link Category 1",
                    CategoryType = CategoryType.Link,
                    CreatedAt = DateTime.Parse("2018-06-04"),
                    CreatedBy = SystemAdministrator.Id,
                    SectionId = section.Id,
                    IsDefault = false
                },

                new Category
                {
                    Name = $"{section.Name} Link Category 2",
                    CategoryType = CategoryType.Link,
                    CreatedAt = DateTime.Parse("2018-06-05"),
                    CreatedBy = SystemAdministrator.Id,
                    SectionId = section.Id,
                    IsDefault = false
                }
            };

            foreach (var category in categories)
            {
                await _categoryRepository.AddAsync(category);
            }

            await _categoryRepository.SaveAsync();

            return categories;
        }

        public async Task InsertLinksAsync(int sectionId)
        {
            var filter = new BlogFilter
            {
                CategoryType = CategoryType.Link,
                SectionId = sectionId
            };

            var defaultCategory = await _categoryRepository.GetDefaultAsync(filter);

            await _linkRepository.AddAsync(new Link
            {
                Url = "https://maricopacountyreads.org/",
                Name = "Summer Reading",
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now,
                CategoryId = defaultCategory.Id,
                SectionId = sectionId
            });
            await _linkRepository.AddAsync(new Link
            {
                Url = "#",
                Name = "Reading Adventure",
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now,
                CategoryId = defaultCategory.Id,
                SectionId = sectionId
            });
            await _linkRepository.AddAsync(new Link
            {
                Url = "#",
                Name = "Find Libraries",
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now,
                CategoryId = defaultCategory.Id,
                SectionId = sectionId
            });

            await _linkRepository.SaveAsync();
        }

        public async Task<ICollection<Category>> InsertFileCategoriesAsync(Section section)
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Name = $"{section.Name} File Category 1",
                    CategoryType = CategoryType.File,
                    CreatedAt = DateTime.Parse("2018-06-04"),
                    CreatedBy = SystemAdministrator.Id,
                    SectionId = section.Id,
                    IsDefault = false
                },

                new Category
                {
                    Name = $"{section.Name} File Category 2",
                    CategoryType = CategoryType.File,
                    CreatedAt = DateTime.Parse("2018-06-05"),
                    CreatedBy = SystemAdministrator.Id,
                    SectionId = section.Id,
                    IsDefault = false,
                    ThumbnailRequired = true
                }
            };

            foreach (var category in categories)
            {
                await _categoryRepository.AddAsync(category);
            }

            await _categoryRepository.SaveAsync();

            return categories;
        }

        public async Task InsertFileTypesAsync()
        {
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = null,
                Icon = "far fa-file",
                IsDefault = true
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".zip",
                Icon = "fa fa-file-archive",
                IsDefault = false
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".mp3",
                Icon = "fa fa-file-audio",
                IsDefault = false
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".cs",
                Icon = "fa fa-file-code",
                IsDefault = false
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".jpg",
                Icon = "fa fa-file-image",
                IsDefault = false
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".xlsx",
                Icon = "fa fa-file-excel alert-success",
                IsDefault = false
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".pptx",
                Icon = "fa fa-file-powerpoint",
                IsDefault = false
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".docx",
                Icon = "fa fa-file-word alert-primary",
                IsDefault = false
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".pdf",
                Icon = "fa fa-file-pdf alert-danger",
                IsDefault = false
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".txt",
                Icon = "fa fa-file-alt",
                IsDefault = false
            });
            await _fileTypeRepository.AddAsync(new FileType
            {
                CreatedAt = DateTime.Now,
                CreatedBy = SystemAdministrator.Id,
                Extension = ".mp4",
                Icon = "fa fa-file-video",
                IsDefault = false
            });
        }

        public async Task InsertPagesAsync(int sectionId)
        {
            // seed data
            await _pageRepository.AddAsync(new Page
            {
                Content = "Sed semper, sapien quis luctus semper, nibh eros sollicitudin tellus, at tincidunt arcu odio a est. Nam nec nulla ex. Nullam et maximus ex, at porttitor velit. Sed ac justo ligula. Morbi sed lectus turpis. Aenean suscipit tellus nec risus aliquam, et dignissim urna mollis. Aliquam erat volutpat. Curabitur risus tellus, facilisis a tempus eu, hendrerit ut elit. Phasellus ut quam consequat, molestie mauris non, faucibus felis. Pellentesque finibus lobortis arcu, a tincidunt erat pulvinar vel. Proin in egestas magna, nec feugiat velit.",
                CreatedAt = DateTime.Parse("2018-06-04 12:15"),
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Page 1",
                Stub = "test-page-1",
                SectionId = sectionId
            });
            await _pageRepository.AddAsync(new Page
            {
                Content = "Pellentesque sit amet risus eu lorem elementum porttitor. Ut pretium facilisis finibus. Suspendisse potenti. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis vestibulum tortor blandit, imperdiet sem et, venenatis lectus. Sed id metus magna. Etiam vel congue sapien, nec blandit lectus. Etiam feugiat est ornare quam sollicitudin, et luctus neque efficitur. Nullam quis pretium ipsum. Pellentesque eleifend quam vitae laoreet varius. Integer fringilla velit in metus finibus, a aliquet lectus tincidunt. Pellentesque varius eleifend est, id maximus nunc tristique ac.",
                CreatedAt = DateTime.Parse("2018-06-04 13:30"),
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Page 2",
                Stub = "test-page-2",
                SectionId = sectionId
            });
            await _pageRepository.AddAsync(new Page
            {
                Content = "Nam dignissim porta leo vitae sodales. Morbi mollis, libero vitae sagittis tincidunt, mi dui luctus metus, elementum tincidunt nisl nisl at elit. Nulla tellus elit, aliquam in pulvinar id, interdum in velit. Suspendisse non aliquam dolor, vestibulum lacinia est. Pellentesque placerat nibh blandit, gravida nulla sed, tristique nisi. Nunc volutpat ultrices augue et congue. In in lacus condimentum dui finibus sagittis nec ut neque. Aenean accumsan nisl quis convallis rutrum. Cras vel imperdiet est. Curabitur et sagittis dui.",
                CreatedAt = DateTime.Parse("2018-06-04 15:00"),
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Page 3",
                Stub = "test-page-3",
                SectionId = sectionId
            });
            await _pageRepository.AddAsync(new Page
            {
                Content = "Ut auctor risus diam, sed aliquam quam iaculis ac. Sed rutrum tortor eget ante consequat, ac malesuada ligula dictum. Phasellus non urna interdum, vehicula augue ac, egestas orci. Sed ut nisl ipsum. Donec hendrerit, nisl vitae interdum pretium, ligula lorem varius nisi, non cursus libero libero eu dolor. Fusce bibendum, lorem sed tempor condimentum, enim ante sollicitudin dolor, faucibus viverra quam erat ac neque. Integer sagittis magna eu augue eleifend, at pellentesque diam malesuada.",
                CreatedAt = DateTime.Parse("2018-06-04 17:25"),
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Page 4",
                Stub = "test-page-4",
                SectionId = sectionId
            });

            await _pageRepository.SaveAsync();
        }

        public async Task InsertUsersAsync()
        {
            await _userRepository.AddAsync(new User
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
            });

            await _userRepository.AddAsync(new User
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
                SupervisorId = 3
            });

            await _userRepository.AddAsync(new User
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
                SupervisorId = 4
            });

            await _userRepository.SaveAsync();
        }

        public IEnumerable<Calendar> GetCalendars()
        {
            // TODO repository/database
            return new List<Calendar>
            {
                new Calendar
                {
                    IsPinned = true,
                    Name = "Staff Training",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-19 10:00"),
                    CreatedBy = 1,
                    CreatedAt = DateTime.Now,
                },
                new Calendar
                {
                    Name = "Fun Event!",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-08 12:00"),
                    CreatedBy = 1,
                    CreatedAt = DateTime.Now,
                },
                new Calendar
                {
                    Name = "Important Date Reminder",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-12 9:00"),
                    CreatedBy = 1,
                    CreatedAt = DateTime.Now,
                }
            };
        }
    }
}

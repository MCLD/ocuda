using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class InsertSampleDataService : IInsertSampleDataService
    {
        private readonly IFileLibraryRepository _fileLibraryRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IFileTypeRepository _fileTypeRepository;
        private readonly ILinkLibraryRepository _linkLibraryRepository;
        private readonly ILinkRepository _linkRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IPostCategoryRepository _postCategoryRepository;
        private readonly IPostRepository _postRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserMetadataTypeRepository _userMetadataTypeRepository;
        private readonly ISiteSettingRepository _siteSettingRepository;

        public InsertSampleDataService(IFileLibraryRepository fileLibraryRepository,
            IFileRepository fileRepository,
            IFileTypeRepository fileTypeRepository,
            ILinkLibraryRepository linkLibraryRepository,
            ILinkRepository linkRepository,
            IPageRepository pageRepository,
            IPostCategoryRepository postCategoryRepository,
            IPostRepository postRepository,
            ISectionRepository sectionRepository,
            IUserMetadataTypeRepository userMetadataTypeRepository,
            IUserRepository userRepository,
            ISiteSettingRepository siteSettingRepository)
        {
            _fileLibraryRepository = fileLibraryRepository
                ?? throw new ArgumentNullException(nameof(fileLibraryRepository));
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
            _fileTypeRepository = fileTypeRepository
                ?? throw new ArgumentNullException(nameof(fileTypeRepository));
            _linkLibraryRepository = linkLibraryRepository
                ?? throw new ArgumentNullException(nameof(linkLibraryRepository));
            _linkRepository = linkRepository
                ?? throw new ArgumentNullException(nameof(linkRepository));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _postCategoryRepository = postCategoryRepository
                ?? throw new ArgumentNullException(nameof(postCategoryRepository));
            _postRepository = postRepository
                ?? throw new ArgumentNullException(nameof(postRepository));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _userMetadataTypeRepository = userMetadataTypeRepository
                ?? throw new ArgumentNullException(nameof(userMetadataTypeRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
            _siteSettingRepository = siteSettingRepository
                ?? throw new ArgumentNullException(nameof(siteSettingRepository));
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

            await InsertUserMetadataTypesAsync();
            await InsertUsersAsync();
            var fileTypes = await InsertFileTypesAsync();

            foreach (var section in sections)
            {
                await InsertPagesAsync(section.Id);

                var postCategories = await InsertPostCategoriesAsync(section.Id);
                foreach (var category in postCategories)
                {
                    await InsertPostsAsync(category.Id);
                }

                await InsertFileLibrariesAsync(section, fileTypes);

                var linkLibraries = await InsertLinkLibrariesAsync(section);
                foreach (var library in linkLibraries)
                {
                    await InsertLinksAsync(library.Id);
                }
            }
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

            await _sectionRepository.AddRangeAsync(sections);
            await _sectionRepository.SaveAsync();

            return sections;
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
                    Icon = "fa fa-file-archive"
                    },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".mp3",
                    Icon = "fa fa-file-audio"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".cs",
                    Icon = "fa fa-file-code"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".jpg",
                    Icon = "fa fa-file-image"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".xlsx",
                    Icon = "fa fa-file-excel alert-success"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".pptx",
                    Icon = "fa fa-file-powerpoint"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".docx",
                    Icon = "fa fa-file-word alert-primary"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".pdf",
                    Icon = "fa fa-file-pdf alert-danger"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".txt",
                    Icon = "fa fa-file-alt"
                },
                new FileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Extension = ".mp4",
                    Icon = "fa fa-file-video"
                }
            };

            await _fileTypeRepository.AddRangeAsync(fileTypes);
            await _fileTypeRepository.SaveAsync();

            return fileTypes;
        }

        public async Task InsertPagesAsync(int sectionId)
        {
            var dumbledore = await _userRepository.FindByUsernameAsync("dumbledore");
            var snape = await _userRepository.FindByUsernameAsync("snape");
            var potter = await _userRepository.FindByUsernameAsync("potter");

            var pages = new List<Page>
            {
                new Page
                {
                    Content = "Sed semper, sapien quis luctus semper, nibh eros sollicitudin tellus, at tincidunt arcu odio a est. Nam nec nulla ex. Nullam et maximus ex, at porttitor velit. Sed ac justo ligula. Morbi sed lectus turpis. Aenean suscipit tellus nec risus aliquam, et dignissim urna mollis. Aliquam erat volutpat. Curabitur risus tellus, facilisis a tempus eu, hendrerit ut elit. Phasellus ut quam consequat, molestie mauris non, faucibus felis. Pellentesque finibus lobortis arcu, a tincidunt erat pulvinar vel. Proin in egestas magna, nec feugiat velit.",
                    CreatedAt = DateTime.Parse("2018-06-04 12:15"),
                    CreatedBy = dumbledore.Id,
                    Title = "Test Page 1",
                    Stub = "test-page-1",
                    SectionId = sectionId
                },
                new Page
                {
                    Content = "Pellentesque sit amet risus eu lorem elementum porttitor. Ut pretium facilisis finibus. Suspendisse potenti. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis vestibulum tortor blandit, imperdiet sem et, venenatis lectus. Sed id metus magna. Etiam vel congue sapien, nec blandit lectus. Etiam feugiat est ornare quam sollicitudin, et luctus neque efficitur. Nullam quis pretium ipsum. Pellentesque eleifend quam vitae laoreet varius. Integer fringilla velit in metus finibus, a aliquet lectus tincidunt. Pellentesque varius eleifend est, id maximus nunc tristique ac.",
                    CreatedAt = DateTime.Parse("2018-06-04 13:30"),
                    CreatedBy = snape.Id,
                    Title = "Test Page 2",
                    Stub = "test-page-2",
                    SectionId = sectionId
                },
                new Page
                {
                    Content = "Nam dignissim porta leo vitae sodales. Morbi mollis, libero vitae sagittis tincidunt, mi dui luctus metus, elementum tincidunt nisl nisl at elit. Nulla tellus elit, aliquam in pulvinar id, interdum in velit. Suspendisse non aliquam dolor, vestibulum lacinia est. Pellentesque placerat nibh blandit, gravida nulla sed, tristique nisi. Nunc volutpat ultrices augue et congue. In in lacus condimentum dui finibus sagittis nec ut neque. Aenean accumsan nisl quis convallis rutrum. Cras vel imperdiet est. Curabitur et sagittis dui.",
                    CreatedAt = DateTime.Parse("2018-06-04 15:00"),
                    CreatedBy = potter.Id,
                    Title = "Test Page 3",
                    Stub = "test-page-3",
                    SectionId = sectionId
                },
                new Page
                {
                    Content = "Ut auctor risus diam, sed aliquam quam iaculis ac. Sed rutrum tortor eget ante consequat, ac malesuada ligula dictum. Phasellus non urna interdum, vehicula augue ac, egestas orci. Sed ut nisl ipsum. Donec hendrerit, nisl vitae interdum pretium, ligula lorem varius nisi, non cursus libero libero eu dolor. Fusce bibendum, lorem sed tempor condimentum, enim ante sollicitudin dolor, faucibus viverra quam erat ac neque. Integer sagittis magna eu augue eleifend, at pellentesque diam malesuada.",
                    CreatedAt = DateTime.Parse("2018-06-04 17:25"),
                    CreatedBy = dumbledore.Id,
                    Title = "Test Page 4",
                    Stub = "test-page-4",
                    SectionId = sectionId
                }
            };

            await _pageRepository.AddRangeAsync(pages);
            await _pageRepository.SaveAsync();
        }

        public async Task<ICollection<PostCategory>> InsertPostCategoriesAsync(int sectionId)
        {
            var categories = new List<PostCategory>
            {
                new PostCategory
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = _systemAdministrator.Id,
                    Name = "Post Category 1",
                    SectionId = sectionId
                },
                new PostCategory
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = _systemAdministrator.Id,
                    Name = "Post Category 2",
                    SectionId = sectionId
                }
            };

            await _postCategoryRepository.AddRangeAsync(categories);
            await _postCategoryRepository.SaveAsync();

            return categories;
        }

        public async Task InsertPostsAsync(int categoryId)
        {
            var dumbledore = await _userRepository.FindByUsernameAsync("dumbledore");
            var snape = await _userRepository.FindByUsernameAsync("snape");
            var potter = await _userRepository.FindByUsernameAsync("potter");


            var posts = new List<Post>
            {
                new Post
                {
                    Content = "Sed semper, sapien quis luctus semper, nibh eros sollicitudin tellus, at tincidunt arcu odio a est. Nam nec nulla ex. Nullam et maximus ex, at porttitor velit. Sed ac justo ligula. Morbi sed lectus turpis. Aenean suscipit tellus nec risus aliquam, et dignissim urna mollis. Aliquam erat volutpat. Curabitur risus tellus, facilisis a tempus eu, hendrerit ut elit. Phasellus ut quam consequat, molestie mauris non, faucibus felis. Pellentesque finibus lobortis arcu, a tincidunt erat pulvinar vel. Proin in egestas magna, nec feugiat velit.",
                    CreatedAt = DateTime.Parse("2018-06-04 12:15"),
                    IsPinned = false,
                    CreatedBy = dumbledore.Id,
                    Title = "Test Post 1",
                    Stub = "test-post-1",
                    PostCategoryId = categoryId
                },
                new Post
                {
                    Content = "Pellentesque sit amet risus eu lorem elementum porttitor. Ut pretium facilisis finibus. Suspendisse potenti. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis vestibulum tortor blandit, imperdiet sem et, venenatis lectus. Sed id metus magna. Etiam vel congue sapien, nec blandit lectus. Etiam feugiat est ornare quam sollicitudin, et luctus neque efficitur. Nullam quis pretium ipsum. Pellentesque eleifend quam vitae laoreet varius. Integer fringilla velit in metus finibus, a aliquet lectus tincidunt. Pellentesque varius eleifend est, id maximus nunc tristique ac.",
                    CreatedAt = DateTime.Parse("2018-06-04 13:30"),
                    IsPinned = false,
                    CreatedBy = snape.Id,
                    Title = "Test Post 2",
                    Stub = "test-post-2",
                    PostCategoryId = categoryId
                },
                new Post
                {
                    Content = "Nam dignissim porta leo vitae sodales. Morbi mollis, libero vitae sagittis tincidunt, mi dui luctus metus, elementum tincidunt nisl nisl at elit. Nulla tellus elit, aliquam in pulvinar id, interdum in velit. Suspendisse non aliquam dolor, vestibulum lacinia est. Pellentesque placerat nibh blandit, gravida nulla sed, tristique nisi. Nunc volutpat ultrices augue et congue. In in lacus condimentum dui finibus sagittis nec ut neque. Aenean accumsan nisl quis convallis rutrum. Cras vel imperdiet est. Curabitur et sagittis dui.",
                    CreatedAt = DateTime.Parse("2018-06-04 15:00"),
                    IsPinned = true,
                    CreatedBy = potter.Id,
                    Title = "Test Post 3",
                    Stub = "test-post-3",
                    PostCategoryId = categoryId
                },
                new Post
                {
                    Content = "Ut auctor risus diam, sed aliquam quam iaculis ac. Sed rutrum tortor eget ante consequat, ac malesuada ligula dictum. Phasellus non urna interdum, vehicula augue ac, egestas orci. Sed ut nisl ipsum. Donec hendrerit, nisl vitae interdum pretium, ligula lorem varius nisi, non cursus libero libero eu dolor. Fusce bibendum, lorem sed tempor condimentum, enim ante sollicitudin dolor, faucibus viverra quam erat ac neque. Integer sagittis magna eu augue eleifend, at pellentesque diam malesuada.",
                    CreatedAt = DateTime.Parse("2018-06-04 17:25"),
                    IsPinned = false,
                    CreatedBy = dumbledore.Id,
                    Title = "Test Post 4",
                    Stub = "test-post-4",
                    PostCategoryId = categoryId
                }
            };

            await _postRepository.AddRangeAsync(posts);
            await _postRepository.SaveAsync();
        }

        public async Task InsertFileLibrariesAsync(Section section, ICollection<FileType> fileTypes)
        {
            var libraries = new List<FileLibrary>
            {
                new FileLibrary
                {
                    Name = $"{section.Name} File Library 1",
                    CreatedAt = DateTime.Parse("2018-06-04"),
                    CreatedBy = SystemAdministrator.Id,
                    SectionId = section.Id,
                    FileTypes = fileTypes.Select(_ => new FileLibraryFileType{ FileType = _})
                        .ToList()
                },
                new FileLibrary
                {
                    Name = $"{section.Name} File Library 2",
                    CreatedAt = DateTime.Parse("2018-06-05"),
                    CreatedBy = SystemAdministrator.Id,
                    SectionId = section.Id,
                    FileTypes = fileTypes.Select(_ => new FileLibraryFileType{ FileType = _})
                        .ToList()
                }
            };

            await _fileLibraryRepository.AddRangeAsync(libraries);
            await _fileLibraryRepository.SaveAsync();
        }

        public async Task<ICollection<LinkLibrary>> InsertLinkLibrariesAsync(Section section)
        {
            var libraries = new List<LinkLibrary>
            {
                new LinkLibrary
                {
                    Name = "Navigation",
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    SectionId = section.Id,
                    IsNavigation = true
                },
                new LinkLibrary
                {
                    Name = $"{section.Name} Link Library 1",
                    CreatedAt = DateTime.Parse("2018-06-04"),
                    CreatedBy = SystemAdministrator.Id,
                    SectionId = section.Id
                },
                new LinkLibrary
                {
                    Name = $"{section.Name} Link Category 2",
                    CreatedAt = DateTime.Parse("2018-06-05"),
                    CreatedBy = SystemAdministrator.Id,
                    SectionId = section.Id
                }
            };

            await _linkLibraryRepository.AddRangeAsync(libraries);
            await _linkLibraryRepository.SaveAsync();

            return libraries;
        }

        public async Task InsertLinksAsync(int libraryId)
        {
            var dumbledore = await _userRepository.FindByUsernameAsync("dumbledore");
            var snape = await _userRepository.FindByUsernameAsync("snape");
            var potter = await _userRepository.FindByUsernameAsync("potter");

            var links = new List<Link>
            {
                new Link
                {
                    Url = "https://maricopacountyreads.org/",
                    Name = "Summer Reading",
                    CreatedBy = dumbledore.Id,
                    CreatedAt = DateTime.Now,
                    LinkLibraryId = libraryId
                },
                new Link
                {
                    Url = "#",
                    Name = "Reading Adventure",
                    CreatedBy = snape.Id,
                    CreatedAt = DateTime.Now,
                    LinkLibraryId = libraryId
                },
                new Link
                {
                    Url = "#",
                    Name = "Find Libraries",
                    CreatedBy = potter.Id,
                    CreatedAt = DateTime.Now,
                    LinkLibraryId = libraryId
                }
            };

            await _linkRepository.AddRangeAsync(links);
            await _linkRepository.SaveAsync();
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

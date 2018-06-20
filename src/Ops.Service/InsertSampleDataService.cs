using System;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Service
{
    public class InsertSampleDataService
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILinkRepository _linkRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IPostRepository _postRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserRepository _userRepository;
        public InsertSampleDataService(IFileRepository fileRepository,
            ILinkRepository linkRepository,
            IPageRepository pageRepository,
            IPostRepository postRepository,
            ISectionRepository sectionRepository,
            IUserRepository userRepository)
        {
            _fileRepository = fileRepository
                ?? throw new ArgumentNullException(nameof(fileRepository));
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
        }

        private User _systemAdministrator;
        private User SystemAdministrator
        {
            get
            {
                if (_systemAdministrator == null)
                {
                    _systemAdministrator = _userRepository.GetSystemAdministrator();
                }
                return _systemAdministrator;
            }
        }

        private async Task<Section> GetDefaultSectionAsync()
        {
            var defaultSection = await _sectionRepository.GetDefaultSectionAsync();
            if (defaultSection == null)
            {
                await _sectionRepository.AddAsync(new Section
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = SystemAdministrator.Id,
                    Name = "Default"
                });
                await _sectionRepository.SaveAsync();

                defaultSection = await _sectionRepository.GetDefaultSectionAsync();
            }
            return defaultSection;
        }

        public async Task InsertPostsAsync()
        {
            var defaultSection = await GetDefaultSectionAsync();

            // seed data
            await _postRepository.AddAsync(new Post
            {
                Content = "Nam dignissim porta leo vitae sodales. Morbi mollis, libero vitae sagittis tincidunt, mi dui luctus metus, elementum tincidunt nisl nisl at elit. Nulla tellus elit, aliquam in pulvinar id, interdum in velit. Suspendisse non aliquam dolor, vestibulum lacinia est. Pellentesque placerat nibh blandit, gravida nulla sed, tristique nisi. Nunc volutpat ultrices augue et congue. In in lacus condimentum dui finibus sagittis nec ut neque. Aenean accumsan nisl quis convallis rutrum. Cras vel imperdiet est. Curabitur et sagittis dui.",
                CreatedAt = DateTime.Parse("2018-06-04 15:00"),
                IsPinned = true,
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Post 3",
                Stub = "test-post-3",
                SectionId = defaultSection.Id
            });
            await _postRepository.AddAsync(new Post
            {
                Content = "Ut auctor risus diam, sed aliquam quam iaculis ac. Sed rutrum tortor eget ante consequat, ac malesuada ligula dictum. Phasellus non urna interdum, vehicula augue ac, egestas orci. Sed ut nisl ipsum. Donec hendrerit, nisl vitae interdum pretium, ligula lorem varius nisi, non cursus libero libero eu dolor. Fusce bibendum, lorem sed tempor condimentum, enim ante sollicitudin dolor, faucibus viverra quam erat ac neque. Integer sagittis magna eu augue eleifend, at pellentesque diam malesuada.",
                CreatedAt = DateTime.Parse("2018-06-04 17:25"),
                IsPinned = false,
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Post 4",
                Stub = "test-post-4",
                SectionId = defaultSection.Id
            });
            await _postRepository.AddAsync(new Post
            {
                Content = "Pellentesque sit amet risus eu lorem elementum porttitor. Ut pretium facilisis finibus. Suspendisse potenti. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis vestibulum tortor blandit, imperdiet sem et, venenatis lectus. Sed id metus magna. Etiam vel congue sapien, nec blandit lectus. Etiam feugiat est ornare quam sollicitudin, et luctus neque efficitur. Nullam quis pretium ipsum. Pellentesque eleifend quam vitae laoreet varius. Integer fringilla velit in metus finibus, a aliquet lectus tincidunt. Pellentesque varius eleifend est, id maximus nunc tristique ac.",
                CreatedAt = DateTime.Parse("2018-06-04 13:30"),
                IsPinned = false,
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Post 2",
                Stub = "test-post-2",
                SectionId = defaultSection.Id
            });
            await _postRepository.AddAsync(new Post
            {
                Content = "Sed semper, sapien quis luctus semper, nibh eros sollicitudin tellus, at tincidunt arcu odio a est. Nam nec nulla ex. Nullam et maximus ex, at porttitor velit. Sed ac justo ligula. Morbi sed lectus turpis. Aenean suscipit tellus nec risus aliquam, et dignissim urna mollis. Aliquam erat volutpat. Curabitur risus tellus, facilisis a tempus eu, hendrerit ut elit. Phasellus ut quam consequat, molestie mauris non, faucibus felis. Pellentesque finibus lobortis arcu, a tincidunt erat pulvinar vel. Proin in egestas magna, nec feugiat velit.",
                CreatedAt = DateTime.Parse("2018-06-04 12:15"),
                IsPinned = false,
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Post 1",
                Stub = "test-post-1",
                SectionId = defaultSection.Id
            });

            await _postRepository.SaveAsync();
        }

        public async Task InsertLinks()
        {
            var defaultSection = await GetDefaultSectionAsync();
            await _linkRepository.AddAsync(new Link
            {
                Url = "https://maricopacountyreads.org/",
                Name = "Summer Reading",
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now,
                SectionId = defaultSection.Id
            });
            await _linkRepository.AddAsync(new Link
            {
                Url = "#",
                Name = "Reading Adventure",
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now,
                SectionId = defaultSection.Id
            });
            await _linkRepository.AddAsync(new Link
            {
                Url = "#",
                Name = "Find Libraries",
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now,
                SectionId = defaultSection.Id
            });

            await _linkRepository.SaveAsync();
        }

        public async Task InsertFiles()
        {
            var defaultSection = await GetDefaultSectionAsync();
            await _fileRepository.AddAsync(new File
            {
                CreatedAt = DateTime.Parse("2018-05-01"),
                IsFeatured = true,
                FilePath = "/file.txt",
                Name = "Important File!",
                Icon = "fa-file-word alert-primary",
                CreatedBy = SystemAdministrator.Id,
                SectionId = defaultSection.Id
            });
            await _fileRepository.AddAsync(new File
            {
                CreatedAt = DateTime.Parse("2018-06-04"),
                FilePath = "/file.txt",
                Name = "New File 2",
                Icon = "fa-file-excel alert-success",
                CreatedBy = SystemAdministrator.Id,
                SectionId = defaultSection.Id
            });
            await _fileRepository.AddAsync(new File
            {
                CreatedAt = DateTime.Parse("2018-05-20"),
                FilePath = "/file.txt",
                Name = "New File 1",
                Icon = "fa-file-pdf alert-danger",
                CreatedBy = SystemAdministrator.Id,
                SectionId = defaultSection.Id
            });
            await _fileRepository.SaveAsync();
        }

        public async Task InsertSections()
        {
            await GetDefaultSectionAsync();

            // insert some seed data
            await _sectionRepository.AddAsync(new Section
            {
                Icon = "fa-smile",
                Name = "Human Resources",
                Path = "HumanResources",
                SortOrder = 0,
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now
            });
            await _sectionRepository.AddAsync(new Section
            {
                Icon = "fa-users",
                Name = "Operations",
                Path = "Operations",
                SortOrder = 1,
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now
            });
            await _sectionRepository.AddAsync(new Section
            {
                Icon = "fa-comments",
                Name = "Communications",
                Path = "Communications",
                SortOrder = 2,
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now
            });
            await _sectionRepository.AddAsync(new Section
            {
                Icon = "fa-thumbs-up",
                Name = "Services",
                Path = "Services",
                SortOrder = 3,
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now
            });
            await _sectionRepository.AddAsync(new Section
            {
                Icon = "fa-laptop",
                Name = "IT",
                Path = "IT",
                SortOrder = 4,
                CreatedBy = SystemAdministrator.Id,
                CreatedAt = DateTime.Now
            });

            await _sectionRepository.SaveAsync();
        }

        public async Task InsertPagesAsync()
        {
            var defaultSection = await GetDefaultSectionAsync();

            // seed data
            await _pageRepository.AddAsync(new Page
            {
                Content = "Nam dignissim porta leo vitae sodales. Morbi mollis, libero vitae sagittis tincidunt, mi dui luctus metus, elementum tincidunt nisl nisl at elit. Nulla tellus elit, aliquam in pulvinar id, interdum in velit. Suspendisse non aliquam dolor, vestibulum lacinia est. Pellentesque placerat nibh blandit, gravida nulla sed, tristique nisi. Nunc volutpat ultrices augue et congue. In in lacus condimentum dui finibus sagittis nec ut neque. Aenean accumsan nisl quis convallis rutrum. Cras vel imperdiet est. Curabitur et sagittis dui.",
                CreatedAt = DateTime.Parse("2018-06-04 15:00"),
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Page 3",
                Stub = "test-page-3",
                SectionId = defaultSection.Id
            });
            await _pageRepository.AddAsync(new Page
            {
                Content = "Ut auctor risus diam, sed aliquam quam iaculis ac. Sed rutrum tortor eget ante consequat, ac malesuada ligula dictum. Phasellus non urna interdum, vehicula augue ac, egestas orci. Sed ut nisl ipsum. Donec hendrerit, nisl vitae interdum pretium, ligula lorem varius nisi, non cursus libero libero eu dolor. Fusce bibendum, lorem sed tempor condimentum, enim ante sollicitudin dolor, faucibus viverra quam erat ac neque. Integer sagittis magna eu augue eleifend, at pellentesque diam malesuada.",
                CreatedAt = DateTime.Parse("2018-06-04 17:25"),
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Page 4",
                Stub = "test-page-4",
                SectionId = defaultSection.Id
            });
            await _pageRepository.AddAsync(new Page
            {
                Content = "Pellentesque sit amet risus eu lorem elementum porttitor. Ut pretium facilisis finibus. Suspendisse potenti. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis vestibulum tortor blandit, imperdiet sem et, venenatis lectus. Sed id metus magna. Etiam vel congue sapien, nec blandit lectus. Etiam feugiat est ornare quam sollicitudin, et luctus neque efficitur. Nullam quis pretium ipsum. Pellentesque eleifend quam vitae laoreet varius. Integer fringilla velit in metus finibus, a aliquet lectus tincidunt. Pellentesque varius eleifend est, id maximus nunc tristique ac.",
                CreatedAt = DateTime.Parse("2018-06-04 13:30"),
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Page 2",
                Stub = "test-page-2",
                SectionId = defaultSection.Id
            });
            await _pageRepository.AddAsync(new Page
            {
                Content = "Sed semper, sapien quis luctus semper, nibh eros sollicitudin tellus, at tincidunt arcu odio a est. Nam nec nulla ex. Nullam et maximus ex, at porttitor velit. Sed ac justo ligula. Morbi sed lectus turpis. Aenean suscipit tellus nec risus aliquam, et dignissim urna mollis. Aliquam erat volutpat. Curabitur risus tellus, facilisis a tempus eu, hendrerit ut elit. Phasellus ut quam consequat, molestie mauris non, faucibus felis. Pellentesque finibus lobortis arcu, a tincidunt erat pulvinar vel. Proin in egestas magna, nec feugiat velit.",
                CreatedAt = DateTime.Parse("2018-06-04 12:15"),
                CreatedBy = SystemAdministrator.Id,
                Title = "Test Page 1",
                Stub = "test-page-1",
                SectionId = defaultSection.Id
            });

            await _pageRepository.SaveAsync();
        }
    }
}

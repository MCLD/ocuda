using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ops.Service
{
    public class PostService
    {
        public IEnumerable<Post> GetPosts()
        {
            return new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Content = "Nam dignissim porta leo vitae sodales. Morbi mollis, libero vitae sagittis tincidunt, mi dui luctus metus, elementum tincidunt nisl nisl at elit. Nulla tellus elit, aliquam in pulvinar id, interdum in velit. Suspendisse non aliquam dolor, vestibulum lacinia est. Pellentesque placerat nibh blandit, gravida nulla sed, tristique nisi. Nunc volutpat ultrices augue et congue. In in lacus condimentum dui finibus sagittis nec ut neque. Aenean accumsan nisl quis convallis rutrum. Cras vel imperdiet est. Curabitur et sagittis dui.",
                    CreatedAt = DateTime.Parse("2018-06-04 15:00"),
                    CreatedBy = 1,
                    IsPinned = true,
                    Title = "Test Post 3"
                },
                new Post
                {
                    Id = 2,
                    Content = "Ut auctor risus diam, sed aliquam quam iaculis ac. Sed rutrum tortor eget ante consequat, ac malesuada ligula dictum. Phasellus non urna interdum, vehicula augue ac, egestas orci. Sed ut nisl ipsum. Donec hendrerit, nisl vitae interdum pretium, ligula lorem varius nisi, non cursus libero libero eu dolor. Fusce bibendum, lorem sed tempor condimentum, enim ante sollicitudin dolor, faucibus viverra quam erat ac neque. Integer sagittis magna eu augue eleifend, at pellentesque diam malesuada.",
                    CreatedAt = DateTime.Parse("2018-06-04 17:25"),
                    CreatedBy = 1,
                    IsPinned = false,
                    Title = "Test Post 4"
                },
                new Post
                {
                    Id = 3,
                    Content = "Pellentesque sit amet risus eu lorem elementum porttitor. Ut pretium facilisis finibus. Suspendisse potenti. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis vestibulum tortor blandit, imperdiet sem et, venenatis lectus. Sed id metus magna. Etiam vel congue sapien, nec blandit lectus. Etiam feugiat est ornare quam sollicitudin, et luctus neque efficitur. Nullam quis pretium ipsum. Pellentesque eleifend quam vitae laoreet varius. Integer fringilla velit in metus finibus, a aliquet lectus tincidunt. Pellentesque varius eleifend est, id maximus nunc tristique ac.",
                    CreatedAt = DateTime.Parse("2018-06-04 13:30"),
                    CreatedBy = 1,
                    IsPinned = false,
                    Title = "Test Post 2"
                },
                new Post
                {
                    Id = 4,
                    Content = "Sed semper, sapien quis luctus semper, nibh eros sollicitudin tellus, at tincidunt arcu odio a est. Nam nec nulla ex. Nullam et maximus ex, at porttitor velit. Sed ac justo ligula. Morbi sed lectus turpis. Aenean suscipit tellus nec risus aliquam, et dignissim urna mollis. Aliquam erat volutpat. Curabitur risus tellus, facilisis a tempus eu, hendrerit ut elit. Phasellus ut quam consequat, molestie mauris non, faucibus felis. Pellentesque finibus lobortis arcu, a tincidunt erat pulvinar vel. Proin in egestas magna, nec feugiat velit.",
                    CreatedAt = DateTime.Parse("2018-06-04 12:15"),
                    CreatedBy = 1,
                    IsPinned = false,
                    Title = "Test Post 1"
                }
            };
        }

        public Post GetPostById(int id)
        {
            return new Post
            {
                Id = id,
                Content = "Sed semper, sapien quis luctus semper, nibh eros sollicitudin tellus, at tincidunt arcu odio a est. Nam nec nulla ex. Nullam et maximus ex, at porttitor velit. Sed ac justo ligula. Morbi sed lectus turpis. Aenean suscipit tellus nec risus aliquam, et dignissim urna mollis. Aliquam erat volutpat. Curabitur risus tellus, facilisis a tempus eu, hendrerit ut elit. Phasellus ut quam consequat, molestie mauris non, faucibus felis. Pellentesque finibus lobortis arcu, a tincidunt erat pulvinar vel. Proin in egestas magna, nec feugiat velit.",
                CreatedAt = DateTime.Parse("2018-06-04 12:15"),
                CreatedBy = 1,
                Title = "Test Post 1",
                Stub = "test-post-1",
                IsDraft = true,
                IsPinned = true,
                ShowOnHomepage = true
            };
        }
        
        public async Task<Post> CreatePostAsync(Post post)
        {
            // call create method from repository
            return post;
        }

        public async Task<Post> EditPostAsync(Post post)
        {
            // get existing post and update properties that changed
            // call edit method on existing post
            return post;
        }

        public async Task DeletePostAsync(int id)
        {
            // call delete method from repository
        }
    }
}

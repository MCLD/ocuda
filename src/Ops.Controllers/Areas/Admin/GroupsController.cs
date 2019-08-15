using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    public class GroupsController: BaseController<GroupsController>
    {
        private readonly IGroupService _groupService;
        private readonly IConfiguration _config;
        public static string Name { get { return "Locations"; } }

        public GroupsController(IConfiguration config,
            ServiceFacades.Controller<GroupsController> context,
            IGroupService groupService) : base(context)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _groupService = groupService
                ?? throw new ArgumentNullException(nameof(groupService));
        }


    }
}

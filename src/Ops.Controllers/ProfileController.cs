using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Profile;
using Ocuda.Ops.Service;

namespace Ocuda.Ops.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly UserService _userService;

        public ProfileController(UserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<IActionResult> Index(string id)
        {
            var viewModel = new IndexViewModel();
            
            if(!string.IsNullOrWhiteSpace(id))
            {
                var user = await _userService.GetByUsernameAsync(id);

                if(user != null)
                {
                    viewModel.User = user;
                }
                else
                {
                    ShowAlertDanger("Could not find user with username: ", id);
                    return RedirectToAction(nameof(Index), "Home");
                }
            }
            else
            {
                //TODO get the logged in user
                var user = await _userService.GetByUsernameAsync(CurrentUsername);
                viewModel.User = user;
            }

            if (viewModel.User.SupervisorId.HasValue)
            {
                viewModel.User.Supervisor = 
                    await _userService.GetByIdAsync(viewModel.User.SupervisorId.Value);
            }

            //TODO get the logged in user
            if (viewModel.User.Username == CurrentUsername)
            {
                viewModel.CanEdit = true;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditNickname(IndexViewModel model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var user = await _userService.EditNicknameAsync(model.User);
                    ShowAlertSuccess($"Updated nickname: {user.Nickname}");
                }
                catch(Exception ex)
                {
                    ShowAlertDanger("Unable to update nickname: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

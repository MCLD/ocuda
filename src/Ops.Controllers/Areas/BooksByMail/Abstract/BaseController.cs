using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace BooksByMail.Controllers.Abstract
{
    public abstract class BaseController: Controller
    {
        protected string AlertDanger
        {
            set
            {
                TempData[Keys.TempData.AlertDanger] = value;
            }
        }

        protected string AlertWarning
        {
            set
            {
                TempData[Keys.TempData.AlertWarning] = value;
            }
        }
        protected string AlertInfo
        {
            set
            {
                TempData[Keys.TempData.AlertInfo] = value;
            }
        }
        protected string AlertSuccess
        {
            set
            {
                TempData[Keys.TempData.AlertSuccess] = value;
            }
        }

        private string Fa(string iconName, string iconStyle = "fa")
        {
            return $"<span class=\"{iconStyle} fa-{iconName}\" aria-hidden=\"true\"></span>";
        }

        protected void ShowAlertDanger(string message, string details = null)
        {
            AlertDanger = $"{Fa("exclamation-triangle")} {message}{details}";
        }

        protected void ShowAlertWarning(string message, string details = null)
        {
            AlertWarning = $"{Fa("exclamation-circle")} {message}{details}";
        }

        protected void ShowAlertSuccess(string message, string faIconName = null)
        {
            if (!string.IsNullOrEmpty(faIconName))
            {
                AlertSuccess = $"{Fa(faIconName)} {message}";
            }
            else
            {
                AlertSuccess = $"{Fa("thumbs-up", "far")} {message}";
            }
        }

        protected void ShowAlertInfo(string message, string faIconName = null)
        {
            if (!string.IsNullOrEmpty(faIconName))
            {
                AlertInfo = $"{Fa(faIconName)} {message}";
            }
            else
            {
                AlertInfo = $"{Fa("check-circle")} {message}";
            }
        }

        protected string CurrentUsername
        {
            get
            {
                return HttpContext.User.Claims
                    .Where(_ => _.Type == Keys.ClaimType.Username)
                    .FirstOrDefault()?
                    .Value;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Valid notification statuses
    /// </summary>
    public enum NotificationStatus
    {
        /// <summary>
        /// Call complete, answered by person
        /// </summary>
        CallCompletedVoice = 1,

        /// <summary>
        /// Call complete, answered by machine
        /// </summary>
        CallCompletedMachine,

        /// <summary>
        /// Call incomplete, hang up
        /// </summary>
        CallHangUp,

        /// <summary>
        /// Call incomplete, busy tone
        /// </summary>
        CallBusy,

        /// <summary>
        /// Call incomplete, no answer
        /// </summary>
        CallNoAnswer,

        /// <summary>
        /// Call incomplete, no ring
        /// </summary>
        CallNoRing,

        /// <summary>
        /// Call failed, no dial tone
        /// </summary>
        CallNoDialTone,

        /// <summary>
        /// Call failed, intercept tones heard
        /// </summary>
        CallInterceptTonesHeard,

        /// <summary>
        /// Call failed, bad phone number
        /// </summary>
        CallBadPhoneNumber,

        /// <summary>
        /// Call failed, number of max retries exceeded
        /// </summary>
        CallMaxRetriesExceeded,

        /// <summary>
        /// Call failed, other error
        /// </summary>
        CallFailedGeneralError,

        /// <summary>
        /// Email sent
        /// </summary>
        EmailCompleted,

        /// <summary>
        /// Email failed, invalid address
        /// </summary>
        EmailInvalidAddress,

        /// <summary>
        /// Email failed, other error
        /// </summary>
        EmailFailed,

        /// <summary>
        /// Mail printed
        /// </summary>
        MailPrinted
    }
}

using System;

namespace Ocuda.Utility.Extensions
{
    public static class ISessionExtensions
    {
        public static bool GetBoolean(this Microsoft.AspNetCore.Http.ISession session, string key)
        {
            ArgumentNullException.ThrowIfNull(session);

            return session.TryGetValue(key, out byte[] value) && value.Length > 0 && value[0] == 1;
        }

        public static void SetBoolean(this Microsoft.AspNetCore.Http.ISession session,
            string key,
            bool value)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (value)
            {
                session.Set(key, [1]);
            }
            else
            {
                session.Remove(key);
            }
        }
    }
}
using System.Text.RegularExpressions;

namespace MRK
{
    public static class Utils
    {
        public static bool ValidateUsername(string username)
        {
            return Regex.IsMatch(username, "^[a-zA-Z0-9_-]{3,16}$");
        }

        public static bool ValidatePassword(string password)
        {
            return Regex.IsMatch(password, "^[A-Za-z0-9\\s$&+,:;=?@#|'<>.^*()%!-]{3,32}$");
        }

        public static bool ValidateEmail(string email)
        {
            return Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");
        }
    }
}

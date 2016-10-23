using System;
using System.Web;

namespace RBS.Library
{
    public class Context
    {
        // Public constant
        public readonly string FUNCTION_ID = "RBS";
        public readonly string HOME_PAGE = "~/";
        public readonly string LOGIN_PAGE = "~/User/Login";
               
        public readonly string SYSTEM_ID = "System";
        public readonly int ITERATION = 1000;
        public readonly string STR_ERROR_MSG_LOGIN_FAIL = "Login failed. Either email or password is incorrect.";
        public readonly string STR_ERROR_MSG_PASSWORD_POLICY = "Password should contain at least 8 digits";
        public readonly string STR_ERROR_MSG_PASSWORD_WRONG = "Current password is incorrect.";
        public readonly string STR_ERROR_MSG_USERNAME_DUPLICATE = "Entered email has been used.";
        public readonly string STR_ERROR_MSG_MEETINGROOM_DUPLICATE = "Entered meeting room has been used.";
        public readonly string STR_SETTING = "Setting";

        // Public constant for audit logging
        public readonly string STR_INFO = "INFO";
        public readonly string STR_ERROR = "ERROR";
        public readonly string STR_LOGIN = "LOGIN";
        public readonly string STR_LOGOUT = "LOGOUT";
        public readonly string STR_CREATE = "CREATE";
        public readonly string STR_READ = "READ";
        public readonly string STR_INSERT = "INSERT";
        public readonly string STR_UPDATE = "UPDATE";
        public readonly string STR_DELETE = "DELETE";
        public readonly string STR_CREATE_PASSWORD = "CREATE PASSWORD";
        public readonly string STR_CHANGE_PASSWORD = "CHANGE PASSWORD";
        public readonly string STR_USER = "User";
        public readonly string STR_ROOM = "Room";
        public readonly string STR_MEETING = "Meeting";

        // Private constant
        //private readonly string STR_ISAUTHENTICATED = "IsAuthenticated";
        //private readonly string STR_USERID = "UserID";
        //private readonly string STR_ISADMIN = "IsAdmin";
        //private readonly string STR_ERROR_MSG = "ErrorMessage";

        // Regex
        public readonly string REGEX_PASSWORD = "^(?=.*[a-z].*[a-z].*[a-z])(?=.*[A-Z].*[A-Z])(?=.*[0-9].*[0-9]).{8,}$";
        public readonly string REGEX_EMAIL = "^(?=.*[a-z].*[a-z].*[a-z])(?=.*[A-Z].*[A-Z])(?=.*[0-9].*[0-9]).{8,}$";

        public HttpSessionStateBase Session { get; set; }

        public Context()
        {
            PreviousPage = String.Empty;
            IsAuthenticated = false;
            IsAdmin = false;
            UserID = String.Empty;
            ErrorMessage = String.Empty;
        }

        // Purpose: Get previous page
        public string PreviousPage { get; set; }

        // Purpose: Check if this session is authenticated
        public bool IsAuthenticated { get; set; }

        // Purpose: Check if this is admin user
        public bool IsAdmin { get; set; }

        // Purpose: Get and set user id
        public string UserID { get; set; }

        // Purpose: Get and set user role
        public string Role { get; set; }

        // Purpose: Get and set Session Key
        public string SessionKey { get; set; }

        // Purpose: Check if this is the login page. 
        public bool IsLoginPage
        {
            get
            {
                bool isValid = false;

                if (System.Web.HttpContext.Current.Request.Url.ToString().ToUpper().Contains(STR_LOGIN))
                {
                    isValid = true;
                }

                return isValid;
            }

        }

        // Purpose: Store error message
        public string ErrorMessage { get; set; }

    }
}
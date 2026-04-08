using System.Xml.Linq;

namespace IMS_Application.Common.Constants
{
    public static class ErrorMessages
    {
        public const string InvalidCredentials = "Invalid email address or password";
        public const string InvalidPassword = "Invalid password";
        public const string UnexpectedError = "An unexpected error occurred. Please try again later.";
        public const string UserAlreadyExists = "User already exists";
        public const string InvalidRefreshToken = "Invalid refresh token.";
        public const string InvalidOrExpiredToken = "Invalid or expired refresh token. Please log in again.";
        public const string NoRefreshToken = "No refresh token found.";

        public const string CategoryalreadyExist = "Category Already Exist";
        public const string ServerError = "Internal server error.";
        public const string CommentRequires = "Comment text is required";
        public const string TicketIdNotExist = "Ticket ID does not exist";



    }
}

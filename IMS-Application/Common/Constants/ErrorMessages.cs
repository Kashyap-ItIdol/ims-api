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
        public const string ServerError = "Internal server error.";
        public const string CommentRequires = "Comment text is required";
        public const string TicketIdNotExist = "Ticket ID does not exist";
        public const string CategoryalreadyExist = "Category Already Exist";
        public const string UserNotFound = "User ID not found in claims";
        public const string CategoryNotFound = "Category not found.";
        public const string CategoryNameRequired = "Category name is required.";
        public const string CategoryNameTooShort = "Category name must be at least 2 characters.";
        public const string CategoryNameInvalidChars = "Category name contains invalid characters.";
        public const string DuplicateCategoryName = "Category name already exists.";
        public const string InvalidInput = "Invalid input provided.";
        public const string CategoryAlreadyDeleted = "Category is already deleted.";
        public const string TicketNotFound = "Ticket not found.";
        public const string InvalidTicketAssignee = "Assigned user must be a Support Engineer.";
        public const string UnauthorizedTicketView = "You are not authorized to view this ticket.";
        public const string UserNotFoundError = "User not found.";
        public const string RoleNotFoundError = "User role not found.";
        public const string CreatorNotFoundError = "Creator not found.";
        public const string InvalidTicketType = "TicketType must be one of: Hardware, Software, Server, Website.";
        public const string InvalidTicketPriority = "Priority must be one of valid TicketPriority values.";
        public const string InvalidTicketStatus = "Invalid status value. Valid values are: Open, InProgress, Solved, Closed.";
        public const string SubCategoryNameRequired = "SubCategory name is required.";
        public const string SubCategoryNameTooShort = "SubCategory name must be at least 2 characters.";
        public const string SubCategoryNameInvalidChars = "SubCategory name contains invalid characters.";
        public const string SubCategoryCategoryIdInvalid = "Valid CategoryId is required.";
        public const string SubCategoryCategoryNotFound = "Category not found or inactive.";
        public const string DuplicateSubCategoryName = "SubCategory with this name already exists in the category.";
        public const string SubCategoryInvalidUser = "Invalid user.";
        public const string ForgotPasswordUserNotFound = "User not found with this email address";
        public const string OtpSendFailed = "Failed to send OTP. Please try again.";
        public const string InvalidOrExpiredOtp = "Invalid or expired OTP";
        public const string InvalidResetToken = "Invalid or expired reset token";
        public const string ResetPasswordUserNotFound = "User not found for password reset";
        public const string CategoryOrSubCategoryNotFound = "category or subcategory is not available in the table";
        public const string SearchQueryRequired = "Search query is required.";
    }
}

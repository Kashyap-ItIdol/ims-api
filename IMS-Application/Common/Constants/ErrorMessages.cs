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
        
        // Asset Assignment Error Messages
        public const string AssetAssignmentNotFound = "Asset assignment not found";
        public const string AssetAssignmentRequired = "Asset assignment data is required";
        public const string AssetNotFound = "Asset not found";
        public const string EmployeeNotFound = "Employee not found";
        public const string AssetAlreadyAssigned = "Asset is already assigned";
        public const string InvalidAssetAssignmentId = "Invalid asset assignment ID";
        public const string AssetAssignmentDateInvalid = "Assignment date cannot be more than 7 days in the future";
        public const string ReturnDateInvalid = "Expected return date must be on or after assigned date";
        
        // Client Asset Error Messages
        public const string ClientAssetNotFound = "Client asset not found";
        public const string ClientAssetRequired = "Client asset data is required";
        public const string InvalidClientAssetId = "Invalid client asset ID";
        public const string ClientAssetNameRequired = "Client asset name is required";
        public const string ClientAssetAlreadyExists = "Client asset already exists";
        public const string ClientAssetBrandRequired = "Brand is required";
        public const string ClientAssetModelRequired = "Model is required";
        public const string ClientAssetConditionRequired = "Condition is required";
        public const string ClientAssetClientNameRequired = "Client name is required";
        public const string ClientAssetClientPOCRequired = "Client POC is required";
        public const string ClientAssetSalesPOCRequired = "Sales POC is required";
        
        // Attachment Error Messages
        public const string AttachmentNotFound = "Attachment not found";
        public const string AttachmentRequired = "Attachment file is required";
        public const string AttachmentUploadFailed = "Failed to upload attachment";
        public const string AttachmentDeleteFailed = "Failed to delete attachment";
        public const string AttachmentDownloadFailed = "Failed to download attachment";
        public const string FileNotFound = "File not found";
        public const string InvalidFileFormat = "Invalid file format";
        public const string FileSizeExceeded = "File size exceeds limit";
    }
}

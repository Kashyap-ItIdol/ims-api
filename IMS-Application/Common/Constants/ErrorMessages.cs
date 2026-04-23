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
        public const string AssetNotFound = "Asset not found";
        public const string SerialAlreadyExists = "Serial already exists";
        public const string TableAlreadyAssigned = "Table is already assigned to another user";
        public const string UserNotFound = "User not found";
        public const string OnlyAvailableAssetsCanBeAssigned = "Only available assets can be assigned";
        public const string AssetsListEmpty = "At least one asset is required";
        public const string ExactlyOnePrimaryAssetRequired = "Exactly one primary asset is required";
        public const string ItemNameRequired = "ItemName is required";
        public const string SerialNumberRequired = "Serial number is required";
        public const string InvalidCategoryId = "Invalid CategoryId";
        public const string InvalidSubCategoryId = "Invalid SubCategoryId";
        public const string InvalidConditionId = "Invalid ConditionId";
        public const string VendorRequired = "Vendor is required";
        public const string PurchaseCostInvalid = "PurchaseCost must be greater than 0";
        public const string PurchaseDateRequired = "PurchaseDate is required";
        
        public const string AssignedAssetMustBeAssigned = "Assigned asset must have status = Assigned";
        public const string ExpectedReturnDateBeforeAssignedDate = "ExpectedReturnDate cannot be before AssignedDate";
        public const string SerialAlreadyExistsFormatted = "Serial already exists: {0}";
        public const string UserNotFoundById = "User not found with id {0}";
        public const string TableAlreadyAssignedToUser = "Table {0} is already assigned to another user";
        public const string CannotAssignChildParentNotAssigned = "Cannot assign child when parent is not assigned";
        public const string ChildMustMatchParentAssignment = "Child must be assigned to same user as parent";
        public const string AssignedToRequired = "AssignedTo is required";
        public const string TableAlreadyAssignedShort = "Table {0} already assigned";
        public const string InvalidParentAsset = "Invalid parent asset";
        public const string ChildAssetNotFound = "Child asset not found";
        public const string AssetAlreadyAttached = "Asset is already attached";
        public const string OnlyAvailableAssetsAttachable = "Only available assets can be attached";
        public const string InvalidChildAsset = "Invalid child asset";

        public const string InvalidSerialFormat = "Serial number must be 5-20 alphanumeric characters (A-Z0-9-).";
        public const string InvalidPurchaseDate = "Purchase date must be a valid date not in the future.";
        public const string InvalidPurchaseCost = "Purchase cost must be greater than 0 and <= 1,000,000.";
        public const string InvalidDateRange = "Expected return date must be after assigned date.";
        public const string InvalidStringLength = "Field length must be between 1-200 characters.";
        public const string InvalidTableNo = "Table number must be alphanumeric (A-Z0-9).";
        public const string InvalidIpAddress = "Invalid IP address format.";
        public const string ValidationFailed = "Validation failed. Please check the errors.";
    }
}

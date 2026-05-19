namespace IMS_Application.Common.Constants

{
    public static class LogicStrings
    {
        public const string Active = "Active";
        public const string Unassigned = "Unassigned";
        public const string Unknown = "Unknown";
        public const string AdminRole = "Admin";
        public const string SupportEngineerRole = "Support Engineer";
        public const string EmployeeRole = "Employee";
        public const string CategoryItemName = "Category";
        public const string AssetItemName = "Asset";
        public const string SubCategoryItemName = "SubCategory";
        public const string TicketItemName = "Ticket";
        public const string ActionCreated = "Created";
        public const string ActionUpdated = "Updated";
        public const string ActionDeleted = "Deleted";
        public const string ActionAssigned = "Assigned";
        public const string ActionAttached = "Attached";
        public const string ActionDetached = "Detached";
        public const string ActionUnlinked = "Unlinked";
        public const string ActionCreatedAndAttached = "Created & Attached";
        public const string ActionNetworkAdded = "Network Added";
        public const string ActionNetworkUpdated = "Network Updated";
        public const string ActionUnlinkedAndUpdated = "Unlinked";
        public const string ActionDetachedRemovedFromParent = "Detached";
        public const string AssetsCsvHeader = "Item Name,Status,Category,Subcategory,Brand,Model,Serial Number,Condition,Vendor Name,Purchase Cost,Purchase Date,Invoice Number,Warranty Expiry Date,AMC Expiry Date";
        public const string CsvHeaderItemName = "Item Name";
        public const string CsvHeaderStatus = "Status";
        public const string CsvHeaderCategory = "Category";
        public const string CsvHeaderSubCategory = "Subcategory";
        public const string CsvHeaderBrand = "Brand";
        public const string CsvHeaderModel = "Model";
        public const string CsvHeaderSerialNumber = "Serial Number";
        public const string CsvHeaderCondition = "Condition";
        public const string CsvHeaderVendorName = "Vendor Name";
        public const string CsvHeaderPurchaseCost = "Purchase Cost";
        public const string CsvHeaderPurchaseDate = "Purchase Date";
        public const string CsvHeaderInvoiceNumber = "Invoice Number";
        public const string CsvHeaderWarrantyExpiryDate = "Warranty Expiry Date";
        public const string CsvHeaderAmcExpiryDate = "AMC Expiry Date";
        public const string CsvHeaderAssignedTo = "AssignedTo";
        public const string CsvHeaderAssignedDate = "AssignedDate";
        public const string CsvHeaderNotes = "Notes";
        public const string CsvHeaderMismatchMessage = "CSV header does not match expected format.";
        public const string ImportItemNameAndSerialNoRequired = "ItemName and SerialNo are required.";
        public const string ImportCategoryIdInvalid = "Category must be a valid numeric CategoryId.";
        public const string ImportSubCategoryIdInvalid = "SubCategory must be a valid numeric SubCategoryId.";
        public const string ImportStatusIdInvalid = "Status must be a valid numeric StatusId.";
        public const string ImportBrandModelVendorRequired = "Brand, Model and Vendor are required.";
        public const string ImportConditionIdInvalid = "ConditionId is required and must be numeric (expected column: Condition).";
        public const string ImportPurchaseCostRequired = "PurchaseCost is required (expected column: PurchaseCost).";
        public const string ImportPurchaseCostInvalidDecimal = "PurchaseCost is required and must be decimal (expected column: PurchaseCost).";
        public const string ImportPurchaseDateRequired = "PurchaseDate is required (expected column: PurchaseDate).";
        public const string ImportPurchaseDateInvalid = "PurchaseDate is required and must be a valid date (expected column: PurchaseDate).";
        public const string ImportInvoiceNumberRequired = "InvoiceNumber is required (expected column: InvoiceNumber).";
        public const string RemovedFromParentAsset = "Removed from parent asset";
    }
}

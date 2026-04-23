# Asset Validation Enhancement TODO

## Plan Overview
Implement real-world validation for assets + global exception handling for proper JSON error responses.

## Steps to Complete:
- [ ] Step 1: Add new error messages to ErrorMessages.cs
- [ ] Step 2: Update GlobalExceptionHandler.cs to handle ValidationException with structured errors
- [ ] Step 3: Enhance AddAssetValidator.cs with real-world rules
- [ ] Step 4: Enhance UpdateAssetValidator.cs
- [ ] Step 5: Enhance AssignAssetValidator.cs  
- [ ] Step 6: Enhance other child validators (Attach, Detach, CreateChild)
- [ ] Step 7: Remove redundant manual validations from AssetService.cs
- [ ] Step 8: Test all asset endpoints with invalid data (expect 400 JSON with errors)
- [ ] Step 9: dotnet build & verify no regressions

- [x] Step 1: Add new error messages to ErrorMessages.cs
- [x] Step 2: Update GlobalExceptionHandler.cs to handle ValidationException with structured errors
- [x] Step 3: Enhance AddAssetValidator.cs with real-world rules

**Progress: Step 3 Complete. Starting Step 4: UpdateAssetValidator**

Last Updated: 2024

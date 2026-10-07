namespace PartManagementSystem.Common
{
    public class ViewModelValidation
    {
        /* AssetUpload Begin */

        public const int AssetNameMinLength = 1;
        public const int AssetNameMaxLength = 100;

        public const int PartNumberMinLength = 1;
        public const int PartNumberMaxLength = 50;

        public const int AssetUploadDescriptionMaxLength = 500;

        /* AssetUpload Ending */

        /* Register Begin */
        public const int UsernameMinLength = 3;
        public const int UsernameMaxLength = 30;

        public const int MinimumRegistrationAge = 16;
        public const int FirstNameMinLength = 1;
        public const int FirstNameMaxLength = 50;

        public const int LastNameMinLength = 1;
        public const int LastNameMaxLength = 50;

        public const int CityMinLength = 1;
        public const int CityMaxLength = 50;

        public const int CountryMinLength = 1;
        public const int CountryMaxLength = 80;

        /* Register Енд */

        /* Home Begin */

        public const int ProjectsMaxLength = 200;

        public const int ProjectCountMinLength = 0;
        public const int ProjectCountMaxLength = 200;

        public const int NewProjectNameMinLenght = 1;
        public const int NewProjectNameMaxLenght = 50;

        public const int NewProjectDescriptionMaxlength = 200;

        /* Home End */

        /* AddRevision Begin */

        public const int ChangeNotesMaxLength = 150;

        /* AddRevision End */

    }
}

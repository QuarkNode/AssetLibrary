namespace AssetLibrary.Data.Common
{
    public class ViewModelValidation
    {
        /* Asset Begin */

        public const int AssetNameMinLength = 1;
        public const int AssetNameMaxLength = 100;

        public const int AssetPartNumberMinLength = 1;
        public const int AssetPartNumberMaxLength = 50;

        public const int AssetDescriptionMaxLength = 500;

        /* Asset Ending */

        /* Register Begin */
        public const int RegisterUsernameMinLength = 3;
        public const int RegisterUsernameMaxLength = 30;

        public const int RegisterMinimumRegistrationAge = 16;
        public const int RegisterFirstNameMinLength = 1;
        public const int RegisterFirstNameMaxLength = 50;

        public const int RegisterLastNameMinLength = 1;
        public const int RegisterLastNameMaxLength = 50;

        public const int RegisterCityMinLength = 1;
        public const int RegisterCityMaxLength = 50;

        public const int RegisterCountryMinLength = 1;
        public const int RegisterCountryMaxLength = 80;

        /* Register End */

        /* Home Begin */

        public const int ProjectsMaxLength = 20;

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

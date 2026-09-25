namespace PartManagementSystem.Common
{
    public class EntityValidation
    {
        /* User Begin */

        public const int CompanyNameMinLength = 1;
        public const int CompanyNameMaxLength = 70;

        public const int FirstNameMinLength = 1;
        public const int FirstNameMaxLength = 50;

        public const int MiddleNameMinLength = 1;
        public const int MiddleNameMaxLength = 50;

        public const int LastNameMinLength = 1;
        public const int LastNameMaxLength = 50;

        public const int AgeMinLength = 1;
        public const int AgeMaxLength = 120;

        public const int EmailMinLength = 1;
        public const int EmailMaxLength = 100;

        public const int OccupationMinLength = 1;
        public const int OccupationMaxLength = 100;

        public const int CityMinLength = 1;
        public const int CityMaxLength = 50;

        public const int CountryMinLength = 1;
        public const int CountryMaxLength = 80;

        /* User End */


        /* Asset Begin */

        public const int PartNumberMinLength = 1;
        public const int PartNumberMaxLength = 50;

        public const int AssetNameMinLength = 1;
        public const int AssetNameMaxLength = 70;

        public const int AssetDescriptionMaxLength = 200;


        /* Asset End */


        /* Revision Begin */

        public const int FilePathMaxLength = 500;

        public const int FileExtensionMinLength = 1;
        public const int FileExtensionMaxLength = 50;

        public const int RevisionChangeNotesMaxLength = 200;


        /* Revision End */


        /* Project Begin */

        public const int ProjectNameMinLength = 1;
        public const int ProjectNameMaxLength = 100;

        public const int ProjectDescriptionMaxLength = 200;

        /* Project End */


        /* Material Begin */

        public const int MaterialNameMinLength = 1;
        public const int MaterialNameMaxLength = 100;

        /* Material End */


        /* Tag Begin */

        public const int TagNameMinLength = 1;
        public const int TagNameMaxLength = 20;

        /* Tag End */
    }
}

namespace AssetLibrary.Data.Models.Enum
{
    using System.ComponentModel.DataAnnotations;

    public enum Status
    {
        Draft = 0,

        [Display(Name = "In Review")]
        InReview,
        Released
    }
}
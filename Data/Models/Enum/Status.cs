
namespace PartManagementSystem.Data.Models.Enum
{
    using System.ComponentModel;
    public enum Status
    {
        [Description("Draft")]
        Draft = 0,

        [Description("In Review")]
        InReview,

        [Description("Released")]
        Released
    }
}
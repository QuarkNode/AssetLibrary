namespace AssetLibrary.Data.Exceptions
{
    public class ProjectLimitExceededException : Exception
    {
        public ProjectLimitExceededException(int limit) : base($"You can have at most {limit} projects.")
        {

        }
    }
}

// Stream A. Scene names in one place (they must match Build Settings).
namespace ByteWorld
{
    public static class SceneNames
    {
        public const string Boot = "Boot";
        public const string World = "World";
        public const string Build = "Build";

        public static string ForJob(JobId job) => job switch
        {
            JobId.Scout => "Job_Scout",
            JobId.Gigi => "Job_Gigi",
            JobId.Twins => "Job_Twins",
            _ => "Job_Patch"
        };
    }
}

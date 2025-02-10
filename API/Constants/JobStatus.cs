namespace API.Constants;

public static class JobStatus
{
    public const string Error = "Error";
    public const string Queued = "Queued";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
}

public static class JobType
{
    public const string Immediate = "IMMEDIATE";
    public const string Frequent = "FREQUENT";
}
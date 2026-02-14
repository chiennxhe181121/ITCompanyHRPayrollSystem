namespace HumanResourcesManager.DAL.Enum
{
    public enum AttendanceStatus
    {
        Pending = 0,
        CompletedWork = 1, // Present
        InsufficientWork = 2, // Present
        MissingCheckOut = 3,
        ApprovedLeave = 4,
        Absent = 5
    }
}

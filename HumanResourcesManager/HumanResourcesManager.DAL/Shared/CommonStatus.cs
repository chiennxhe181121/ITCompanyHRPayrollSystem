namespace HumanResourcesManager.DAL.Shared
{
    public static class CommonStatus
    {
        // ===== Request chung =====
        public const int Pending = 0;
        public const int Approved = 10;
        public const int Rejected = 20;
        public const int Cancelled = 21;

        // ===== Active / InActive =====
        public const int Active = 90;
        public const int InActive = 91;

        // ===== Attendance / OT =====
        public const int CheckedIn = 30;
        public const int CheckedOut = 31;

        // ===== Payroll =====
        public const int Calculated = 40;
        public const int Paid = 41;
    }
}

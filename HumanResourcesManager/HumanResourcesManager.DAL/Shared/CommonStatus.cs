namespace HumanResourcesManager.DAL.Shared
{
    /// <summary>
    /// Common status constants used across the whole system
    /// </summary>
    public static class CommonStatus
    {
        // ==================================================
        // 1️⃣ Active / Inactive (User, Employee, Role, etc.)
        // ==================================================
        public const int Active = 1;        // Đang hoạt động
        public const int Inactive = 0;      // Ngưng hoạt động / bị khóa
        public const int Deleted = -1;      // Đã xóa mềm (soft delete)

        // ==================================================
        // 2️⃣ Request workflow (Leave, OT, Approval, etc.)
        // ==================================================
        public const int Pending = 10;      // Chờ duyệt
        public const int Approved = 11;     // Đã duyệt
        public const int Rejected = 12;     // Bị từ chối
        public const int Cancelled = 13;    // Người tạo tự hủy

        // ==================================================
        // 3️⃣ Attendance
        // ==================================================
        public const int CheckedIn = 20;    // Đã check-in
        public const int CheckedOut = 21;   // Đã check-out
        public const int Absent = 22;       // Vắng mặt
        public const int Late = 23;         // Đi trễ
        public const int EarlyLeave = 24;   // Về sớm

        // ==================================================
        // 4️⃣ Contract
        // ==================================================
        public const int ContractValid = 30;      // Hợp đồng còn hiệu lực
        public const int ContractExpired = 31;   // Hết hạn
        public const int ContractTerminated = 32;// Chấm dứt

        // ==================================================
        // 5️⃣ Payroll
        // ==================================================
        public const int PayrollDraft = 40;       // Nháp
        public const int PayrollCalculated = 41;  // Đã tính lương
        public const int PayrollApproved = 42;    // Đã duyệt
        public const int PayrollPaid = 43;        // Đã trả lương

        // ==================================================
        // 6️⃣ User Account
        // ==================================================
        public const int AccountLocked = 50;      // Tài khoản bị khóa
        public const int PasswordExpired = 51;    // Mật khẩu hết hạn

        // ==================================================
        // 7️⃣ Generic Result (optional – dùng cho API)
        // ==================================================
        public const int Success = 200;
        public const int Failed = 500;
    }
}

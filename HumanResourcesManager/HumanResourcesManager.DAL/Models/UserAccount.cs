namespace HumanResourcesManager.DAL.Models
{
    public class UserAccount
    {
        public int UserId { get; set; }

        // ===== RELATION =====
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // ===== AUTH INFO =====
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        // ===== ROLE =====
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        // ===== STATUS =====
        // 1 = Active (được phép đăng nhập)
        // 0 = Locked (bị khóa)
        // -1 = Deleted (xóa mềm)
        public int Status { get; set; }
    }
}

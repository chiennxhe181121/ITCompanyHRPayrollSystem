namespace HumanResourcesManager.DAL.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        // EMP / HR / ADMIN
        public string RoleCode { get; set; } = null!;
        public string RoleName { get; set; } = null!;

        // ===== ROLE STATUS =====
        // 1 = Active (được phép gán cho user)
        // 0 = Disabled (role tạm ngưng, user thuộc role này không login được)
        public int Status { get; set; }

        public ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
    }
}

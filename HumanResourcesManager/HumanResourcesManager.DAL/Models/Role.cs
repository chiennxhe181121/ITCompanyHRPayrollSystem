namespace HumanResourcesManager.DAL.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        public string RoleCode { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public int Status { get; set; } 

        public ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
    }
}

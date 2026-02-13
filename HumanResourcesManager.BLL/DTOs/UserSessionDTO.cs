namespace HumanResourcesManager.BLL.DTOs
{
    public class UserSessionDTO
    {
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public string Username { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string RoleCode { get; set; } = null!;
        public string RoleName { get; set; } = null!;

        //  Thêm ( 13/02 )
        public string? ImgAvatar { get; set; }
    }
}

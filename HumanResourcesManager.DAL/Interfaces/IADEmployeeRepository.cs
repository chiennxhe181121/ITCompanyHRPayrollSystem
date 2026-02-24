using HumanResourcesManager.DAL.Models;


// HoangDH
namespace HumanResourcesManager.DAL.Interfaces
{
    public interface IADEmployeeRepository
    {

        IEnumerable<Employee> GetAll();


        // Lấy mã nhân viên cuối cùng theo prefix (ví dụ "EMP26")
        string? GetLastEmployeeCode(string prefix);

        void Add(Employee employee);
        void Save();

        bool ExistsEmail(string email);
        bool ExistsPhone(string phone);

        IEnumerable<Department> GetDepartments();
        IEnumerable<Position> GetPositions();

        Employee? GetById(int id);

        void Update(Employee employee);

        bool ExistsEmail(string email, int excludeId);
        bool ExistsPhone(string phone, int excludeId);

        int GetRoleIdByCode(string roleCode);

        // profile
        Employee? GetByUserId(int userId);
    }
}
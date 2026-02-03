using HumanResourcesManager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManager.DAL.Data
{
    public class HumanManagerContext : DbContext
    {
        public HumanManagerContext()
        {
        }

        public HumanManagerContext(DbContextOptions<HumanManagerContext> options)
            : base(options)
        {
        }


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlServer(
        //            "Server=(localdb)\\MSSQLLocalDB;Database=HumanManagerDB;Trusted_Connection=True;TrustServerCertificate=True");
        //    }
        //}

        #region DbSet
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Allowance> Allowances { get; set; }
        public DbSet<EmployeeAllowance> EmployeeAllowances { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<PayrollDetail> PayrollDetails { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<OverTimeRequest> OverTimeRequests { get; set; }
        public DbSet<OTAttendance> OTAttendances { get; set; }
        public DbSet<Role> Roles { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================== Department =====================
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.DepartmentId);

                entity.Property(d => d.DepartmentName)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            // ===================== Position =====================
            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasKey(p => p.PositionId);

                entity.Property(p => p.PositionName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.BaseSalary)
                      .HasPrecision(18, 2);
            });

            // ===================== Employee =====================
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);

                entity.Property(e => e.EmployeeCode)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasIndex(e => e.EmployeeCode).IsUnique();

                entity.Property(e => e.FullName)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(e => e.Status)
                      .IsRequired();

                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Position)
                      .WithMany(p => p.Employees)
                      .HasForeignKey(e => e.PositionId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.UserAccount)
                       .WithOne(a => a.Employee)
                       .HasForeignKey<Employee>(e => e.UserId)
                       .OnDelete(DeleteBehavior.SetNull);
            });

            // ===================== Contract =====================
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(c => c.ContractId);

                entity.Property(c => c.BasicSalary)
                      .HasPrecision(18, 2);

                entity.HasOne(c => c.Employee)
                      .WithMany(e => e.Contracts)
                      .HasForeignKey(c => c.EmployeeId);
            });

            // ===================== Allowance =====================
            modelBuilder.Entity<Allowance>(entity =>
            {
                entity.HasKey(a => a.AllowanceId);

                entity.Property(a => a.Amount)
                      .HasPrecision(18, 2);
            });

            // ===================== EmployeeAllowance =====================
            modelBuilder.Entity<EmployeeAllowance>(entity =>
            {
                entity.HasKey(ea => new { ea.EmployeeId, ea.AllowanceId });

                entity.HasOne(ea => ea.Employee)
                      .WithMany(e => e.EmployeeAllowances)
                      .HasForeignKey(ea => ea.EmployeeId);

                entity.HasOne(ea => ea.Allowance)
                      .WithMany(a => a.EmployeeAllowances)
                      .HasForeignKey(ea => ea.AllowanceId);
            });

            // ===================== Attendance =====================
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(a => a.AttendanceId);

                entity.HasOne(a => a.Employee)
                      .WithMany(e => e.Attendances)
                      .HasForeignKey(a => a.EmployeeId);
            });

            // ===================== LeaveType =====================
            modelBuilder.Entity<LeaveType>(entity =>
            {
                entity.HasKey(lt => lt.LeaveTypeId);

                entity.Property(lt => lt.LeaveName)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // ===================== LeaveRequest =====================
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(lr => lr.LeaveRequestId);

                entity.HasOne(lr => lr.Employee)
                      .WithMany(e => e.LeaveRequests)
                      .HasForeignKey(lr => lr.EmployeeId);

                entity.HasOne(lr => lr.LeaveType)
                      .WithMany(lt => lt.LeaveRequests)
                      .HasForeignKey(lr => lr.LeaveTypeId);
            });

            // ===================== Payroll =====================
            modelBuilder.Entity<Payroll>(entity =>
            {
                entity.HasKey(p => p.PayrollId);

                entity.Property(p => p.BasicSalary).HasPrecision(18, 2);
                entity.Property(p => p.TotalOT).HasPrecision(18, 2);
                entity.Property(p => p.TotalAllowance).HasPrecision(18, 2);
                entity.Property(p => p.LatePenalty).HasPrecision(18, 2);
                entity.Property(p => p.NetSalary).HasPrecision(18, 2);

                entity.HasOne(p => p.Employee)
                      .WithMany(e => e.Payrolls)
                      .HasForeignKey(p => p.EmployeeId);
            });

            // ===================== PayrollDetail =====================
            modelBuilder.Entity<PayrollDetail>(entity =>
            {
                entity.HasKey(pd => pd.PayrollDetailId);

                entity.Property(pd => pd.Amount)
                      .HasPrecision(18, 2);

                entity.HasOne(pd => pd.Payroll)
                      .WithMany(p => p.PayrollDetails)
                      .HasForeignKey(pd => pd.PayrollId);
            });

            // ===================== Role =====================
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleId);

                entity.Property(r => r.RoleName)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // ===================== UserAccount =====================
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.HasIndex(u => u.Username).IsUnique();

                entity.Property(u => u.Status).IsRequired();
              
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.UserAccounts)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== OverTimeRequest =====================
            modelBuilder.Entity<OverTimeRequest>(entity =>
            {
                entity.HasKey(ot => ot.Id);

                entity.Property(ot => ot.Reason)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(ot => ot.TaskRef)
                      .HasMaxLength(100);

                entity.Property(ot => ot.Status)
                      .IsRequired();

                // Employee được giao OT
                entity.HasOne(ot => ot.Employee)
                      .WithMany(e => e.OverTimeRequests)
                      .HasForeignKey(ot => ot.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Manager giao OT (Employee)
                entity.HasOne(ot => ot.Manager)
                      .WithMany()
                      .HasForeignKey(ot => ot.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 🔥 1–1 OTRequest ↔ OTAttendance
                entity.HasOne(ot => ot.OTAttendance)
                      .WithOne(a => a.OverTimeRequest)
                      .HasForeignKey<OTAttendance>(a => a.OverTimeRequestId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


        }

    }
}

using System;
using System.Collections.Generic;
using HumanResourcesManager.DAL.Enum;

namespace HumanResourcesManager.BLL.DTOs
{
    public class PayrollDTO
    {
        public int? PayrollId { get; set; }

        public int EmployeeId { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal TotalOT { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal MissingMinutesPenalty { get; set; }
        public string? EmployeeName { get; set; }
        public decimal NetSalary { get; set; } // chỉ để hiển thị
        public int TotalWorkingDays { get; set; }
        public int LateDays { get; set; }
        public int TotalMissingMinutes { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int MissingCheckoutDays { get; set; }
        public int AbsentDays { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal MissingCheckoutPenalty { get; set; }
        public decimal InsufficientWorkPenalty { get; set; }
        public List<PayrollDetailDTO> PayrollDetails { get; set; } = new();

        // ==== FIX: thêm danh sách Employee để binding dropdown ====
        public List<EmployeeDTO> Employees { get; set; } = new();
        public List<PenaltyDetailDTO> AbsentDetails { get; set; } = new();
        public List<PenaltyDetailDTO> MissingCheckoutDetails { get; set; } = new();
        public List<PenaltyDetailDTO> InsufficientDetails { get; set; } = new();

    }

    public class PayrollDetailDTO
    {
        public int? PayrollDetailId { get; set; } // null khi create

        public PayrollDetailType Type { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
    }
    public class PenaltyDetailDTO
    {
        public DateTime WorkDate { get; set; }
        public string Reason { get; set; } = "";
        public decimal Amount { get; set; }
    }
}
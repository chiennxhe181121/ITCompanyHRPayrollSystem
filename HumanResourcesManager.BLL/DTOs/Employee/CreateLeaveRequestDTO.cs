using System.ComponentModel.DataAnnotations;

public class CreateLeaveRequestDTO
{
    [Required(ErrorMessage = "Vui lòng chọn loại phép.")]
    [Range(1, int.MaxValue, ErrorMessage = "Loại phép không hợp lệ.")]
    public int LeaveTypeId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
    [DataType(DataType.Date)]
    public DateTime FromDate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
    [DataType(DataType.Date)]
    public DateTime ToDate { get; set; }

    [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự.")]
    public string? Reason { get; set; }
}

using System.Globalization;

public static class LunarHelper
{
    public static List<DateTime> GetTetHolidayDates(int year)
    {
        var lunar = new ChineseLunisolarCalendar();

        // Mùng 1 tháng 1 âm lịch
        int lunarYear = lunar.GetYear(new DateTime(year, 1, 1));
        DateTime tet = lunar.ToDateTime(
            lunarYear,
            1,      // tháng 1 âm
            1,      // ngày 1 âm
            0, 0, 0, 0
        );

        // Ví dụ nghỉ 5 ngày (từ mùng 1 → mùng 5)
        return Enumerable.Range(0, 5)
            .Select(i => tet.AddDays(i))
            .ToList();
    }
}

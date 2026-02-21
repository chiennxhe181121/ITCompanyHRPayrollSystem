using System.Globalization;

public static class LunarHelper
{
    public static List<DateTime> GetTetHolidayDates(int year)
    {
        var lunar = new ChineseLunisolarCalendar();

        DateTime start = new DateTime(year, 1, 21);
        DateTime end = new DateTime(year, 2, 20);

        DateTime? tet = null;

        for (var d = start; d <= end; d = d.AddDays(1))
        {
            if (lunar.GetMonth(d) == 1 &&
                lunar.GetDayOfMonth(d) == 1)
            {
                tet = d;
                break;
            }
        }

        if (tet == null)
            throw new Exception("Không tìm thấy Tết");

        return Enumerable.Range(0, 5)
            .Select(i => tet.Value.AddDays(i))
            .ToList();
    }
}

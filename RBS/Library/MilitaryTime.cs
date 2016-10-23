using System;

namespace RBS.Library
{
    public static class MilitaryTime
	{
        private static Context context = new Context();

        public static string ChangeToMilitaryTime(DateTime date)
        {
            string value = date.ToString("HHmm");
            return value;
        }

        public static DateTime ParseMilitaryTime(string time, int year, int month, int day)
        {
            //
            // Convert hour part of string to integer.
            //
            string hour = time.Substring(0, 2);
            int hourInt = int.Parse(hour);
            if (hourInt >= 24)
            {
                throw new ArgumentOutOfRangeException("Invalid hour");
            }
            //
            // Convert minute part of string to integer.
            //
            string minute = time.Substring(2, 2);
            int minuteInt = int.Parse(minute);
            if (minuteInt >= 60)
            {
                throw new ArgumentOutOfRangeException("Invalid minute");
            }
            //
            // Return the DateTime.
            //
            return new DateTime(year, month, day, hourInt, minuteInt, 0);
        }

	}
}
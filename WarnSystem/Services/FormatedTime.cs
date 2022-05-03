using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnSystem.Services
{
    public class FormatedTime
    {
        public static readonly double MinuteInSeconds = 60;
        public static readonly double HourInSeconds = MinuteInSeconds * MinuteInSeconds;
        public static readonly double DayInSeconds = HourInSeconds * 24;
        public static readonly double WeekInSeconds = DayInSeconds * 7;
        public static readonly double MonthInSeconds = DayInSeconds * 30;
        public static readonly double YearInSeconds = MonthInSeconds * 12;

        public static string FormatSeconds(TimeSpan TimeSpan)
        {
            return FormatSeconds(TimeSpan.TotalSeconds);
        }

        public static string FormatSeconds(double Seconds)
        {
            string FormatedSeconds = "";

            if (Seconds > YearInSeconds) FormatedSeconds += $"{InSeconds(ref Seconds, YearInSeconds)} Year(s), ";
            if (Seconds > MonthInSeconds) FormatedSeconds += $"{InSeconds(ref Seconds, MonthInSeconds)} Month(s), ";
            if (Seconds > DayInSeconds) FormatedSeconds += $"{InSeconds(ref Seconds, DayInSeconds)} Day(s), ";
            if (Seconds > HourInSeconds) FormatedSeconds += $"{InSeconds(ref Seconds, HourInSeconds)} Hour(s), ";
            if (Seconds > MinuteInSeconds) FormatedSeconds += $"{InSeconds(ref Seconds, MinuteInSeconds)} Minute(s), ";
            FormatedSeconds += $"{Seconds.ToString("N0")} Second(s)";

            return FormatedSeconds;
        }

        private static double InSeconds(ref double Seconds, double InSeconds)
        {
            if (Seconds < InSeconds) return 0;
            double seconds = Seconds - (Seconds % InSeconds);
            Seconds -= seconds;
            return seconds / InSeconds;
        }

        public static double Parse(string Value)
        {
            Value = Value.ToLower();
            double seconds = GetSeconds(Value);

            seconds += GetFromKey(Value, "s");
            seconds += GetFromKey(Value, "min") * MinuteInSeconds;
            seconds += GetFromKey(Value, "h") * HourInSeconds;
            seconds += GetFromKey(Value, "d") * DayInSeconds;
            seconds += GetFromKey(Value, "w") * WeekInSeconds;
            seconds += GetFromKey(Value, "m") * MonthInSeconds;
            seconds += GetFromKey(Value, "y") * YearInSeconds;

            return seconds;
        }

        private static double GetFromKey(string Value, string key)
        {
            if (!Value.Contains(key)) return 0;
            if (Value.IndexOf(key) != 0 && !char.IsDigit(Value[Value.IndexOf(key) - 1]) || Value.IndexOf(key) + key.Length < Value.Length - 1 && !char.IsDigit(Value[Value.IndexOf(key) + key.Length])) return 0;
            return GetValue(Value, Value.IndexOf(key));
        }

        private static double GetSeconds(string Value)
        {
            if (Value.Length == 0) return 0;
            if (!char.IsDigit(Value[Value.Length - 1])) return 0;
            return GetValue(Value, Value.Length);
        }

        private static double GetValue(string Value, int index)
        {
            for (var i = index - 1; i >= 0; i--)
            {
                if (char.IsDigit(Value[i]) && i != 0) continue;
                int offset = i == 0 ? 0 : 1;
                if (double.TryParse(Value.Substring(i + offset, index - i - offset), out double result)) return result;
                return 0;
            }
            return 0;
        }
    }
}

using Common.EntityModels;
using Common.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StrategyTrader.Logic
{
    internal class TradingCalendar
    {
        private readonly ICollection<InstrumentSession> instrumentSessions;
        public bool IsRolloverDay => (TimeOnExchange.DayOfYear >= RolloverDate.DayOfYear && !IsWeekend(TimeOnExchange));

        public DateTime RolloverDate => ExpirationDate.AddDays(-ExpirationRule.DaysBefore);

        public DateTime ExpirationDate { get; set; }

        public ExpirationRule ExpirationRule { get; set; }

        public DateTime TimeOnExchange => TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById(ExchangeTimeZone));

        public string ExchangeTimeZone{ get; set; }

        public TradingCalendar(ExpirationRule expirationRule, DateTime expirationDate, ICollection<InstrumentSession> instrumentSessions, string exchangeTimezone)
        {
            this.instrumentSessions = instrumentSessions;
            ExchangeTimeZone = exchangeTimezone;
            ExpirationRule = expirationRule;
            ExpirationDate = expirationDate;
            
        }

        public bool IsTradingDay()
        {
            var dt = TimeOnExchange;
            if (IsWeekend(dt)) return false;

            if (instrumentSessions?.Count > 0)
            {
                var day = dt.DayOfWeek.ToInt();
                bool toReturn = false;
                Parallel.ForEach(instrumentSessions, session =>
                {
                    if (day == (int)session.OpeningDay)
                    {
                        toReturn = true;
                    }
                });
                return toReturn;
            }

            if (Properties.Settings.Default.TradingOnBankingHoliday)
            {
                return IsFederalHoliday(dt);
            }

            return true;
        }

        private static bool IsWeekend(DateTime dt)
        {
            if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }
            return false;
        }

        public bool IsTradingHour()
        {
            if (instrumentSessions?.Count > 0)
            {
                DateTime now = TimeOnExchange;
                var day = now.DayOfWeek.ToInt();
                var nowInMinutes = now.Minute;
                bool toReturn = false;
                Parallel.ForEach(instrumentSessions, session =>
                {
                    if (day == (int)session.OpeningDay && session.ClosingTime.Minutes > nowInMinutes)
                    {
                        toReturn = true;
                    }
                });
                return toReturn;
            }

            return true;
        }

        /// <summary>
        /// Determines if this date is a federal holiday.
        /// </summary>
        /// <param name="date">This date</param>
        /// <returns>True if this date is a federal holiday</returns>
        public static bool IsFederalHoliday(DateTime date)
        {
            // to ease typing
            int nthWeekDay = (int)(Math.Ceiling((double)date.Day / 7.0d));
            DayOfWeek dayName = date.DayOfWeek;
            bool isThursday = dayName == DayOfWeek.Thursday;
            bool isFriday = dayName == DayOfWeek.Friday;
            bool isMonday = dayName == DayOfWeek.Monday;
            bool isWeekend = dayName == DayOfWeek.Saturday || dayName == DayOfWeek.Sunday;

            // New Years Day (Jan 1, or preceding Friday/following Monday if weekend)
            if ((date.Month == 12 && date.Day == 31 && isFriday) ||
                (date.Month == 1 && date.Day == 1 && !isWeekend) ||
                (date.Month == 1 && date.Day == 2 && isMonday)) return true;

            // MLK day (3rd monday in January)
            if (date.Month == 1 && isMonday && nthWeekDay == 3) return true;

            // President’s Day (3rd Monday in February)
            if (date.Month == 2 && isMonday && nthWeekDay == 3) return true;

            // Memorial Day (Last Monday in May)
            if (date.Month == 5 && isMonday && date.AddDays(7).Month == 6) return true;

            // Independence Day (July 4, or preceding Friday/following Monday if weekend)
            if ((date.Month == 7 && date.Day == 3 && isFriday) ||
                (date.Month == 7 && date.Day == 4 && !isWeekend) ||
                (date.Month == 7 && date.Day == 5 && isMonday)) return true;

            // Labor Day (1st Monday in September)
            if (date.Month == 9 && isMonday && nthWeekDay == 1) return true;

            // Columbus Day (2nd Monday in October)
            if (date.Month == 10 && isMonday && nthWeekDay == 2) return true;

            // Veteran’s Day (November 11, or preceding Friday/following Monday if weekend))
            if ((date.Month == 11 && date.Day == 10 && isFriday) ||
                (date.Month == 11 && date.Day == 11 && !isWeekend) ||
                (date.Month == 11 && date.Day == 12 && isMonday)) return true;

            // Thanksgiving Day (4th Thursday in November)
            if (date.Month == 11 && isThursday && nthWeekDay == 4) return true;

            // Christmas Day (December 25, or preceding Friday/following Monday if weekend))
            if ((date.Month == 12 && date.Day == 24 && isFriday) ||
                (date.Month == 12 && date.Day == 25 && !isWeekend) ||
                (date.Month == 12 && date.Day == 25 && isMonday)) return true;

            return false;
        }
    }
}
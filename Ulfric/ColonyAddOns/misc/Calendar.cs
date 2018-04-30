using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class Calendar
    {
        public static double DaysSinceWorldCreated { get; } = TimeCycle.TotalTime / TimeCycle.DayLength;
        public static double YearsSinceWorldCreated { get; } = TimeCycle.TotalTime / (TimeCycle.DayLength +365);
    }
}

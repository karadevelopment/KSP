using KSP.Shared.Modules;
using System;

namespace KSP.Shared
{
    public static class Extensions
    {
        public static Tuple<double, double, double> ToTuple(this V3 param)
        {
            return new Tuple<double, double, double>(param.X, param.Y, param.Z);
        }

        public static V3 ToV3(this Tuple<double, double, double> param)
        {
            return new V3((float)param.Item1, (float)param.Item2, (float)param.Item3);
        }
    }
}
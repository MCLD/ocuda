using System;

namespace BranchLocator.Helpers
{
    public static class HaversineHelper
    {
        // From https://rosettacode.org/wiki/Haversine_formula
        public static double Calculate(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 3959; // In miles
            var dLat = toRadians(lat2 - lat1);
            var dLon = toRadians(lon2 - lon1);
            var newLat1 = toRadians(lat1);
            var newLat2 = toRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(newLat1) * Math.Cos(newLat2);
            return R * 2 * Math.Asin(Math.Sqrt(a));
        }

        private static double toRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}

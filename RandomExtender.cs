using System;

namespace ASD.Graphs
{
    public static class RandomExtender
    {
        public static double Next(this Random rnd, double minValue, double maxValue, bool integer)
        {
            if (integer) return rnd.Next((int)(minValue + 0.5), (int)(maxValue + 1.5));
            var number = 2.0 * rnd.NextDouble();//why????
            if (number <= 1.0) return minValue + number * (maxValue - minValue);
            number -= 1.0;
            return minValue + number * (maxValue - minValue);
        }
    }
}

using Microsoft.Xna.Framework;
using System;

namespace Oudidon
{
    public static class MathUtils
    {
        public static float NormalizedParabolicPosition(float t)
        {
            return 4 * t * (1 - t);
        }

        public static bool OverlapsWith(Rectangle first, Rectangle second)
        {
            return !(first.Bottom < second.Top
                    || first.Right < second.Left
                    || first.Top > second.Bottom
                    || first.Left > second.Right);
        }

        public static double NextGaussian(double mu = 0, double sigma = 1)
        {
            var u1 = 1.0 - CommonRandom.Random.NextDouble(); //uniform(0,1] random doubles
            var u2 = 1.0 - CommonRandom.Random.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            var randNormal = mu + sigma * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }
    }
}

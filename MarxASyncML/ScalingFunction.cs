using System.Threading.Tasks;

namespace MarxASyncML
{
    public class MinMax
    {
        public double min;
        public double max;
    }

    public class ScalingFunction
    {
        public Task<MinMax> FindMinMax(double[] input)
        {
            MinMax minMax = new MinMax();
            minMax.min = double.MaxValue;
            minMax.max = double.MinValue;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] > minMax.max)
                    minMax.max = input[i];

                if (input[i] < minMax.min)
                    minMax.min = input[i];
            }

            return Task.FromResult(minMax);
        }

        public Task<double[]> LinearScaleToRange(double[] input, MinMax oldMinMax, MinMax newMinMax)
        {
            double[] scaled = new double[input.Length];
            double oldMinMaxDiff = oldMinMax.max - oldMinMax.min;

            Parallel.For(0, input.Length, i =>
            {
                double inputOldMinDiff = input[i] - oldMinMax.min;
                scaled[i] = (newMinMax.min * (1 - (inputOldMinDiff / oldMinMaxDiff))) + (newMinMax.max * (inputOldMinDiff / oldMinMaxDiff));
            });

            return Task.FromResult(scaled);
        }
    }
}

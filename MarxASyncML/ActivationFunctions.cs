using System;
using System.Threading.Tasks;

namespace MarxASyncML
{
    public class ActivationFunctions
    {
        public Task<double[]> Step(double[] x, double min, double max)
        {
            double[] rX = new double[x.Length];

            Parallel.For(0, x.Length, i =>
            {
                if (x[i] < ((max - min) / 2) + min)
                    rX[i] = min;
                else
                    rX[i] = max;
            });

            return Task.FromResult(rX);
        }

        public Task<double[]> Sigmoid(double[] x)
        {
            double[] rX = new double[x.Length];

            Parallel.For(0, x.Length, i =>
            {
                rX[i] = 1.0f / (1.0f + (float)Math.Exp(-x[i]));
            });

            return Task.FromResult(rX);
        }

        public Task<double[]> TanSigmoid(double[] x)
        {
            double[] rX = new double[x.Length];

            Parallel.For(0, x.Length, i =>
            {
                rX[i] = 2 / (1 + Math.Exp(-2 * x[i])) - 1;
            });

            return Task.FromResult(rX);
        }

        public Task<double[]> LogSigmoid(double[] x)
        {
            double[] rX = new double[x.Length];

            Parallel.For(0, x.Length, i =>
            {
                rX[i] = 1 / (1 + Math.Exp(-x[i]));
            });

            return Task.FromResult(rX);
        }

        public Task<double[]> TruncatedLogSigmoid(double[] x)
        {
            double[] rX = new double[x.Length];

            Parallel.For(0, x.Length, i =>
            {
                if (x[i] < -45.0) rX[i] = 0.0;
                else if (x[i] > 45.0) rX[i] = 1.0;
                else rX[i] = 1.0 / (1.0 + Math.Exp(-x[i]));
            });

            return Task.FromResult(rX);
        }

        public Task<double[]> TruncatedHyperTanFunction(double[] x)
        {
            double[] rX = new double[x.Length];

            Parallel.For(0, x.Length, i =>
            {
                if (x[i] < -45.0) rX[i] = -1.0;
                else if (x[i] > 45.0) rX[i] = 1.0;
                else rX[i] = Math.Tanh(x[i]);
            });

            return Task.FromResult(rX);
        }

        public Task<double[]> ByteOutput(double[] x, int midPoint)
        {
            double[] rX = new double[x.Length];

            Parallel.For(0, x.Length, i =>
            {
                if (x[i] < midPoint)
                    rX[i] = 0;
                else
                    rX[i] = 1;
            });

            return Task.FromResult(rX);
        }
    }
}

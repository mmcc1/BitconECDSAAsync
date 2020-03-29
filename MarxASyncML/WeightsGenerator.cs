using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MarxASyncML
{
    public class WeightsGenerator
    {
        private readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public Task<double[]> CreateRandomWeightsPositive(int numElements)
        {
            double[] weights = new double[numElements];

            Parallel.For(0, numElements, async i =>
            {
                weights[i] = await NextDoubleBetween0and1();
            });
            
            return Task.FromResult(weights);
        }

        public Task<double[]> CreateRandomWeightsIntPositive(int numElements)
        {
            double[] weights = new double[numElements];

            Parallel.For(0, numElements, async i =>
            {
                weights[i] = await RandomInteger(0, 255);
            });

            return Task.FromResult(weights);
        }

        #region Rng Methods

        private Task<double> NextDoubleBetween0and1()
        {
            // Step 1: fill an array with 8 random bytes
            var rng = new RNGCryptoServiceProvider();
            var bytes = new Byte[8];
            rng.GetBytes(bytes);
            // Step 2: bit-shift 11 and 53 based on double's mantissa bits
            var ul = BitConverter.ToUInt64(bytes, 0) / (1 << 11);
            return Task.FromResult(ul / (Double)(1UL << 53));
        }

        private Task<int> RandomInteger(int min, int max)
        {
            uint scale = uint.MaxValue;
            while (scale == uint.MaxValue)
            {
                // Get four random bytes.
                byte[] four_bytes = new byte[4];
                rng.GetBytes(four_bytes);

                // Convert that into an uint.
                scale = BitConverter.ToUInt32(four_bytes, 0);
            }

            // Add min to the scaled difference between max and min.
            return Task.FromResult((int)(min + (max - min) * (scale / (double)uint.MaxValue)));
        }

        #endregion
    }
}

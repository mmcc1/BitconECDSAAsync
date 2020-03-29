using System.Threading.Tasks;

namespace MarxASyncML
{
    public class GeneticAlgorithm
    {
        public async Task<bool[]> EvaluateFitness(double[] parentWeights, double mutationRate)
        {
            WeightsGenerator wg = new WeightsGenerator();
            bool[] shouldKeep = new bool[parentWeights.Length];
            double[] k = await wg.CreateRandomWeightsPositive(1);

            for (int i = 0; i < parentWeights.Length; i++)
            {
                if (k[0] > mutationRate)  //Change this value to determine evolution rate.
                    shouldKeep[i] = true;
                else
                    shouldKeep[i] = false;
            }

            return shouldKeep;
        }

        public Task<double[]> CrossOverAndMutation(bool[] evaluateFitnessResult, double[] weights)
        {
            WeightsGenerator wg = new WeightsGenerator();
            double[] childWeights = new double[weights.Length];

            Parallel.For(0, weights.Length, async i =>
            {
                if (evaluateFitnessResult[i])
                    childWeights[i] = weights[i];
                else
                {
                    double[] l = await wg.CreateRandomWeightsPositive(1);
                    childWeights[i] = l[0];
                }
            });

            return Task.FromResult(childWeights);
        }
    }
}

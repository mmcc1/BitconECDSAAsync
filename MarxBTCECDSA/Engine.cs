﻿using MarxASyncML;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarxBTCECDSA
{
    public class Engine : EngineBase
    {
        private List<double[]> parentWeights;  //Only used as an indicator of improvement, not recorded anywhere
        double min;
        double max;
        double maxStat;

        public Engine()
        {
            parentWeights = new List<double[]>();
            min = double.MaxValue;
            max = double.MinValue;
        }

        #region Deep Learning network design

        //The neural network design. Simple to use, number of networks determines number of inputs in next layer.
        internal override async Task DesignNN()
        {
            //Create three layers
            NeuralNetworkLayerDesign nnld1 = new NeuralNetworkLayerDesign() { LayerNumber = 0, NumberOfInputs = 20, NumberOfNetworks = 20 };
            nnld1.Weights = await weightsGenerator.CreateRandomWeightsPositive(nnld1.NumberOfInputs * nnld1.NumberOfNetworks);  //generate random weights

            NeuralNetworkLayerDesign nnld2 = new NeuralNetworkLayerDesign() { LayerNumber = 1, NumberOfInputs = nnld1.NumberOfNetworks, NumberOfNetworks = 32 };
            nnld2.Weights = await weightsGenerator.CreateRandomWeightsPositive(nnld2.NumberOfInputs * nnld2.NumberOfNetworks);

            NeuralNetworkLayerDesign nnld3 = new NeuralNetworkLayerDesign() { LayerNumber = 2, NumberOfInputs = nnld2.NumberOfNetworks, NumberOfNetworks = 256 };
            nnld3.Weights = await weightsGenerator.CreateRandomWeightsPositive(nnld3.NumberOfInputs * nnld3.NumberOfNetworks);

            NeuralNetworkLayerDesign nnld4 = new NeuralNetworkLayerDesign() { LayerNumber = 3, NumberOfInputs = nnld3.NumberOfNetworks, NumberOfNetworks = 256 };
            nnld4.Weights = await weightsGenerator.CreateRandomWeightsPositive(nnld4.NumberOfInputs * nnld4.NumberOfNetworks);

            nnld1.Biases = new double[nnld1.Weights.Length];  //Set biases to zero.
            nnld2.Biases = new double[nnld2.Weights.Length];
            nnld3.Biases = new double[nnld3.Weights.Length];
            nnld4.Biases = new double[nnld4.Weights.Length];

            nnld.Add(nnld1);
            nnld.Add(nnld2);
            nnld.Add(nnld3);
            nnld.Add(nnld4);
        }

        #endregion

        #region Execute

        public override async Task Execute()
        {
            Console.WriteLine(string.Format("Engine Starting..."));
            await DesignNN();
            bool shouldRun = true;

            while (shouldRun)  //Infinite loop
            {
                await GenerateDataset();
                await GenerateValidationDataset();
                await Train();
            }
        }

        #endregion

        private Task Train()
        {
            Parallel.For(0, dataSet.Count, async i =>
            {
                //No activation function used between hidden layers.
                double[] hiddenLayer1 = await neuralNetwork.PerceptronLayer(nnld[0].NumberOfNetworks, dataSet[i].PublicAddressDouble, nnld[0].Weights, nnld[0].NumberOfInputs, nnld[0].Biases);
                double[] hiddenLayer2 = await neuralNetwork.PerceptronLayer(nnld[1].NumberOfNetworks, hiddenLayer1, nnld[1].Weights, nnld[1].NumberOfInputs, nnld[1].Biases);
                double[] hiddenLayer3 = await neuralNetwork.PerceptronLayer(nnld[2].NumberOfNetworks, hiddenLayer2, nnld[2].Weights, nnld[2].NumberOfInputs, nnld[2].Biases);
                double[] outputlayer = await neuralNetwork.PerceptronLayer(nnld[3].NumberOfNetworks, hiddenLayer3, nnld[3].Weights, nnld[3].NumberOfInputs, nnld[3].Biases);
                //MinMax mm = scalingFunction.FindMinMax(outputlayer);  //debug to calculate min/max of outputlayer.  Used to determine mid-point
                //min = mm.min < min ? mm.min : min;
                //max = mm.max > max ? mm.max : max;
                outputlayer = await activationFunctions.ByteOutput(outputlayer, 29180215);     //Output is a 32 byte key (32 x 8 bits).  Value used in ByteOutputE is drawn from a 12 hour run to determine mid point.

                await Assess(outputlayer, i);

                //if(i % 1000 == 0)
                //    Console.WriteLine(string.Format("Min: {0}, Max: {1}", min, max));
            });

            //Console.WriteLine(string.Format("Summary - Min: {0}, Max: {1}", min, max

            return Task.CompletedTask;
        }

        private async Task Assess(double[] outputlayer, int index)
        {
            int matchCount = 0;
            outputlayer = await ConvertFromBinaryToDouble(outputlayer);               //Convert output of neural network to double

            for (int i = 0; i < dataSet[index].PrivateKey.Length; i++)          //Check how many bytes of the output match private key of input public address
            {
                if (dataSet[index].PrivateKey[i] == (int)outputlayer[i])
                    matchCount++;
            }

            if (matchCount > currentMaxBytes)                                   //If the number of bytes is an improvement, record those weights
            {
                currentMaxBytes = matchCount;
                parentWeights.Clear();
                parentWeights.Add(nnld[0].Weights);
                parentWeights.Add(nnld[1].Weights);
                parentWeights.Add(nnld[2].Weights);
                parentWeights.Add(nnld[3].Weights);

                currentDeathRate = 0;

                await Validate();                                                     //Test for generalisation across validation set
            }
            else if (matchCount > 0)
            {
                await Validate();                                                     //The number of bytes found does not correlate with generalisation of a particular byte so test                                                     
                currentDeathRate++;
            }
            else
                currentDeathRate++;



            if (currentDeathRate >= deathRate)                                  //Reset. Clears parent weights so that new weights are found.
            {
                currentDeathRate = 0;
                parentWeights.Clear();
                currentMaxBytes = 0;
            }

            await UpdateWeights();
        }



        #region Validation of generalisation

        private async Task Validate()
        {
            //Console.WriteLine("Validating...");
            bool shouldSave = false;

            WeightStore ws = new WeightStore() { Statistics = new double[32], WeightsHL0 = nnld[0].Weights, WeightsHL1 = nnld[1].Weights, WeightsHL2 = nnld[2].Weights, WeightsOL = nnld[3].Weights };

            for(int i = 0; i < valdataSet.Count; i++)
            {
                double[] hiddenLayer1 = await neuralNetwork.PerceptronLayer(nnld[0].NumberOfNetworks, valdataSet[i].PublicAddressDouble, nnld[0].Weights, nnld[0].NumberOfInputs, nnld[0].Biases);
                double[] hiddenLayer2 = await neuralNetwork.PerceptronLayer(nnld[1].NumberOfNetworks, hiddenLayer1, nnld[1].Weights, nnld[1].NumberOfInputs, nnld[1].Biases);
                double[] hiddenLayer3 = await neuralNetwork.PerceptronLayer(nnld[2].NumberOfNetworks, hiddenLayer2, nnld[2].Weights, nnld[2].NumberOfInputs, nnld[2].Biases);
                double[] outputlayer = await neuralNetwork.PerceptronLayer(nnld[3].NumberOfNetworks, hiddenLayer3, nnld[3].Weights, nnld[3].NumberOfInputs, nnld[3].Biases);
                outputlayer = await activationFunctions.ByteOutput(outputlayer, 29180215);

                ws = await ValidateTest(outputlayer, i, ws);
            }

            for (int i = 0; i < ws.Statistics.Length; i++)
                if (ws.Statistics[i] > 50)  //Means 0.005 based upon validation set of 10000.  Adjust as required.  Use to determine what gets saved
                {
                    shouldSave = true;

                    if (ws.Statistics[i] > maxStat)
                    {
                        maxStat = ws.Statistics[i];
                        Console.WriteLine(string.Format("Current highest stat is: {0}, Probability: {1}", maxStat, maxStat / 10000));
                    }
                }

            if (shouldSave)
            {
                //Console.WriteLine(Environment.NewLine);
                //SerialiseWeightsAndSaveToDB(ws.Statistics);  //Save to DB using entity framework - uncomment to enable - remember to set connection string in WeightsDBContext
            }
        }

        private async Task<WeightStore> ValidateTest(double[] outputlayer, int index, WeightStore ws)
        {
            outputlayer = await ConvertFromBinaryToDouble(outputlayer);

            for (int i = 0; i < valdataSet[index].PrivateKey.Length; i++)
            {
                if (valdataSet[index].PrivateKey[i] == (int)outputlayer[i])  //Check output against private keys
                    ws.Statistics[i]++;
            }

            return ws;
        }

        #endregion

        #region Convert Output layer from binary to double array

        //Output layer is a binary representation of 32 bytes.  This turns it into an array of doubles.
        private Task<double[]> ConvertFromBinaryToDouble(double[] data)
        {
            double[] value = new double[32];
            int index = 0;
            for (int i = 0; i < 32; i++)
            {
                if (data[index++] == 1)
                    value[i] += 1;
                if (data[index++] == 1)
                    value[i] += 2;
                if (data[index++] == 1)
                    value[i] += 4;
                if (data[index++] == 1)
                    value[i] += 8;
                if (data[index++] == 1)
                    value[i] += 16;
                if (data[index++] == 1)
                    value[i] += 32;
                if (data[index++] == 1)
                    value[i] += 64;
                if (data[index++] == 1)
                    value[i] += 128;
            }

            return Task.FromResult(value);
        }

        #endregion

        #region Update Weights

        private async Task UpdateWeights()
        {
            if (parentWeights.Count == 0)  //No successful weights found yet
            {
                nnld[0].Weights = await weightsGenerator.CreateRandomWeightsPositive(nnld[0].NumberOfInputs * nnld[0].NumberOfNetworks);
                nnld[1].Weights = await weightsGenerator.CreateRandomWeightsPositive(nnld[1].NumberOfInputs * nnld[1].NumberOfNetworks);
                nnld[2].Weights = await weightsGenerator.CreateRandomWeightsPositive(nnld[2].NumberOfInputs * nnld[2].NumberOfNetworks);
                nnld[3].Weights = await weightsGenerator.CreateRandomWeightsPositive(nnld[3].NumberOfInputs * nnld[3].NumberOfNetworks);
            }
            else  //Derive a new set of weights from previously successful weights - play around with evolution rate in EvaluateFitness2.
            {
                nnld[0].Weights = await geneticAlgorithm.CrossOverAndMutation(await geneticAlgorithm.EvaluateFitness(nnld[0].Weights, 0.025), nnld[0].Weights);
                nnld[1].Weights = await geneticAlgorithm.CrossOverAndMutation(await geneticAlgorithm.EvaluateFitness(nnld[1].Weights, 0.025), nnld[1].Weights);
                nnld[2].Weights = await geneticAlgorithm.CrossOverAndMutation(await geneticAlgorithm.EvaluateFitness(nnld[2].Weights, 0.025), nnld[2].Weights);
                nnld[3].Weights = await geneticAlgorithm.CrossOverAndMutation(await geneticAlgorithm.EvaluateFitness(nnld[3].Weights, 0.025), nnld[3].Weights);
            }
        }

        #endregion
        /*
        #region Serialise weights and save to DB

        //Weights are stored as serialised strings rather than objects - lazy me.
        private void SerialiseWeightsAndSaveToDB(double[] weightStatistics)
        {
            WeightsDBContext wdbc = new WeightsDBContext();
            WeightStatistics ws = new WeightStatistics();

            ws.WeightsHL0 = SerialiseWeights(nnld[0].Weights);
            ws.WeightsHL1 = SerialiseWeights(nnld[1].Weights);
            ws.WeightsHL2 = SerialiseWeights(nnld[2].Weights);
            ws.WeightsOL = SerialiseWeights(nnld[3].Weights);

            ws.Byte0 = (int)weightStatistics[0];
            ws.Byte1 = (int)weightStatistics[1];
            ws.Byte2 = (int)weightStatistics[2];
            ws.Byte3 = (int)weightStatistics[3];
            ws.Byte4 = (int)weightStatistics[4];
            ws.Byte5 = (int)weightStatistics[5];
            ws.Byte6 = (int)weightStatistics[6];
            ws.Byte7 = (int)weightStatistics[7];
            ws.Byte8 = (int)weightStatistics[8];
            ws.Byte9 = (int)weightStatistics[9];
            ws.Byte10 = (int)weightStatistics[10];
            ws.Byte11 = (int)weightStatistics[11];
            ws.Byte12 = (int)weightStatistics[12];
            ws.Byte13 = (int)weightStatistics[13];
            ws.Byte14 = (int)weightStatistics[14];
            ws.Byte15 = (int)weightStatistics[15];
            ws.Byte16 = (int)weightStatistics[16];
            ws.Byte17 = (int)weightStatistics[17];
            ws.Byte18 = (int)weightStatistics[18];
            ws.Byte19 = (int)weightStatistics[19];
            ws.Byte20 = (int)weightStatistics[20];
            ws.Byte21 = (int)weightStatistics[21];
            ws.Byte22 = (int)weightStatistics[22];
            ws.Byte23 = (int)weightStatistics[23];
            ws.Byte24 = (int)weightStatistics[24];
            ws.Byte25 = (int)weightStatistics[25];
            ws.Byte26 = (int)weightStatistics[26];
            ws.Byte27 = (int)weightStatistics[27];
            ws.Byte28 = (int)weightStatistics[28];
            ws.Byte29 = (int)weightStatistics[29];
            ws.Byte30 = (int)weightStatistics[30];
            ws.Byte31 = (int)weightStatistics[31];

            wdbc.Add(ws);
            wdbc.SaveChanges();

        }

        private string SerialiseWeights(double[] w)
        {
            StringBuilder weights = new StringBuilder();

            for (int i = 0; i < w.Length; i++)
            {
                weights.Append(w[i].ToString() + ",");
            }

            return weights.ToString();
        }

        #endregion
    */
    }
}

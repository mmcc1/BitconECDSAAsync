using System;
using System.Threading.Tasks;

namespace MarxBTCECDSA
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Bitcoin ECDSA Cracker...");
            await ExecuteEngine();
        }

        private static async Task ExecuteEngine()
        {
            Engine eng = new Engine();
            await eng.Execute();

        }
    }
}

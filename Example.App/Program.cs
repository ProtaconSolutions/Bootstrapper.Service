using System;
using System.Threading;

namespace Example.App
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Hello, i am runnable program 1.");
                Thread.Sleep(5*1000);
            }
        }
    }
}

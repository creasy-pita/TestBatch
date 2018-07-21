using System;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            try
            {
                TestThrowSubThreadExceptionToMainThread();
                Console.WriteLine("dddddddd:");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("sssssss:" + ex.Message);
                Console.ReadKey();
            }
        }


        public static void TestThrowSubThreadExceptionToMainThread()
        {
            try { 
            Thread t = new Thread(Method1);
            t.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("eeeeeeee:" + ex.Message);
                Console.ReadKey();
            }
        }

        public static void Method1()
        {
            int a = 0; int b = 1;
            a = b / a;
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using TestBatch;
using TestBatch.Controllers;

namespace TestBatchUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            ValuesController valuesController = new ValuesController();

            valuesController.Get(2);
        }
        [TestMethod]
        public void Main()
        {
            try {
                TestThrowSubThreadExceptionToMainThread();
                Console.WriteLine("dddddddd:");
            }
            catch (Exception ex)
            {
                Console.WriteLine("dddddddd:"+ ex.Message);
                Console.ReadKey();
            }
        }

        public void TestThrowSubThreadExceptionToMainThread()
        {
            Thread t = new Thread(Method1);
            t.Start();

        }

        public void Method1()
        {
            int a = 0;int b = 1;
            a = b / a;
        }
    }


}

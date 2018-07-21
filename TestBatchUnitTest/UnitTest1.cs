using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpRift;

namespace SharpRiftTests
{
    [TestClass]
    public class OVRInterfaceTests
    {
        [TestMethod]
        public void TestInitializeAndShutdown()
        {
            VR.Initialize();
            VR.Shutdown();
        }

        [TestMethod]
        public void TestVersion()
        {
            Assert.AreEqual("0.4.2", VR.GetVersionString());
        }
    }
}

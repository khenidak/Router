using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace SFRouter
{
    [TestClass]
    public partial class SFRouterTests
    {
       

        [TestInitialize]
        public void Setup()
        {
            Trace.WriteLine("***********************************************************");
            Trace.WriteLine(@"Must run .\ps\SetupClusterForTest.ps1 manually (As Admin)");
            Trace.WriteLine("***********************************************************");
        }


        [TestCleanup]
        public void Cleanup()
        {



            Trace.WriteLine("***********************************************************");
            Trace.WriteLine(@"Must run .\ps\CleanClusterAfterTest.ps1 manually (As Admin)");
            Trace.WriteLine("***********************************************************");

        }

    }
}

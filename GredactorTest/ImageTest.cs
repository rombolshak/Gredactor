using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gredactor;
using System.IO;



namespace GredactorTest
{
    /// <summary>
    /// Summary description for ImageTest
    /// </summary>
    [TestClass]
    public class ImageTest
    {
        ImageHandler image;
        public ImageTest()
        {
            image = ImageHandler.GetInstanse();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            image.Reset();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestImageIsNullOnStartup()
        {
            Assert.IsNull(image.Image);
        }

        [TestMethod]
        public void TestItIsOnlyOneImageHandler()
        {
            ImageHandler image2 = ImageHandler.GetInstanse();
            Assert.AreSame(image, image2);
        }

        [TestMethod]
        public void TestImageOpen()
        {
            image.Open(@"C:\Users\roma\Documents\Visual Studio 2010\Projects\CG_1\Gredactor\bin\Debug\test.bmp");
            Assert.IsNotNull(image.Image);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Why we can save file before it has been opened?")]
        public void TestCannotSaveWithoutOpen()
        {
            image.Save();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Why we can save file before it has been opened?")]
        public void TestCannotSaveAsWithoutOpen()
        {
            image.SaveAs("somefile.bmp");
        }

        [TestMethod]
        public void TestSaveAs()
        {
            image.Open(@"C:\Users\roma\Documents\Visual Studio 2010\Projects\CG_1\Gredactor\bin\Debug\test.bmp");
            image.SaveAs(@"C:\Users\roma\Documents\Visual Studio 2010\Projects\CG_1\Gredactor\bin\Debug\saved.bmp");
            Assert.IsTrue(File.Exists(@"C:\Users\roma\Documents\Visual Studio 2010\Projects\CG_1\Gredactor\bin\Debug\saved.bmp"));
            File.Delete(@"C:\Users\roma\Documents\Visual Studio 2010\Projects\CG_1\Gredactor\bin\Debug\saved.bmp");
            image.Save();
            Assert.IsTrue(File.Exists(@"C:\Users\roma\Documents\Visual Studio 2010\Projects\CG_1\Gredactor\bin\Debug\saved.bmp"));
            File.Delete(@"C:\Users\roma\Documents\Visual Studio 2010\Projects\CG_1\Gredactor\bin\Debug\saved.bmp");
        }

        [TestMethod]
        public void TestCannotSelectWithoutOpen()
        {
            Assert.Fail();
        }
    }
}

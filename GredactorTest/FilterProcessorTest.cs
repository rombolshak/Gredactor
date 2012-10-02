using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GredactorTest
{
    /// <summary>
    /// Summary description for FilterProcessorTest
    /// </summary>
    [TestClass]
    public class FilterProcessorTest
    {
        FilterProcessing.FilterProcessor processor;
        public FilterProcessorTest()
        {
            //
            // TODO: Add constructor logic here
            //
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
        public void TestConstructorWithoutParams()
        {
            Assert.IsNotNull(new FilterProcessing.FilterProcessor());
        }

        [TestMethod]
        public void TestConstructorWithMatrix()
        {
            Assert.IsNotNull(new FilterProcessing.FilterProcessor(new double[][] { new double[] { 1, 1, 1 }, new double[] { 0, 0, 1 }, new double[] { 1, 0, 0 } }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConstructorWithNonSquareMatrix()
        {
            new FilterProcessing.FilterProcessor(new double[][] {
                new double[] {1,2,3},
                new double[] {4,5,6}
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConstructorWithNullMatrix()
        {
            new FilterProcessing.FilterProcessor(new double[][] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConstructorWithWrongMatrix()
        {
            new FilterProcessing.FilterProcessor(new double[][] {
                new double[] {1,2,3},
                new double[] {4,5},
                new double[] {6,7,8,9}
            });
        }

        [TestMethod]
        public void TestProcessMatrix()
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(4, 4);
            for (int x = 0; x < 4; ++x) for (int y = 0; y < 4; ++y) bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(x + y * 4 + 1, x + y * 4 + 1, x + y * 4 + 1));
            processor = new FilterProcessing.FilterProcessor(new double[][] {
                new double[] {0,0,0},
                new double[] {0,0,0},
                new double[] {0,1,0}
            });
            bmp = processor.Process(bmp, null);
            for (int x = 0; x < 3; ++x) for (int y = 0; y < 4; ++y) Assert.IsTrue(bmp.GetPixel(x, y) == System.Drawing.Color.FromArgb(x + y * 4 + 2, x + y * 4 + 2, x + y * 4 + 2));
            for (int y = 0; y < 4; ++y) Assert.IsTrue(bmp.GetPixel(3, y) == System.Drawing.Color.FromArgb(255, 0, 0, 0));
        }

        [TestMethod]
        public void TestProcessOtherMatrix()
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(5, 5);
            for (int x = 0; x < 5; ++x) for (int y = 0; y < 5; ++y) bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(x + y * 5, x + y * 5, x + y * 5));
            processor = new FilterProcessing.FilterProcessor(new double[][] {
                new double[] {1,0,0},
                new double[] {0,1,0},
                new double[] {0,0,1}
            });
            bmp = processor.Process(bmp, null);
            Assert.IsTrue(bmp.GetPixel(0, 0) == System.Drawing.Color.FromArgb(3, 3, 3));
            Assert.IsTrue(bmp.GetPixel(1, 0) == System.Drawing.Color.FromArgb(4,4,4));
            Assert.IsTrue(bmp.GetPixel(2, 0) == System.Drawing.Color.FromArgb(5,5,5));
            Assert.IsTrue(bmp.GetPixel(3, 0) == System.Drawing.Color.FromArgb(6,6,6));
            Assert.IsTrue(bmp.GetPixel(4, 0) == System.Drawing.Color.FromArgb(4,4,4));

            Assert.IsTrue(bmp.GetPixel(0, 1) == System.Drawing.Color.FromArgb(8,8,8));
            Assert.IsTrue(bmp.GetPixel(1, 1) == System.Drawing.Color.FromArgb(6,6,6));
            Assert.IsTrue(bmp.GetPixel(2, 1) == System.Drawing.Color.FromArgb(7,7,7));
            Assert.IsTrue(bmp.GetPixel(3, 1) == System.Drawing.Color.FromArgb(8,8,8));
            Assert.IsTrue(bmp.GetPixel(4, 1) == System.Drawing.Color.FromArgb(6,6,6));

            Assert.IsTrue(bmp.GetPixel(0, 2) == System.Drawing.Color.FromArgb(13,13,13));
            Assert.IsTrue(bmp.GetPixel(1, 2) == System.Drawing.Color.FromArgb(11,11,11));
            Assert.IsTrue(bmp.GetPixel(2, 2) == System.Drawing.Color.FromArgb(12,12,12));
            Assert.IsTrue(bmp.GetPixel(3, 2) == System.Drawing.Color.FromArgb(13,13,13));
            Assert.IsTrue(bmp.GetPixel(4, 2) == System.Drawing.Color.FromArgb(11,11,11));

            Assert.IsTrue(bmp.GetPixel(0, 3) == System.Drawing.Color.FromArgb(18,18,18));
            Assert.IsTrue(bmp.GetPixel(1, 3) == System.Drawing.Color.FromArgb(16,16,16));
            Assert.IsTrue(bmp.GetPixel(2, 3) == System.Drawing.Color.FromArgb(17,17,17));
            Assert.IsTrue(bmp.GetPixel(3, 3) == System.Drawing.Color.FromArgb(18,18,18));
            Assert.IsTrue(bmp.GetPixel(4, 3) == System.Drawing.Color.FromArgb(16,16,16));

            Assert.IsTrue(bmp.GetPixel(0, 4) == System.Drawing.Color.FromArgb(20,20,20));
            Assert.IsTrue(bmp.GetPixel(1, 4) == System.Drawing.Color.FromArgb(18,18,18));
            Assert.IsTrue(bmp.GetPixel(2, 4) == System.Drawing.Color.FromArgb(19,19,19));
            Assert.IsTrue(bmp.GetPixel(3, 4) == System.Drawing.Color.FromArgb(20,20,20));
            Assert.IsTrue(bmp.GetPixel(4, 4) == System.Drawing.Color.FromArgb(21,21,21));
        }

        [TestMethod]
        public void TestSeparation()
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(5, 5);
            for (int x = 0; x < 5; ++x) for (int y = 0; y < 5; ++y) bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(x + y * 5, x + y * 5, x + y * 5));
            processor = new FilterProcessing.FilterProcessor(new double[][] { new double[] { 1, 0, 0 } }, true);
            bmp = processor.Process(bmp, null);

            Assert.IsTrue(bmp.GetPixel(0, 0) == System.Drawing.Color.FromArgb(0, 0, 0));
            Assert.IsTrue(bmp.GetPixel(1, 0) == System.Drawing.Color.FromArgb(0, 0, 0));
            Assert.IsTrue(bmp.GetPixel(2, 0) == System.Drawing.Color.FromArgb(0, 0, 0));
            Assert.IsTrue(bmp.GetPixel(3, 0) == System.Drawing.Color.FromArgb(0, 0, 0));
            Assert.IsTrue(bmp.GetPixel(4, 0) == System.Drawing.Color.FromArgb(0, 0, 0));

            Assert.IsTrue(bmp.GetPixel(0, 1) == System.Drawing.Color.FromArgb(0, 0, 0));
            Assert.IsTrue(bmp.GetPixel(1, 1) == System.Drawing.Color.FromArgb(0, 0, 0));
            Assert.IsTrue(bmp.GetPixel(2, 1) == System.Drawing.Color.FromArgb(1,1,1));
            Assert.IsTrue(bmp.GetPixel(3, 1) == System.Drawing.Color.FromArgb(2,2,2));
            Assert.IsTrue(bmp.GetPixel(4, 1) == System.Drawing.Color.FromArgb(3,3,3));

            Assert.IsTrue(bmp.GetPixel(0, 2) == System.Drawing.Color.FromArgb(0,0,0));
            Assert.IsTrue(bmp.GetPixel(1, 2) == System.Drawing.Color.FromArgb(5,5,5));
            Assert.IsTrue(bmp.GetPixel(2, 2) == System.Drawing.Color.FromArgb(6,6,6));
            Assert.IsTrue(bmp.GetPixel(3, 2) == System.Drawing.Color.FromArgb(7,7,7));
            Assert.IsTrue(bmp.GetPixel(4, 2) == System.Drawing.Color.FromArgb(8,8,8));

            Assert.IsTrue(bmp.GetPixel(0, 3) == System.Drawing.Color.FromArgb(0,0,0));
            Assert.IsTrue(bmp.GetPixel(1, 3) == System.Drawing.Color.FromArgb(10,10,10));
            Assert.IsTrue(bmp.GetPixel(2, 3) == System.Drawing.Color.FromArgb(11, 11,11));
            Assert.IsTrue(bmp.GetPixel(3, 3) == System.Drawing.Color.FromArgb(12,12,12));
            Assert.IsTrue(bmp.GetPixel(4, 3) == System.Drawing.Color.FromArgb(13,13,13));

            Assert.IsTrue(bmp.GetPixel(0, 4) == System.Drawing.Color.FromArgb(0,0,0));
            Assert.IsTrue(bmp.GetPixel(1, 4) == System.Drawing.Color.FromArgb(15,15,15));
            Assert.IsTrue(bmp.GetPixel(2, 4) == System.Drawing.Color.FromArgb(16,16,16));
            Assert.IsTrue(bmp.GetPixel(3, 4) == System.Drawing.Color.FromArgb(17,17,17));
            Assert.IsTrue(bmp.GetPixel(4, 4) == System.Drawing.Color.FromArgb(18,18,18));
        }
    }
}

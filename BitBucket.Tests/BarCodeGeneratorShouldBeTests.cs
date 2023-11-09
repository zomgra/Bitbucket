using Bitbucket.Services;
using System.Collections.Generic;
using Xunit;

namespace BitBucket.Tests
{
    public class BarCodeGeneratorShouldBeTests
    {
        private readonly BarCodeGenerator _barcodeGenerator;

        public BarCodeGeneratorShouldBeTests()
        {
            _barcodeGenerator = new BarCodeGenerator();
        }
        [Fact]
        public void Return25LengthBarCode()
        {
            var barcode = _barcodeGenerator.GenerateBarCode(2,2);
            Assert.Equal(25, barcode.Length);
        }

        [Fact]
        public void FirstTwoAndLastTwoIsNotNumber()
        {
            var barcode = _barcodeGenerator.GenerateBarCode(2, 2);
            string firstTwo = barcode.Substring(0, 2);
            string lastTwo = barcode.Substring(barcode.Length - 2);

            Assert.False(int.TryParse(firstTwo, out int r));
            Assert.False(int.TryParse(lastTwo, out int i));
        }
    }
}
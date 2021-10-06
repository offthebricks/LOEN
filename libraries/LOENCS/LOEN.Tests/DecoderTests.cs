using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LOEN.Tests
{
	[TestClass]
	public class DecoderTests
	{
		[TestMethod]
		public void TestFullyLoaded()
		{
			var testObj = new TestModels.FullyLoaded(true);
			string val, check;

			check = Encoder.Encode(testObj);
			testObj = new TestModels.FullyLoaded(false);
			testObj = Decoder.Decode<TestModels.FullyLoaded>(check);
			val = Encoder.Encode(testObj);

			Assert.AreEqual(check, val);
		}
	}
}

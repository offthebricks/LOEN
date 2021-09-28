using System;
using LOEN;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LOEN.Tests
{
	[TestClass]
	public class EncoderTests
	{
		/// <summary>
		/// This method is extra important since other tests rely upon it
		/// It should be very similar to the one found in the LOEN class
		/// </summary>
		private string escapeString(string str)
		{
			//escape all double quotes
			str = '"' + str.Replace('"', '\"') + '"';
			//escape all newlines
			str = str.Replace("\n", "\\n");
			//escape all carriage returns
			str = str.Replace("\r", "\\r");
			return str;
		}

		private static bool isAlphaNumeric(string str)
		{
			bool result = true;
			foreach (char c in str)
			{
				if (!char.IsLetterOrDigit(c))
				{
					result = false;
				}
			}
			return result;
		}

		[TestMethod]
		public void TestInts()
		{
			var testObj = new TestModels.FullyLoaded(true);
			string val;

			val = Encoder.Encode(testObj.posint);
			Assert.AreEqual("+" + testObj.posint.ToString(), val);

			val = Encoder.Encode(testObj.negint);
			Assert.AreEqual(testObj.negint.ToString(), val);

			val = Encoder.Encode(testObj.zeroint);
			Assert.AreEqual("+0", val);
		}

		[TestMethod]
		public void TestFloats()
		{
			var testObj = new TestModels.FullyLoaded(true);
			string val;

			val = Encoder.Encode(testObj.posfloat);
			Assert.AreEqual("+" + testObj.posfloat.ToString(), val);

			val = Encoder.Encode(testObj.negfloat);
			Assert.AreEqual(testObj.negfloat.ToString(), val);

			val = Encoder.Encode(testObj.zerofloat);
			Assert.AreEqual("+0", val);
		}

		[TestMethod]
		public void TestDoubles()
		{
			var testObj = new TestModels.FullyLoaded(true);
			string val;

			val = Encoder.Encode(testObj.posdouble);
			Assert.AreEqual("+" + testObj.posdouble.ToString(), val);

			val = Encoder.Encode(testObj.negdouble);
			Assert.AreEqual(testObj.negdouble.ToString(), val);

			val = Encoder.Encode(testObj.zerodouble);
			Assert.AreEqual("+0", val);
		}

		[TestMethod]
		public void TestStrings()
		{
			var testObj = new TestModels.FullyLoaded(true);
			string val;

			val = Encoder.Encode(testObj.strnowhitespace);
			Assert.AreEqual(testObj.strnowhitespace, val);

			val = Encoder.Encode(testObj.stroneline);
			Assert.AreEqual(this.escapeString(testObj.stroneline), val);

			val = Encoder.Encode(testObj.strmultiline);
			Assert.AreEqual(this.escapeString(testObj.strmultiline), val);

			val = Encoder.Encode(testObj.strempty);
			Assert.AreEqual(testObj.strempty, val);
		}

		[TestMethod]
		public void TestObjectShortList()
		{
			var testObj = new TestModels.ShortList(237);
			string val, check = "";
			var members = testObj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);

			val = Encoder.Encode(testObj);

			foreach(var member in members)
			{
				if(check != "")
				{
					check += ",";
				}
				switch (member.Name)
				{
					case "id": check += "id" + Encoder.Encode(testObj.id); break;
					case "name": check += "name" + Encoder.Encode(testObj.name); break;
					case "label": check += "label:" + Encoder.Encode(testObj.label); break;
					case "property": check += "property:" + Encoder.Encode(testObj.property); break;
				}
			}
			check = "{" + check + "}";

			Assert.AreEqual(check, val);
		}

		[TestMethod]
		public void TestStandardArray()
		{
			var testObj = new TestModels.FullyLoaded(true);
			string val, check = "";

			foreach (string item in testObj.arrstr)
			{
				check += Encoder.Encode(item);
			}
			check = "[" + check + "]";

			val = Encoder.Encode(testObj.arrstr);

			Assert.AreEqual(check, val);
		}

		[TestMethod]
		public void TestCompressedArray()
		{
			var testObj = new TestModels.FullyLoaded(true);
			var shortlistObj = new TestModels.ShortList(237);
			var members = shortlistObj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
			string val, check = "", subcheck;

			val = Encoder.Encode(testObj.arrshortlist);

			foreach (var member in members)
			{
				if (check != "")
				{
					check += ",";
				}
				switch (member.Name)
				{
					case "id":
					case "name":
					case "label":
					case "property":
						check += member.Name;
						break;
					default: break;
				}
			}
			check = "[" + check + "]";

			foreach(TestModels.ShortList obj in testObj.arrshortlist)
			{
				subcheck = "";
				members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
				foreach (var member in members)
				{
					switch (member.Name)
					{
						case "id": subcheck += Encoder.Encode(obj.id); break;
						case "name": subcheck += Encoder.Encode(obj.name); break;
						case "label": subcheck += ":" + Encoder.Encode(obj.label); break;
						case "property": subcheck += ":" + Encoder.Encode(obj.property); break;
					}
				}
				check += "[" + subcheck + "]";
			}

			check = "<" + check + ">";

			Assert.AreEqual(check, val);
		}

		[TestMethod]
		public void TestStandardList()
		{
			var testObj = new TestModels.FullyLoaded(true);
			string val, check = "";

			foreach(string item in testObj.liststr)
			{
				check += Encoder.Encode(item);
			}
			check = "[" + check + "]";

			val = Encoder.Encode(testObj.liststr);

			Assert.AreEqual(check, val);
		}

		[TestMethod]
		public void TestCompressedLists()
		{
			var testObj = new TestModels.FullyLoaded(true);
			var shortlistObj = new TestModels.ShortList(237);
			var members = shortlistObj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
			string val, check = "", subcheck;

			val = Encoder.Encode(testObj.listshortlist);

			foreach (var member in members)
			{
				if (check != "")
				{
					check += ",";
				}
				switch (member.Name)
				{
					case "id":
					case "name":
					case "label":
					case "property":
						check += member.Name;
						break;
					default: break;
				}
			}
			check = "[" + check + "]";

			foreach (TestModels.ShortList obj in testObj.listshortlist)
			{
				subcheck = "";
				members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
				foreach (var member in members)
				{
					switch (member.Name)
					{
						case "id": subcheck += Encoder.Encode(obj.id); break;
						case "name": subcheck += Encoder.Encode(obj.name); break;
						case "label": subcheck += ":" + Encoder.Encode(obj.label); break;
						case "property": subcheck += ":" + Encoder.Encode(obj.property); break;
					}
				}
				check += "[" + subcheck + "]";
			}

			check = "<" + check + ">";

			Assert.AreEqual(check, val);

			val = Encoder.Encode(testObj.listdictionary);

			check = "";
			foreach (var key in testObj.listdictionary[0].Keys)
			{
				if (check != "")
				{
					//check += ",";
				}
				check += escapeString(key.ToString());
			}
			check = "[" + check + "]";

			foreach(var dic in testObj.listdictionary)
			{
				check += "[";
				foreach(var item in dic)
				{
					check += Encoder.Encode(item.Value);
				}
				check += "]";
			}
			check = "<" + check + ">";

			Assert.AreEqual(check, val);
		}

		[TestMethod]
		public void TestDictionaries()
		{
			var testObj = new TestModels.FullyLoaded(true);
			string check = "", val;
			int i;

			val = Encoder.Encode(testObj.dicint);
			check = "";
			for (i = 2; i < 7; i++)
			{
				if(i > 2)
				{
					check += ",";
				}
				check += i.ToString() + "+" + (i + 10).ToString();
			}
			check = "{" + check + "}";

			Assert.AreEqual(check, val);

			val = Encoder.Encode(testObj.dicstr);
			check = "";
			for (i = 3; i < 8; i++)
			{
				if(i > 3)
				{
					check += ",";
				}
				check += "\"idx = " + i.ToString() + "\"\"value is " + (i + 10).ToString() + "\"";
			}
			check = "{" + check + "}";

			Assert.AreEqual(check, val);

			val = Encoder.Encode(testObj.dicshortlist);
			check = "";
			foreach(var item in testObj.dicshortlist)
			{
				if (!string.IsNullOrEmpty(check))
				{
					check += ",";
				}
				check += escapeString(item.Key.ToString()) + Encoder.Encode(item.Value);
			}
			check = "{" + check + "}";

			Assert.AreEqual(check, val);
		}
	}
}

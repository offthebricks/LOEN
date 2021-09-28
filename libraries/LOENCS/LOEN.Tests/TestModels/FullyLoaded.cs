using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOEN.Tests.TestModels
{
	public class FullyLoaded
	{
		public byte byteval = 0x13;	//19
		public char charval = (char)0x72;

		public int posint = 516;
		public int negint = -732;
		public int zeroint = 0;

		public float posfloat = (float)51.67;
		public float negfloat = (float)-83.21;
		public float zerofloat = 0;

		public double posdouble = 51.67;
		public double negdouble = -83.21;
		public double zerodouble = 0;

		public string strnowhitespace = "TestPerson";
		public string stroneline = "abc is like 123";
		public string strmultiline = 
@"abc
is like
123";
		public string strempty = "";

		public ShortList objshortlist;

		public string[] arrstr;
		public ShortList[] arrshortlist;

		public List<string> liststr;
		public List<ShortList> listshortlist;
		public List<Dictionary<string, string>> listdictionary;

		public Dictionary<int, int> dicint;
		public Dictionary<string, string> dicstr;
		public Dictionary<string, ShortList> dicshortlist;

		public FullyLoaded() { }

		public FullyLoaded(bool initialize)
		{
			if (!initialize)
			{
				return;
			}
			int i;
			objshortlist = new ShortList(123);

			arrstr = new string[5];
			arrstr[0] = strnowhitespace;
			arrstr[1] = stroneline;
			arrstr[2] = strmultiline;
			arrstr[3] = strempty;
			arrshortlist = new ShortList[5];
			arrshortlist[0] = new ShortList(0);
			arrshortlist[1] = new ShortList(1);
			arrshortlist[2] = new ShortList(2);
			arrshortlist[3] = new ShortList(3);
			arrshortlist[4] = new ShortList(4);

			liststr = new List<string>();
			liststr.Add(strnowhitespace);
			liststr.Add(stroneline);
			liststr.Add(strmultiline);
			liststr.Add(strempty);

			listshortlist = new List<ShortList>();
			for (i = 0; i < 5; i++)
			{
				listshortlist.Add(new ShortList(i));
			}

			listdictionary = new List<Dictionary<string, string>>();
			for(int v=0; v<5; v++)
			{
				var tmpdic = new Dictionary<string, string>();
				for (i = 3; i < 6; i++)
				{
					tmpdic.Add("idx = " + i.ToString(), "value is " + (v + i + 10).ToString());
				}
				listdictionary.Add(tmpdic);
			}

			dicint = new Dictionary<int, int>();
			for (i = 2; i < 7; i++)
			{
				dicint.Add(i, i + 10);
			}

			dicstr = new Dictionary<string, string>();
			for (i = 3; i < 8; i++)
			{
				dicstr.Add("idx = " + i.ToString(), "value is " + (i + 10).ToString());
			}

			dicshortlist = new Dictionary<string, ShortList>();
			dicshortlist.Add("list 1", new ShortList(1));
			dicshortlist.Add("list 2", new ShortList(2));
			dicshortlist.Add("list 3", new ShortList(3));
			dicshortlist.Add("list 4", new ShortList(4));
		}
	}

	public class ShortList
	{
		public int id;
		public string name = "test person";
		public string label = "nospacehere";

		private string _property = "isprop";
		public string property
		{
			get { return _property; }
			set { _property = value; }
		}

		public ShortList() { }

		public ShortList(int idx)
		{
			this.id = idx;
			label += idx.ToString();
		}
	}
}

/*
Copyright 2021 OffTheBricks - https://github.com/offthebricks/LOEN

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace LOEN
{
	#region utilities
	class Utilities
	{
		public static string skipWhitespace(ref string str)
		{
			int pos = 0; string check = "";

			do
			{
				if (pos >= str.Length)
				{
					throw new Exception("missing closing character - end of encoding found");
				}
				check = str.Substring(pos, 1);
				pos++;
			} while (check.Trim().Length == 0);

			if (pos > 1)
			{
				str = str.Substring(pos - 1);
			}

			return check;
		}

		public static bool isAlphaNumeric(string str)
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

		public static bool IsNumberType(object value)
		{
			return value is sbyte
					|| value is byte
					|| value is char
					|| value is short
					|| value is ushort
					|| value is int
					|| value is uint
					|| value is long
					|| value is ulong
					|| value is float
					|| value is double
					|| value is decimal;
		}

		public static object CastNumber(object source, object target)
		{
			if(target is sbyte)
			{
				return Convert.ToSByte(source);
			}
			if (target is byte)
			{
				return Convert.ToByte(source);
			}
			if(target is char)
			{
				return Convert.ToChar(Convert.ToInt16(source));
			}
			if (target is short)
			{
				return Convert.ToInt16(source);
			}
			if (target is ushort)
			{
				return Convert.ToUInt16(source);
			}
			if (target is int)
			{
				return Convert.ToInt32(source);
			}
			if (target is uint)
			{
				return Convert.ToUInt32(source);
			}
			if (target is long)
			{
				return Convert.ToInt64(source);
			}
			if (target is ulong)
			{
				return Convert.ToUInt64(source);
			}
			if (target is float)
			{
				return Convert.ToSingle(source);
			}
			if (target is decimal)
			{
				return Convert.ToDecimal(source);
			}
			//target is most likely a double, and source is already a double
			return source;
		}

		public static bool is_array(object obj)
		{
			Type type = obj.GetType();
			//Make sure this isn't a dictionary
			if (is_dictionary(obj))
			{
				return false;
			}
			return typeof(IEnumerable).IsAssignableFrom(type);
		}

		/// <summary>
		/// Determines if the object type is of Associated Array / Dictionary
		/// </summary>
		/// <param name="type">Type of the object in question</param>
		/// <returns>true for yes, false for no</returns>
		public static bool is_dictionary(object obj)
		{
			Type type = obj.GetType();
			if (type.Name.Contains("Dictionary") && !type.Name.Contains("DictionaryEntry"))
			{
				return true;
			}
			return false;
		}

		public static bool is_object(object obj)
		{
			if (obj is string)
			{
				return false;
			}
			if (obj is object)
			{
				return true;
			}
			return false;
		}
	}

	#endregion
	#region encoding

	public class Encoder
	{
		/// <summary>
		/// Calls doEncode to convert inobj to a LOEN encoded string
		/// </summary>
		public static string Encode(object inobj, bool compressArrays = true, bool escapeLineEndings = true)
		{
			var loen = new Encoder(compressArrays, escapeLineEndings);
			return loen.doEncode(inobj);
		}

		/// <summary>
		/// Alias of Encode
		/// </summary>
		public static string Serialize(object inobj, bool compressArrays = true, bool escapeLineEndings = true)
		{
			return Encode(inobj, compressArrays, escapeLineEndings);
		}

		/// <summary>
		/// Alias of Encode
		/// </summary>
		public static string Stringify(object inobj, bool compressArrays = true, bool escapeLineEndings = true)
		{
			return Encode(inobj, compressArrays, escapeLineEndings);
		}

		/****************************************************/

		private bool CompressionEnabled = true;
		private bool EscapeLineEndings = true;

		public Encoder(bool enableArrayCompression = true, bool enableEscapeLineEndings = true)
		{
			this.CompressionEnabled = enableArrayCompression;
			this.EscapeLineEndings = enableEscapeLineEndings;
		}

		public string escapeString(string str)
		{
			if (!Utilities.isAlphaNumeric(str))
			{
				//escape all double quotes
				str = '"' + str.Replace('"', '\"') + '"';
				if (this.EscapeLineEndings)
				{
					//escape all newlines
					str = str.Replace("\n", "\\n");
					//escape all carriage returns
					str = str.Replace("\r", "\\r");
				}
			}
			return str;
		}

		/// <summary>
		/// Encodes the supplied object to a LOEN encode string
		/// </summary>
		public string doEncode(object obj)
		{
			string str = "";

			if (obj == null)
			{
				str += "=n";
			}
			else
			{
				if (obj is bool)
				{
					bool check = (bool)obj;
					if (check)
					{
						str += "=t";
					}
					else
					{
						str += "=f";
					}
				}
				else if (Utilities.IsNumberType(obj))
				{
					double num;
					if (obj is char)
					{
						obj = Convert.ToInt16(obj);
					}
					num = Convert.ToDouble(obj);
					if(num >= 0)
					{
						str += "+" + obj.ToString();
					}
					else
					{
						str += obj.ToString();
					}
				}
				else if (obj is string)
				{
					str = ":" + this.escapeString((string)obj);
				}
				else if (Utilities.is_dictionary(obj))
				{
					//encode dictionary as object
					var objdic = (IDictionary)obj;
					//build a list of stringified keys first
					var keylist = new List<string>();
					foreach(var key in objdic.Keys)
					{
						keylist.Add(key.ToString());
					}
					//loop values and encode
					int i = 0;
					string tmp;
					foreach(var value in objdic.Values)
					{
						if (!string.IsNullOrEmpty(str))
						{
							str += ",";
						}
						tmp = this.doEncode(value);
						//if the value is already prefix with a non-alphanumeric characer, don't need the colon
						if (!Utilities.isAlphaNumeric(tmp.Substring(0, 1)))
						{
							str += this.escapeString(keylist[i]) + tmp;
						}
						else
						{
							str += this.escapeString(keylist[i]) + ":" + tmp;
						}
						i++; 
					}
					//add object closure
					str = "{" + str + "}";
				}
				//check if is an array or list - must come after 'string' check, as string is an array of 'char'
				else if (Utilities.is_array(obj))
				{
					var inlist = new List<object>();
					IList list = (IList)obj;
					foreach(object item in list)
					{
						inlist.Add(item);
					}
					str = encode_array(inlist);
				}
				//check if is an object (might not work this way)
				else if (obj is object)
				{
					var members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
					string tmp;

					foreach (var member in members)
					{
						if (!string.IsNullOrEmpty(str))
						{
							str += ",";
						}
						if(member.MemberType == MemberTypes.Field)
						{
							tmp = this.doEncode(((FieldInfo)member).GetValue(obj));
						}
						else if(member.MemberType == MemberTypes.Property)
						{
							tmp = this.doEncode(((PropertyInfo)member).GetValue(obj, null));
						}
						else
						{
							continue;
						}
						//if the value is already prefix with a non-alphanumeric characer, don't need the colon
						if(!string.IsNullOrEmpty(tmp) && !Utilities.isAlphaNumeric(tmp.Substring(0, 1))){
							str += this.escapeString(member.Name) + tmp;
						}
						else
						{
							str += this.escapeString(member.Name) + ":" + tmp;
						}
					}
					str = "{" + str + "}";
				}
				else
				{
					throw new LOENException("Unsupported type passed for encoding: " + obj.GetType().Name);
				}
			}

			return str;
		}

		private string encode_array(List<object> arr)
		{
			if(arr.Count == 0)
			{
				return "";
			}
			if(arr.Count == 1)
			{
				return "[" + this.doEncode(arr[0]) + "]";
			}
			string str = null;
			MemberInfo[] members = null;
			var keys = new List<string>();
			var check = new List<string>();
			bool compress = this.CompressionEnabled;
			if (compress)
			{
				for(int i=0; i<2; i++)
				{
					if (Utilities.is_dictionary(arr[i]))
					{
						//get a list of sub dictionary key name, for analysis
						foreach (var key in ((IDictionary)arr[i]).Keys)
						{
							check.Add(key.ToString());
						}
					}
					else if(Utilities.is_object(arr[i]))
					{
						//get a list of names of sub object fields and properties, for analysis
						members = arr[i].GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
						foreach (var member in members)
						{
							if (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property)
							{
								check.Add(member.Name);
							}
							else
							{
								continue;
							}
						}
					}
					else
					{
						compress = false;
						break;
					}
					//check if keys are the same in the first two array values
					if(i == 0)
					{
						keys = check;
						check = new List<string>();
						continue;
					}
					for(int v=0; v<keys.Count; v++)
					{
						if(v >= check.Count || keys[v] != check[v])
						{
							compress = false;
							break;
						}
					}
				}
				if (compress)
				{
					string tmp;
					foreach(string key in keys)
					{
						tmp = this.escapeString(key);
						if(str == null)
						{
							str = "";
						}
						else if (Utilities.isAlphaNumeric(tmp.Substring(0, 1)))
						{
							tmp = "," + tmp;
						}
						str += tmp;
					}
					str = "[" + str + "]";
				}
			}
			for (int i = 0; i < arr.Count; i++)
			{
				if(str == null)
				{
					str = "";
				}
				if (compress)
				{
					int v = 0;
					string substr = null, tmp = "";
					foreach(string key in keys)
					{
						//if is dictionary
						if (members == null)
						{
							int j = 0;
							foreach(var item in ((IDictionary)arr[i]).Values)
							{
								if(v == j)
								{
									tmp = this.doEncode(item);
									break;
								}
								j++;
							}
							v++;
						}
						//if is object
						else
						{
							foreach (var member in members)
							{
								if (member.Name == key && (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property))
								{
									if (member.MemberType == MemberTypes.Field)
									{
										tmp = this.doEncode(((FieldInfo)member).GetValue(arr[i]));
									}
									else if (member.MemberType == MemberTypes.Property)
									{
										tmp = this.doEncode(((PropertyInfo)member).GetValue(arr[i], null));
									}
									break;
								}
							}
						}
						if(substr == null)
						{
							substr = "";
						}
						if (tmp.Length == 0 || Utilities.isAlphaNumeric(tmp.Substring(0, 1)))
						{
							//assume this is a string
							tmp = ":" + tmp;
						}
						substr += tmp;
					}
					str += "[" + substr + "]";
				}
				else
				{
					str += this.doEncode(arr[i]);
				}
			}
			if (compress)
			{
				return "<" + str + ">";
			}
			return "[" + str + "]";
		}
	}
	#endregion

	#region decoder
	public class Decoder
	{
		public static TOutput Decode<TOutput>(string loen) where TOutput : new()
		{
			if (string.IsNullOrEmpty(loen))
			{
				object obj = new Object();
				if (typeof(TOutput) == typeof(string))
				{
					obj = "";
				}
				return (TOutput)obj;
			}

			object result;
			string str = loen;
			try
			{
				result = parseSegment(ref str);
			}
			catch (LOENException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				//guess but also provide the actual exception
				throw new LOENException("error in encoding - invalid separator or closing - " + loen.Substring(0, 15) + "...", e);
			}

			//convert from type object to type TOutput
			var output = new TOutput();
			return (TOutput)ConvertGenericToDefined(result, (object)output);
		}

		/// <summary>
		/// Alias of Decode
		/// </summary>
		public static object Deserialize<TOutput>(string loen) where TOutput : new()
		{
			return Decode<TOutput>(loen);
		}

		/// <summary>
		/// Alias of Decode
		/// </summary>
		public static object Parse<TOutput>(string loen) where TOutput : new()
		{
			return Decode<TOutput>(loen);
		}

		private static object CreateInstance(Type objtype, object loenobj = null)
		{
			object obj;
			//initialize the variable so that type detecting code will work
			if(objtype == typeof(string))
			{
				obj = "";
			}
			else if (typeof(IEnumerable).IsAssignableFrom(objtype))
			{
				//if is an array
				if (objtype.HasElementType)
				{
					obj = (IList)Array.CreateInstance(objtype.GetElementType(), ((IList)loenobj).Count);
				}
				else if (objtype.GenericTypeArguments.Length > 1)
				{
					obj = (IDictionary)Activator.CreateInstance(objtype);
				}
				//initialize list
				else
				{
					obj = (IList)Activator.CreateInstance(objtype);
				}
			}
			else
			{
				obj = Activator.CreateInstance(objtype);
			}
			return obj;
		}

		private static object ConvertGenericToDefined(object generic, object obj)
		{
			object tmp = null, tmp2 = null;

			try
			{
				if (obj is bool)
				{
					obj = generic;
				}
				else if (Utilities.IsNumberType(obj))
				{
					obj = Utilities.CastNumber(generic, obj);
				}
				else if (obj is string)
				{
					obj = generic;
				}
				else if (Utilities.is_dictionary(obj))
				{
					var gendic = (IDictionary)generic;
					var keys = new List<object>();
					//decoder dictionary keys are always strings, so need to check if that's the actual type we need
					tmp2 = CreateInstance(((IDictionary)obj).GetType().GetGenericArguments()[0]);
					foreach (var key in gendic.Keys)
					{
						if(tmp2 is string)
						{
							keys.Add(key);
						}
						else
						{
							keys.Add(Utilities.CastNumber(key, tmp2));
						}
					}
					int i = 0;
					foreach(var item in gendic.Values)
					{
						tmp2 = CreateInstance(((IDictionary)obj).GetType().GetGenericArguments()[1], item);
						tmp = ConvertGenericToDefined(item, tmp2);
						((IDictionary)obj).Add(keys[i], tmp);
						i++;
					}
				}
				//check if is an array or list - must come after 'string' check, as string is an array of 'char'
				else if (Utilities.is_array(obj))
				{
					int i = 0;
					foreach(var item in (IList)generic)
					{
						if (obj.GetType().HasElementType)
						{
							tmp2 = CreateInstance(obj.GetType().GetElementType(), item);
						}
						else
						{
							tmp2 = CreateInstance(obj.GetType().GetGenericArguments()[0], item);
						}
						tmp = ConvertGenericToDefined(item, tmp2);
						//if this is an array that is ready for values
						if (((IList)obj).Count == ((IList)generic).Count)
						{
							((IList)obj)[i] = tmp;
						}
						//this is a list which does not have all elements initialized
						else
						{
							((IList)obj).Add(tmp);
						}
						i++;
					}
				}
				//check if is an object (might not work this way)
				else if (obj is object)
				{
					//generic must be a dictionary
					var gendic = (IDictionary)generic;
					//get members of target object
					var members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);

					foreach (var member in members)
					{
						if (member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
						{
							continue;
						}
						//find matching key index in dictionary
						int i = 0, v = 0;
						foreach(var key in gendic.Keys)
						{
							if(key.ToString() == member.Name)
							{
								break;
							}
							i++;
						}
						//find value matching the key
						foreach(var item in gendic.Values)
						{
							if(i == v)
							{
								tmp = item;
								break;
							}
							v++;
						}
						//set the dictionary value to the target object
						if (member.MemberType == MemberTypes.Field)
						{
							var field = (FieldInfo)member;
							tmp2 = field.GetValue(obj);
							if(tmp2 == null)
							{
								tmp2 = CreateInstance(field.FieldType, tmp);
							}
							if (tmp == null)
							{
								field.SetValue(obj, null);
							}
							else
							{
								field.SetValue(obj, ConvertGenericToDefined(tmp, tmp2));
							}
						}
						else if (member.MemberType == MemberTypes.Property)
						{
							var prop = (PropertyInfo)member;
							tmp2 = prop.GetValue(obj);
							if (tmp2 == null)
							{
								//initialize the variable so that type detecting code will work
								tmp2 = Activator.CreateInstance(prop.PropertyType);
							}
							if (tmp == null)
							{
								prop.SetValue(obj, null);
							}
							else
							{
								prop.SetValue(obj, ConvertGenericToDefined(tmp, tmp2));
							}
						}
					}
				}
				else
				{
					throw new LOENException("unknown type passed for decoding: " + obj.GetType().Name);
				}
			}
			catch (LOENException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				throw new LOENException("Error in converting LOEN object to type: " + obj.GetType().Name, e);
			}
			
			return obj;
		}

		private static object parseSegment(ref string str)
		{
			string type = Utilities.skipWhitespace(ref str);

			if (!Utilities.isAlphaNumeric(type))
			{
				str = str.Substring(1);
				switch (type)
				{
					case "{":
						return parseObject(ref str);
					case "[":
						return parseArray(ref str);
					case "<":
						return parseCompressedArray(ref str);
					case "\"":
						return parseQuotedString(ref str);
					case ":":
						break;
					case "-":
						str = "-" + str;        //negative number
						return parseValue(ref str, true);
					case "+":
					case "=":
						return parseValue(ref str, true);
					//catch content closing or separator
					case ",":
					case "}":
					case "]":
					case ">":
						//need to restore 'type' to signal the calling code
						str = type + str;
						throw null;// new Exception();
					default:
						throw new LOENException("invalid segment character: (" + type + ") " + str.Substring(0, 15) + "...");
				}
			}

			return parseValue(ref str);
		}

		private static object parseValue(ref string str, bool equalflag = false)
		{
			//check for next symbol
			int i = 0;
			string res = "";
			string onechar = Utilities.skipWhitespace(ref str);
			switch (onechar)
			{
				case "\"":
				case "{":
				case "[":
					return parseSegment(ref str);
				case "-":
				case ".":
					break;
				default:
					if (!Utilities.isAlphaNumeric(onechar))
					{
						return "";
					}
					break;
			}
			str = str.Substring(1);
			int strlen = str.Length;
			while ((Utilities.isAlphaNumeric(onechar) || onechar == "." || (res.Length == 0 && onechar == "-")) && i <= strlen)
			{
				res += onechar;
				onechar = str.Substring(i, 1);
				i++;
			}
			str = str.Substring(res.Length - 1);
			if (equalflag)
			{
				if (res == "n" || res == "null")
				{
					return null;
				}
				if (res == "t" || res == "true")
				{
					return true;
				}
				if (res == "f" || res == "false")
				{
					return false;
				}
				try
				{
					return Convert.ToDouble(res);
				}
				catch (Exception e)
				{
					throw new LOENException("invalid value found in numeric field: (" + res + ")", e);
				}
			}
			return res;
		}

		private static string parseQuotedString(ref string str)
		{
			string res = null;
			int pos = 0;
			do
			{
				pos = str.IndexOf("\"", pos);
				//check if quote is preceded by a backslash '\' - might not handle double backslash before quote
				if (str.Substring(pos - 1, 1) == "\\")
				{
					//increment pos so to look for the next double quote
					pos++;
				}
				//no slash so grab value
				else
				{
					res = str.Substring(0, pos);
					str = str.Substring(pos + 1);
				}
			} while (res == null);
			//replace all escaped double quotes with regular double quotes
			res = res.Replace("\\\"", "\"");
			//replace all escaped newlines with regular newlines
			res = res.Replace("\\n", "\n");
			//replace all escaped carriage returns with regular carriage returns
			res = res.Replace("\\r", "\r");

			return res;
		}

		private static Dictionary<string, object> parseObject(ref string str)
		{
			var res = new Dictionary<string, object>();
			string prop, check = "";
			object tmp;

			do
			{
				//handle any whitespace or commas before the property name
				prop = "";
				if(check.Trim().Length == 0 || !Utilities.isAlphaNumeric(check))
				{
					check = Utilities.skipWhitespace(ref str);
				}
				else
				{
					prop = check;
					check = str.Substring(0, 1);
				}
				//if the property is double quoted
				if(check == "\"")
				{
					str = str.Substring(1);
					prop = parseQuotedString(ref str);
				}
				//if this is a normal property without quotes
				else
				{
					//look for first non-alphanumeric value to isolate the property name
					while (Utilities.isAlphaNumeric(check))
					{
						prop += check;
						str = str.Substring(1);

						if(str.Length == 0)
						{
							throw new LOENException("error in encoding - missing closing '}' - end of encoding found");
						}
						check = str.Substring(0, 1).Trim();
					}
				}
				if(prop.Length > 0)
				{
					try
					{
						tmp = parseSegment(ref str);
						res.Add(prop, tmp);
					}
					catch (LOENException e)
					{
						throw e;
					}
					catch (Exception e)
					{
						check = str.Substring(0, 1);
						if(check == ",")
						{
							str = str.Substring(1);
						}
						else if(check != "}")
						{
							throw new LOENException("error in encoding detected (expected '}') - " + str.Substring(0, 15) + "...", e);
						}
					}
				}
				check = str.Substring(0, 1);
				str = str.Substring(1);
			} while (check != "}");

			return res;
		}

		private static List<object> parseArray(ref string str)
		{
			var res = new List<object>();
			string check;
			object tmp;
			do
			{
				try
				{
					tmp = parseSegment(ref str);
					res.Add(tmp);
				}
				catch(LOENException e)
				{
					throw e;
				}
				catch
				{
					check = str.Substring(0, 1);
					if(check == ",")
					{
						str = str.Substring(1);
					}
					else if(check != "]")
					{
						throw new LOENException("error in encoding detected (expected ']', got '" + check + "' - " + str.Substring(0, 15) + "...");
					}
				}
			} while (str.Substring(0, 1) != "]");
			str = str.Substring(1);

			return res;
		}

		private static List<object> parseCompressedArray(ref string str)
		{
			var res = new List<object>();
			var arr = new List<List<object>>();
			var keys = new List<object>();
			var tmp = new List<object>();
			string check;
			do
			{
				if (keys.Count == 0)
				{
					try
					{
						keys = (List<object>)parseSegment(ref str);
					}
					catch (LOENException e)
					{
						throw e;
					}
					catch (Exception e)
					{
						throw new LOENException("error in compressed array keys encoding - " + str.Substring(0, 15) + "...", e);
					}
				}
				else
				{
					try
					{
						tmp = (List<object>)parseSegment(ref str);			//will always be a list
						arr.Add(tmp);
					}
					catch (LOENException e)
					{
						throw e;
					}
					catch
					{
						check = str.Substring(0, 1);
						if (check == ",")
						{
							str = str.Substring(1);
						}
						else if (check != ">")
						{
							throw new LOENException("error in encoding detected (expected '>') - " + str.Substring(0, 15) + "...");
						}
					}
				}
			} while (str.Substring(0, 1) != ">");
			str = str.Substring(1);

			int i;
			Dictionary<string, object> row;
			foreach (var data in arr)
			{
				row = new Dictionary<string, object>();
				i = 0;
				foreach (string key in keys)
				{
					if(i >= data.Count)
					{
						throw new LOENException("missing key (" + key + ") in compressed array");
					}
					row.Add(key, data[i]);
					i++;
				}
				res.Add(row);
			}

			return res;
		}
	}
	#endregion

	public partial class LOENException : Exception
	{
		public LOENException(string message) : base(message) { }

		public LOENException(string message, Exception inner) : base(message, inner) { }
	}
}

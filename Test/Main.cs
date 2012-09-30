using System;
using YAJLSharp;

namespace Test
{
	public class TestParser : YAJLParser
	{
		protected override bool Null()
		{
			Console.WriteLine("Null");
			return true;
		}

		protected override bool Number(IntPtr numberVal, IntPtr numberLen)
		{
			Console.WriteLine("Number");
			return true;
		}

		protected override bool String(IntPtr stringVal, IntPtr stringLen)
		{
			Console.WriteLine("String");
			return true;
		}

		protected override bool Integer(long value)
		{
			Console.WriteLine("Intger");
			return true;
		}

		protected override bool Boolean(bool value)
		{
			Console.WriteLine("Boolean");
			return true;
		}

		protected override bool Double(double value)
		{
			Console.WriteLine("Double");
			return true;
		}

		protected override bool StartMap()
		{
			Console.WriteLine("StartMap");
			return true;
		}

		protected override bool MapKey(IntPtr key, IntPtr length)
		{
			Console.WriteLine("MapKey");
			return true;
		}

		protected override bool EndMap()
		{
			Console.WriteLine("EndMap");
			return true;
		}

		protected override bool StartArray()
		{
			Console.WriteLine("StartArray");
			return true;
		}

		protected override bool EndArray()
		{
			Console.WriteLine("EndArray");
			return true;
		}
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
			var p = new TestParser();
			string text = "{\"a\":null, \"b\":\"NESAMONE\"}";
			p.Parse(text);
			p.Complete();
		}
	}
}

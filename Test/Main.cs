using System;
using YAJLSharp;

namespace Test
{
	public class TestParser : YAJLParser
	{
		protected override bool Null(IntPtr ctx)
		{
			Console.WriteLine("Null");
			return true;
		}

		protected override bool Number (IntPtr ctx, IntPtr numberVal, IntPtr numberLen)
		{
			Console.WriteLine("Number");
			return true;
		}

		protected override bool String(IntPtr ctx, IntPtr stringVal, IntPtr stringLen)
		{
			Console.WriteLine("String");
			return true;
		}

		protected override bool Integer(IntPtr ctx, long value)
		{
			Console.WriteLine("Intger");
			return true;
		}

		protected override bool Boolean(IntPtr ctx, bool value)
		{
			Console.WriteLine("Boolean");
			return true;
		}

		protected override bool Double(IntPtr ctx, double value)
		{
			Console.WriteLine("Double");
			return true;
		}

		protected override bool StartMap(IntPtr ctx)
		{
			Console.WriteLine("StartMap");
			return true;
		}

		protected override bool MapKey(IntPtr ctx, IntPtr key, IntPtr length)
		{
			Console.WriteLine("MapKey");
			return true;
		}

		protected override bool EndMap(IntPtr ctx)
		{
			Console.WriteLine("EndMap");
			return true;
		}

		protected override bool StartArray(IntPtr ctx)
		{
			Console.WriteLine("StartArray");
			return true;
		}

		protected override bool EndArray(IntPtr ctx)
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

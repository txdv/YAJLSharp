using System;
using System.Runtime.InteropServices;

namespace YAJLSharp
{
	public abstract class YAJLParser : NativeYAJLParser
	{
		protected override bool Number(IntPtr numberVal, IntPtr numberLen)
		{
			return Number(Marshal.PtrToStringAuto(numberVal, numberLen.ToInt32()));
		}

		protected virtual bool Number(string value)
		{
			return true;
		}

		protected override bool String(IntPtr stringVal, IntPtr stringLen)
		{
			return String(Marshal.PtrToStringAuto(stringVal, stringLen.ToInt32()));
		}

		protected virtual bool String(string value)
		{
			return true;
		}

		protected override bool MapKey(IntPtr key, IntPtr keyLen)
		{
			return MapKey(Marshal.PtrToStringAuto(key, keyLen.ToInt32()));
		}

		protected virtual bool MapKey(string key)
		{
			return true;
		}

	}
}

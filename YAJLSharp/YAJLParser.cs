using System;
using System.Text;
using System.Runtime.InteropServices;

namespace YAJLSharp
{
	unsafe public abstract class YAJLParser : NativeYAJLParser
	{
		protected override bool Number(IntPtr numberVal, IntPtr numberLen)
		{
			return Number(Marshal.PtrToStringAuto(numberVal, numberLen.ToInt32()));
		}

		protected virtual bool Number(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			fixed (byte *numberVal = bytes) {
				return Number((IntPtr)numberVal, (IntPtr)bytes.Length);
			}
		}

		protected override bool String(IntPtr stringVal, IntPtr stringLen)
		{
			return String(Marshal.PtrToStringAuto(stringVal, stringLen.ToInt32()));
		}

		protected virtual bool String(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			fixed (byte *stringVal = bytes) {
				return String((IntPtr)stringVal, (IntPtr)bytes.Length);
			}
		}

		protected override bool MapKey(IntPtr key, IntPtr keyLen)
		{
			return MapKey(Marshal.PtrToStringAuto(key, keyLen.ToInt32()));
		}

		protected virtual bool MapKey(string key)
		{
			var bytes = Encoding.UTF8.GetBytes(key);
			fixed (byte *keyVal = bytes) {
				return Number((IntPtr)keyVal, (IntPtr)bytes.Length);
			}
		}
	}
}

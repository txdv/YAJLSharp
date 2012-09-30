using System;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace YAJLSharp
{
	unsafe public abstract class YAJLParser : NativeYAJLParser
	{
		bool oNumber;
		bool oString;
		bool oMapKey;

		public YAJLParser()
			: base()
		{
			var methods = typeof(YAJLParser).GetMethods(BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly);
			oNumber = IsOverriden("Number", Get(methods, "Number"), typeof(YAJLParser));
			oString = IsOverriden("String", Get(methods, "String"), typeof(YAJLParser));
			oMapKey = IsOverriden("MapKey", Get(methods, "MapKey"), typeof(YAJLParser));
		}

		MethodInfo Get(MethodInfo[] methods, string name)
		{
			foreach (var method in methods) {
				if (method.Name == name && method.GetParameters()[0].ParameterType == typeof(string)) {
					return method;
				}
			}
			return null;
		}

		protected override bool Number(IntPtr numberVal, IntPtr numberLen)
		{
			if (oNumber) {
				return Number(Marshal.PtrToStringAuto(numberVal, numberLen.ToInt32()));
			} else {
				return base.Number(numberVal, numberLen);
			}
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
			if (oString) {
				return String(Marshal.PtrToStringAuto(stringVal, stringLen.ToInt32()));
			} else {
				return base.String(stringVal, stringLen);
			}
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
			if (oMapKey) {
				return MapKey(Marshal.PtrToStringAuto(key, keyLen.ToInt32()));
			} else {
				return base.MapKey(key, keyLen);
			}
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

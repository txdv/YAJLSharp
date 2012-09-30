using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace YAJLSharp
{
	enum yajl_option {
		yajl_allow_comments = 0x01,
		yajl_dont_validate_strings = 0x02,
		yajl_allow_trailing_garbage = 0x04,
		yajl_allow_multiple_values = 0x08,
		yajl_allow_partial_values = 0x10
	}

	[StructLayout(LayoutKind.Sequential)]
	struct yajl_callbacks {
		public IntPtr yajl_null;
		public IntPtr yajl_boolean;
		public IntPtr yajl_integer;
		public IntPtr yajl_double;

		public IntPtr yajl_number;
		public IntPtr yajl_string;

		public IntPtr yajl_start_map;
		public IntPtr yajl_map_key;
		public IntPtr yajl_end_map;

		public IntPtr yajl_start_array;
		public IntPtr yajl_end_array;
	};

	delegate int yajl_null_delegate(IntPtr ctx);
	delegate int yajl_boolean_delegate(IntPtr ctx, int boolVal);
	delegate int yajl_integer_delegate(IntPtr ctx, long integerVal);
	delegate int yajl_double_delegate(IntPtr ctx, double doubleVal);
	delegate int yajl_number_delegate(IntPtr ctx, IntPtr numberVal, IntPtr numLen);
	delegate int yajl_string_delegate(IntPtr ctx, IntPtr stringVal, IntPtr stringLen);

	delegate int yajl_start_map_delegate(IntPtr ctx);
	delegate int yajl_map_key_delegate(IntPtr ctx, IntPtr key, IntPtr keyLen);
	delegate int yajl_end_map_delegate(IntPtr ctx);

	delegate int yajl_start_array_delegate(IntPtr ctx);
	delegate int yajl_end_array_delegate(IntPtr ctx);

	[StructLayout(LayoutKind.Sequential)]
	struct yajl_sharpcallbacks {
		public yajl_null_delegate yajl_null;
		public yajl_boolean_delegate yajl_boolean;
		public yajl_integer_delegate yajl_integer;
		public yajl_double_delegate yajl_double;
		public yajl_number_delegate yajl_number;
		public yajl_string_delegate yajl_string;

		public yajl_start_map_delegate yajl_start_map;
		public yajl_map_key_delegate yajl_map_key;
		public yajl_end_map_delegate yajl_end_map;

		public yajl_start_array_delegate yajl_start_array;
		public yajl_end_array_delegate yajl_end_array;
	}

	abstract unsafe public class NativeYAJLParser : IDisposable
	{
		public IntPtr Handle { get; private set; }
		public IntPtr Context { get; protected set; }

		[DllImport("yajl")]
		static extern IntPtr yajl_alloc(IntPtr callbacks, IntPtr allocationFunctions, IntPtr ctx);

		yajl_callbacks callbacks;

		yajl_sharpcallbacks base_callbacks;
		yajl_sharpcallbacks this_callbacks;

		GCHandle gchandle;
		public NativeYAJLParser()
		{
			this_callbacks.yajl_null = yajl_null;
			this_callbacks.yajl_boolean = yajl_boolean;
			this_callbacks.yajl_integer = yajl_integer;
			this_callbacks.yajl_double = yajl_double;
			this_callbacks.yajl_number = yajl_number;
			this_callbacks.yajl_string = yajl_string;

			this_callbacks.yajl_start_map = yajl_start_map;
			this_callbacks.yajl_map_key = yajl_map_key;
			this_callbacks.yajl_end_map = yajl_end_map;

			this_callbacks.yajl_start_array = start_array;
			this_callbacks.yajl_end_array = end_array;

			Initialize();

			gchandle = GCHandle.Alloc(callbacks, GCHandleType.Pinned);
			Handle = yajl_alloc(gchandle.AddrOfPinnedObject(), IntPtr.Zero, IntPtr.Zero);
		}

		~NativeYAJLParser()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		[DllImport("yajl")]
		static extern void yajl_free(IntPtr handle);

		protected void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			if (gchandle.IsAllocated) {
				gchandle.Free();
			}

			if (Handle != IntPtr.Zero) {
				yajl_free(Handle);
				Handle = IntPtr.Zero;
			}
		}

		public bool Parse(string text)
		{
			return Parse(System.Text.Encoding.ASCII.GetBytes(text));
		}

		public bool Parse(byte[] array)
		{
			return Parse(array, 0, array.Length);
		}

		[DllImport("yajl")]
		static extern yajl_status yajl_parse(IntPtr handle, IntPtr jsonText, IntPtr jsonTextLength);

		public bool Parse(byte[] array, int start, int count)
		{
			GCHandle arraygchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			IntPtr addr = (IntPtr)(arraygchandle.AddrOfPinnedObject().ToInt64() + start);
			var code = yajl_parse(Handle, addr, (IntPtr)count);
			var ret = Ensure.Success(Handle, code);
			arraygchandle.Free();
			return ret;
		}

		[DllImport("yajl")]
		static extern yajl_status yajl_complete_parse(IntPtr handle);
		public bool Complete()
		{
			var code = yajl_complete_parse(Handle);
			return Ensure.Success(Handle, code);
		}

		[DllImport("yajl")]
		static extern IntPtr yajl_get_bytes_consumed(IntPtr ptr);

		public long BytesConsumed {
			get {
				return yajl_get_bytes_consumed(Handle).ToInt64();
			}
		}

		[DllImport("yajl")]
		static extern int yajl_config(IntPtr handle, yajl_option option, int value);

		void yajl_config(yajl_option option, bool value)
		{
			yajl_config(Handle, option, (value ? 1 : 0));
		}

		public bool AllowComments {
			set {
				yajl_config(yajl_option.yajl_allow_comments, value);
			}
		}

		public bool DontValidateStrings {
			set {
				yajl_config(yajl_option.yajl_dont_validate_strings, value);
			}
		}

		public bool AllowTrailingGarbage {
			set {
				yajl_config(yajl_option.yajl_allow_trailing_garbage, value);
			}
		}

		public bool AllowMultipleValues {
			set {
				yajl_config(yajl_option.yajl_allow_multiple_values, value);
			}
		}

		public bool AllowPartialValues {
			set {
				yajl_config(yajl_option.yajl_allow_partial_values, value);
			}
		}

		public virtual void Initialize()
		{
			if (callbacks.yajl_null    != IntPtr.Zero) base_callbacks.yajl_null    = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_null,    typeof(yajl_null_delegate   )) as yajl_null_delegate;
			if (callbacks.yajl_boolean != IntPtr.Zero) base_callbacks.yajl_boolean = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_boolean, typeof(yajl_boolean_delegate)) as yajl_boolean_delegate;
			if (callbacks.yajl_double  != IntPtr.Zero) base_callbacks.yajl_double  = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_double,  typeof(yajl_double_delegate))  as yajl_double_delegate;
			if (callbacks.yajl_number  != IntPtr.Zero) base_callbacks.yajl_number  = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_number,  typeof(yajl_number_delegate))  as yajl_number_delegate;
			if (callbacks.yajl_string  != IntPtr.Zero) base_callbacks.yajl_string  = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_string,  typeof(yajl_string_delegate))  as yajl_string_delegate;

			if (callbacks.yajl_start_map != IntPtr.Zero) base_callbacks.yajl_start_map = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_string, typeof(yajl_start_map_delegate)) as yajl_start_map_delegate;
			if (callbacks.yajl_map_key   != IntPtr.Zero) base_callbacks.yajl_map_key   = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_string, typeof(yajl_map_key_delegate))   as yajl_map_key_delegate;
			if (callbacks.yajl_end_map   != IntPtr.Zero) base_callbacks.yajl_end_map   = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_string, typeof(yajl_end_map_delegate))   as yajl_end_map_delegate;

			if (callbacks.yajl_start_array != IntPtr.Zero) base_callbacks.yajl_start_array = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_string, typeof(yajl_start_array_delegate)) as yajl_start_array_delegate;
			if (callbacks.yajl_end_array   != IntPtr.Zero) base_callbacks.yajl_end_array   = Marshal.GetDelegateForFunctionPointer(callbacks.yajl_string, typeof(yajl_end_array_delegate))   as yajl_end_array_delegate;

			if (IsOverriden("Null"))    callbacks.yajl_null    = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_null);
			if (IsOverriden("Boolean")) callbacks.yajl_boolean = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_boolean);
			if (IsOverriden("Double"))  callbacks.yajl_double  = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_double);
			if (IsOverriden("Number"))  callbacks.yajl_number  = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_number);
			if (IsOverriden("String"))  callbacks.yajl_string  = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_string);

			if (IsOverriden("StartMap")) callbacks.yajl_start_map = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_start_map);
			if (IsOverriden("MapKey"))   callbacks.yajl_map_key   = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_map_key);
			if (IsOverriden("EndMap"))   callbacks.yajl_end_map   = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_end_map);

			if (IsOverriden("StartArray")) callbacks.yajl_start_array = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_start_array);
			if (IsOverriden("EndArray"))   callbacks.yajl_end_array   = Marshal.GetFunctionPointerForDelegate(this_callbacks.yajl_end_array);
		}

		bool IsOverriden(string name)
		{
			for (Type type = this.GetType(); type != typeof(NativeYAJLParser); type = type.BaseType) {
				if (IsOverriden(type, name)) {
					return true;
				}
			}
			return false;
		}

		bool IsOverriden(Type type, string name)
		{
			return type.GetMethod(name, BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly) != null;
		}

		protected virtual bool Null()
		{
			if (base_callbacks.yajl_null != null) {
				return base_callbacks.yajl_null(Context) == 0 ? false : true;;
			}
			return true;
		}
		int yajl_null(IntPtr ctx)
		{
			Context = ctx;
			return Null() ? 1 : 0;
		}

		protected virtual bool Boolean(bool value)
		{
			if (base_callbacks.yajl_boolean != null) {
				return base_callbacks.yajl_boolean(Context, (value ? 1 : 0)) == 0 ? false : true;;
			}
			return true;
		}
		int yajl_boolean(IntPtr ctx, int boolVal)
		{
			Context = ctx;
			return Boolean((boolVal == 0 ? false : true)) ? 1 : 0;
		}

		protected virtual bool Integer(long value)
		{
			if (base_callbacks.yajl_integer != null) {
				return base_callbacks.yajl_integer(Context, value) == 0 ? false : true;
			}
			return true;
		}
		int yajl_integer(IntPtr ctx, long integerVal)
		{
			Context = ctx;
			return Integer(integerVal) ? 1 : 0;
		}

		protected virtual bool Double(double value)
		{
			if (base_callbacks.yajl_double != null) {
				return base_callbacks.yajl_double(Context, value) == 0 ? false : true;;
			}
			return true;
		}
		int yajl_double(IntPtr ctx, double doubleVal)
		{
			Context = ctx;
			return Double(doubleVal) ? 1 : 0;
		}

		protected virtual bool Number(IntPtr numberVal, IntPtr numberLen)
		{
			if (base_callbacks.yajl_number != null) {
				return base_callbacks.yajl_number(Context, numberVal, numberLen) == 0 ? false : true;
			}
			return true;
		}
		int yajl_number(IntPtr ctx, IntPtr numberVal, IntPtr numberLen)
		{
			Context = ctx;
			return Number(numberVal, numberLen) ? 1 : 0;
		}

		protected virtual bool String(IntPtr stringVal, IntPtr stringLen)
		{
			if (base_callbacks.yajl_string != null) {
				return base_callbacks.yajl_string(Context, stringVal, stringLen) == 0 ? false : true;
			}
			return true;
		}
		int yajl_string(IntPtr ctx, IntPtr stringVal, IntPtr stringLen)
		{
			Context = ctx;
			return String(stringVal, stringLen) ? 1 : 0;
		}

		protected virtual bool StartMap()
		{
			if (base_callbacks.yajl_start_map != null) {
				return base_callbacks.yajl_start_map(Context) == 0 ? false : true;
			}
			return true;
		}
		int yajl_start_map(IntPtr ctx)
		{
			Context = ctx;
			return StartMap() ? 1 : 0;
		}

		protected virtual bool MapKey(IntPtr key, IntPtr keyLen)
		{
			if (base_callbacks.yajl_map_key != null) {
				return base_callbacks.yajl_map_key(Context, key, keyLen) == 0 ? false : true;
			}
			return true;
		}
		int yajl_map_key(IntPtr ctx, IntPtr key, IntPtr keyLen)
		{
			Context = ctx;
			return MapKey(key, keyLen) ? 1 : 0;
		}

		protected virtual bool EndMap()
		{
			if (base_callbacks.yajl_end_map != null) {
				return base_callbacks.yajl_end_map(Context) == 0 ? false : true;
			}
			return true;
		}
		int yajl_end_map(IntPtr ctx)
		{
			Context = ctx;
			return EndMap() ? 1 : 0;
		}

		protected virtual bool StartArray()
		{
			if (base_callbacks.yajl_start_array != null) {
				return base_callbacks.yajl_start_array(Context) == 0 ? false : true;
			}
			return true;
		}
		int start_array(IntPtr ctx)
		{
			Context = ctx;
			return StartArray() ? 1 : 0;
		}

		protected virtual bool EndArray()
		{
			if (base_callbacks.yajl_end_array != null) {
				return base_callbacks.yajl_end_array(Context) == 0 ? false : true;
			}
			return true;
		}
		int end_array(IntPtr ctx)
		{
			Context = ctx;
			return EndArray() ? 1 : 0;
		}
	}
}


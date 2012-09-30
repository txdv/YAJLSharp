using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace YAJLSharp
{
	/*
	[StructLayout(LayoutKind.Sequential)]
	unsafe struct yajl_handle {
		public IntPtr callbacks;
		public IntPtr context;
		public yajl_lexer lexer;
		public sbyte *parseError;
		public IntPtr bytesConsumed;
		public yajl_buf decodeBuf;
		public yajl_bytestack stateStack;
		public yajl_alloc_funcs alloc;
		public uint flags;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct yajl_lexer {
		public IntPtr sizeOff;
		public IntPtr charOff;
		public int error;
		public yajl_buf buf;
		public IntPtr bufOff;
		public uint bufInUse;
		public uint allowComments;
		public uint validateUTF8;

		public yajl_alloc_funcs *alloc;
		// todo
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct yajl_buf {
		public IntPtr len;
		public IntPtr used;
		public IntPtr data;
		public yajl_alloc_funcs *alloc;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct yajl_bytestack {
		public byte *stack;
		public IntPtr size;
		public IntPtr used;
		public yajl_alloc_funcs *yaf;
		// todo
	}

	[StructLayout(LayoutKind.Sequential)]
	struct yajl_alloc_funcs {
		public IntPtr malloc; // void *, size_t
		public IntPtr realloc; // void *, void *, size_t
		public IntPtr free; // void *, void *
		public IntPtr ctx;
	}
*/
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

	abstract unsafe public class YAJLParser
	{
		public IntPtr Handle { get; protected set; }

		[DllImport("yajl")]
		static extern IntPtr yajl_alloc(IntPtr callbacks, IntPtr allocationFunctions, IntPtr ctx);

		yajl_callbacks callbacks;

		yajl_sharpcallbacks base_callbacks;
		yajl_sharpcallbacks this_callbacks;

		public YAJLParser()
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

			var gchandle = GCHandle.Alloc(callbacks, GCHandleType.Pinned);
			Handle = yajl_alloc(gchandle.AddrOfPinnedObject(), IntPtr.Zero, IntPtr.Zero);
		}

		public void Parse(string text)
		{
			Parse(System.Text.Encoding.ASCII.GetBytes(text));
		}

		public void Parse(byte[] array)
		{
			Parse(array, 0, array.Length);
		}

		[DllImport("yajl")]
		static extern int yajl_parse(IntPtr handle, IntPtr jsonText, IntPtr jsonTextLength);

		public void Parse(byte[] array, int start, int count)
		{

			GCHandle arraygchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			IntPtr addr = (IntPtr)(arraygchandle.AddrOfPinnedObject().ToInt64() + start);
			yajl_parse(Handle, addr, (IntPtr)count);
			arraygchandle.Free();
		}

		[DllImport("yajl")]
		static extern int yajl_complete_parse(IntPtr handle);
		public void Complete()
		{
			yajl_complete_parse(Handle);
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
			for (Type type = this.GetType(); type != typeof(YAJLParser); type = type.BaseType) {
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

		protected virtual bool Null(IntPtr ctx)
		{
			if (base_callbacks.yajl_null != null) {
				return base_callbacks.yajl_null(ctx) == 0 ? false : true;;
			}
			return true;
		}
		int yajl_null(IntPtr ctx)
		{
			return Null(ctx) ? 1 : 0;
		}

		protected virtual bool Boolean(IntPtr ctx, bool value)
		{
			if (base_callbacks.yajl_boolean != null) {
				return base_callbacks.yajl_boolean(ctx, (value ? 1 : 0)) == 0 ? false : true;;
			}
			return true;
		}
		int yajl_boolean(IntPtr ctx, int boolVal)
		{
			return Boolean(ctx, (boolVal == 0 ? false : true)) ? 1 : 0;
		}

		protected virtual bool Integer(IntPtr ctx, long value)
		{
			if (base_callbacks.yajl_integer != null) {
				return base_callbacks.yajl_integer(ctx, value) == 0 ? false : true;
			}
			return true;
		}
		int yajl_integer(IntPtr ctx, long integerVal)
		{
			return Integer(ctx, integerVal) ? 1 : 0;
		}

		protected virtual bool Double(IntPtr ctx, double value)
		{
			if (base_callbacks.yajl_double != null) {
				return base_callbacks.yajl_double(ctx, value) == 0 ? false : true;;
			}
			return true;
		}
		int yajl_double(IntPtr ctx, double doubleVal)
		{
			return Double(ctx, doubleVal) ? 1 : 0;
		}

		protected virtual bool Number(IntPtr ctx, IntPtr numberVal, IntPtr numberLen)
		{
			if (base_callbacks.yajl_number != null) {
				return base_callbacks.yajl_number(ctx, numberVal, numberLen) == 0 ? false : true;
			}
			return true;
		}
		int yajl_number(IntPtr ctx, IntPtr numberVal, IntPtr numberLen)
		{
			return Number(ctx, numberVal, numberLen) ? 1 : 0;
		}

		protected virtual bool String(IntPtr ctx, IntPtr stringVal, IntPtr stringLen)
		{
			if (base_callbacks.yajl_string != null) {
				return base_callbacks.yajl_string(ctx, stringVal, stringLen) == 0 ? false : true;
			}
			return true;
		}
		int yajl_string(IntPtr ctx, IntPtr stringVal, IntPtr stringLen)
		{
			return String(ctx, stringVal, stringLen) ? 1 : 0;
		}

		protected virtual bool StartMap(IntPtr ctx)
		{
			if (base_callbacks.yajl_start_map != null) {
				return base_callbacks.yajl_start_map(ctx) == 0 ? false : true;
			}
			return true;
		}
		int yajl_start_map(IntPtr ctx)
		{
			return StartMap(ctx) ? 1 : 0;
		}

		protected virtual bool MapKey(IntPtr ctx, IntPtr key, IntPtr length)
		{
			if (base_callbacks.yajl_map_key != null) {
				return base_callbacks.yajl_map_key(ctx, key, length) == 0 ? false : true;
			}
			return true;
		}
		int yajl_map_key(IntPtr ctx, IntPtr key, IntPtr keyLen)
		{
			return MapKey(ctx, key, keyLen) ? 1 : 0;
		}

		protected virtual bool EndMap(IntPtr ctx)
		{
			if (base_callbacks.yajl_end_map != null) {
				return base_callbacks.yajl_end_map(ctx) == 0 ? false : true;
			}
			return true;
		}
		int yajl_end_map(IntPtr ctx)
		{
			return EndMap(ctx) ? 1 : 0;
		}

		protected virtual bool StartArray(IntPtr ctx)
		{
			if (base_callbacks.yajl_start_array != null) {
				return base_callbacks.yajl_start_array(ctx) == 0 ? false : true;
			}
			return true;
		}
		int start_array(IntPtr ctx)
		{
			return StartArray(ctx) ? 1 : 0;
		}

		protected virtual bool EndArray(IntPtr ctx)
		{
			if (base_callbacks.yajl_end_array != null) {
				return base_callbacks.yajl_end_array(ctx) == 0 ? false : true;
			}
			return true;
		}
		int end_array(IntPtr ctx)
		{
			return EndArray(ctx) ? 1 : 0;
		}
	}
}


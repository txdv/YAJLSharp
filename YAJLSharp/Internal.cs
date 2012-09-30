using System;
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
	}
*/
}


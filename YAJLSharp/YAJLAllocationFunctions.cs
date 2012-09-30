using System;
using System.Runtime.InteropServices;

namespace YAJLSharp
{
	[StructLayout(LayoutKind.Sequential)]
	struct yajl_alloc_funcs {
		public IntPtr malloc; // void *, size_t
		public IntPtr realloc; // void *, void *, size_t
		public IntPtr free; // void *, void *
		public IntPtr ctx;
	}

	delegate IntPtr yajl_malloc_func_delegate(IntPtr ctx, IntPtr size);
	delegate IntPtr yajl_realloc_func_delegate(IntPtr ctx, IntPtr ptr, IntPtr size);
	delegate void yajl_free_func_delegate(IntPtr ctx, IntPtr ptr);

	[StructLayout(LayoutKind.Sequential)]
	struct yajlsharp_alloc_funcs
	{
		public yajl_malloc_func_delegate malloc;
		public yajl_realloc_func_delegate realloc;
		public yajl_free_func_delegate free;
	}

	public abstract class YAJLAllocationFunctions : IDisposable
	{
		yajl_alloc_funcs alloc_funcs;
		yajlsharp_alloc_funcs sharp_alloc_funcs;

		public IntPtr Context {
			get {
				return alloc_funcs.ctx;
			}
		}

		public IntPtr Handle { get; protected set; }

		GCHandle gchandle;

		public YAJLAllocationFunctions()
		{
			sharp_alloc_funcs.malloc = Malloc;
			sharp_alloc_funcs.realloc = Realloc;
			sharp_alloc_funcs.free = Free;

			alloc_funcs.malloc  = Marshal.GetFunctionPointerForDelegate(sharp_alloc_funcs.malloc);
			alloc_funcs.realloc = Marshal.GetFunctionPointerForDelegate(sharp_alloc_funcs.realloc);
			alloc_funcs.free    = Marshal.GetFunctionPointerForDelegate(sharp_alloc_funcs.free);

			gchandle = GCHandle.Alloc(alloc_funcs, GCHandleType.Pinned);
			Handle = gchandle.AddrOfPinnedObject();
		}

		~YAJLAllocationFunctions()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			if (gchandle.IsAllocated) {
				gchandle.Free();
			}
		}

		public virtual IntPtr Malloc(IntPtr context, IntPtr size)
		{
			return IntPtr.Zero;
		}

		public virtual IntPtr Realloc(IntPtr context, IntPtr pointer, IntPtr size)
		{
			return IntPtr.Zero;
		}

		public virtual void Free(IntPtr context, IntPtr pointer)
		{
		}
	}
}


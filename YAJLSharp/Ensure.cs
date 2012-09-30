using System;
using System.Runtime.InteropServices;

namespace YAJLSharp
{
	enum yajl_status : int {
		yajl_status_ok,
		yajl_status_client_canceled,
		yajl_status_error
	}

	unsafe static class Ensure
	{
		[DllImport("yajl")]
		static extern sbyte *yajl_status_to_string(yajl_status code);

		static string GetString(yajl_status code)
		{
			return new string(yajl_status_to_string(code));
		}

		[DllImport("yajl")]
		static extern IntPtr yajl_get_error(IntPtr handle, int verbose, IntPtr jsonText, IntPtr jsonTextLength);

		[DllImport("yajl")]
		static extern void yajl_free_error(IntPtr handle, IntPtr str);

		public static bool Success(IntPtr handle, yajl_status code)
		{
			switch (code) {
			case yajl_status.yajl_status_ok:
				return true;
			case yajl_status.yajl_status_client_canceled:
				return false;
			default:
				IntPtr str = yajl_get_error(handle, 1, IntPtr.Zero, IntPtr.Zero);
				var exp = new Exception(Marshal.PtrToStringAuto(str));
				yajl_free_error(handle, str);
				throw exp;
			}
		}
	}
}


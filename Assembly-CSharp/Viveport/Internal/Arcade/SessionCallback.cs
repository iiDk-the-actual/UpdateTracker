using System;
using System.Runtime.InteropServices;

namespace Viveport.Internal.Arcade
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SessionCallback(int code, [MarshalAs(UnmanagedType.LPStr)] string message);
}

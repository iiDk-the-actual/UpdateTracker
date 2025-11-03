using System;
using System.Runtime.InteropServices;

namespace Viveport.Internal
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetLicenseCallback([MarshalAs(UnmanagedType.LPStr)] string message, [MarshalAs(UnmanagedType.LPStr)] string signature);
}

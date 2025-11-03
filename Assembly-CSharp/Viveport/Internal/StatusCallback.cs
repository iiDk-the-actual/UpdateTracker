using System;
using System.Runtime.InteropServices;

namespace Viveport.Internal
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void StatusCallback(int nResult);
}

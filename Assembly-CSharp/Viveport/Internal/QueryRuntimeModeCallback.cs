using System;
using System.Runtime.InteropServices;

namespace Viveport.Internal
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void QueryRuntimeModeCallback(int nResult, int nMode);
}

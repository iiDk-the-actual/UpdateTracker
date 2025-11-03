using System;
using System.Runtime.InteropServices;

namespace Viveport.Internal
{
	internal struct IAPCurrency_t
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		internal string m_pName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
		internal string m_pSymbol;
	}
}

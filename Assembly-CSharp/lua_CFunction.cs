using System;
using System.Runtime.InteropServices;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int lua_CFunction(lua_State* L);

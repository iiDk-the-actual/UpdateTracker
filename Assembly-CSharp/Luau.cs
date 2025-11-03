using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class Luau
{
	[DllImport("luau")]
	public unsafe static extern lua_State* luaL_newstate();

	[DllImport("luau")]
	public unsafe static extern void luaL_openlibs(lua_State* L);

	[DllImport("luau")]
	public unsafe static extern sbyte* luau_compile([MarshalAs(UnmanagedType.LPStr)] string source, [NativeInteger] UIntPtr size, lua_CompileOptions* options, [NativeInteger] UIntPtr* outsize);

	[DllImport("luau")]
	public unsafe static extern int luau_load(lua_State* L, [MarshalAs(UnmanagedType.LPStr)] string chunkname, sbyte* data, [NativeInteger] UIntPtr size, int env);

	[DllImport("luau")]
	public unsafe static extern void lua_pushvalue(lua_State* L, int idx);

	[DllImport("luau")]
	public unsafe static extern void lua_pushcclosurek(lua_State* L, lua_CFunction fn, [MarshalAs(UnmanagedType.LPStr)] string debugname, int nup, lua_Continuation cont);

	[DllImport("luau")]
	public unsafe static extern void lua_pushcclosurek(lua_State* L, FunctionPointer<lua_CFunction> fn, [MarshalAs(UnmanagedType.LPStr)] string debugname, int nup, lua_Continuation cont);

	[DllImport("luau")]
	public unsafe static extern void lua_pushcclosurek(lua_State* L, FunctionPointer<lua_CFunction> fn, byte* debugname, int nup, int* cont);

	public unsafe static void lua_pushcfunction(lua_State* L, FunctionPointer<lua_CFunction> fn, [MarshalAs(UnmanagedType.LPStr)] string debugname)
	{
		Luau.lua_pushcclosurek(L, fn, debugname, 0, null);
	}

	public unsafe static void lua_pushcfunction(lua_State* L, lua_CFunction fn, [MarshalAs(UnmanagedType.LPStr)] string debugname)
	{
		Luau.lua_pushcclosurek(L, fn, debugname, 0, null);
	}

	[DllImport("luau")]
	public unsafe static extern void lua_settop(lua_State* L, int idx);

	[DllImport("luau")]
	public unsafe static extern int lua_gettop(lua_State* L);

	[DllImport("luau")]
	public unsafe static extern sbyte* lua_tolstring(lua_State* L, int idx, int* len);

	[DllImport("luau")]
	public unsafe static extern int lua_resume(lua_State* L, lua_State* from, int nargs);

	[DllImport("luau")]
	public unsafe static extern void lua_setfield(lua_State* L, int index, [MarshalAs(UnmanagedType.LPStr)] string k);

	[DllImport("luau")]
	public unsafe static extern void lua_setfield(lua_State* L, int index, byte* k);

	public unsafe static void lua_setglobal(lua_State* L, string s)
	{
		Luau.lua_setfield(L, -10002, s);
	}

	public unsafe static void lua_register(lua_State* L, lua_CFunction f, string n)
	{
		lua_Continuation lua_Continuation = null;
		Luau.lua_pushcclosurek(L, f, n, 0, lua_Continuation);
		Luau.lua_setglobal(L, n);
	}

	public unsafe static void lua_pop(lua_State* L, int n)
	{
		Luau.lua_settop(L, -n - 1);
	}

	public unsafe static sbyte* lua_tostring(lua_State* L, int idx)
	{
		return Luau.lua_tolstring(L, idx, null);
	}

	[DllImport("luau")]
	public unsafe static extern int lua_isstring(lua_State* L, int index);

	[DllImport("luau")]
	public unsafe static extern int lua_type(lua_State* L, int index);

	[DllImport("luau")]
	public unsafe static extern int lua_pushstring(lua_State* L, [MarshalAs(UnmanagedType.LPStr)] string s);

	[DllImport("luau")]
	public unsafe static extern int lua_pushstring(lua_State* L, byte* s);

	[DllImport("luau")]
	public unsafe static extern int lua_error(lua_State* L);

	[DllImport("luau")]
	public unsafe static extern void luaL_errorL(lua_State* L, [MarshalAs(UnmanagedType.LPStr)] string fmt, [MarshalAs(UnmanagedType.LPStr)] params string[] a);

	[DllImport("luau")]
	public unsafe static extern void luaL_errorL(lua_State* L, sbyte* fmt);

	[DllImport("luau")]
	public unsafe static extern int lua_toboolean(lua_State* L, int index);

	[DllImport("luau")]
	public unsafe static extern byte* lua_debugtrace(lua_State* L);

	[DllImport("luau")]
	public unsafe static extern void lua_close(lua_State* L);

	[DllImport("luau")]
	public unsafe static extern int lua_ref(lua_State* L, int idx);

	[DllImport("luau")]
	public unsafe static extern void lua_unref(lua_State* L, int rid);

	public unsafe static void lua_getref(lua_State* L, int rid)
	{
		Luau.lua_rawgeti(L, -10000, rid);
	}

	[DllImport("luau")]
	public unsafe static extern void* lua_touserdatatagged(lua_State* L, int idx, int tag);

	[DllImport("luau")]
	public unsafe static extern void* lua_touserdata(lua_State* L, int index);

	[DllImport("luau")]
	public unsafe static extern void* lua_newuserdatatagged(lua_State* L, int sz, int tag);

	[DllImport("luau")]
	public unsafe static extern void lua_getuserdatametatable(lua_State* L, int tag);

	[DllImport("luau")]
	public unsafe static extern void lua_setuserdatametatable(lua_State* L, int tag, int idx);

	[DllImport("luau")]
	public unsafe static extern int lua_setmetatable(lua_State* L, int objindex);

	[DllImport("luau")]
	public unsafe static extern int luaL_newmetatable(lua_State* L, [MarshalAs(UnmanagedType.LPStr)] string tname);

	[DllImport("luau")]
	public unsafe static extern int lua_getfield(lua_State* L, int idx, [MarshalAs(UnmanagedType.LPStr)] string k);

	[DllImport("luau")]
	public unsafe static extern int lua_getfield(lua_State* L, int idx, byte* k);

	[DllImport("luau")]
	public unsafe static extern int luaL_getmetafield(lua_State* L, int idx, byte* k);

	[DllImport("luau")]
	public unsafe static extern int luaL_getmetafield(lua_State* L, int idx, [MarshalAs(UnmanagedType.LPStr)] string k);

	public unsafe static void luaL_getmetatable(lua_State* L, string n)
	{
		Luau.lua_getfield(L, -10000, n);
	}

	public unsafe static void luaL_getmetatable(lua_State* L, byte* n)
	{
		Luau.lua_getfield(L, -10000, n);
	}

	public unsafe static void lua_getglobal(lua_State* L, string n)
	{
		Luau.lua_getfield(L, -10002, n);
	}

	[DllImport("luau")]
	public unsafe static extern int lua_getmetatable(lua_State* L, int objindex);

	[DllImport("luau")]
	public unsafe static extern byte* lua_namecallatom(lua_State* L, int* atom);

	[DllImport("luau")]
	public unsafe static extern byte* luaL_checklstring(lua_State* L, int numArg, int* l);

	public unsafe static byte* luaL_checkstring(lua_State* L, int n)
	{
		return Luau.luaL_checklstring(L, n, null);
	}

	[DllImport("luau")]
	public unsafe static extern void lua_pushnumber(lua_State* L, double n);

	[DllImport("luau")]
	public unsafe static extern double luaL_checknumber(lua_State* L, int numArg);

	[DllImport("luau")]
	public unsafe static extern void lua_setreadonly(lua_State* L, int idx, int enabled);

	[DllImport("luau")]
	public unsafe static extern double lua_tonumberx(lua_State* L, int index, int* isnum);

	[DllImport("luau")]
	public unsafe static extern int lua_gc(lua_State* L, int what, int data);

	[DllImport("luau")]
	public unsafe static extern void lua_call(lua_State* L, int nargs, int nresults);

	[DllImport("luau")]
	public unsafe static extern int lua_pcall(lua_State* L, int nargs, int nresults, int fn);

	[DllImport("luau")]
	public unsafe static extern int lua_status(lua_State* L);

	[DllImport("luau")]
	public unsafe static extern void* luaL_checkudata(lua_State* L, int arg, [MarshalAs(UnmanagedType.LPStr)] string tname);

	[DllImport("luau")]
	public unsafe static extern void* luaL_checkudata(lua_State* L, int arg, byte* tname);

	[DllImport("luau")]
	public unsafe static extern int lua_objlen(lua_State* L, int index);

	[DllImport("luau")]
	public unsafe static extern double luaL_optnumber(lua_State* L, int narg, double d);

	[DllImport("luau")]
	public unsafe static extern void lua_createtable(lua_State* L, int narr, int nrec);

	[DllImport("luau")]
	public unsafe static extern void lua_pushlightuserdatatagged(lua_State* L, void* p, int tag);

	[DllImport("luau")]
	public unsafe static extern void lua_pushnil(lua_State* L);

	[DllImport("luau")]
	public unsafe static extern int lua_next(lua_State* L, int index);

	[DllImport("luau")]
	public unsafe static extern void lua_rawseti(lua_State* L, int idx, int n);

	[DllImport("luau")]
	public unsafe static extern void lua_rawgeti(lua_State* L, int index, int n);

	[DllImport("luau")]
	public unsafe static extern void lua_rawget(lua_State* L, int index);

	[DllImport("luau")]
	public unsafe static extern void lua_rawset(lua_State* L, int index);

	[DllImport("luau")]
	public unsafe static extern void lua_remove(lua_State* L, int index);

	[DllImport("luau")]
	public unsafe static extern void lua_pushboolean(lua_State* L, int b);

	[DllImport("luau")]
	public unsafe static extern int lua_rawequal(lua_State* L, int a, int b);

	public unsafe static void* lua_newuserdata(lua_State* L, int size)
	{
		return Luau.lua_newuserdatatagged(L, size, 0);
	}

	public unsafe static double lua_tonumber(lua_State* L, int index)
	{
		return Luau.lua_tonumberx(L, index, null);
	}

	public unsafe static T* lua_class_push<[IsUnmanaged] T>(lua_State* L) where T : struct, ValueType
	{
		T* ptr = (T*)Luau.lua_newuserdata(L, sizeof(T) + 4);
		FixedString32Bytes name = BurstClassInfo.ClassList.MetatableNames<T>.Name;
		Luau.luaL_getmetatable(L, (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref name) + 2);
		Luau.lua_setmetatable(L, -2);
		return ptr;
	}

	public unsafe static T* lua_class_push<[IsUnmanaged] T>(lua_State* L, FixedString32Bytes name) where T : struct, ValueType
	{
		T* ptr = (T*)Luau.lua_newuserdata(L, sizeof(T) + 4);
		Luau.luaL_getmetatable(L, (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref name) + 2);
		Luau.lua_setmetatable(L, -2);
		return ptr;
	}

	public unsafe static void lua_class_push(lua_State* L, FixedString32Bytes name, IntPtr ptr)
	{
		FixedString32Bytes fixedString32Bytes = "__ptr";
		Luau.lua_createtable(L, 0, 0);
		Luau.lua_pushlightuserdatatagged(L, (void*)ptr, 0);
		Luau.lua_setfield(L, -2, (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes) + 2);
		Luau.luaL_getmetatable(L, (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref name) + 2);
		Luau.lua_setmetatable(L, -2);
	}

	public unsafe static T* lua_class_get<[IsUnmanaged] T>(lua_State* L, int idx) where T : struct, ValueType
	{
		int num = Luau.lua_type(L, idx);
		FixedString32Bytes name = BurstClassInfo.ClassList.MetatableNames<T>.Name;
		byte* ptr = (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref name) + 2;
		if (num == 8)
		{
			T* ptr2 = (T*)Luau.luaL_checkudata(L, idx, ptr);
			if (ptr2 != null)
			{
				return ptr2;
			}
		}
		if (num == 6)
		{
			Luau.lua_getmetatable(L, idx);
			Luau.luaL_getmetatable(L, ptr);
			bool flag = Luau.lua_rawequal(L, -1, -2) == 1;
			Luau.lua_pop(L, 2);
			if (flag)
			{
				Luau.lua_getfield(L, idx, "__ptr");
				if (Luau.lua_type(L, -1) == 2)
				{
					T* ptr3 = (T*)Luau.lua_touserdata(L, -1);
					Luau.lua_pop(L, 1);
					if (ptr3 != null)
					{
						return ptr3;
					}
				}
				Luau.lua_pop(L, 1);
			}
		}
		FixedString32Bytes fixedString32Bytes = "\"Invalid Type\"";
		Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes) + 2));
		return null;
	}

	public unsafe static T* lua_class_get<[IsUnmanaged] T>(lua_State* L, int idx, FixedString32Bytes name) where T : struct, ValueType
	{
		int num = Luau.lua_type(L, idx);
		byte* ptr = (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref name) + 2;
		if (num == 8)
		{
			T* ptr2 = (T*)Luau.luaL_checkudata(L, idx, ptr);
			if (ptr2 != null)
			{
				return ptr2;
			}
		}
		if (num == 6)
		{
			Luau.lua_getmetatable(L, idx);
			Luau.luaL_getmetatable(L, ptr);
			bool flag = Luau.lua_rawequal(L, -1, -2) == 1;
			Luau.lua_pop(L, 1);
			if (flag)
			{
				FixedString32Bytes fixedString32Bytes = "__ptr";
				Luau.lua_getfield(L, idx, (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes) + 2);
				if (Luau.lua_type(L, -1) == 2)
				{
					T* ptr3 = (T*)Luau.lua_touserdata(L, -1);
					Luau.lua_pop(L, 1);
					if (ptr3 != null)
					{
						return ptr3;
					}
				}
				Luau.lua_pop(L, 1);
			}
		}
		FixedString32Bytes fixedString32Bytes2 = "\"Invalid Type\"";
		Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes2) + 2));
		return null;
	}

	public unsafe static byte* lua_class_get(lua_State* L, int idx, FixedString32Bytes name)
	{
		int num = Luau.lua_type(L, idx);
		byte* ptr = (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref name) + 2;
		if (num == 8)
		{
			byte* ptr2 = (byte*)Luau.luaL_checkudata(L, idx, ptr);
			if (ptr2 != null)
			{
				return ptr2;
			}
		}
		if (num == 6)
		{
			Luau.lua_getmetatable(L, idx);
			Luau.luaL_getmetatable(L, ptr);
			bool flag = Luau.lua_rawequal(L, -1, -2) == 1;
			Luau.lua_pop(L, 1);
			if (flag)
			{
				FixedString32Bytes fixedString32Bytes = "__ptr";
				Luau.lua_getfield(L, idx, (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes) + 2);
				if (Luau.lua_type(L, -1) == 2)
				{
					byte* ptr3 = (byte*)Luau.lua_touserdata(L, -1);
					Luau.lua_pop(L, 1);
					if (ptr3 != null)
					{
						return ptr3;
					}
				}
				Luau.lua_pop(L, 1);
			}
		}
		FixedString32Bytes fixedString32Bytes2 = "\"Invalid Type\"";
		Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes2) + 2));
		return null;
	}

	public unsafe static IntPtr lua_light_ptr(lua_State* L, int idx)
	{
		FixedString32Bytes fixedString32Bytes = "__ptr";
		Luau.lua_getfield(L, idx, (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes) + 2);
		if (Luau.lua_type(L, -1) == 2)
		{
			void* ptr = Luau.lua_touserdata(L, -1);
			Luau.lua_pop(L, 1);
			if (ptr != null)
			{
				return (IntPtr)ptr;
			}
		}
		return IntPtr.Zero;
	}

	public unsafe static bool lua_class_check<[IsUnmanaged] T>(lua_State* L, int idx) where T : struct, ValueType
	{
		return Luau.lua_objlen(L, idx) == sizeof(T);
	}

	[MonoPInvokeCallback(typeof(lua_CFunction))]
	public unsafe static int lua_print(lua_State* L)
	{
		string text = "";
		int num = Luau.lua_gettop(L);
		for (int i = 1; i <= num; i++)
		{
			int num2 = Luau.lua_type(L, i);
			if (num2 == 5 || num2 == 3)
			{
				sbyte* ptr = Luau.lua_tostring(L, i);
				text += Marshal.PtrToStringAnsi((IntPtr)((void*)ptr));
			}
			else
			{
				if (num2 != 1)
				{
					Luau.luaL_errorL(L, "Invalid String", Array.Empty<string>());
					return 0;
				}
				int num3 = Luau.lua_toboolean(L, i);
				text += ((num3 == 1) ? "true" : "false");
			}
		}
		LuauHud.Instance.LuauLog(text);
		return 0;
	}

	public const int LUA_GLOBALSINDEX = -10002;

	public const int LUA_REGISTRYINDEX = -10000;

	public enum lua_Types
	{
		LUA_TNIL,
		LUA_TBOOLEAN,
		LUA_TLIGHTUSERDATA,
		LUA_TNUMBER,
		LUA_TVECTOR,
		LUA_TSTRING,
		LUA_TTABLE,
		LUA_TFUNCTION,
		LUA_TUSERDATA,
		LUA_TTHREAD,
		LUA_TBUFFER,
		LUA_TPROTO,
		LUA_TUPVAL,
		LUA_TDEADKEY,
		LUA_T_COUNT = 11
	}

	public enum lua_Status
	{
		LUA_OK,
		LUA_YIELD,
		LUA_ERRRUN,
		LUA_ERRSYNTAX,
		LUA_ERRMEM,
		LUA_ERRERR,
		LUA_BREAK
	}

	public enum gc_status
	{
		LUA_GCSTOP,
		LUA_GCRESTART,
		LUA_GCCOLLECT,
		LUA_GCCOUNT,
		LUA_GCISRUNNING,
		LUA_GCSTEP,
		LUA_GCSETGOAL,
		LUA_GCSETSTEPMUL,
		LUA_GCSETSTEPSIZE
	}

	public static class lua_TypeID
	{
		public static string get(Type t)
		{
			string text;
			if (Luau.lua_TypeID.names.TryGetValue(t, out text))
			{
				return text;
			}
			return "";
		}

		public static void push(Type t, string name)
		{
			Luau.lua_TypeID.names.TryAdd(t, name);
		}

		private static Dictionary<Type, string> names = new Dictionary<Type, string>();
	}

	public static class lua_ClassFields<T>
	{
		public static FieldInfo Get(string name)
		{
			Dictionary<int, FieldInfo> dictionary;
			FieldInfo fieldInfo;
			if (Luau.lua_ClassFields<T>.classDictionarys.TryGetValue(typeof(T).GetHashCode(), out dictionary) && dictionary.TryGetValue(name.GetHashCode(), out fieldInfo))
			{
				return fieldInfo;
			}
			return null;
		}

		public static void Add(string name, FieldInfo field)
		{
			Dictionary<int, FieldInfo> dictionary;
			if (Luau.lua_ClassFields<T>.classDictionarys.TryGetValue(typeof(T).GetHashCode(), out dictionary))
			{
				dictionary.TryAdd(name.GetHashCode(), field);
				return;
			}
			Dictionary<int, FieldInfo> dictionary2 = new Dictionary<int, FieldInfo>();
			dictionary2.TryAdd(name.GetHashCode(), field);
			Luau.lua_ClassFields<T>.classDictionarys.TryAdd(typeof(T).GetHashCode(), dictionary2);
		}

		private static Dictionary<int, Dictionary<int, FieldInfo>> classDictionarys = new Dictionary<int, Dictionary<int, FieldInfo>>();
	}

	public static class lua_ClassProperties<T>
	{
		public static lua_CFunction Get(string name)
		{
			Dictionary<string, lua_CFunction> dictionary;
			lua_CFunction lua_CFunction;
			if (Luau.lua_ClassProperties<T>.classProperties.TryGetValue(typeof(T), out dictionary) && dictionary.TryGetValue(name, out lua_CFunction))
			{
				return lua_CFunction;
			}
			return null;
		}

		public static void Add(string name, lua_CFunction field)
		{
			Dictionary<string, lua_CFunction> dictionary;
			if (Luau.lua_ClassProperties<T>.classProperties.TryGetValue(typeof(T), out dictionary))
			{
				dictionary.TryAdd(name, field);
				return;
			}
			Dictionary<string, lua_CFunction> dictionary2 = new Dictionary<string, lua_CFunction>();
			dictionary2.TryAdd(name, field);
			Luau.lua_ClassProperties<T>.classProperties.TryAdd(typeof(T), dictionary2);
		}

		private static Dictionary<Type, Dictionary<string, lua_CFunction>> classProperties = new Dictionary<Type, Dictionary<string, lua_CFunction>>();
	}

	public static class lua_ClassFunctions<T>
	{
		public static lua_CFunction Get(string name)
		{
			Dictionary<string, lua_CFunction> dictionary;
			lua_CFunction lua_CFunction;
			if (Luau.lua_ClassFunctions<T>.classProperties.TryGetValue(typeof(T), out dictionary) && dictionary.TryGetValue(name, out lua_CFunction))
			{
				return lua_CFunction;
			}
			return null;
		}

		public static void Add(string name, lua_CFunction field)
		{
			Dictionary<string, lua_CFunction> dictionary;
			if (Luau.lua_ClassFunctions<T>.classProperties.TryGetValue(typeof(T), out dictionary))
			{
				dictionary.TryAdd(name, field);
				return;
			}
			Dictionary<string, lua_CFunction> dictionary2 = new Dictionary<string, lua_CFunction>();
			dictionary2.TryAdd(name, field);
			Luau.lua_ClassFunctions<T>.classProperties.TryAdd(typeof(T), dictionary2);
		}

		private static Dictionary<Type, Dictionary<string, lua_CFunction>> classProperties = new Dictionary<Type, Dictionary<string, lua_CFunction>>();
	}
}

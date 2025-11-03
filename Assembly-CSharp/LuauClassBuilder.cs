using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class LuauClassBuilder<[IsUnmanaged] T> where T : struct, ValueType
{
	public LuauClassBuilder(string className)
	{
		this._className = className;
		this._classType = typeof(T);
	}

	public LuauClassBuilder<T> AddField(string luaName, string fieldName = null)
	{
		if (fieldName == null)
		{
			fieldName = luaName;
		}
		FieldInfo field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
		if (field == null)
		{
			throw new ArgumentException(string.Concat(new string[]
			{
				"Property ",
				fieldName,
				" does not exist on type ",
				typeof(T).Name,
				"."
			}));
		}
		this._classFields.TryAdd(LuaHashing.ByteHash(luaName), field);
		return this;
	}

	public LuauClassBuilder<T> AddStaticFunction(string luaName, lua_CFunction function)
	{
		this._staticFunctions.TryAdd(luaName, function);
		return this;
	}

	public LuauClassBuilder<T> AddStaticFunction(string luaName, FunctionPointer<lua_CFunction> function)
	{
		this._staticFunctionPtrs.TryAdd(luaName, function);
		return this;
	}

	public LuauClassBuilder<T> AddProperty(string luaName, lua_CFunction function)
	{
		this._properties.TryAdd(luaName, function);
		return this;
	}

	public LuauClassBuilder<T> AddProperty(string luaName, FunctionPointer<lua_CFunction> function)
	{
		this._propertyPtrs.TryAdd(luaName, function);
		return this;
	}

	public LuauClassBuilder<T> AddFunction(string luaName, lua_CFunction function)
	{
		if (luaName.StartsWith("__"))
		{
			this._staticFunctions.TryAdd(luaName, function);
		}
		this._functions.TryAdd(LuaHashing.ByteHash(luaName), function);
		return this;
	}

	public LuauClassBuilder<T> AddFunction(string luaName, FunctionPointer<lua_CFunction> function)
	{
		if (luaName.StartsWith("__"))
		{
			this._staticFunctionPtrs.TryAdd(luaName, function);
		}
		this._functionPtrs.TryAdd(LuaHashing.ByteHash(luaName), function);
		return this;
	}

	public unsafe LuauClassBuilder<T> Build(lua_State* L, bool global)
	{
		BurstClassInfo.NewClass<T>(this._className, this._classFields, this._functions, this._functionPtrs);
		Luau.luaL_newmetatable(L, this._className);
		FunctionPointer<lua_CFunction> functionPointer = BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(BurstClassInfo.Index));
		Luau.lua_pushcfunction(L, functionPointer, null);
		Luau.lua_setfield(L, -2, "__index");
		FunctionPointer<lua_CFunction> functionPointer2 = BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(BurstClassInfo.NameCall));
		Luau.lua_pushcfunction(L, functionPointer2, null);
		Luau.lua_setfield(L, -2, "__namecall");
		FunctionPointer<lua_CFunction> functionPointer3 = BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(BurstClassInfo.NewIndex));
		Luau.lua_pushcfunction(L, functionPointer3, null);
		Luau.lua_setfield(L, -2, "__newindex");
		foreach (KeyValuePair<string, lua_CFunction> keyValuePair in this._staticFunctions)
		{
			Luau.lua_pushcfunction(L, keyValuePair.Value, keyValuePair.Key);
			Luau.lua_setfield(L, -2, keyValuePair.Key);
		}
		foreach (KeyValuePair<string, FunctionPointer<lua_CFunction>> keyValuePair2 in this._staticFunctionPtrs)
		{
			Luau.lua_pushcfunction(L, keyValuePair2.Value, keyValuePair2.Key);
			Luau.lua_setfield(L, -2, keyValuePair2.Key);
		}
		FixedString32Bytes fixedString32Bytes = "metahash";
		byte* ptr = (byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes) + 2;
		Luau.lua_pushnumber(L, (double)LuaHashing.ByteHash(this._className));
		Luau.lua_setfield(L, -2, ptr);
		Luau.lua_setreadonly(L, -1, 1);
		Luau.lua_pop(L, 1);
		if (global)
		{
			Luau.lua_createtable(L, 0, 0);
			foreach (KeyValuePair<string, lua_CFunction> keyValuePair3 in this._staticFunctions)
			{
				Luau.lua_pushcfunction(L, keyValuePair3.Value, keyValuePair3.Key);
				Luau.lua_setfield(L, -2, keyValuePair3.Key);
			}
			foreach (KeyValuePair<string, FunctionPointer<lua_CFunction>> keyValuePair4 in this._staticFunctionPtrs)
			{
				Luau.lua_pushcfunction(L, keyValuePair4.Value, keyValuePair4.Key);
				Luau.lua_setfield(L, -2, keyValuePair4.Key);
			}
			Luau.lua_pushnumber(L, (double)LuaHashing.ByteHash(this._className));
			Luau.lua_setfield(L, -2, ptr);
			Luau.luaL_getmetatable(L, this._className);
			Luau.lua_setmetatable(L, -2);
			Luau.lua_setglobal(L, this._className);
		}
		return this;
	}

	private string _className;

	private Type _classType;

	private Dictionary<string, lua_CFunction> _staticFunctions = new Dictionary<string, lua_CFunction>();

	private Dictionary<string, FunctionPointer<lua_CFunction>> _staticFunctionPtrs = new Dictionary<string, FunctionPointer<lua_CFunction>>();

	private Dictionary<int, FieldInfo> _classFields = new Dictionary<int, FieldInfo>();

	private Dictionary<string, lua_CFunction> _properties = new Dictionary<string, lua_CFunction>();

	private Dictionary<string, FunctionPointer<lua_CFunction>> _propertyPtrs = new Dictionary<string, FunctionPointer<lua_CFunction>>();

	private Dictionary<int, lua_CFunction> _functions = new Dictionary<int, lua_CFunction>();

	private Dictionary<int, FunctionPointer<lua_CFunction>> _functionPtrs = new Dictionary<int, FunctionPointer<lua_CFunction>>();
}

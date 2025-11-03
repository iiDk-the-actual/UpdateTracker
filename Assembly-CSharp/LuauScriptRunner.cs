using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

public class LuauScriptRunner
{
	public unsafe static bool ErrorCheck(lua_State* L, int status)
	{
		if (status != 0)
		{
			sbyte* ptr = Luau.lua_tostring(L, -1);
			LuauHud.Instance.LuauLog(new string(ptr));
			sbyte* ptr2 = (sbyte*)Luau.lua_debugtrace(L);
			LuauHud.Instance.LuauLog(new string(ptr2));
			LuauHud.Instance.LuauLog("Error code: " + status.ToString());
			Luau.lua_close(L);
			return true;
		}
		return false;
	}

	public bool Tick(float deltaTime)
	{
		if (!this.ShouldTick)
		{
			return false;
		}
		this.preTickCallback(this.L);
		LuauVm.ProcessEvents();
		if (!this.ShouldTick)
		{
			return false;
		}
		Luau.lua_settop(this.L, 0);
		Luau.lua_getfield(this.L, -10002, "tick");
		if (Luau.lua_type(this.L, -1) == 7)
		{
			Luau.lua_pushnumber(this.L, (double)deltaTime);
			int num = Luau.lua_pcall(this.L, 1, 0, 0);
			this.ShouldTick = !LuauScriptRunner.ErrorCheck(this.L, num);
			if (this.ShouldTick)
			{
				this.postTickCallback(this.L);
				Luau.lua_settop(this.L, 0);
				int num2 = Luau.lua_gc(this.L, 3, 0);
				Luau.lua_gc(this.L, 6, num2);
			}
			return this.ShouldTick;
		}
		Luau.lua_pop(this.L, 1);
		return false;
	}

	public unsafe LuauScriptRunner(string script, string name, [CanBeNull] lua_CFunction bindings = null, [CanBeNull] lua_CFunction preTick = null, [CanBeNull] lua_CFunction postTick = null)
	{
		this.Script = script;
		this.ScriptName = name;
		this.L = Luau.luaL_newstate();
		LuauScriptRunner.ScriptRunners.Add(this);
		Luau.luaL_openlibs(this.L);
		Bindings.Vec3Builder(this.L);
		Bindings.QuatBuilder(this.L);
		if (bindings != null)
		{
			bindings(this.L);
		}
		this.postTickCallback = postTick;
		this.preTickCallback = preTick;
		UIntPtr uintPtr = (UIntPtr)((IntPtr)0);
		Luau.lua_register(this.L, new lua_CFunction(Luau.lua_print), "print");
		byte[] bytes = Encoding.UTF8.GetBytes(script);
		sbyte* ptr = Luau.luau_compile(script, (UIntPtr)((IntPtr)bytes.Length), null, &uintPtr);
		Luau.luau_load(this.L, name, ptr, uintPtr, 0);
		int num = Luau.lua_resume(this.L, null, 0);
		this.ShouldTick = !LuauScriptRunner.ErrorCheck(this.L, num);
	}

	public LuauScriptRunner FromFile(string filePath, [CanBeNull] lua_CFunction bindings = null, [CanBeNull] lua_CFunction tick = null)
	{
		return new LuauScriptRunner(File.ReadAllText(Path.Join(Application.persistentDataPath, "Scripts", filePath)), filePath, bindings, tick, null);
	}

	~LuauScriptRunner()
	{
		LuauVm.ClassBuilders.Clear();
		Bindings.LuauPlayerList.Clear();
		Bindings.LuauGameObjectList.Clear();
		Bindings.LuauGameObjectListReverse.Clear();
		Bindings.LuauGameObjectStates.Clear();
		Bindings.LuauVRRigList.Clear();
		Bindings.LuauAIAgentList.Clear();
		Bindings.Components.ComponentList.Clear();
		ReflectionMetaNames.ReflectedNames.Clear();
		if (BurstClassInfo.ClassList.InfoFields.Data.IsCreated)
		{
			BurstClassInfo.ClassList.InfoFields.Data.Clear();
		}
	}

	public static List<LuauScriptRunner> ScriptRunners = new List<LuauScriptRunner>();

	public bool ShouldTick;

	private lua_CFunction postTickCallback;

	private lua_CFunction preTickCallback;

	public string ScriptName;

	public string Script;

	public unsafe lua_State* L;
}

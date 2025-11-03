using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaTagScripts.VirtualStumpCustomMaps;
using GT_CustomMapSupportRuntime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR;

[BurstCompile]
public static class Bindings
{
	public unsafe static void GameObjectBuilder(lua_State* L)
	{
		LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.LuauGameObject>("GameObject").AddField("position", "Position").AddField("rotation", "Rotation").AddField("scale", "Scale")
			.AddStaticFunction("findGameObject", new lua_CFunction(Bindings.GameObjectFunctions.FindGameObject))
			.AddFunction("setCollision", new lua_CFunction(Bindings.GameObjectFunctions.SetCollision))
			.AddFunction("setVisibility", new lua_CFunction(Bindings.GameObjectFunctions.SetVisibility))
			.AddFunction("setActive", new lua_CFunction(Bindings.GameObjectFunctions.SetActive))
			.AddFunction("setText", new lua_CFunction(Bindings.GameObjectFunctions.SetText))
			.AddFunction("onTouched", new lua_CFunction(Bindings.GameObjectFunctions.OnTouched))
			.AddFunction("setVelocity", new lua_CFunction(Bindings.GameObjectFunctions.SetVelocity))
			.AddFunction("getVelocity", new lua_CFunction(Bindings.GameObjectFunctions.GetVelocity))
			.AddFunction("setColor", new lua_CFunction(Bindings.GameObjectFunctions.SetColor))
			.AddFunction("findChild", new lua_CFunction(Bindings.GameObjectFunctions.FindChildGameObject))
			.AddFunction("clone", new lua_CFunction(Bindings.GameObjectFunctions.CloneGameObject))
			.AddFunction("destroy", new lua_CFunction(Bindings.GameObjectFunctions.DestroyGameObject))
			.AddFunction("findComponent", new lua_CFunction(Bindings.GameObjectFunctions.FindComponent))
			.AddFunction("equals", new lua_CFunction(Bindings.GameObjectFunctions.Equals))
			.Build(L, true));
	}

	public unsafe static void GorillaLocomotionSettingsBuilder(lua_State* L)
	{
		LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.GorillaLocomotionSettings>("PSettings").AddField("velocityLimit", null).AddField("slideVelocityLimit", null).AddField("maxJumpSpeed", null)
			.AddField("jumpMultiplier", null)
			.Build(L, false));
		Bindings.LocomotionSettings = Luau.lua_class_push<Bindings.GorillaLocomotionSettings>(L);
		Bindings.LocomotionSettings->velocityLimit = GTPlayer.Instance.velocityLimit;
		Bindings.LocomotionSettings->slideVelocityLimit = GTPlayer.Instance.slideVelocityLimit;
		Bindings.LocomotionSettings->maxJumpSpeed = 6.5f;
		Bindings.LocomotionSettings->jumpMultiplier = 1.1f;
		Luau.lua_setglobal(L, "PlayerSettings");
	}

	public unsafe static void PlayerInputBuilder(lua_State* L)
	{
		LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.PlayerInput>("PInput").AddField("leftXAxis", null).AddField("rightXAxis", null).AddField("leftYAxis", null)
			.AddField("rightYAxis", null)
			.AddField("leftTrigger", null)
			.AddField("rightTrigger", null)
			.AddField("leftGrip", null)
			.AddField("rightGrip", null)
			.AddField("leftPrimaryButton", null)
			.AddField("rightPrimaryButton", null)
			.AddField("leftSecondaryButton", null)
			.AddField("rightSecondaryButton", null)
			.Build(L, false));
		Bindings.LocalPlayerInput = Luau.lua_class_push<Bindings.PlayerInput>(L);
		Bindings.UpdateInputs();
		Luau.lua_setglobal(L, "PlayerInput");
	}

	public unsafe static void UpdateInputs()
	{
		if (Bindings.LocalPlayerInput != null)
		{
			Bindings.LocalPlayerInput->leftPrimaryButton = ControllerInputPoller.PrimaryButtonPress(XRNode.LeftHand);
			Bindings.LocalPlayerInput->rightPrimaryButton = ControllerInputPoller.PrimaryButtonPress(XRNode.RightHand);
			Bindings.LocalPlayerInput->leftSecondaryButton = ControllerInputPoller.SecondaryButtonPress(XRNode.LeftHand);
			Bindings.LocalPlayerInput->rightSecondaryButton = ControllerInputPoller.SecondaryButtonPress(XRNode.RightHand);
			Bindings.LocalPlayerInput->leftGrip = ControllerInputPoller.GripFloat(XRNode.LeftHand);
			Bindings.LocalPlayerInput->rightGrip = ControllerInputPoller.GripFloat(XRNode.RightHand);
			Bindings.LocalPlayerInput->leftTrigger = ControllerInputPoller.TriggerFloat(XRNode.LeftHand);
			Bindings.LocalPlayerInput->rightTrigger = ControllerInputPoller.TriggerFloat(XRNode.RightHand);
			Vector2 vector = ControllerInputPoller.Primary2DAxis(XRNode.LeftHand);
			Vector2 vector2 = ControllerInputPoller.Primary2DAxis(XRNode.RightHand);
			Bindings.LocalPlayerInput->leftXAxis = vector.x;
			Bindings.LocalPlayerInput->leftYAxis = vector.y;
			Bindings.LocalPlayerInput->rightXAxis = vector2.x;
			Bindings.LocalPlayerInput->rightYAxis = vector2.y;
		}
	}

	public unsafe static void Vec3Builder(lua_State* L)
	{
		LuauVm.ClassBuilders.Append(new LuauClassBuilder<Vector3>("Vec3").AddField("x", null).AddField("y", null).AddField("z", null)
			.AddStaticFunction("new", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.New)))
			.AddFunction("__add", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Add)))
			.AddFunction("__sub", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Sub)))
			.AddFunction("__mul", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Mul)))
			.AddFunction("__div", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Div)))
			.AddFunction("__unm", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Unm)))
			.AddFunction("__eq", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Eq)))
			.AddFunction("__tostring", new lua_CFunction(Bindings.Vec3Functions.ToString))
			.AddFunction("toString", new lua_CFunction(Bindings.Vec3Functions.ToString))
			.AddFunction("dot", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Dot)))
			.AddFunction("cross", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Cross)))
			.AddFunction("projectOnTo", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Project)))
			.AddFunction("length", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Length)))
			.AddFunction("normalize", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Normalize)))
			.AddFunction("getSafeNormal", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.SafeNormal)))
			.AddStaticFunction("rotate", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Rotate)))
			.AddFunction("rotate", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Rotate)))
			.AddStaticFunction("distance", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Distance)))
			.AddFunction("distance", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Distance)))
			.AddStaticFunction("lerp", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Lerp)))
			.AddFunction("lerp", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.Lerp)))
			.AddProperty("zeroVector", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.ZeroVector)))
			.AddProperty("oneVector", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.OneVector)))
			.AddStaticFunction("nearlyEqual", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.Vec3Functions.NearlyEqual)))
			.Build(L, true));
	}

	public unsafe static void QuatBuilder(lua_State* L)
	{
		LuauVm.ClassBuilders.Append(new LuauClassBuilder<Quaternion>("Quat").AddField("x", null).AddField("y", null).AddField("z", null)
			.AddField("w", null)
			.AddStaticFunction("new", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.QuatFunctions.New)))
			.AddFunction("__mul", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.QuatFunctions.Mul)))
			.AddFunction("__eq", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.QuatFunctions.Eq)))
			.AddFunction("__tostring", new lua_CFunction(Bindings.QuatFunctions.ToString))
			.AddFunction("toString", new lua_CFunction(Bindings.QuatFunctions.ToString))
			.AddStaticFunction("fromEuler", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.QuatFunctions.FromEuler)))
			.AddStaticFunction("fromDirection", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.QuatFunctions.FromDirection)))
			.AddFunction("getUpVector", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.QuatFunctions.GetUpVector)))
			.AddFunction("euler", BurstCompiler.CompileFunctionPointer<lua_CFunction>(new lua_CFunction(Bindings.QuatFunctions.Euler)))
			.Build(L, true));
	}

	public unsafe static void PlayerBuilder(lua_State* L)
	{
		LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.LuauPlayer>("Player").AddField("playerID", "PlayerID").AddField("playerName", "PlayerName").AddField("playerMaterial", "PlayerMaterial")
			.AddField("isMasterClient", "IsMasterClient")
			.AddField("bodyPosition", "BodyPosition")
			.AddField("velocity", "Velocity")
			.AddField("isPCVR", "IsPCVR")
			.AddField("leftHandPosition", "LeftHandPosition")
			.AddField("rightHandPosition", "RightHandPosition")
			.AddField("headRotation", "HeadRotation")
			.AddField("leftHandRotation", "LeftHandRotation")
			.AddField("rightHandRotation", "RightHandRotation")
			.AddField("isInVStump", "IsInVStump")
			.AddField("isEntityAuthority", "IsEntityAuthority")
			.AddStaticFunction("getPlayerByID", new lua_CFunction(Bindings.PlayerFunctions.GetPlayerByID))
			.Build(L, true));
	}

	public unsafe static void AIAgentBuilder(lua_State* L)
	{
		LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.LuauAIAgent>("AIAgent").AddField("entityID", "EntityID").AddField("agentPosition", "EntityPosition").AddField("agentRotation", "EntityRotation")
			.AddFunction("__tostring", new lua_CFunction(Bindings.AIAgentFunctions.ToString))
			.AddFunction("toString", new lua_CFunction(Bindings.AIAgentFunctions.ToString))
			.AddFunction("setDestination", new lua_CFunction(Bindings.AIAgentFunctions.SetDestination))
			.AddFunction("destroyAgent", new lua_CFunction(Bindings.AIAgentFunctions.DestroyEntity))
			.AddFunction("playAgentAnimation", new lua_CFunction(Bindings.AIAgentFunctions.PlayAgentAnimation))
			.AddFunction("getTargetPlayer", new lua_CFunction(Bindings.AIAgentFunctions.GetTarget))
			.AddFunction("setTargetPlayer", new lua_CFunction(Bindings.AIAgentFunctions.SetTarget))
			.AddStaticFunction("findPrePlacedAIAgentByID", new lua_CFunction(Bindings.AIAgentFunctions.FindPrePlacedAIAgentByID))
			.AddStaticFunction("getAIAgentByEntityID", new lua_CFunction(Bindings.AIAgentFunctions.GetAIAgentByEntityID))
			.AddStaticFunction("spawnAIAgent", new lua_CFunction(Bindings.AIAgentFunctions.SpawnAIAgent))
			.Build(L, true));
	}

	public unsafe static void GrabbableEntityBuilder(lua_State* L)
	{
		LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.LuauGrabbableEntity>("GrabbableEntity").AddField("entityID", "EntityID").AddField("entityPosition", "EntityPosition").AddField("entityRotation", "EntityRotation")
			.AddFunction("__tostring", new lua_CFunction(Bindings.GrabbableEntityFunctions.ToString))
			.AddFunction("toString", new lua_CFunction(Bindings.GrabbableEntityFunctions.ToString))
			.AddFunction("destroyGrabbable", new lua_CFunction(Bindings.GrabbableEntityFunctions.DestroyEntity))
			.AddStaticFunction("findPrePlacedGrabbableEntityByID", new lua_CFunction(Bindings.GrabbableEntityFunctions.FindPrePlacedGrabbableEntityByID))
			.AddStaticFunction("getGrabbableEntityByEntityID", new lua_CFunction(Bindings.GrabbableEntityFunctions.GetGrabbableEntityByEntityID))
			.AddStaticFunction("getHoldingActorNumberByEntityID", new lua_CFunction(Bindings.GrabbableEntityFunctions.GetHoldingActorNumberByEntityID))
			.AddStaticFunction("getHoldingActorNumberByLuauID", new lua_CFunction(Bindings.GrabbableEntityFunctions.GetHoldingActorNumberByLuauID))
			.AddStaticFunction("spawnGrabbableEntity", new lua_CFunction(Bindings.GrabbableEntityFunctions.SpawnGrabbableEntity))
			.Build(L, true));
	}

	[MonoPInvokeCallback(typeof(lua_CFunction))]
	public unsafe static int LuaStartVibration(lua_State* L)
	{
		bool flag = Luau.lua_toboolean(L, 1) == 1;
		float num = (float)Luau.luaL_checknumber(L, 2);
		float num2 = (float)Luau.luaL_checknumber(L, 3);
		GorillaTagger.Instance.StartVibration(flag, num, num2);
		return 0;
	}

	[MonoPInvokeCallback(typeof(lua_CFunction))]
	public unsafe static int LuaPlaySound(lua_State* L)
	{
		int num = (int)Luau.luaL_checknumber(L, 1);
		Vector3 vector = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
		float num2 = (float)Luau.luaL_checknumber(L, 3);
		if (num < 0 || num >= VRRig.LocalRig.clipToPlay.Length)
		{
			return 0;
		}
		AudioSource.PlayClipAtPoint(VRRig.LocalRig.clipToPlay[num], vector, num2);
		return 0;
	}

	public unsafe static void RoomStateBuilder(lua_State* L)
	{
		LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.LuauRoomState>("RState").AddField("isQuest", "IsQuest").AddField("fps", "FPS").AddField("isPrivate", "IsPrivate")
			.AddField("code", "RoomCode")
			.Build(L, false));
		Bindings.RoomState = Luau.lua_class_push<Bindings.LuauRoomState>(L);
		Bindings.UpdateRoomState();
		Bindings.RoomState->IsQuest = false;
		Bindings.RoomState->IsPrivate = !PhotonNetwork.CurrentRoom.IsVisible;
		Bindings.RoomState->RoomCode = PhotonNetwork.CurrentRoom.Name;
		Luau.lua_setglobal(L, "Room");
	}

	public unsafe static void UpdateRoomState()
	{
		Bindings.RoomState->FPS = 1f / Time.smoothDeltaTime;
	}

	public static Dictionary<GameObject, IntPtr> LuauGameObjectList = new Dictionary<GameObject, IntPtr>();

	public static List<KeyValuePair<GameObject, IntPtr>> LuauGameObjectDepthList = new List<KeyValuePair<GameObject, IntPtr>>();

	public static Dictionary<IntPtr, GameObject> LuauGameObjectListReverse = new Dictionary<IntPtr, GameObject>();

	public static Dictionary<GameObject, Bindings.LuauGameObjectInitialState> LuauGameObjectStates = new Dictionary<GameObject, Bindings.LuauGameObjectInitialState>();

	public static Dictionary<GameObject, int> LuauTriggerCallbacks = new Dictionary<GameObject, int>();

	public static Dictionary<int, IntPtr> LuauPlayerList = new Dictionary<int, IntPtr>();

	public static Dictionary<int, VRRig> LuauVRRigList = new Dictionary<int, VRRig>();

	public unsafe static Bindings.GorillaLocomotionSettings* LocomotionSettings;

	public unsafe static Bindings.PlayerInput* LocalPlayerInput;

	public unsafe static Bindings.LuauRoomState* RoomState;

	public static Dictionary<int, IntPtr> LuauAIAgentList = new Dictionary<int, IntPtr>();

	public static Dictionary<int, IntPtr> LuauGrabbablesList = new Dictionary<int, IntPtr>();

	public static class LuaEmit
	{
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Emit(lua_State* L)
		{
			if (Bindings.LuaEmit.callTime < Time.time - 1f)
			{
				Bindings.LuaEmit.callTime = Time.time - 1f;
			}
			Bindings.LuaEmit.callTime += 1f / Bindings.LuaEmit.callCount;
			if (Bindings.LuaEmit.callTime > Time.time)
			{
				LuauHud.Instance.LuauLog("Emit rate limit reached, event not sent");
				return 0;
			}
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others
			};
			if (Luau.lua_type(L, 2) != 6)
			{
				Luau.luaL_errorL(L, "Argument 2 must be a table", Array.Empty<string>());
				return 0;
			}
			Luau.lua_pushnil(L);
			int num = 0;
			List<object> list = new List<object>();
			list.Add(Marshal.PtrToStringAnsi((IntPtr)((void*)Luau.luaL_checkstring(L, 1))));
			while (Luau.lua_next(L, 2) != 0 && num++ < 10)
			{
				Luau.lua_Types lua_Types = (Luau.lua_Types)Luau.lua_type(L, -1);
				if (lua_Types <= Luau.lua_Types.LUA_TNUMBER)
				{
					if (lua_Types == Luau.lua_Types.LUA_TBOOLEAN)
					{
						list.Add(Luau.lua_toboolean(L, -1) == 1);
						Luau.lua_pop(L, 1);
						continue;
					}
					if (lua_Types == Luau.lua_Types.LUA_TNUMBER)
					{
						list.Add(Luau.luaL_checknumber(L, -1));
						Luau.lua_pop(L, 1);
						continue;
					}
				}
				else if (lua_Types == Luau.lua_Types.LUA_TTABLE || lua_Types == Luau.lua_Types.LUA_TUSERDATA)
				{
					Luau.luaL_getmetafield(L, -1, "metahash");
					BurstClassInfo.ClassInfo classInfo;
					if (!BurstClassInfo.ClassList.InfoFields.Data.TryGetValue((int)Luau.luaL_checknumber(L, -1), out classInfo))
					{
						FixedString64Bytes fixedString64Bytes = "\"Internal Class Info Error No Metatable Found\"";
						Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString64Bytes>(ref fixedString64Bytes) + 2));
						return 0;
					}
					Luau.lua_pop(L, 1);
					FixedString32Bytes fixedString32Bytes = "Vec3";
					if ((in classInfo.Name) == (in fixedString32Bytes))
					{
						list.Add(*Luau.lua_class_get<Vector3>(L, -1));
						Luau.lua_pop(L, 1);
						continue;
					}
					fixedString32Bytes = "Quat";
					if ((in classInfo.Name) == (in fixedString32Bytes))
					{
						list.Add(*Luau.lua_class_get<Quaternion>(L, -1));
						Luau.lua_pop(L, 1);
						continue;
					}
					fixedString32Bytes = "Player";
					if ((in classInfo.Name) == (in fixedString32Bytes))
					{
						int playerID = Luau.lua_class_get<Bindings.LuauPlayer>(L, -1)->PlayerID;
						NetPlayer netPlayer = null;
						foreach (NetPlayer netPlayer2 in RoomSystem.PlayersInRoom)
						{
							if (netPlayer2.ActorNumber == playerID)
							{
								netPlayer = netPlayer2;
							}
						}
						if (netPlayer == null)
						{
							list.Add(null);
						}
						else
						{
							list.Add(netPlayer.GetPlayerRef());
						}
						Luau.lua_pop(L, 1);
						continue;
					}
					FixedString32Bytes fixedString32Bytes2 = "\"Unknown Type in table\"";
					Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes2) + 2));
					continue;
				}
				FixedString32Bytes fixedString32Bytes3 = "\"Unknown Type in table\"";
				Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes3) + 2));
				return 0;
			}
			if (PhotonNetwork.InRoom)
			{
				PhotonNetwork.RaiseEvent(180, list.ToArray(), raiseEventOptions, SendOptions.SendReliable);
			}
			return 0;
		}

		private static float callTime = 0f;

		private static float callCount = 20f;
	}

	[BurstCompile]
	public struct LuauGameObject
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 Scale;
	}

	[BurstCompile]
	public struct LuauGameObjectInitialState
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 Scale;

		public bool Visible;

		public bool Collidable;

		public bool Created;
	}

	[BurstCompile]
	public static class GameObjectFunctions
	{
		public static int GetDepth(GameObject gameObject)
		{
			int num = 0;
			Transform transform = gameObject.transform;
			while (transform.parent != null)
			{
				num++;
				transform = transform.parent;
			}
			return num;
		}

		public static void UpdateDepthList()
		{
			Bindings.LuauGameObjectDepthList.Clear();
			Bindings.LuauGameObjectDepthList = Bindings.LuauGameObjectList.OrderByDescending((KeyValuePair<GameObject, IntPtr> kv) => Bindings.GameObjectFunctions.GetDepth(kv.Key)).ToList<KeyValuePair<GameObject, IntPtr>>();
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int New(lua_State* L)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Bindings.LuauGameObject* ptr = Luau.lua_class_push<Bindings.LuauGameObject>(L);
			ptr->Position = gameObject.transform.position;
			ptr->Rotation = gameObject.transform.rotation;
			ptr->Scale = gameObject.transform.localScale;
			Bindings.LuauGameObjectList.TryAdd(gameObject, (IntPtr)((void*)ptr));
			Bindings.LuauGameObjectListReverse.TryAdd((IntPtr)((void*)ptr), gameObject);
			return 1;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int FindGameObject(lua_State* L)
		{
			GameObject gameObject = GameObject.Find(new string((sbyte*)Luau.luaL_checkstring(L, 1)));
			if (!(gameObject != null))
			{
				return 0;
			}
			if (!CustomMapLoader.IsCustomScene(gameObject.scene.name))
			{
				return 0;
			}
			IntPtr intPtr;
			if (Bindings.LuauGameObjectList.TryGetValue(gameObject, out intPtr))
			{
				Luau.lua_class_push(L, "GameObject", intPtr);
			}
			else
			{
				Bindings.LuauGameObject* ptr = Luau.lua_class_push<Bindings.LuauGameObject>(L);
				ptr->Position = gameObject.transform.position;
				ptr->Rotation = gameObject.transform.rotation;
				ptr->Scale = gameObject.transform.localScale;
				Bindings.LuauGameObjectInitialState luauGameObjectInitialState = default(Bindings.LuauGameObjectInitialState);
				luauGameObjectInitialState.Position = gameObject.transform.localPosition;
				luauGameObjectInitialState.Rotation = gameObject.transform.localRotation;
				luauGameObjectInitialState.Scale = gameObject.transform.localScale;
				luauGameObjectInitialState.Visible = true;
				luauGameObjectInitialState.Collidable = true;
				luauGameObjectInitialState.Created = false;
				MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
				Collider component2 = gameObject.GetComponent<Collider>();
				if (component2.IsNotNull())
				{
					luauGameObjectInitialState.Collidable = component2.enabled;
				}
				if (component.IsNotNull())
				{
					luauGameObjectInitialState.Visible = component.enabled;
				}
				Bindings.LuauGameObjectList.TryAdd(gameObject, (IntPtr)((void*)ptr));
				Bindings.LuauGameObjectListReverse.TryAdd((IntPtr)((void*)ptr), gameObject);
				Bindings.LuauGameObjectStates.TryAdd(gameObject, luauGameObjectInitialState);
				Bindings.GameObjectFunctions.UpdateDepthList();
			}
			return 1;
		}

		public static Transform FindChild(Transform parent, string name)
		{
			foreach (object obj in parent)
			{
				Transform transform = (Transform)obj;
				if (transform.name == name)
				{
					return transform;
				}
				Transform transform2 = Bindings.GameObjectFunctions.FindChild(transform, name);
				if (transform2 != null)
				{
					return transform2;
				}
			}
			return null;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int FindChildGameObject(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				string text = new string((sbyte*)Luau.luaL_checkstring(L, 2));
				Transform transform = Bindings.GameObjectFunctions.FindChild(gameObject.transform, text);
				GameObject gameObject2 = ((transform != null) ? transform.gameObject : null);
				if (gameObject2.IsNotNull())
				{
					IntPtr intPtr;
					if (Bindings.LuauGameObjectList.TryGetValue(gameObject2, out intPtr))
					{
						Luau.lua_class_push(L, "GameObject", intPtr);
					}
					else
					{
						Bindings.LuauGameObject* ptr2 = Luau.lua_class_push<Bindings.LuauGameObject>(L);
						ptr2->Position = gameObject2.transform.position;
						ptr2->Rotation = gameObject2.transform.rotation;
						ptr2->Scale = gameObject2.transform.localScale;
						Bindings.LuauGameObjectInitialState luauGameObjectInitialState = default(Bindings.LuauGameObjectInitialState);
						luauGameObjectInitialState.Position = gameObject2.transform.localPosition;
						luauGameObjectInitialState.Rotation = gameObject2.transform.localRotation;
						luauGameObjectInitialState.Scale = gameObject2.transform.localScale;
						luauGameObjectInitialState.Visible = true;
						luauGameObjectInitialState.Collidable = true;
						luauGameObjectInitialState.Created = false;
						MeshRenderer component = gameObject2.GetComponent<MeshRenderer>();
						Collider component2 = gameObject2.GetComponent<Collider>();
						if (component2.IsNotNull())
						{
							luauGameObjectInitialState.Collidable = component2.enabled;
						}
						if (component.IsNotNull())
						{
							luauGameObjectInitialState.Visible = component.enabled;
						}
						Bindings.LuauGameObjectList.TryAdd(gameObject2, (IntPtr)((void*)ptr2));
						Bindings.LuauGameObjectListReverse.TryAdd((IntPtr)((void*)ptr2), gameObject2);
						Bindings.LuauGameObjectStates.TryAdd(gameObject2, luauGameObjectInitialState);
						Bindings.GameObjectFunctions.UpdateDepthList();
					}
					return 1;
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int FindComponent(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				if (gameObject == null)
				{
					return 0;
				}
				string text = new string((sbyte*)Luau.luaL_checkstring(L, 2));
				if (text == "ParticleSystem")
				{
					ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
					if (component == null)
					{
						return 0;
					}
					Bindings.Components.LuauParticleSystemBindings.LuauParticleSystem* ptr2 = Luau.lua_class_push<Bindings.Components.LuauParticleSystemBindings.LuauParticleSystem>(L);
					Bindings.Components.ComponentList.TryAdd((IntPtr)((void*)ptr2), component);
					return 1;
				}
				else if (text == "AudioSource")
				{
					AudioSource component2 = gameObject.GetComponent<AudioSource>();
					if (component2 == null)
					{
						return 0;
					}
					Bindings.Components.LuauAudioSourceBindings.LuauAudioSource* ptr3 = Luau.lua_class_push<Bindings.Components.LuauAudioSourceBindings.LuauAudioSource>(L);
					Bindings.Components.ComponentList.TryAdd((IntPtr)((void*)ptr3), component2);
					return 1;
				}
				else if (text == "Light")
				{
					Light component3 = gameObject.GetComponent<Light>();
					if (component3 == null)
					{
						return 0;
					}
					Bindings.Components.LuauLightBindings.LuauLight* ptr4 = Luau.lua_class_push<Bindings.Components.LuauLightBindings.LuauLight>(L);
					Bindings.Components.ComponentList.TryAdd((IntPtr)((void*)ptr4), component3);
					return 1;
				}
				else if (text == "Animator")
				{
					Animator component4 = gameObject.GetComponent<Animator>();
					if (component4 == null)
					{
						return 0;
					}
					Bindings.Components.LuauAnimatorBindings.LuauAnimator* ptr5 = Luau.lua_class_push<Bindings.Components.LuauAnimatorBindings.LuauAnimator>(L);
					Bindings.Components.ComponentList.TryAdd((IntPtr)((void*)ptr5), component4);
					return 1;
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int CloneGameObject(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, gameObject.transform.parent, false);
				Bindings.LuauGameObject* ptr2 = Luau.lua_class_push<Bindings.LuauGameObject>(L);
				ptr2->Position = gameObject2.transform.position;
				ptr2->Rotation = gameObject2.transform.rotation;
				ptr2->Scale = gameObject2.transform.localScale;
				Bindings.LuauGameObjectInitialState luauGameObjectInitialState = default(Bindings.LuauGameObjectInitialState);
				luauGameObjectInitialState.Position = gameObject2.transform.localPosition;
				luauGameObjectInitialState.Rotation = gameObject2.transform.localRotation;
				luauGameObjectInitialState.Scale = gameObject2.transform.localScale;
				luauGameObjectInitialState.Visible = true;
				luauGameObjectInitialState.Collidable = true;
				luauGameObjectInitialState.Created = true;
				MeshRenderer component = gameObject2.GetComponent<MeshRenderer>();
				Collider component2 = gameObject2.GetComponent<Collider>();
				if (component2.IsNotNull())
				{
					luauGameObjectInitialState.Collidable = component2.enabled;
				}
				if (component.IsNotNull())
				{
					luauGameObjectInitialState.Visible = component.enabled;
				}
				Bindings.LuauGameObjectList.TryAdd(gameObject2, (IntPtr)((void*)ptr2));
				Bindings.LuauGameObjectListReverse.TryAdd((IntPtr)((void*)ptr2), gameObject2);
				Bindings.LuauGameObjectStates.TryAdd(gameObject2, luauGameObjectInitialState);
				Bindings.GameObjectFunctions.UpdateDepthList();
				return 1;
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int DestroyGameObject(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			Bindings.LuauGameObjectInitialState luauGameObjectInitialState;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject) && Bindings.LuauGameObjectStates.TryGetValue(gameObject, out luauGameObjectInitialState))
			{
				if (!luauGameObjectInitialState.Created)
				{
					Luau.luaL_errorL(L, "Cannot destroy a non-instantiated GameObject.", Array.Empty<string>());
					return 0;
				}
				Queue<GameObject> queue = new Queue<GameObject>();
				queue.Enqueue(gameObject);
				while (queue.Count != 0)
				{
					GameObject gameObject2 = queue.Dequeue();
					IntPtr intPtr;
					if (Bindings.LuauGameObjectList.TryGetValue(gameObject2, out intPtr))
					{
						Bindings.LuauGameObjectList.Remove(gameObject2);
						Bindings.LuauGameObjectListReverse.Remove(intPtr);
						Bindings.LuauGameObjectStates.Remove(gameObject2);
						foreach (object obj in gameObject2.transform)
						{
							Transform transform = (Transform)obj;
							queue.Enqueue(transform.gameObject);
						}
					}
				}
				Bindings.GameObjectFunctions.UpdateDepthList();
				gameObject.Destroy();
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SetCollision(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				Collider component = gameObject.GetComponent<Collider>();
				if (component.IsNotNull())
				{
					component.enabled = Luau.lua_toboolean(L, 2) == 1;
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SetVisibility(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
				if (component.IsNotNull())
				{
					component.enabled = Luau.lua_toboolean(L, 2) == 1;
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SetActive(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				gameObject.SetActive(Luau.lua_toboolean(L, 2) == 1);
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SetText(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				string text = new string(Luau.lua_tostring(L, 2));
				TextMeshPro component = gameObject.GetComponent<TextMeshPro>();
				if (component.IsNotNull())
				{
					component.text = text;
				}
				else
				{
					TextMesh component2 = gameObject.GetComponent<TextMesh>();
					if (component2.IsNotNull())
					{
						component2.text = text;
					}
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int OnTouched(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				int num;
				if (Bindings.LuauTriggerCallbacks.TryGetValue(gameObject, out num))
				{
					Luau.lua_unref(L, num);
					Bindings.LuauTriggerCallbacks.Remove(gameObject);
				}
				if (Luau.lua_type(L, 2) == 7)
				{
					int num2 = Luau.lua_ref(L, 2);
					Bindings.LuauTriggerCallbacks.TryAdd(gameObject, num2);
				}
				else
				{
					FixedString32Bytes fixedString32Bytes = "Callback must be a function";
					Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes) + 2));
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SetVelocity(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				Vector3 vector = *Luau.lua_class_get<Vector3>(L, 2);
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if (component.IsNotNull())
				{
					component.linearVelocity = vector;
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int GetVelocity(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				if (gameObject.IsNull())
				{
					return 0;
				}
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				Vector3* ptr2 = Luau.lua_class_push<Vector3>(L, "Vec3");
				if (component.IsNotNull())
				{
					*ptr2 = component.linearVelocity;
				}
				else
				{
					*ptr2 = Vector3.zero;
				}
			}
			return 1;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SetColor(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 2);
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				Color color = new Color(Mathf.Clamp01(vector.x / 255f), Mathf.Clamp01(vector.y / 255f), Mathf.Clamp01(vector.z / 255f), 1f);
				TextMeshPro component = gameObject.GetComponent<TextMeshPro>();
				if (component != null)
				{
					component.color = color;
					return 0;
				}
				TextMesh component2 = gameObject.GetComponent<TextMesh>();
				if (component2 != null)
				{
					component2.color = color;
					return 0;
				}
				Renderer component3 = gameObject.GetComponent<Renderer>();
				if (component3 != null)
				{
					component3.material.color = color;
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Equals(lua_State* L)
		{
			Bindings.LuauGameObject* ptr = Luau.lua_class_get<Bindings.LuauGameObject>(L, 1, "GameObject");
			GameObject gameObject;
			if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr), out gameObject))
			{
				Bindings.LuauGameObject* ptr2 = Luau.lua_class_get<Bindings.LuauGameObject>(L, 2, "GameObject");
				GameObject gameObject2;
				if (Bindings.LuauGameObjectListReverse.TryGetValue((IntPtr)((void*)ptr2), out gameObject2) && gameObject == gameObject2)
				{
					Luau.lua_pushboolean(L, 1);
					return 1;
				}
			}
			Luau.lua_pushboolean(L, 0);
			return 1;
		}
	}

	[BurstCompile]
	public struct LuauPlayer
	{
		public int PlayerID;

		public FixedString32Bytes PlayerName;

		public int PlayerMaterial;

		[MarshalAs(UnmanagedType.U1)]
		public bool IsMasterClient;

		public Vector3 BodyPosition;

		public Vector3 Velocity;

		[MarshalAs(UnmanagedType.U1)]
		public bool IsPCVR;

		public Vector3 LeftHandPosition;

		public Vector3 RightHandPosition;

		[MarshalAs(UnmanagedType.U1)]
		public bool IsEntityAuthority;

		public Quaternion HeadRotation;

		public Quaternion LeftHandRotation;

		public Quaternion RightHandRotation;

		[MarshalAs(UnmanagedType.U1)]
		public bool IsInVStump;
	}

	[BurstCompile]
	public static class PlayerFunctions
	{
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int GetPlayerByID(lua_State* L)
		{
			int num = (int)Luau.luaL_checknumber(L, 1);
			foreach (NetPlayer netPlayer in RoomSystem.PlayersInRoom)
			{
				if (netPlayer.ActorNumber == num)
				{
					IntPtr intPtr;
					if (Bindings.LuauPlayerList.TryGetValue(netPlayer.ActorNumber, out intPtr))
					{
						Luau.lua_class_push(L, "Player", intPtr);
					}
					else
					{
						Bindings.LuauPlayer* ptr = Luau.lua_class_push<Bindings.LuauPlayer>(L);
						ptr->PlayerID = netPlayer.ActorNumber;
						ptr->PlayerMaterial = 0;
						ptr->IsMasterClient = netPlayer.IsMasterClient;
						Bindings.LuauPlayerList[netPlayer.ActorNumber] = (IntPtr)((void*)ptr);
						GorillaGameManager instance = GorillaGameManager.instance;
						VRRig vrrig = ((instance != null) ? instance.FindPlayerVRRig(netPlayer) : null);
						if (vrrig != null)
						{
							ptr->PlayerName = vrrig.playerNameVisible;
							Bindings.LuauVRRigList[netPlayer.ActorNumber] = vrrig;
							Bindings.PlayerFunctions.UpdatePlayer(L, vrrig, ptr);
							Bindings.LuauPlayerList[netPlayer.ActorNumber] = (IntPtr)((void*)ptr);
						}
					}
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static void UpdatePlayer(lua_State* L, VRRig p, Bindings.LuauPlayer* data)
		{
			data->BodyPosition = p.transform.position;
			data->Velocity = p.LatestVelocity();
			data->LeftHandPosition = p.leftHandTransform.position;
			data->RightHandPosition = p.rightHandTransform.position;
			data->HeadRotation = p.head.rigTarget.rotation;
			data->LeftHandRotation = p.leftHandTransform.rotation;
			data->RightHandRotation = p.rightHandTransform.rotation;
			if (p.isLocal)
			{
				data->IsInVStump = CustomMapManager.IsLocalPlayerInVirtualStump();
			}
			else if (p.creator != null)
			{
				data->IsInVStump = CustomMapManager.IsRemotePlayerInVirtualStump(p.creator.UserId);
			}
			else
			{
				data->IsInVStump = false;
			}
			data->IsEntityAuthority = CustomMapsGameManager.instance.IsNotNull() && CustomMapsGameManager.instance.gameEntityManager.IsNotNull() && CustomMapsGameManager.instance.gameEntityManager.IsZoneAuthority();
		}
	}

	[BurstCompile]
	public struct LuauAIAgent
	{
		public int EntityID;

		public Vector3 EntityPosition;

		public Quaternion EntityRotation;
	}

	[BurstCompile]
	public struct LuauGrabbableEntity
	{
		public int EntityID;

		public Vector3 EntityPosition;

		public Quaternion EntityRotation;
	}

	[BurstCompile]
	public static class GrabbableEntityFunctions
	{
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int ToString(lua_State* L)
		{
			string text = "NULL";
			Bindings.LuauGrabbableEntity* ptr = Luau.lua_class_get<Bindings.LuauGrabbableEntity>(L, 1);
			if (ptr != null)
			{
				text = string.Concat(new string[]
				{
					"ID: ",
					ptr->EntityID.ToString(),
					" | Pos: ",
					ptr->EntityPosition.ToString(),
					" | Rot: ",
					ptr->EntityRotation.ToString()
				});
			}
			Luau.lua_pushstring(L, text);
			return 1;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int GetGrabbableEntityByEntityID(lua_State* L)
		{
			int num = (int)Luau.luaL_checknumber(L, 1);
			Debug.Log(string.Format("[LuauBindings::GetGrabbableEntityByEntityID] ID: {0}", num));
			GameEntityManager gameEntityManager = CustomMapsGameManager.instance.gameEntityManager;
			if (gameEntityManager.IsNotNull())
			{
				GameEntityId entityIdFromNetId = gameEntityManager.GetEntityIdFromNetId(num);
				GameEntity gameEntity = gameEntityManager.GetGameEntity(entityIdFromNetId);
				if (gameEntity.IsNotNull())
				{
					if (gameEntity.gameObject.IsNull())
					{
						return 0;
					}
					Debug.Log("[LuauBindings::GetGrabbableEntityByEntityID] Found agent: " + gameEntity.gameObject.name);
					IntPtr intPtr;
					if (Bindings.LuauGrabbablesList.TryGetValue(num, out intPtr))
					{
						Bindings.GrabbableEntityFunctions.UpdateEntity(gameEntity, (Bindings.LuauGrabbableEntity*)(void*)intPtr);
						Luau.lua_class_push(L, "GrabbableEntity", intPtr);
					}
					else
					{
						Bindings.LuauGrabbableEntity* ptr = Luau.lua_class_push<Bindings.LuauGrabbableEntity>(L);
						Bindings.GrabbableEntityFunctions.UpdateEntity(gameEntity, ptr);
						Bindings.LuauGrabbablesList[num] = (IntPtr)((void*)ptr);
					}
					return 1;
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int GetHoldingActorNumberByLuauID(lua_State* L)
		{
			short num = (short)Luau.luaL_checknumber(L, 1);
			Debug.Log(string.Format("[LuauBindings::GetHoldingActorNumberByLuauID] ID: {0}", num));
			GameEntityManager gameEntityManager = CustomMapsGameManager.instance.gameEntityManager;
			if (gameEntityManager.IsNull())
			{
				return 0;
			}
			List<GameEntity> gameEntities = gameEntityManager.GetGameEntities();
			for (int i = 0; i < gameEntities.Count; i++)
			{
				if (!gameEntities[i].gameObject.IsNull())
				{
					CustomMapsGrabbablesController component = gameEntities[i].gameObject.GetComponent<CustomMapsGrabbablesController>();
					if (!component.IsNull())
					{
						Debug.Log("[LuauBindings::GetHoldingActorNumberByLuauID] checking GrabbableController on " + string.Format("{0}, id: {1}", component.gameObject.name, component.luaAgentID));
						if (component.luaAgentID == num)
						{
							Luau.lua_pushnumber(L, (double)component.GetGrabbingActor());
							return 1;
						}
					}
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int GetHoldingActorNumberByEntityID(lua_State* L)
		{
			int num = (int)Luau.luaL_checknumber(L, 1);
			Debug.Log(string.Format("[LuauBindings::GetHoldingActorNumberByEntityID] ID: {0}", num));
			GameEntityManager gameEntityManager = CustomMapsGameManager.instance.gameEntityManager;
			if (gameEntityManager.IsNull())
			{
				return 0;
			}
			GameEntityId entityIdFromNetId = gameEntityManager.GetEntityIdFromNetId(num);
			GameEntity gameEntity = gameEntityManager.GetGameEntity(entityIdFromNetId);
			if (gameEntity.IsNotNull() || gameEntity.gameObject.IsNull())
			{
				return 0;
			}
			Debug.Log("[LuauBindings::GetHoldingActorNumberByEntityID] Found agent: " + gameEntity.gameObject.name);
			CustomMapsGrabbablesController component = gameEntity.gameObject.GetComponent<CustomMapsGrabbablesController>();
			if (component.IsNull())
			{
				return 0;
			}
			Luau.lua_pushnumber(L, (double)component.GetGrabbingActor());
			return 1;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int FindPrePlacedGrabbableEntityByID(lua_State* L)
		{
			short num = (short)Luau.luaL_checknumber(L, 1);
			Debug.Log(string.Format("[LuauBindings::FindPrePlacedGrabbableEntityByID] ID: {0}", num));
			GameEntityManager gameEntityManager = CustomMapsGameManager.instance.gameEntityManager;
			if (gameEntityManager.IsNotNull())
			{
				List<GameEntity> gameEntities = gameEntityManager.GetGameEntities();
				for (int i = 0; i < gameEntities.Count; i++)
				{
					if (!gameEntities[i].gameObject.IsNull())
					{
						CustomMapsGrabbablesController component = gameEntities[i].gameObject.GetComponent<CustomMapsGrabbablesController>();
						if (!component.IsNull())
						{
							Debug.Log("[LuauBindings::FindPrePlacedGrabbableEntityByID] checking GrabbableController on " + string.Format("{0}, id: {1}", component.gameObject.name, component.luaAgentID));
							if (component.luaAgentID == num)
							{
								IntPtr intPtr;
								if (Bindings.LuauGrabbablesList.TryGetValue(gameEntities[i].GetNetId(), out intPtr))
								{
									Bindings.GrabbableEntityFunctions.UpdateEntity(gameEntities[i], (Bindings.LuauGrabbableEntity*)(void*)intPtr);
									Luau.lua_class_push(L, "GrabbableEntity", intPtr);
								}
								else
								{
									Bindings.LuauGrabbableEntity* ptr = Luau.lua_class_push<Bindings.LuauGrabbableEntity>(L);
									Bindings.GrabbableEntityFunctions.UpdateEntity(gameEntities[i], ptr);
									Bindings.LuauGrabbablesList[gameEntities[i].GetNetId()] = (IntPtr)((void*)ptr);
								}
								return 1;
							}
						}
					}
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SpawnGrabbableEntity(lua_State* L)
		{
			Debug.Log("[LuauBindings::SpawnGrabbableEntity]");
			CustomMapsGameManager instance = CustomMapsGameManager.instance;
			GameEntityManager gameEntityManager = (instance.IsNotNull() ? instance.gameEntityManager : null);
			if (gameEntityManager.IsNull())
			{
				LuauHud.Instance.LuauLog("SpawnGrabbableEntity failed. EntityManager is null.");
				return 0;
			}
			if (!gameEntityManager.IsZoneAuthority())
			{
				LuauHud.Instance.LuauLog("SpawnGrabbableEntity failed. Local Player doesn't have Entity Authority.");
				return 0;
			}
			if (Bindings.LuauAIAgentList.Count + Bindings.LuauGrabbablesList.Count == Constants.aiAgentLimit)
			{
				LuauHud.Instance.LuauLog(string.Format("SpawnGrabbableEntity failed, EntityLimit of {0}", Constants.aiAgentLimit) + " has already been reached.");
				return 0;
			}
			int num = (int)Luau.luaL_checknumber(L, 1);
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			Quaternion quaternion = *Luau.lua_class_get<Quaternion>(L, 3, "Quat");
			GameEntityId gameEntityId = instance.SpawnGrabbableAtLocation(num, vector, quaternion);
			Debug.Log("[LuauBindings::SpawnGrabbableEntity] spawnedGrabbable");
			if (!gameEntityId.IsValid())
			{
				LuauHud.Instance.LuauLog("SpawnGrabbableEntity failed to create entity.");
				return 0;
			}
			Debug.Log("[LuauBindings::SpawnGrabbableEntity] spawnedGrabbable ID valid");
			GameEntity gameEntity = gameEntityManager.GetGameEntity(gameEntityId);
			IntPtr intPtr;
			if (Bindings.LuauGrabbablesList.TryGetValue(gameEntity.GetNetId(), out intPtr))
			{
				Debug.Log("[LuauBindings::SpawnGrabbableEntity] fround grabbable");
				Bindings.GrabbableEntityFunctions.UpdateEntity(gameEntity, (Bindings.LuauGrabbableEntity*)(void*)intPtr);
				Luau.lua_class_push(L, "GrabbableEntity", intPtr);
				return 1;
			}
			Debug.Log("[LuauBindings::SpawnGrabbableEntity] grabbable not found");
			Luau.lua_getglobal(L, "GrabbableEntities");
			Bindings.LuauGrabbableEntity* ptr = Luau.lua_class_push<Bindings.LuauGrabbableEntity>(L);
			Bindings.GrabbableEntityFunctions.UpdateEntity(gameEntity, ptr);
			Bindings.LuauGrabbablesList[gameEntity.GetNetId()] = (IntPtr)((void*)ptr);
			Debug.Log("[LuauBindings::SpawnGrabbableEntity] created new grabbable");
			Luau.lua_rawseti(L, -2, Bindings.LuauGrabbablesList.Count);
			Luau.lua_pop(L, 1);
			Debug.Log("[LuauBindings::SpawnGrabbableEntity] pushing new grabbable");
			Luau.lua_class_push(L, "GrabbableEntity", (IntPtr)((void*)ptr));
			return 1;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static void UpdateEntity(GameEntity entity, Bindings.LuauGrabbableEntity* luaAgent)
		{
			luaAgent->EntityID = entity.GetNetId();
			luaAgent->EntityPosition = entity.transform.position;
			luaAgent->EntityRotation = entity.transform.rotation;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int DestroyEntity(lua_State* L)
		{
			Bindings.LuauGrabbableEntity* ptr = Luau.lua_class_get<Bindings.LuauGrabbableEntity>(L, 1);
			if (ptr != null)
			{
				GameEntityManager entityManager = CustomMapsGameManager.GetEntityManager();
				if (entityManager.IsNotNull())
				{
					GameEntityId entityIdFromNetId = entityManager.GetEntityIdFromNetId(ptr->EntityID);
					entityManager.RequestDestroyItem(entityIdFromNetId);
				}
			}
			return 0;
		}
	}

	[BurstCompile]
	public static class AIAgentFunctions
	{
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int ToString(lua_State* L)
		{
			string text = "NULL";
			Bindings.LuauAIAgent* ptr = Luau.lua_class_get<Bindings.LuauAIAgent>(L, 1);
			if (ptr != null)
			{
				text = string.Concat(new string[]
				{
					"ID: ",
					ptr->EntityID.ToString(),
					" | Pos: ",
					ptr->EntityPosition.ToString(),
					" | Rot: ",
					ptr->EntityRotation.ToString()
				});
			}
			Luau.lua_pushstring(L, text);
			return 1;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int GetAIAgentByEntityID(lua_State* L)
		{
			int num = (int)Luau.luaL_checknumber(L, 1);
			Debug.Log(string.Format("[LuauBindings::GetAIAgentByEntityID] ID: {0}", num));
			GameEntityManager gameEntityManager = CustomMapsGameManager.instance.gameEntityManager;
			if (gameEntityManager.IsNotNull())
			{
				GameEntityId entityIdFromNetId = gameEntityManager.GetEntityIdFromNetId(num);
				GameEntity gameEntity = gameEntityManager.GetGameEntity(entityIdFromNetId);
				if (gameEntity.IsNotNull())
				{
					if (gameEntity.gameObject.IsNull())
					{
						return 0;
					}
					if (gameEntity.gameObject.GetComponent<GameAgent>().IsNotNull())
					{
						Debug.Log("[LuauBindings::GetAIAgentByEntityID] Found agent: " + gameEntity.gameObject.name);
						IntPtr intPtr;
						if (Bindings.LuauAIAgentList.TryGetValue(num, out intPtr))
						{
							Bindings.AIAgentFunctions.UpdateEntity(gameEntity, (Bindings.LuauAIAgent*)(void*)intPtr);
							Luau.lua_class_push(L, "AIAgent", intPtr);
						}
						else
						{
							Bindings.LuauAIAgent* ptr = Luau.lua_class_push<Bindings.LuauAIAgent>(L);
							Bindings.AIAgentFunctions.UpdateEntity(gameEntity, ptr);
							Bindings.LuauAIAgentList[num] = (IntPtr)((void*)ptr);
						}
					}
					return 1;
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int FindPrePlacedAIAgentByID(lua_State* L)
		{
			short num = (short)Luau.luaL_checknumber(L, 1);
			GameAgentManager gameAgentManager = CustomMapsGameManager.instance.gameAgentManager;
			if (gameAgentManager.IsNotNull())
			{
				List<GameAgent> agents = gameAgentManager.GetAgents();
				for (int i = 0; i < agents.Count; i++)
				{
					if (!agents[i].gameObject.IsNull())
					{
						CustomMapsAIBehaviourController component = agents[i].gameObject.GetComponent<CustomMapsAIBehaviourController>();
						if (!component.IsNull() && component.luaAgentID == num)
						{
							IntPtr intPtr;
							if (Bindings.LuauAIAgentList.TryGetValue(agents[i].entity.GetNetId(), out intPtr))
							{
								Bindings.AIAgentFunctions.UpdateEntity(agents[i].entity, (Bindings.LuauAIAgent*)(void*)intPtr);
								Luau.lua_class_push(L, "AIAgent", intPtr);
							}
							else
							{
								Bindings.LuauAIAgent* ptr = Luau.lua_class_push<Bindings.LuauAIAgent>(L);
								Bindings.AIAgentFunctions.UpdateEntity(agents[i].entity, ptr);
								Bindings.LuauAIAgentList[agents[i].entity.GetNetId()] = (IntPtr)((void*)ptr);
							}
							return 1;
						}
					}
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SpawnAIAgent(lua_State* L)
		{
			CustomMapsGameManager instance = CustomMapsGameManager.instance;
			GameEntityManager gameEntityManager = (instance.IsNotNull() ? instance.gameEntityManager : null);
			if (gameEntityManager.IsNull())
			{
				LuauHud.Instance.LuauLog("SpawnAIAgent failed. EntityManager is null.");
				return 0;
			}
			if (!gameEntityManager.IsZoneAuthority())
			{
				LuauHud.Instance.LuauLog("SpawnAIAgent failed. Local Player doesn't have Entity Authority.");
				return 0;
			}
			if (Bindings.LuauAIAgentList.Count + Bindings.LuauGrabbablesList.Count == Constants.aiAgentLimit)
			{
				LuauHud.Instance.LuauLog(string.Format("SpawnAIAgent failed, AIAgentLimit of {0}", Constants.aiAgentLimit) + " has already been reached.");
				return 0;
			}
			int num = (int)Luau.luaL_checknumber(L, 1);
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			Quaternion quaternion = *Luau.lua_class_get<Quaternion>(L, 3, "Quat");
			GameEntityId gameEntityId = instance.SpawnEnemyAtLocation(num, vector, quaternion);
			if (gameEntityId.IsValid())
			{
				GameEntity gameEntity = gameEntityManager.GetGameEntity(gameEntityId);
				if ((gameEntity.IsNotNull() ? gameEntity.gameObject.GetComponent<GameAgent>() : null).IsNotNull())
				{
					IntPtr intPtr;
					if (Bindings.LuauAIAgentList.TryGetValue(gameEntity.GetNetId(), out intPtr))
					{
						Bindings.AIAgentFunctions.UpdateEntity(gameEntity, (Bindings.LuauAIAgent*)(void*)intPtr);
						Luau.lua_class_push(L, "AIAgent", intPtr);
						return 1;
					}
					Luau.lua_getglobal(L, "AIAgents");
					Bindings.LuauAIAgent* ptr = Luau.lua_class_push<Bindings.LuauAIAgent>(L);
					Bindings.AIAgentFunctions.UpdateEntity(gameEntity, ptr);
					Bindings.LuauAIAgentList[gameEntity.GetNetId()] = (IntPtr)((void*)ptr);
					Luau.lua_rawseti(L, -2, Bindings.LuauAIAgentList.Count);
					Luau.lua_pop(L, 1);
					Luau.lua_class_push(L, "AIAgent", (IntPtr)((void*)ptr));
					return 1;
				}
			}
			LuauHud.Instance.LuauLog("SpawnAIAgent failed to create entity.");
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SetDestination(lua_State* L)
		{
			Bindings.LuauAIAgent* ptr = Luau.lua_class_get<Bindings.LuauAIAgent>(L, 1);
			Vector3* ptr2 = Luau.lua_class_get<Vector3>(L, 2);
			GameEntityManager gameEntityManager = CustomMapsGameManager.instance.gameEntityManager;
			if (gameEntityManager.IsNotNull())
			{
				CustomMapsAIBehaviourController component = gameEntityManager.GetGameEntity(gameEntityManager.GetEntityIdFromNetId(ptr->EntityID)).gameObject.GetComponent<CustomMapsAIBehaviourController>();
				if (component.IsNotNull())
				{
					component.RequestDestination(*ptr2);
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int PlayAgentAnimation(lua_State* L)
		{
			Bindings.LuauAIAgent* ptr = Luau.lua_class_get<Bindings.LuauAIAgent>(L, 1);
			string text = Marshal.PtrToStringAnsi((IntPtr)((void*)Luau.luaL_checkstring(L, 2)));
			if (ptr != null)
			{
				GameEntityManager entityManager = CustomMapsGameManager.GetEntityManager();
				if (entityManager.IsNotNull())
				{
					CustomMapsAIBehaviourController behaviorControllerForEntity = CustomMapsGameManager.GetBehaviorControllerForEntity(entityManager.GetEntityIdFromNetId(ptr->EntityID));
					if (behaviorControllerForEntity.IsNotNull())
					{
						behaviorControllerForEntity.PlayAnimation(text, 0f);
					}
				}
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SetTarget(lua_State* L)
		{
			Bindings.LuauAIAgent* ptr = Luau.lua_class_get<Bindings.LuauAIAgent>(L, 1);
			if (ptr == null)
			{
				return 0;
			}
			GameEntityManager entityManager = CustomMapsGameManager.GetEntityManager();
			if (entityManager.IsNull() || !entityManager.IsAuthority())
			{
				return 0;
			}
			int num = (int)Luau.luaL_checknumber(L, 2);
			RigContainer rigContainer;
			if (!VRRigCache.Instance.TryGetVrrig(num, out rigContainer))
			{
				num = -1;
			}
			CustomMapsAIBehaviourController behaviorControllerForEntity = CustomMapsGameManager.GetBehaviorControllerForEntity(entityManager.GetEntityIdFromNetId(ptr->EntityID));
			if (behaviorControllerForEntity.IsNull())
			{
				return 0;
			}
			if (num == -1)
			{
				behaviorControllerForEntity.ClearTarget();
			}
			else
			{
				GRPlayer component = rigContainer.Rig.GetComponent<GRPlayer>();
				behaviorControllerForEntity.SetTarget(component);
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int GetTarget(lua_State* L)
		{
			Bindings.LuauAIAgent* ptr = Luau.lua_class_get<Bindings.LuauAIAgent>(L, 1);
			if (ptr != null)
			{
				GameEntityManager entityManager = CustomMapsGameManager.GetEntityManager();
				if (entityManager.IsNotNull() && entityManager.IsAuthority())
				{
					CustomMapsAIBehaviourController behaviorControllerForEntity = CustomMapsGameManager.GetBehaviorControllerForEntity(entityManager.GetEntityIdFromNetId(ptr->EntityID));
					if (behaviorControllerForEntity.IsNotNull() && behaviorControllerForEntity.TargetPlayer.IsNotNull() && behaviorControllerForEntity.TargetPlayer.MyRig.IsNotNull() && !behaviorControllerForEntity.TargetPlayer.MyRig.OwningNetPlayer.IsNull)
					{
						Luau.lua_pushnumber(L, (double)behaviorControllerForEntity.TargetPlayer.MyRig.OwningNetPlayer.ActorNumber);
						return 1;
					}
				}
			}
			Luau.lua_pushnumber(L, -1.0);
			return 1;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static void UpdateEntity(GameEntity entity, Bindings.LuauAIAgent* luaAgent)
		{
			luaAgent->EntityID = entity.GetNetId();
			luaAgent->EntityPosition = entity.transform.position;
			luaAgent->EntityRotation = entity.transform.rotation;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int DestroyEntity(lua_State* L)
		{
			Bindings.LuauAIAgent* ptr = Luau.lua_class_get<Bindings.LuauAIAgent>(L, 1);
			if (ptr != null)
			{
				GameEntityManager entityManager = CustomMapsGameManager.GetEntityManager();
				if (entityManager.IsNotNull())
				{
					GameEntityId entityIdFromNetId = entityManager.GetEntityIdFromNetId(ptr->EntityID);
					entityManager.RequestDestroyItem(entityIdFromNetId);
				}
			}
			return 0;
		}
	}

	[BurstCompile]
	public static class Vec3Functions
	{
		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int New(lua_State* L)
		{
			return Bindings.Vec3Functions.New_00004444$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Add(lua_State* L)
		{
			return Bindings.Vec3Functions.Add_00004445$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Sub(lua_State* L)
		{
			return Bindings.Vec3Functions.Sub_00004446$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Mul(lua_State* L)
		{
			return Bindings.Vec3Functions.Mul_00004447$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Div(lua_State* L)
		{
			return Bindings.Vec3Functions.Div_00004448$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Unm(lua_State* L)
		{
			return Bindings.Vec3Functions.Unm_00004449$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Eq(lua_State* L)
		{
			return Bindings.Vec3Functions.Eq_0000444A$BurstDirectCall.Invoke(L);
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int ToString(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Luau.lua_pushstring(L, vector.ToString());
			return 1;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Dot(lua_State* L)
		{
			return Bindings.Vec3Functions.Dot_0000444C$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Cross(lua_State* L)
		{
			return Bindings.Vec3Functions.Cross_0000444D$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Project(lua_State* L)
		{
			return Bindings.Vec3Functions.Project_0000444E$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Length(lua_State* L)
		{
			return Bindings.Vec3Functions.Length_0000444F$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Normalize(lua_State* L)
		{
			return Bindings.Vec3Functions.Normalize_00004450$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SafeNormal(lua_State* L)
		{
			return Bindings.Vec3Functions.SafeNormal_00004451$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Distance(lua_State* L)
		{
			return Bindings.Vec3Functions.Distance_00004452$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Lerp(lua_State* L)
		{
			return Bindings.Vec3Functions.Lerp_00004453$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Rotate(lua_State* L)
		{
			return Bindings.Vec3Functions.Rotate_00004454$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int ZeroVector(lua_State* L)
		{
			return Bindings.Vec3Functions.ZeroVector_00004455$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int OneVector(lua_State* L)
		{
			return Bindings.Vec3Functions.OneVector_00004456$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int NearlyEqual(lua_State* L)
		{
			return Bindings.Vec3Functions.NearlyEqual_00004457$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int New$BurstManaged(lua_State* L)
		{
			Vector3* ptr = Luau.lua_class_push<Vector3>(L, "Vec3");
			ptr->x = (float)Luau.luaL_optnumber(L, 1, 0.0);
			ptr->y = (float)Luau.luaL_optnumber(L, 2, 0.0);
			ptr->z = (float)Luau.luaL_optnumber(L, 3, 0.0);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Add$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = vector + vector2;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Sub$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = vector - vector2;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Mul$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			float num = (float)Luau.luaL_checknumber(L, 2);
			*Luau.lua_class_push<Vector3>(L, "Vec3") = vector * num;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Div$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			float num = (float)Luau.luaL_checknumber(L, 2);
			*Luau.lua_class_push<Vector3>(L, "Vec3") = vector / num;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Unm$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = -vector;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Eq$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			int num = ((vector == vector2) ? 1 : 0);
			Luau.lua_pushnumber(L, (double)num);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Dot$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			double num = (double)Vector3.Dot(vector, vector2);
			Luau.lua_pushnumber(L, num);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Cross$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = Vector3.Cross(vector, vector2);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Project$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = Vector3.Project(vector, vector2);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Length$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Luau.lua_pushnumber(L, (double)Vector3.Magnitude(vector));
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Normalize$BurstManaged(lua_State* L)
		{
			Luau.lua_class_get<Vector3>(L, 1, "Vec3")->Normalize();
			return 0;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int SafeNormal$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = vector.normalized;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Distance$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			Luau.lua_pushnumber(L, (double)Vector3.Distance(vector, vector2));
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Lerp$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			double num = Luau.luaL_checknumber(L, 3);
			*Luau.lua_class_push<Vector3>(L, "Vec3") = Vector3.Lerp(vector, vector2, (float)num);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Rotate$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Quaternion quaternion = *Luau.lua_class_get<Quaternion>(L, 2, "Quat");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = quaternion * vector;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int ZeroVector$BurstManaged(lua_State* L)
		{
			Vector3* ptr = Luau.lua_class_push<Vector3>(L, "Vec3");
			ptr->x = 0f;
			ptr->y = 0f;
			ptr->z = 0f;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int OneVector$BurstManaged(lua_State* L)
		{
			Vector3* ptr = Luau.lua_class_push<Vector3>(L, "Vec3");
			ptr->x = 1f;
			ptr->y = 1f;
			ptr->z = 1f;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int NearlyEqual$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			float num = (float)Luau.luaL_optnumber(L, 3, 0.0001);
			bool flag = Math.Abs(vector.x - vector2.x) <= num;
			if (flag && Math.Abs(vector.y - vector2.y) > num)
			{
				flag = false;
			}
			if (flag && Math.Abs(vector.z - vector2.z) > num)
			{
				flag = false;
			}
			Luau.lua_pushboolean(L, flag ? 1 : 0);
			return 1;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int New_00004444$PostfixBurstDelegate(lua_State* L);

		internal static class New_00004444$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.New_00004444$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.New_00004444$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.New_00004444$PostfixBurstDelegate>(new Bindings.Vec3Functions.New_00004444$PostfixBurstDelegate(Bindings.Vec3Functions.New)).Value;
				}
				A_0 = Bindings.Vec3Functions.New_00004444$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.New_00004444$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.New_00004444$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.New$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Add_00004445$PostfixBurstDelegate(lua_State* L);

		internal static class Add_00004445$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Add_00004445$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Add_00004445$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Add_00004445$PostfixBurstDelegate>(new Bindings.Vec3Functions.Add_00004445$PostfixBurstDelegate(Bindings.Vec3Functions.Add)).Value;
				}
				A_0 = Bindings.Vec3Functions.Add_00004445$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Add_00004445$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Add_00004445$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Add$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Sub_00004446$PostfixBurstDelegate(lua_State* L);

		internal static class Sub_00004446$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Sub_00004446$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Sub_00004446$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Sub_00004446$PostfixBurstDelegate>(new Bindings.Vec3Functions.Sub_00004446$PostfixBurstDelegate(Bindings.Vec3Functions.Sub)).Value;
				}
				A_0 = Bindings.Vec3Functions.Sub_00004446$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Sub_00004446$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Sub_00004446$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Sub$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Mul_00004447$PostfixBurstDelegate(lua_State* L);

		internal static class Mul_00004447$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Mul_00004447$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Mul_00004447$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Mul_00004447$PostfixBurstDelegate>(new Bindings.Vec3Functions.Mul_00004447$PostfixBurstDelegate(Bindings.Vec3Functions.Mul)).Value;
				}
				A_0 = Bindings.Vec3Functions.Mul_00004447$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Mul_00004447$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Mul_00004447$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Mul$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Div_00004448$PostfixBurstDelegate(lua_State* L);

		internal static class Div_00004448$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Div_00004448$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Div_00004448$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Div_00004448$PostfixBurstDelegate>(new Bindings.Vec3Functions.Div_00004448$PostfixBurstDelegate(Bindings.Vec3Functions.Div)).Value;
				}
				A_0 = Bindings.Vec3Functions.Div_00004448$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Div_00004448$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Div_00004448$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Div$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Unm_00004449$PostfixBurstDelegate(lua_State* L);

		internal static class Unm_00004449$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Unm_00004449$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Unm_00004449$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Unm_00004449$PostfixBurstDelegate>(new Bindings.Vec3Functions.Unm_00004449$PostfixBurstDelegate(Bindings.Vec3Functions.Unm)).Value;
				}
				A_0 = Bindings.Vec3Functions.Unm_00004449$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Unm_00004449$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Unm_00004449$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Unm$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Eq_0000444A$PostfixBurstDelegate(lua_State* L);

		internal static class Eq_0000444A$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Eq_0000444A$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Eq_0000444A$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Eq_0000444A$PostfixBurstDelegate>(new Bindings.Vec3Functions.Eq_0000444A$PostfixBurstDelegate(Bindings.Vec3Functions.Eq)).Value;
				}
				A_0 = Bindings.Vec3Functions.Eq_0000444A$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Eq_0000444A$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Eq_0000444A$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Eq$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Dot_0000444C$PostfixBurstDelegate(lua_State* L);

		internal static class Dot_0000444C$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Dot_0000444C$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Dot_0000444C$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Dot_0000444C$PostfixBurstDelegate>(new Bindings.Vec3Functions.Dot_0000444C$PostfixBurstDelegate(Bindings.Vec3Functions.Dot)).Value;
				}
				A_0 = Bindings.Vec3Functions.Dot_0000444C$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Dot_0000444C$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Dot_0000444C$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Dot$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Cross_0000444D$PostfixBurstDelegate(lua_State* L);

		internal static class Cross_0000444D$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Cross_0000444D$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Cross_0000444D$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Cross_0000444D$PostfixBurstDelegate>(new Bindings.Vec3Functions.Cross_0000444D$PostfixBurstDelegate(Bindings.Vec3Functions.Cross)).Value;
				}
				A_0 = Bindings.Vec3Functions.Cross_0000444D$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Cross_0000444D$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Cross_0000444D$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Cross$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Project_0000444E$PostfixBurstDelegate(lua_State* L);

		internal static class Project_0000444E$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Project_0000444E$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Project_0000444E$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Project_0000444E$PostfixBurstDelegate>(new Bindings.Vec3Functions.Project_0000444E$PostfixBurstDelegate(Bindings.Vec3Functions.Project)).Value;
				}
				A_0 = Bindings.Vec3Functions.Project_0000444E$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Project_0000444E$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Project_0000444E$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Project$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Length_0000444F$PostfixBurstDelegate(lua_State* L);

		internal static class Length_0000444F$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Length_0000444F$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Length_0000444F$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Length_0000444F$PostfixBurstDelegate>(new Bindings.Vec3Functions.Length_0000444F$PostfixBurstDelegate(Bindings.Vec3Functions.Length)).Value;
				}
				A_0 = Bindings.Vec3Functions.Length_0000444F$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Length_0000444F$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Length_0000444F$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Length$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Normalize_00004450$PostfixBurstDelegate(lua_State* L);

		internal static class Normalize_00004450$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Normalize_00004450$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Normalize_00004450$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Normalize_00004450$PostfixBurstDelegate>(new Bindings.Vec3Functions.Normalize_00004450$PostfixBurstDelegate(Bindings.Vec3Functions.Normalize)).Value;
				}
				A_0 = Bindings.Vec3Functions.Normalize_00004450$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Normalize_00004450$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Normalize_00004450$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Normalize$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int SafeNormal_00004451$PostfixBurstDelegate(lua_State* L);

		internal static class SafeNormal_00004451$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.SafeNormal_00004451$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.SafeNormal_00004451$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.SafeNormal_00004451$PostfixBurstDelegate>(new Bindings.Vec3Functions.SafeNormal_00004451$PostfixBurstDelegate(Bindings.Vec3Functions.SafeNormal)).Value;
				}
				A_0 = Bindings.Vec3Functions.SafeNormal_00004451$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.SafeNormal_00004451$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.SafeNormal_00004451$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.SafeNormal$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Distance_00004452$PostfixBurstDelegate(lua_State* L);

		internal static class Distance_00004452$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Distance_00004452$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Distance_00004452$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Distance_00004452$PostfixBurstDelegate>(new Bindings.Vec3Functions.Distance_00004452$PostfixBurstDelegate(Bindings.Vec3Functions.Distance)).Value;
				}
				A_0 = Bindings.Vec3Functions.Distance_00004452$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Distance_00004452$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Distance_00004452$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Distance$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Lerp_00004453$PostfixBurstDelegate(lua_State* L);

		internal static class Lerp_00004453$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Lerp_00004453$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Lerp_00004453$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Lerp_00004453$PostfixBurstDelegate>(new Bindings.Vec3Functions.Lerp_00004453$PostfixBurstDelegate(Bindings.Vec3Functions.Lerp)).Value;
				}
				A_0 = Bindings.Vec3Functions.Lerp_00004453$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Lerp_00004453$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Lerp_00004453$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Lerp$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Rotate_00004454$PostfixBurstDelegate(lua_State* L);

		internal static class Rotate_00004454$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.Rotate_00004454$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.Rotate_00004454$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.Rotate_00004454$PostfixBurstDelegate>(new Bindings.Vec3Functions.Rotate_00004454$PostfixBurstDelegate(Bindings.Vec3Functions.Rotate)).Value;
				}
				A_0 = Bindings.Vec3Functions.Rotate_00004454$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.Rotate_00004454$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.Rotate_00004454$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.Rotate$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int ZeroVector_00004455$PostfixBurstDelegate(lua_State* L);

		internal static class ZeroVector_00004455$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.ZeroVector_00004455$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.ZeroVector_00004455$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.ZeroVector_00004455$PostfixBurstDelegate>(new Bindings.Vec3Functions.ZeroVector_00004455$PostfixBurstDelegate(Bindings.Vec3Functions.ZeroVector)).Value;
				}
				A_0 = Bindings.Vec3Functions.ZeroVector_00004455$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.ZeroVector_00004455$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.ZeroVector_00004455$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.ZeroVector$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int OneVector_00004456$PostfixBurstDelegate(lua_State* L);

		internal static class OneVector_00004456$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.OneVector_00004456$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.OneVector_00004456$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.OneVector_00004456$PostfixBurstDelegate>(new Bindings.Vec3Functions.OneVector_00004456$PostfixBurstDelegate(Bindings.Vec3Functions.OneVector)).Value;
				}
				A_0 = Bindings.Vec3Functions.OneVector_00004456$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.OneVector_00004456$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.OneVector_00004456$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.OneVector$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int NearlyEqual_00004457$PostfixBurstDelegate(lua_State* L);

		internal static class NearlyEqual_00004457$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.Vec3Functions.NearlyEqual_00004457$BurstDirectCall.Pointer == 0)
				{
					Bindings.Vec3Functions.NearlyEqual_00004457$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.Vec3Functions.NearlyEqual_00004457$PostfixBurstDelegate>(new Bindings.Vec3Functions.NearlyEqual_00004457$PostfixBurstDelegate(Bindings.Vec3Functions.NearlyEqual)).Value;
				}
				A_0 = Bindings.Vec3Functions.NearlyEqual_00004457$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.Vec3Functions.NearlyEqual_00004457$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.Vec3Functions.NearlyEqual_00004457$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.Vec3Functions.NearlyEqual$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}
	}

	[BurstCompile]
	public static class QuatFunctions
	{
		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int New(lua_State* L)
		{
			return Bindings.QuatFunctions.New_00004458$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Mul(lua_State* L)
		{
			return Bindings.QuatFunctions.Mul_00004459$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Eq(lua_State* L)
		{
			return Bindings.QuatFunctions.Eq_0000445A$BurstDirectCall.Invoke(L);
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int ToString(lua_State* L)
		{
			Quaternion quaternion = *Luau.lua_class_get<Quaternion>(L, 1, "Quat");
			Luau.lua_pushstring(L, quaternion.ToString());
			return 1;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int FromEuler(lua_State* L)
		{
			return Bindings.QuatFunctions.FromEuler_0000445C$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int FromDirection(lua_State* L)
		{
			return Bindings.QuatFunctions.FromDirection_0000445D$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int GetUpVector(lua_State* L)
		{
			return Bindings.QuatFunctions.GetUpVector_0000445E$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int Euler(lua_State* L)
		{
			return Bindings.QuatFunctions.Euler_0000445F$BurstDirectCall.Invoke(L);
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int New$BurstManaged(lua_State* L)
		{
			Quaternion* ptr = Luau.lua_class_push<Quaternion>(L, "Quat");
			ptr->x = (float)Luau.luaL_optnumber(L, 1, 0.0);
			ptr->y = (float)Luau.luaL_optnumber(L, 2, 0.0);
			ptr->z = (float)Luau.luaL_optnumber(L, 3, 0.0);
			ptr->w = (float)Luau.luaL_optnumber(L, 4, 0.0);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Mul$BurstManaged(lua_State* L)
		{
			Quaternion quaternion = *Luau.lua_class_get<Quaternion>(L, 1, "Quat");
			Quaternion quaternion2 = *Luau.lua_class_get<Quaternion>(L, 2, "Quat");
			*Luau.lua_class_push<Quaternion>(L, "Quat") = quaternion * quaternion2;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Eq$BurstManaged(lua_State* L)
		{
			Quaternion quaternion = *Luau.lua_class_get<Quaternion>(L, 1, "Quat");
			Quaternion quaternion2 = *Luau.lua_class_get<Quaternion>(L, 2, "Quat");
			int num = ((quaternion == quaternion2) ? 1 : 0);
			Luau.lua_pushnumber(L, (double)num);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int FromEuler$BurstManaged(lua_State* L)
		{
			float num = (float)Luau.luaL_optnumber(L, 1, 0.0);
			float num2 = (float)Luau.luaL_optnumber(L, 2, 0.0);
			float num3 = (float)Luau.luaL_optnumber(L, 3, 0.0);
			Luau.lua_class_push<Quaternion>(L, "Quat")->eulerAngles = new Vector3(num, num2, num3);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int FromDirection$BurstManaged(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Luau.lua_class_push<Quaternion>(L, "Quat")->SetLookRotation(vector);
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int GetUpVector$BurstManaged(lua_State* L)
		{
			Quaternion quaternion = *Luau.lua_class_get<Quaternion>(L, 1, "Quat");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = quaternion * Vector3.up;
			return 1;
		}

		[BurstCompile]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe static int Euler$BurstManaged(lua_State* L)
		{
			Quaternion quaternion = *Luau.lua_class_get<Quaternion>(L, 1, "Quat");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = quaternion.eulerAngles;
			return 1;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int New_00004458$PostfixBurstDelegate(lua_State* L);

		internal static class New_00004458$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.QuatFunctions.New_00004458$BurstDirectCall.Pointer == 0)
				{
					Bindings.QuatFunctions.New_00004458$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.QuatFunctions.New_00004458$PostfixBurstDelegate>(new Bindings.QuatFunctions.New_00004458$PostfixBurstDelegate(Bindings.QuatFunctions.New)).Value;
				}
				A_0 = Bindings.QuatFunctions.New_00004458$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.QuatFunctions.New_00004458$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.QuatFunctions.New_00004458$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.QuatFunctions.New$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Mul_00004459$PostfixBurstDelegate(lua_State* L);

		internal static class Mul_00004459$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.QuatFunctions.Mul_00004459$BurstDirectCall.Pointer == 0)
				{
					Bindings.QuatFunctions.Mul_00004459$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.QuatFunctions.Mul_00004459$PostfixBurstDelegate>(new Bindings.QuatFunctions.Mul_00004459$PostfixBurstDelegate(Bindings.QuatFunctions.Mul)).Value;
				}
				A_0 = Bindings.QuatFunctions.Mul_00004459$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.QuatFunctions.Mul_00004459$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.QuatFunctions.Mul_00004459$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.QuatFunctions.Mul$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Eq_0000445A$PostfixBurstDelegate(lua_State* L);

		internal static class Eq_0000445A$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.QuatFunctions.Eq_0000445A$BurstDirectCall.Pointer == 0)
				{
					Bindings.QuatFunctions.Eq_0000445A$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.QuatFunctions.Eq_0000445A$PostfixBurstDelegate>(new Bindings.QuatFunctions.Eq_0000445A$PostfixBurstDelegate(Bindings.QuatFunctions.Eq)).Value;
				}
				A_0 = Bindings.QuatFunctions.Eq_0000445A$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.QuatFunctions.Eq_0000445A$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.QuatFunctions.Eq_0000445A$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.QuatFunctions.Eq$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int FromEuler_0000445C$PostfixBurstDelegate(lua_State* L);

		internal static class FromEuler_0000445C$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.QuatFunctions.FromEuler_0000445C$BurstDirectCall.Pointer == 0)
				{
					Bindings.QuatFunctions.FromEuler_0000445C$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.QuatFunctions.FromEuler_0000445C$PostfixBurstDelegate>(new Bindings.QuatFunctions.FromEuler_0000445C$PostfixBurstDelegate(Bindings.QuatFunctions.FromEuler)).Value;
				}
				A_0 = Bindings.QuatFunctions.FromEuler_0000445C$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.QuatFunctions.FromEuler_0000445C$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.QuatFunctions.FromEuler_0000445C$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.QuatFunctions.FromEuler$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int FromDirection_0000445D$PostfixBurstDelegate(lua_State* L);

		internal static class FromDirection_0000445D$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.QuatFunctions.FromDirection_0000445D$BurstDirectCall.Pointer == 0)
				{
					Bindings.QuatFunctions.FromDirection_0000445D$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.QuatFunctions.FromDirection_0000445D$PostfixBurstDelegate>(new Bindings.QuatFunctions.FromDirection_0000445D$PostfixBurstDelegate(Bindings.QuatFunctions.FromDirection)).Value;
				}
				A_0 = Bindings.QuatFunctions.FromDirection_0000445D$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.QuatFunctions.FromDirection_0000445D$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.QuatFunctions.FromDirection_0000445D$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.QuatFunctions.FromDirection$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int GetUpVector_0000445E$PostfixBurstDelegate(lua_State* L);

		internal static class GetUpVector_0000445E$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.QuatFunctions.GetUpVector_0000445E$BurstDirectCall.Pointer == 0)
				{
					Bindings.QuatFunctions.GetUpVector_0000445E$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.QuatFunctions.GetUpVector_0000445E$PostfixBurstDelegate>(new Bindings.QuatFunctions.GetUpVector_0000445E$PostfixBurstDelegate(Bindings.QuatFunctions.GetUpVector)).Value;
				}
				A_0 = Bindings.QuatFunctions.GetUpVector_0000445E$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.QuatFunctions.GetUpVector_0000445E$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.QuatFunctions.GetUpVector_0000445E$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.QuatFunctions.GetUpVector$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int Euler_0000445F$PostfixBurstDelegate(lua_State* L);

		internal static class Euler_0000445F$BurstDirectCall
		{
			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (Bindings.QuatFunctions.Euler_0000445F$BurstDirectCall.Pointer == 0)
				{
					Bindings.QuatFunctions.Euler_0000445F$BurstDirectCall.Pointer = BurstCompiler.CompileFunctionPointer<Bindings.QuatFunctions.Euler_0000445F$PostfixBurstDelegate>(new Bindings.QuatFunctions.Euler_0000445F$PostfixBurstDelegate(Bindings.QuatFunctions.Euler)).Value;
				}
				A_0 = Bindings.QuatFunctions.Euler_0000445F$BurstDirectCall.Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				Bindings.QuatFunctions.Euler_0000445F$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public unsafe static int Invoke(lua_State* L)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = Bindings.QuatFunctions.Euler_0000445F$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(lua_State*), L, functionPointer);
					}
				}
				return Bindings.QuatFunctions.Euler$BurstManaged(L);
			}

			private static IntPtr Pointer;
		}
	}

	public struct GorillaLocomotionSettings
	{
		public float velocityLimit;

		public float slideVelocityLimit;

		public float maxJumpSpeed;

		public float jumpMultiplier;
	}

	[BurstCompile]
	public struct PlayerInput
	{
		public float leftXAxis;

		[MarshalAs(UnmanagedType.U1)]
		public bool leftPrimaryButton;

		public float rightXAxis;

		[MarshalAs(UnmanagedType.U1)]
		public bool rightPrimaryButton;

		public float leftYAxis;

		[MarshalAs(UnmanagedType.U1)]
		public bool leftSecondaryButton;

		public float rightYAxis;

		[MarshalAs(UnmanagedType.U1)]
		public bool rightSecondaryButton;

		public float leftTrigger;

		public float rightTrigger;

		public float leftGrip;

		public float rightGrip;
	}

	public static class JSON
	{
		public unsafe static Dictionary<object, object> ConsumeTable(lua_State* L, int tableIndex)
		{
			Dictionary<object, object> dictionary = new Dictionary<object, object>();
			Luau.lua_pushnil(L);
			if (tableIndex < 0)
			{
				tableIndex--;
			}
			while (Luau.lua_next(L, tableIndex) != 0)
			{
				Luau.lua_Types lua_Types = (Luau.lua_Types)Luau.lua_type(L, -1);
				Luau.lua_Types lua_Types2 = (Luau.lua_Types)Luau.lua_type(L, -2);
				object obj;
				if (lua_Types2 == Luau.lua_Types.LUA_TSTRING)
				{
					obj = new string(Luau.lua_tostring(L, -2));
				}
				else
				{
					if (lua_Types2 != Luau.lua_Types.LUA_TNUMBER)
					{
						FixedString64Bytes fixedString64Bytes = "Invalid key in table, key must be a string or a number";
						Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString64Bytes>(ref fixedString64Bytes) + 2));
						return null;
					}
					obj = Luau.lua_tonumber(L, -2);
				}
				switch (lua_Types)
				{
				case Luau.lua_Types.LUA_TBOOLEAN:
					dictionary.Add(obj, Luau.lua_toboolean(L, -1) == 1);
					Luau.lua_pop(L, 1);
					continue;
				case Luau.lua_Types.LUA_TNUMBER:
					dictionary.Add(obj, Luau.luaL_checknumber(L, -1));
					Luau.lua_pop(L, 1);
					continue;
				case Luau.lua_Types.LUA_TSTRING:
					dictionary.Add(obj, new string(Luau.lua_tostring(L, -1)));
					Luau.lua_pop(L, 1);
					continue;
				case Luau.lua_Types.LUA_TTABLE:
				case Luau.lua_Types.LUA_TUSERDATA:
					if (Luau.luaL_getmetafield(L, -1, "metahash") == 1)
					{
						BurstClassInfo.ClassInfo classInfo;
						if (!BurstClassInfo.ClassList.InfoFields.Data.TryGetValue((int)Luau.luaL_checknumber(L, -1), out classInfo))
						{
							FixedString64Bytes fixedString64Bytes2 = "\"Internal Class Info Error No Metatable Found\"";
							Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString64Bytes>(ref fixedString64Bytes2) + 2));
							return null;
						}
						Luau.lua_pop(L, 1);
						FixedString32Bytes fixedString32Bytes = "Vec3";
						if ((in classInfo.Name) == (in fixedString32Bytes))
						{
							dictionary.Add(obj, *Luau.lua_class_get<Vector3>(L, -1));
							Luau.lua_pop(L, 1);
							continue;
						}
						fixedString32Bytes = "Quat";
						if ((in classInfo.Name) == (in fixedString32Bytes))
						{
							dictionary.Add(obj, *Luau.lua_class_get<Quaternion>(L, -1));
							Luau.lua_pop(L, 1);
							continue;
						}
						FixedString32Bytes fixedString32Bytes2 = "Invalid type in table";
						Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes2) + 2));
						return null;
					}
					else
					{
						object obj2 = Bindings.JSON.ConsumeTable(L, -1);
						Luau.lua_pop(L, 1);
						if (obj2 != null)
						{
							dictionary.Add(obj, obj2);
							continue;
						}
						return null;
					}
					break;
				}
				FixedString32Bytes fixedString32Bytes3 = "Unknown type in table";
				Luau.luaL_errorL(L, (sbyte*)((byte*)UnsafeUtility.AddressOf<FixedString32Bytes>(ref fixedString32Bytes3) + 2));
				return null;
			}
			return dictionary;
		}

		private static int ParseStrictInt(string input)
		{
			if (string.IsNullOrEmpty(input) || input != input.Trim())
			{
				return -1;
			}
			int num;
			if (!int.TryParse(input, out num))
			{
				return -1;
			}
			return num;
		}

		private static bool CompareKeys(JObject obj, HashSet<string> set)
		{
			HashSet<string> hashSet = new HashSet<string>(from p in obj.Properties()
				select p.Name);
			return set.SetEquals(hashSet);
		}

		public unsafe static bool PushTable(lua_State* L, JObject table)
		{
			Luau.lua_createtable(L, 0, 0);
			foreach (KeyValuePair<string, JToken> keyValuePair in table)
			{
				if (keyValuePair.Key != null && keyValuePair.Value != null)
				{
					int num = Bindings.JSON.ParseStrictInt(keyValuePair.Key);
					if (num == -1)
					{
						Luau.lua_pushstring(L, keyValuePair.Key);
					}
					if (keyValuePair.Value is JObject)
					{
						if (Bindings.JSON.CompareKeys((JObject)keyValuePair.Value, new HashSet<string> { "x", "y", "z" }))
						{
							JObject jobject = keyValuePair.Value as JObject;
							float num2 = jobject["x"].ToObject<float>();
							float num3 = jobject["y"].ToObject<float>();
							float num4 = jobject["z"].ToObject<float>();
							Vector3 vector = new Vector3(num2, num3, num4);
							*Luau.lua_class_push<Vector3>(L) = vector;
						}
						else if (Bindings.JSON.CompareKeys((JObject)keyValuePair.Value, new HashSet<string> { "x", "y", "z", "w" }))
						{
							JObject jobject2 = keyValuePair.Value as JObject;
							float num5 = jobject2["x"].ToObject<float>();
							float num6 = jobject2["y"].ToObject<float>();
							float num7 = jobject2["z"].ToObject<float>();
							float num8 = jobject2["w"].ToObject<float>();
							Quaternion quaternion = new Quaternion(num5, num6, num7, num8);
							*Luau.lua_class_push<Quaternion>(L) = quaternion;
						}
						else
						{
							Bindings.JSON.PushTable(L, (JObject)keyValuePair.Value);
						}
					}
					else if (keyValuePair.Value is JValue)
					{
						JTokenType type = keyValuePair.Value.Type;
						if (type == JTokenType.Integer)
						{
							Luau.lua_pushnumber(L, (double)keyValuePair.Value.ToObject<int>());
						}
						else if (type == JTokenType.Boolean)
						{
							Luau.lua_pushboolean(L, keyValuePair.Value.ToObject<bool>() ? 1 : 0);
						}
						else if (type == JTokenType.Float)
						{
							Luau.lua_pushnumber(L, keyValuePair.Value.ToObject<double>());
						}
						else
						{
							if (type != JTokenType.String)
							{
								continue;
							}
							Luau.lua_pushstring(L, keyValuePair.Value.ToString());
						}
					}
					if (num == -1)
					{
						Luau.lua_rawset(L, -3);
					}
					else
					{
						Luau.lua_rawseti(L, -2, num);
					}
				}
			}
			return true;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int DataSave(lua_State* L)
		{
			int num;
			try
			{
				string text = JsonConvert.SerializeObject(Bindings.JSON.ConsumeTable(L, 1), Formatting.Indented);
				if (text.Length > 10000)
				{
					Luau.luaL_errorL(L, "Save exceeds 10000 bytes", Array.Empty<string>());
					num = 0;
				}
				else
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(Path.Join(Bindings.JSON.ModIODirectory, "saves", CustomMapLoader.LoadedMapModId.ToString()));
					if (!directoryInfo.Exists)
					{
						directoryInfo.Create();
					}
					File.WriteAllText(Path.Join(directoryInfo.FullName, "luau.json"), text);
					num = 0;
				}
			}
			catch
			{
				Luau.luaL_errorL(L, "Argument 2 must be a table", Array.Empty<string>());
				num = 0;
			}
			return num;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int DataLoad(lua_State* L)
		{
			int num;
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(Path.Join(Bindings.JSON.ModIODirectory, "saves", CustomMapLoader.LoadedMapModId.ToString()));
				if (!directoryInfo.Exists)
				{
					Luau.lua_createtable(L, 0, 0);
					num = 1;
				}
				else
				{
					FileInfo[] files = directoryInfo.GetFiles("luau.json");
					if (files.Length == 0)
					{
						Luau.lua_createtable(L, 0, 0);
						num = 1;
					}
					else
					{
						JObject jobject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(files[0].FullName));
						if (Bindings.JSON.PushTable(L, jobject))
						{
							num = 1;
						}
						else
						{
							num = 0;
						}
					}
				}
			}
			catch
			{
				Luau.luaL_errorL(L, "Error while loading data", Array.Empty<string>());
				num = 0;
			}
			return num;
		}

		private static string ModIODirectory = Path.Join(Path.Join(Application.persistentDataPath, "mod.io", "06657"), "data");
	}

	[BurstCompile]
	public struct LuauRoomState
	{
		[MarshalAs(UnmanagedType.U1)]
		public bool IsQuest;

		public float FPS;

		[MarshalAs(UnmanagedType.U1)]
		public bool IsPrivate;

		public FixedString32Bytes RoomCode;
	}

	public static class PlayerUtils
	{
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int TeleportPlayer(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			bool flag = Luau.lua_toboolean(L, 2) == 1;
			if (GTPlayer.hasInstance)
			{
				GTPlayer instance = GTPlayer.Instance;
				Vector3 position = instance.transform.position;
				Vector3 vector2 = instance.mainCamera.transform.position - position;
				Vector3 vector3 = vector - vector2;
				instance.TeleportTo(vector3, instance.transform.rotation, flag, false);
			}
			return 0;
		}

		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int SetVelocity(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1);
			if (GTPlayer.hasInstance)
			{
				GTPlayer.Instance.SetVelocity(vector);
			}
			return 0;
		}
	}

	public static class RayCastUtils
	{
		[MonoPInvokeCallback(typeof(lua_CFunction))]
		public unsafe static int RayCast(lua_State* L)
		{
			Vector3 vector = *Luau.lua_class_get<Vector3>(L, 1, "Vec3");
			Vector3 vector2 = *Luau.lua_class_get<Vector3>(L, 2, "Vec3");
			if (!Physics.Raycast(vector, vector2, out Bindings.RayCastUtils.rayHit))
			{
				return 0;
			}
			Luau.lua_createtable(L, 0, 0);
			Luau.lua_pushstring(L, "distance");
			Luau.lua_pushnumber(L, (double)Bindings.RayCastUtils.rayHit.distance);
			Luau.lua_rawset(L, -3);
			Luau.lua_pushstring(L, "point");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = Bindings.RayCastUtils.rayHit.point;
			Luau.lua_rawset(L, -3);
			Luau.lua_pushstring(L, "normal");
			*Luau.lua_class_push<Vector3>(L, "Vec3") = Bindings.RayCastUtils.rayHit.normal;
			Luau.lua_rawset(L, -3);
			Luau.lua_pushstring(L, "object");
			IntPtr intPtr;
			if (Bindings.LuauGameObjectList.TryGetValue(Bindings.RayCastUtils.rayHit.transform.gameObject, out intPtr))
			{
				Luau.lua_class_push(L, "GameObject", intPtr);
			}
			else
			{
				Luau.lua_pushnil(L);
			}
			Luau.lua_rawset(L, -3);
			Luau.lua_pushstring(L, "player");
			Collider collider = Bindings.RayCastUtils.rayHit.collider;
			VRRig vrrig = ((collider != null) ? collider.GetComponentInParent<VRRig>() : null);
			if (vrrig != null)
			{
				NetPlayer creator = vrrig.creator;
				if (creator != null)
				{
					IntPtr intPtr2;
					if (Bindings.LuauPlayerList.TryGetValue(creator.ActorNumber, out intPtr2))
					{
						Luau.lua_class_push(L, "Player", intPtr2);
					}
					else
					{
						Luau.lua_pushnil(L);
					}
				}
				else
				{
					Luau.lua_pushnil(L);
				}
			}
			else
			{
				Luau.lua_pushnil(L);
			}
			Luau.lua_rawset(L, -3);
			return 1;
		}

		public static RaycastHit rayHit;
	}

	public static class Components
	{
		public unsafe static void Build(lua_State* L)
		{
			Bindings.Components.LuauParticleSystemBindings.Builder(L);
			Bindings.Components.LuauAudioSourceBindings.Builder(L);
			Bindings.Components.LuauLightBindings.Builder(L);
			Bindings.Components.LuauAnimatorBindings.Builder(L);
		}

		public static Dictionary<IntPtr, object> ComponentList = new Dictionary<IntPtr, object>();

		public static class LuauParticleSystemBindings
		{
			public unsafe static void Builder(lua_State* L)
			{
				LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.Components.LuauParticleSystemBindings.LuauParticleSystem>("ParticleSystem").AddFunction("play", new lua_CFunction(Bindings.Components.LuauParticleSystemBindings.play)).AddFunction("stop", new lua_CFunction(Bindings.Components.LuauParticleSystemBindings.stop)).AddFunction("clear", new lua_CFunction(Bindings.Components.LuauParticleSystemBindings.clear))
					.Build(L, false));
			}

			public unsafe static ParticleSystem GetParticleSystem(lua_State* L)
			{
				Bindings.Components.LuauParticleSystemBindings.LuauParticleSystem* ptr = Luau.lua_class_get<Bindings.Components.LuauParticleSystemBindings.LuauParticleSystem>(L, 1);
				object obj;
				if (Bindings.Components.ComponentList.TryGetValue((IntPtr)((void*)ptr), out obj))
				{
					ParticleSystem particleSystem = obj as ParticleSystem;
					if (particleSystem != null)
					{
						return particleSystem;
					}
				}
				return null;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int play(lua_State* L)
			{
				ParticleSystem particleSystem = Bindings.Components.LuauParticleSystemBindings.GetParticleSystem(L);
				if (particleSystem != null)
				{
					particleSystem.Play();
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int stop(lua_State* L)
			{
				ParticleSystem particleSystem = Bindings.Components.LuauParticleSystemBindings.GetParticleSystem(L);
				if (particleSystem != null)
				{
					particleSystem.Stop();
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int clear(lua_State* L)
			{
				ParticleSystem particleSystem = Bindings.Components.LuauParticleSystemBindings.GetParticleSystem(L);
				if (particleSystem != null)
				{
					particleSystem.Clear();
				}
				return 0;
			}

			public struct LuauParticleSystem
			{
				public int x;
			}
		}

		public static class LuauAudioSourceBindings
		{
			public unsafe static void Builder(lua_State* L)
			{
				LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.Components.LuauAudioSourceBindings.LuauAudioSource>("AudioSource").AddFunction("play", new lua_CFunction(Bindings.Components.LuauAudioSourceBindings.play)).AddFunction("setVolume", new lua_CFunction(Bindings.Components.LuauAudioSourceBindings.setVolume)).AddFunction("setLoop", new lua_CFunction(Bindings.Components.LuauAudioSourceBindings.setLoop))
					.AddFunction("setPitch", new lua_CFunction(Bindings.Components.LuauAudioSourceBindings.setPitch))
					.AddFunction("setMinDistance", new lua_CFunction(Bindings.Components.LuauAudioSourceBindings.setMinDistance))
					.AddFunction("setMaxDistance", new lua_CFunction(Bindings.Components.LuauAudioSourceBindings.setMaxDistance))
					.Build(L, false));
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static AudioSource GetAudioSource(lua_State* L)
			{
				Bindings.Components.LuauAudioSourceBindings.LuauAudioSource* ptr = Luau.lua_class_get<Bindings.Components.LuauAudioSourceBindings.LuauAudioSource>(L, 1);
				object obj;
				if (Bindings.Components.ComponentList.TryGetValue((IntPtr)((void*)ptr), out obj))
				{
					AudioSource audioSource = obj as AudioSource;
					if (audioSource != null)
					{
						return audioSource;
					}
				}
				return null;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int play(lua_State* L)
			{
				AudioSource audioSource = Bindings.Components.LuauAudioSourceBindings.GetAudioSource(L);
				if (audioSource != null)
				{
					audioSource.Play();
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int setVolume(lua_State* L)
			{
				AudioSource audioSource = Bindings.Components.LuauAudioSourceBindings.GetAudioSource(L);
				double num = Luau.luaL_checknumber(L, 2);
				if (audioSource != null)
				{
					audioSource.volume = (float)num;
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int setLoop(lua_State* L)
			{
				AudioSource audioSource = Bindings.Components.LuauAudioSourceBindings.GetAudioSource(L);
				bool flag = Luau.lua_toboolean(L, 2) == 1;
				if (audioSource != null)
				{
					audioSource.loop = flag;
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int setPitch(lua_State* L)
			{
				AudioSource audioSource = Bindings.Components.LuauAudioSourceBindings.GetAudioSource(L);
				double num = Luau.luaL_checknumber(L, 2);
				if (audioSource != null)
				{
					audioSource.pitch = (float)num;
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int setMinDistance(lua_State* L)
			{
				AudioSource audioSource = Bindings.Components.LuauAudioSourceBindings.GetAudioSource(L);
				double num = Luau.luaL_checknumber(L, 2);
				if (audioSource != null)
				{
					audioSource.minDistance = (float)num;
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int setMaxDistance(lua_State* L)
			{
				AudioSource audioSource = Bindings.Components.LuauAudioSourceBindings.GetAudioSource(L);
				double num = Luau.luaL_checknumber(L, 2);
				if (audioSource != null)
				{
					audioSource.maxDistance = (float)num;
				}
				return 0;
			}

			public struct LuauAudioSource
			{
				public int x;
			}
		}

		public static class LuauLightBindings
		{
			public unsafe static void Builder(lua_State* L)
			{
				LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.Components.LuauLightBindings.LuauLight>("Light").AddFunction("setColor", new lua_CFunction(Bindings.Components.LuauLightBindings.setColor)).AddFunction("setIntensity", new lua_CFunction(Bindings.Components.LuauLightBindings.setIntensity)).AddFunction("setRange", new lua_CFunction(Bindings.Components.LuauLightBindings.setRange))
					.Build(L, false));
			}

			public unsafe static Light GetLight(lua_State* L)
			{
				Bindings.Components.LuauLightBindings.LuauLight* ptr = Luau.lua_class_get<Bindings.Components.LuauLightBindings.LuauLight>(L, 1);
				object obj;
				if (Bindings.Components.ComponentList.TryGetValue((IntPtr)((void*)ptr), out obj))
				{
					Light light = obj as Light;
					if (light != null)
					{
						return light;
					}
				}
				return null;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int setColor(lua_State* L)
			{
				Light light = Bindings.Components.LuauLightBindings.GetLight(L);
				Vector3 vector = *Luau.lua_class_get<Vector3>(L, 2);
				if (light != null)
				{
					light.color = new Color(vector.x, vector.y, vector.z);
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int setIntensity(lua_State* L)
			{
				Light light = Bindings.Components.LuauLightBindings.GetLight(L);
				double num = Luau.luaL_checknumber(L, 2);
				if (light != null)
				{
					light.intensity = (float)num;
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int setRange(lua_State* L)
			{
				Light light = Bindings.Components.LuauLightBindings.GetLight(L);
				double num = Luau.luaL_checknumber(L, 2);
				if (light != null)
				{
					light.range = (float)num;
				}
				return 0;
			}

			public struct LuauLight
			{
				public int x;
			}
		}

		public static class LuauAnimatorBindings
		{
			public unsafe static void Builder(lua_State* L)
			{
				LuauVm.ClassBuilders.Append(new LuauClassBuilder<Bindings.Components.LuauAnimatorBindings.LuauAnimator>("Animator").AddFunction("setSpeed", new lua_CFunction(Bindings.Components.LuauAnimatorBindings.setSpeed)).AddFunction("startPlayback", new lua_CFunction(Bindings.Components.LuauAnimatorBindings.startPlayback)).AddFunction("stopPlayback", new lua_CFunction(Bindings.Components.LuauAnimatorBindings.stopPlayback))
					.AddFunction("reset", new lua_CFunction(Bindings.Components.LuauAnimatorBindings.reset))
					.Build(L, false));
			}

			public unsafe static Animator GetAnimator(lua_State* L)
			{
				Bindings.Components.LuauAnimatorBindings.LuauAnimator* ptr = Luau.lua_class_get<Bindings.Components.LuauAnimatorBindings.LuauAnimator>(L, 1);
				object obj;
				if (Bindings.Components.ComponentList.TryGetValue((IntPtr)((void*)ptr), out obj))
				{
					Animator animator = obj as Animator;
					if (animator != null)
					{
						return animator;
					}
				}
				return null;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int setSpeed(lua_State* L)
			{
				Animator animator = Bindings.Components.LuauAnimatorBindings.GetAnimator(L);
				double num = Luau.luaL_checknumber(L, 2);
				if (animator != null)
				{
					animator.speed = (float)num;
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int startPlayback(lua_State* L)
			{
				Animator animator = Bindings.Components.LuauAnimatorBindings.GetAnimator(L);
				if (animator != null)
				{
					animator.StartPlayback();
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int stopPlayback(lua_State* L)
			{
				Animator animator = Bindings.Components.LuauAnimatorBindings.GetAnimator(L);
				if (animator != null)
				{
					animator.StopPlayback();
				}
				return 0;
			}

			[MonoPInvokeCallback(typeof(lua_CFunction))]
			public unsafe static int reset(lua_State* L)
			{
				Animator animator = Bindings.Components.LuauAnimatorBindings.GetAnimator(L);
				if (animator != null)
				{
					animator.ResetToEntryState();
				}
				return 0;
			}

			public struct LuauAnimator
			{
				public int x;
			}
		}
	}
}

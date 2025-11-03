using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using GorillaExtensions;
using GorillaGameModes;
using GT_CustomMapSupportRuntime;
using Photon.Pun;
using Photon.Realtime;
using Unity.Collections;
using UnityEngine;

public class LuauVm : MonoBehaviourPunCallbacks, IOnEventCallback
{
	private void LateUpdate()
	{
		foreach (LuauScriptRunner luauScriptRunner in LuauScriptRunner.ScriptRunners)
		{
			if (!luauScriptRunner.Tick(Time.deltaTime))
			{
				LuauHud.Instance.LuauLog(luauScriptRunner.ScriptName + " errored out");
				LuauScriptRunner.ScriptRunners.Remove(luauScriptRunner);
				break;
			}
		}
	}

	private void Start()
	{
	}

	private void Awake()
	{
	}

	public void OnEvent(EventData eventData)
	{
		if (eventData.Code != 180)
		{
			return;
		}
		float num = 0f;
		LuauVm.callTimers.TryGetValue(eventData.Sender, out num);
		if (num < Time.time - 1f)
		{
			num = Time.time - 1f;
		}
		num += 1f / LuauVm.callCount;
		LuauVm.callTimers[eventData.Sender] = num;
		if (num > Time.time)
		{
			return;
		}
		object[] array = new object[]
		{
			NetworkSystem.Instance.GetPlayer(eventData.Sender),
			(object[])eventData.CustomData
		};
		if (array.Length > 20)
		{
			return;
		}
		LuauVm.eventQueue.Enqueue(array);
	}

	public unsafe static int SendEvent(lua_State* L, object[] args, bool useTable = true)
	{
		try
		{
			NetPlayer netPlayer = null;
			if (args[0] is NetPlayer)
			{
				netPlayer = (NetPlayer)args[0];
				args = (object[])args[1];
			}
			if (GorillaGameManager.instance.GameType() != GameModeType.Custom)
			{
				return -1;
			}
			Luau.lua_getfield(L, -10002, "onEvent");
			if (Luau.lua_type(L, -1) != 7)
			{
				Luau.lua_pop(L, 1);
				return 0;
			}
			string text = args[0] as string;
			if (text == null)
			{
				Luau.lua_pop(L, 1);
				return 0;
			}
			if (string.IsNullOrEmpty(text))
			{
				Luau.lua_pop(L, 1);
				return 0;
			}
			if (text.Length > 30)
			{
				Luau.lua_pop(L, 1);
				return 0;
			}
			Luau.lua_pushstring(L, (string)args[0]);
			if (useTable)
			{
				Luau.lua_createtable(L, args.Length, 0);
			}
			int i = 1;
			while (i < args.Length)
			{
				object obj = args[i];
				if (obj.IsType<double>())
				{
					if (double.IsFinite((double)obj))
					{
						Luau.lua_pushnumber(L, (double)obj);
						goto IL_0399;
					}
				}
				else
				{
					if (obj.IsType<bool>())
					{
						Luau.lua_pushboolean(L, (int)obj);
						goto IL_0399;
					}
					if (obj.IsType<Vector3>())
					{
						Vector3 vector = (Vector3)obj;
						vector.ClampMagnitudeSafe(10000000f);
						*Luau.lua_class_push<Vector3>(L, "Vec3") = vector;
						goto IL_0399;
					}
					if (obj.IsType<Quaternion>())
					{
						Quaternion quaternion = (Quaternion)obj;
						if (float.IsFinite(quaternion.x) && float.IsFinite(quaternion.y) && float.IsFinite(quaternion.z) && float.IsFinite(quaternion.w))
						{
							*Luau.lua_class_push<Quaternion>(L, "Quat") = quaternion;
							goto IL_0399;
						}
					}
					else if (obj.IsType<Player>())
					{
						int actorNumber = ((Player)obj).ActorNumber;
						IntPtr intPtr;
						if (Bindings.LuauPlayerList.TryGetValue(actorNumber, out intPtr))
						{
							Luau.lua_class_push(L, "Player", intPtr);
							goto IL_0399;
						}
						NetPlayer netPlayer2 = (NetPlayer)obj;
						if (netPlayer2 == null)
						{
							Luau.lua_pushnil(L);
							goto IL_0399;
						}
						Bindings.LuauPlayer* ptr = Luau.lua_class_push<Bindings.LuauPlayer>(L);
						ptr->PlayerID = netPlayer2.ActorNumber;
						ptr->PlayerName = netPlayer2.SanitizedNickName;
						ptr->PlayerMaterial = 0;
						ptr->IsMasterClient = netPlayer2.IsMasterClient;
						RigContainer rigContainer;
						VRRigCache.Instance.TryGetVrrig(netPlayer2, out rigContainer);
						VRRig rig = rigContainer.Rig;
						Bindings.LuauVRRigList[netPlayer2.ActorNumber] = rig;
						Bindings.PlayerFunctions.UpdatePlayer(L, rig, ptr);
						Bindings.LuauPlayerList[netPlayer2.ActorNumber] = (IntPtr)((void*)ptr);
						goto IL_0399;
					}
					else if (obj.IsType<Bindings.LuauAIAgent>())
					{
						int entityID = ((Bindings.LuauAIAgent)obj).EntityID;
						IntPtr intPtr2;
						if (Bindings.LuauAIAgentList.TryGetValue(entityID, out intPtr2))
						{
							Luau.lua_class_push(L, "AIAgent", intPtr2);
							goto IL_0399;
						}
						bool flag = false;
						if (Bindings.LuauAIAgentList.Count + Bindings.LuauGrabbablesList.Count == Constants.aiAgentLimit)
						{
							Debug.Log("[LuauVM::OnEvent] Custom Map AI Agent limit has already been reached!");
						}
						else
						{
							GameEntityManager entityManager = CustomMapsGameManager.GetEntityManager();
							if (entityManager.IsNotNull())
							{
								GameEntityId entityIdFromNetId = entityManager.GetEntityIdFromNetId(entityID);
								GameEntity gameEntity = entityManager.GetGameEntity(entityIdFromNetId);
								if (gameEntity.IsNotNull() && gameEntity.gameObject.IsNotNull() && gameEntity.gameObject.GetComponent<GameAgent>() != null)
								{
									Bindings.LuauAIAgent* ptr2 = Luau.lua_class_push<Bindings.LuauAIAgent>(L);
									Bindings.AIAgentFunctions.UpdateEntity(gameEntity, ptr2);
									Bindings.LuauAIAgentList[entityID] = (IntPtr)((void*)ptr2);
									flag = true;
								}
							}
						}
						if (!flag)
						{
							Luau.lua_pushnil(L);
							goto IL_0399;
						}
						goto IL_0399;
					}
					else
					{
						if (obj == null)
						{
							Luau.lua_pushnil(L);
							goto IL_0399;
						}
						goto IL_0399;
					}
				}
				IL_03A5:
				i++;
				continue;
				IL_0399:
				if (useTable)
				{
					Luau.lua_rawseti(L, -2, i);
					goto IL_03A5;
				}
				goto IL_03A5;
			}
			if (netPlayer != null)
			{
				int actorNumber2 = netPlayer.ActorNumber;
				IntPtr intPtr3;
				if (Bindings.LuauPlayerList.TryGetValue(actorNumber2, out intPtr3))
				{
					Luau.lua_class_push(L, "Player", intPtr3);
				}
				else
				{
					NetPlayer netPlayer3 = netPlayer;
					if (netPlayer3 == null)
					{
						Luau.lua_pushnil(L);
					}
					else
					{
						Bindings.LuauPlayer* ptr3 = Luau.lua_class_push<Bindings.LuauPlayer>(L);
						ptr3->PlayerID = netPlayer3.ActorNumber;
						ptr3->PlayerName = netPlayer3.SanitizedNickName;
						ptr3->PlayerMaterial = 0;
						ptr3->IsMasterClient = netPlayer3.IsMasterClient;
						RigContainer rigContainer2;
						VRRigCache.Instance.TryGetVrrig(netPlayer3, out rigContainer2);
						VRRig rig2 = rigContainer2.Rig;
						Bindings.LuauVRRigList[netPlayer3.ActorNumber] = rig2;
						Bindings.PlayerFunctions.UpdatePlayer(L, rig2, ptr3);
						Bindings.LuauPlayerList[netPlayer3.ActorNumber] = (IntPtr)((void*)ptr3);
					}
				}
				return Luau.lua_pcall(L, 3, 0, 0);
			}
			return Luau.lua_pcall(L, 2, 0, 0);
		}
		catch (Exception)
		{
		}
		return 0;
	}

	public static void ProcessEvents()
	{
		while (LuauVm.eventQueue.Count > 0)
		{
			object[] array = LuauVm.eventQueue.Dequeue();
			foreach (LuauScriptRunner luauScriptRunner in LuauScriptRunner.ScriptRunners)
			{
				if (luauScriptRunner.ShouldTick)
				{
					int num = LuauVm.SendEvent(luauScriptRunner.L, array, true);
					luauScriptRunner.ShouldTick = !LuauScriptRunner.ErrorCheck(luauScriptRunner.L, num);
				}
			}
		}
		while (LuauVm.localEventQueue.Count > 0)
		{
			object[] array2 = LuauVm.localEventQueue.Dequeue();
			foreach (LuauScriptRunner luauScriptRunner2 in LuauScriptRunner.ScriptRunners)
			{
				if (luauScriptRunner2.ShouldTick)
				{
					int num2 = LuauVm.SendEvent(luauScriptRunner2.L, array2, false);
					luauScriptRunner2.ShouldTick = !LuauScriptRunner.ErrorCheck(luauScriptRunner2.L, num2);
				}
			}
		}
		while (LuauVm.touchEventsQueue.Count > 0)
		{
			GameObject gameObject = LuauVm.touchEventsQueue.Dequeue();
			foreach (LuauScriptRunner luauScriptRunner3 in LuauScriptRunner.ScriptRunners)
			{
				int num3;
				if (luauScriptRunner3.ShouldTick && Bindings.LuauTriggerCallbacks.TryGetValue(gameObject, out num3))
				{
					Luau.lua_getref(luauScriptRunner3.L, num3);
					if (Luau.lua_type(luauScriptRunner3.L, -1) == 7)
					{
						int num4 = Luau.lua_pcall(luauScriptRunner3.L, 0, 0, 0);
						luauScriptRunner3.ShouldTick = !LuauScriptRunner.ErrorCheck(luauScriptRunner3.L, num4);
					}
				}
			}
		}
	}

	protected override void Finalize()
	{
		try
		{
			foreach (GCHandle gchandle in LuauVm.Handles)
			{
				gchandle.Free();
			}
			if (BurstClassInfo.ClassList.InfoFields.Data.IsCreated)
			{
				foreach (KVPair<int, BurstClassInfo.ClassInfo> kvpair in BurstClassInfo.ClassList.InfoFields.Data)
				{
					if (kvpair.Value.FieldList.IsCreated)
					{
						kvpair.Value.FieldList.Dispose();
					}
				}
				BurstClassInfo.ClassList.InfoFields.Data.Dispose();
			}
		}
		catch (ObjectDisposedException ex)
		{
			Debug.Log(ex);
		}
		finally
		{
			base.Finalize();
		}
	}

	public static List<object> ClassBuilders = new List<object>();

	public static List<GCHandle> Handles = new List<GCHandle>();

	private static Dictionary<int, float> callTimers = new Dictionary<int, float>();

	private static float callCount = 25f;

	public static Queue<object[]> eventQueue = new Queue<object[]>();

	public static Queue<object[]> localEventQueue = new Queue<object[]>();

	public static Queue<GameObject> touchEventsQueue = new Queue<GameObject>();
}

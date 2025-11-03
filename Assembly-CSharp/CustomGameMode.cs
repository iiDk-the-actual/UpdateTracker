using System;
using System.Collections.Generic;
using System.IO;
using AOT;
using Fusion;
using GorillaExtensions;
using GorillaGameModes;
using GorillaNetworking;
using GT_CustomMapSupportRuntime;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public sealed class CustomGameMode : GorillaGameManager
{
	public override void OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public override void OnSerializeRead(object obj)
	{
	}

	public override void OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public override object OnSerializeWrite()
	{
		return null;
	}

	public override void AddFusionDataBehaviour(NetworkObject obj)
	{
	}

	public override GameModeType GameType()
	{
		return GameModeType.Custom;
	}

	public unsafe override int MyMatIndex(NetPlayer forPlayer)
	{
		if (CustomGameMode.gameScriptRunner == null)
		{
			return 0;
		}
		if (!CustomGameMode.gameScriptRunner.ShouldTick)
		{
			return 0;
		}
		IntPtr intPtr;
		if (Bindings.LuauPlayerList.TryGetValue(forPlayer.ActorNumber, out intPtr))
		{
			return ((Bindings.LuauPlayer*)(void*)intPtr)->PlayerMaterial;
		}
		return 0;
	}

	public unsafe override void OnPlayerEnteredRoom(NetPlayer player)
	{
		try
		{
			if (CustomGameMode.gameScriptRunner != null)
			{
				if (CustomGameMode.gameScriptRunner.ShouldTick)
				{
					if (!Bindings.LuauPlayerList.ContainsKey(player.ActorNumber))
					{
						lua_State* l = CustomGameMode.gameScriptRunner.L;
						Luau.lua_getglobal(l, "Players");
						int num = Luau.lua_objlen(l, -1);
						Bindings.LuauPlayer* ptr = Luau.lua_class_push<Bindings.LuauPlayer>(l);
						ptr->PlayerID = player.ActorNumber;
						ptr->PlayerMaterial = 0;
						ptr->IsMasterClient = player.IsMasterClient;
						VRRig vrrig = this.FindPlayerVRRig(player);
						ptr->PlayerName = vrrig.playerNameVisible;
						Bindings.LuauVRRigList[player.ActorNumber] = vrrig;
						Bindings.PlayerFunctions.UpdatePlayer(l, vrrig, ptr);
						Bindings.LuauPlayerList[player.ActorNumber] = (IntPtr)((void*)ptr);
						Luau.lua_rawseti(CustomGameMode.gameScriptRunner.L, -2, num + 1);
						ptr->PlayerName = vrrig.playerNameVisible;
						if (player.IsLocal)
						{
							ptr->IsPCVR = PlayFabAuthenticator.instance.platform.ToString() != "Quest";
							Luau.lua_rawgeti(l, -1, num + 1);
							Luau.lua_setglobal(l, "LocalPlayer");
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	public unsafe override void OnPlayerLeftRoom(NetPlayer player)
	{
		try
		{
			if (CustomGameMode.gameScriptRunner != null)
			{
				if (CustomGameMode.gameScriptRunner.ShouldTick)
				{
					lua_State* l = CustomGameMode.gameScriptRunner.L;
					Bindings.LuauPlayerList.Remove(player.ActorNumber);
					Luau.lua_getglobal(l, "Players");
					int num = Luau.lua_objlen(l, -1);
					for (int i = 1; i <= num; i++)
					{
						Luau.lua_rawgeti(l, -1, i);
						Bindings.LuauPlayer* ptr = (Bindings.LuauPlayer*)Luau.lua_touserdata(l, -1);
						Luau.lua_pop(l, 1);
						if (ptr != null && ptr->PlayerID == player.ActorNumber)
						{
							for (int j = i; j < num; j++)
							{
								Luau.lua_rawgeti(l, -1, j + 1);
								Luau.lua_rawseti(l, -2, j);
							}
							Luau.lua_pushnil(l);
							Luau.lua_rawseti(l, -2, num);
							break;
						}
					}
					Luau.lua_pop(l, 1);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	public unsafe override void OnMasterClientSwitched(NetPlayer newMasterClient)
	{
		try
		{
			if (CustomGameMode.gameScriptRunner != null)
			{
				if (CustomGameMode.gameScriptRunner.ShouldTick)
				{
					foreach (KeyValuePair<int, IntPtr> keyValuePair in Bindings.LuauPlayerList)
					{
						Bindings.LuauPlayer* ptr = (Bindings.LuauPlayer*)(void*)keyValuePair.Value;
						ptr->IsMasterClient = false;
					}
					IntPtr intPtr;
					Bindings.LuauPlayerList.TryGetValue(newMasterClient.ActorNumber, out intPtr);
					Bindings.LuauPlayer* ptr2 = (Bindings.LuauPlayer*)(void*)intPtr;
					ptr2->IsMasterClient = true;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	public static void OnPlayerHit(GameEntity entity, int hitPlayer, float damage)
	{
		if (CustomGameMode.gameScriptRunner == null)
		{
			return;
		}
		if (!CustomGameMode.gameScriptRunner.ShouldTick)
		{
			return;
		}
		object[] array = new object[]
		{
			"playerHit",
			(double)entity.GetNetId(),
			(double)hitPlayer,
			(double)damage
		};
		LuauVm.eventQueue.Enqueue(array);
	}

	public static void TaggedByAI(GameEntity entity, int taggedPlayer)
	{
		if (CustomGameMode.gameScriptRunner == null)
		{
			return;
		}
		if (!CustomGameMode.gameScriptRunner.ShouldTick)
		{
			return;
		}
		object[] array = new object[]
		{
			"taggedByAI",
			(double)entity.GetNetId(),
			(double)taggedPlayer
		};
		LuauVm.eventQueue.Enqueue(array);
	}

	public override void HitPlayer(NetPlayer taggedPlayer)
	{
	}

	public unsafe static void OnEntityGrabbed(GameEntity entity, bool isGrabbed)
	{
		if (CustomGameMode.gameScriptRunner == null)
		{
			return;
		}
		if (!CustomGameMode.gameScriptRunner.ShouldTick)
		{
			return;
		}
		lua_State* l = CustomGameMode.gameScriptRunner.L;
		if (!Bindings.LuauGrabbablesList.ContainsKey(entity.GetNetId()))
		{
			return;
		}
		if (isGrabbed)
		{
			object[] array = new object[]
			{
				"entityGrabbed",
				(double)entity.GetNetId()
			};
			LuauVm.localEventQueue.Enqueue(array);
			return;
		}
		object[] array2 = new object[]
		{
			"entityReleased",
			(double)entity.GetNetId()
		};
		LuauVm.localEventQueue.Enqueue(array2);
	}

	public unsafe static void OnGameEntityRemoved(GameEntity entity)
	{
		if (CustomGameMode.gameScriptRunner == null)
		{
			return;
		}
		if (!CustomGameMode.gameScriptRunner.ShouldTick)
		{
			return;
		}
		lua_State* l = CustomGameMode.gameScriptRunner.L;
		if (Bindings.LuauAIAgentList.ContainsKey(entity.GetNetId()))
		{
			Bindings.LuauAIAgentList[entity.GetNetId()] = IntPtr.Zero;
			Luau.lua_getglobal(l, "AIAgents");
			int num = Luau.lua_objlen(l, -1);
			for (int i = 1; i <= num; i++)
			{
				Luau.lua_rawgeti(l, -1, i);
				Bindings.LuauGrabbableEntity* ptr = (Bindings.LuauGrabbableEntity*)Luau.lua_touserdata(l, -1);
				Luau.lua_pop(l, 1);
				if (ptr != null && ptr->EntityID == entity.GetNetId())
				{
					Luau.lua_pushnil(l);
					Luau.lua_rawseti(l, -2, i);
					break;
				}
			}
			Luau.lua_pop(l, 1);
			object[] array = new object[]
			{
				"agentDestroyed",
				(double)entity.id.index
			};
			LuauVm.localEventQueue.Enqueue(array);
			return;
		}
		if (Bindings.LuauGrabbablesList.ContainsKey(entity.GetNetId()))
		{
			Bindings.LuauGrabbablesList[entity.GetNetId()] = IntPtr.Zero;
			Luau.lua_getglobal(l, "GrabbableEntities");
			int num2 = Luau.lua_objlen(l, -1);
			for (int j = 1; j <= num2; j++)
			{
				Luau.lua_rawgeti(l, -1, j);
				Bindings.LuauGrabbableEntity* ptr2 = (Bindings.LuauGrabbableEntity*)Luau.lua_touserdata(l, -1);
				Luau.lua_pop(l, 1);
				if (ptr2 != null && ptr2->EntityID == entity.GetNetId())
				{
					Luau.lua_pushnil(l);
					Luau.lua_rawseti(l, -2, j);
					break;
				}
			}
			Luau.lua_pop(l, 1);
			object[] array2 = new object[]
			{
				"entityDestroyed",
				(double)entity.id.index
			};
			LuauVm.localEventQueue.Enqueue(array2);
		}
	}

	public override void StartPlaying()
	{
		base.StartPlaying();
		try
		{
			PhotonNetwork.AddCallbackTarget(this);
			CustomGameMode.GameModeInitialized = true;
			if (CustomGameMode.LuaScript != "")
			{
				CustomGameMode.LuaStart();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	public unsafe static void LuaStart()
	{
		if (CustomGameMode.LuaScript == "")
		{
			return;
		}
		CustomGameMode.RunGamemodeScript(CustomGameMode.LuaScript);
		if (CustomGameMode.gameScriptRunner.ShouldTick)
		{
			lua_State* l = CustomGameMode.gameScriptRunner.L;
			Bindings.LuauPlayerList.Clear();
			Luau.lua_getglobal(l, "Players");
			Player[] playerList = PhotonNetwork.PlayerList;
			for (int i = 0; i < playerList.Length; i++)
			{
				NetPlayer netPlayer = playerList[i];
				if (netPlayer != null)
				{
					Bindings.LuauPlayer* ptr = Luau.lua_class_push<Bindings.LuauPlayer>(l);
					ptr->PlayerID = netPlayer.ActorNumber;
					ptr->PlayerMaterial = 0;
					ptr->IsMasterClient = netPlayer.IsMasterClient;
					Bindings.LuauPlayerList[netPlayer.ActorNumber] = (IntPtr)((void*)ptr);
					RigContainer rigContainer;
					VRRigCache.Instance.TryGetVrrig(netPlayer, out rigContainer);
					VRRig rig = rigContainer.Rig;
					ptr->PlayerName = rig.playerNameVisible;
					Bindings.LuauVRRigList[netPlayer.ActorNumber] = rig;
					Bindings.PlayerFunctions.UpdatePlayer(l, rig, ptr);
					ptr->PlayerName = rig.playerNameVisible;
					Luau.lua_rawseti(l, -2, i + 1);
					if (netPlayer.IsLocal)
					{
						ptr->IsPCVR = PlayFabAuthenticator.instance.platform.ToString() != "Quest";
						Luau.lua_rawgeti(l, -1, i + 1);
						Luau.lua_setglobal(l, "LocalPlayer");
					}
				}
				else
				{
					Luau.lua_pushnil(l);
					Luau.lua_rawseti(l, -2, i + 1);
				}
			}
			for (int j = playerList.Length; j <= 10; j++)
			{
				Luau.lua_pushnil(l);
				Luau.lua_rawseti(l, -2, j + 1);
			}
			Bindings.LuauAIAgentList.Clear();
			Luau.lua_getglobal(l, "AIAgents");
			List<GameAgent> agents = CustomMapsGameManager.instance.gameAgentManager.GetAgents();
			for (int k = 0; k < agents.Count; k++)
			{
				GameAgent gameAgent = agents[k];
				if (!gameAgent.IsNull() && !gameAgent.entity.IsNull())
				{
					Bindings.LuauAIAgent* ptr2 = Luau.lua_class_push<Bindings.LuauAIAgent>(l);
					Bindings.AIAgentFunctions.UpdateEntity(gameAgent.entity, ptr2);
					Bindings.LuauAIAgentList[gameAgent.entity.GetNetId()] = (IntPtr)((void*)ptr2);
					Luau.lua_rawseti(l, -2, Bindings.LuauAIAgentList.Count);
					if (Bindings.LuauAIAgentList.Count + Bindings.LuauGrabbablesList.Count == Constants.aiAgentLimit)
					{
						Debug.Log("[CustomGameMode::LuaStart] Custom Map AI Agent limit has been reached!");
						break;
					}
				}
			}
			Luau.lua_pop(l, 1);
			Bindings.LuauGrabbablesList.Clear();
			Luau.lua_getglobal(l, "GrabbableEntities");
			List<GameEntity> gameEntities = CustomMapsGameManager.instance.gameEntityManager.GetGameEntities();
			for (int m = 0; m < gameEntities.Count; m++)
			{
				GameEntity gameEntity = gameEntities[m];
				if (!gameEntity.IsNull())
				{
					Bindings.LuauGrabbableEntity* ptr3 = Luau.lua_class_push<Bindings.LuauGrabbableEntity>(l);
					Bindings.GrabbableEntityFunctions.UpdateEntity(gameEntity, ptr3);
					Bindings.LuauGrabbablesList[gameEntity.GetNetId()] = (IntPtr)((void*)ptr3);
					Luau.lua_rawseti(l, -2, Bindings.LuauGrabbablesList.Count);
					if (Bindings.LuauAIAgentList.Count + Bindings.LuauGrabbablesList.Count == Constants.aiAgentLimit)
					{
						Debug.Log("[CustomGameMode::LuaStart] Custom Map AI Agent limit has been reached!");
						break;
					}
				}
			}
			Luau.lua_pop(l, 1);
		}
	}

	public override void StopPlaying()
	{
		base.StopPlaying();
		try
		{
			CustomGameMode.GameModeInitialized = false;
			if (CustomGameMode.gameScriptRunner != null)
			{
				CustomGameMode.StopScript();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	public static void StopScript()
	{
		if (CustomGameMode.gameScriptRunner.ShouldTick)
		{
			Luau.lua_close(CustomGameMode.gameScriptRunner.L);
		}
		LuauScriptRunner.ScriptRunners.Remove(CustomGameMode.gameScriptRunner);
		CustomGameMode.gameScriptRunner.ShouldTick = false;
		CustomGameMode.gameScriptRunner = null;
		foreach (KeyValuePair<GameObject, Bindings.LuauGameObjectInitialState> keyValuePair in Bindings.LuauGameObjectStates)
		{
			Bindings.LuauGameObjectInitialState value = keyValuePair.Value;
			GameObject key = keyValuePair.Key;
			if (key.IsNotNull())
			{
				if (value.Created)
				{
					key.Destroy();
				}
				else
				{
					key.SetActive(true);
					key.transform.localPosition = value.Position;
					key.transform.localRotation = value.Rotation;
					key.transform.localScale = value.Scale;
					MeshRenderer component = key.GetComponent<MeshRenderer>();
					Collider component2 = key.GetComponent<Collider>();
					if (component != null)
					{
						component.enabled = value.Visible;
					}
					if (component2 != null)
					{
						component2.enabled = value.Collidable;
					}
				}
			}
		}
		Bindings.LuauGameObjectStates.Clear();
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

	public static void TouchPlayer(NetPlayer touchedPlayer)
	{
		if (CustomGameMode.gameScriptRunner == null)
		{
			return;
		}
		if (!CustomGameMode.gameScriptRunner.ShouldTick)
		{
			return;
		}
		object[] array = new object[]
		{
			"touchedPlayer",
			touchedPlayer.GetPlayerRef()
		};
		LuauVm.localEventQueue.Enqueue(array);
	}

	public static void TaggedByEnvironment()
	{
		if (CustomGameMode.gameScriptRunner == null)
		{
			return;
		}
		if (!CustomGameMode.gameScriptRunner.ShouldTick)
		{
			return;
		}
		object[] array = new object[2];
		array[0] = "taggedByEnvironment";
		object[] array2 = array;
		LuauVm.localEventQueue.Enqueue(array2);
	}

	[MonoPInvokeCallback(typeof(lua_CFunction))]
	public unsafe static int GameModeBindings(lua_State* L)
	{
		Bindings.GorillaLocomotionSettingsBuilder(L);
		Bindings.PlayerInputBuilder(L);
		Bindings.PlayerBuilder(L);
		Bindings.GameObjectBuilder(L);
		Bindings.AIAgentBuilder(L);
		Bindings.GrabbableEntityBuilder(L);
		Bindings.RoomStateBuilder(L);
		Bindings.Components.Build(L);
		Luau.lua_createtable(L, 10, 0);
		Luau.lua_setglobal(L, "Players");
		Luau.lua_createtable(L, Constants.aiAgentLimit, 0);
		Luau.lua_setglobal(L, "AIAgents");
		Luau.lua_createtable(L, Constants.aiAgentLimit, 0);
		Luau.lua_setglobal(L, "GrabbableEntities");
		Luau.lua_register(L, new lua_CFunction(Bindings.LuaEmit.Emit), "emitEvent");
		Luau.lua_register(L, new lua_CFunction(Bindings.LuaStartVibration), "startVibration");
		Luau.lua_register(L, new lua_CFunction(Bindings.LuaPlaySound), "playSound");
		Luau.lua_register(L, new lua_CFunction(Bindings.JSON.DataSave), "dataSave");
		Luau.lua_register(L, new lua_CFunction(Bindings.JSON.DataLoad), "dataLoad");
		Luau.lua_register(L, new lua_CFunction(Bindings.PlayerUtils.SetVelocity), "setPlayerVelocity");
		Luau.lua_register(L, new lua_CFunction(Bindings.PlayerUtils.TeleportPlayer), "setPlayerPosition");
		Luau.lua_register(L, new lua_CFunction(Bindings.RayCastUtils.RayCast), "rayCast");
		return 0;
	}

	public unsafe override float[] LocalPlayerSpeed()
	{
		if (Bindings.LocomotionSettings == null || CustomGameMode.gameScriptRunner == null || !CustomGameMode.gameScriptRunner.ShouldTick)
		{
			this.playerSpeed[0] = 6.5f;
			this.playerSpeed[1] = 1.1f;
		}
		else
		{
			this.playerSpeed[0] = Bindings.LocomotionSettings->maxJumpSpeed.ClampSafe(0f, 100f);
			this.playerSpeed[1] = Bindings.LocomotionSettings->jumpMultiplier.ClampSafe(0f, 100f);
		}
		return this.playerSpeed;
	}

	[MonoPInvokeCallback(typeof(lua_CFunction))]
	public unsafe static int AfterTickGamemode(lua_State* L)
	{
		try
		{
			foreach (KeyValuePair<GameObject, IntPtr> keyValuePair in Bindings.LuauGameObjectDepthList)
			{
				GameObject key = keyValuePair.Key;
				if (key.IsNotNull())
				{
					Transform transform = key.transform;
					Bindings.LuauGameObject* ptr = (Bindings.LuauGameObject*)(void*)keyValuePair.Value;
					Vector3 position = ptr->Position;
					position = new Vector3((float)Math.Round((double)position.x, 4), (float)Math.Round((double)position.y, 4), (float)Math.Round((double)position.z, 4));
					transform.SetPositionAndRotation(position, ptr->Rotation);
					transform.localScale = ptr->Scale;
				}
			}
		}
		catch (Exception)
		{
		}
		return 0;
	}

	[MonoPInvokeCallback(typeof(lua_CFunction))]
	public unsafe static int PreTickGamemode(lua_State* L)
	{
		try
		{
			Luau.lua_pushboolean(L, (PhotonNetwork.InRoom && CustomGameMode.WasInRoom) ? 1 : 0);
			Luau.lua_setglobal(L, "InRoom");
			foreach (KeyValuePair<int, IntPtr> keyValuePair in Bindings.LuauPlayerList)
			{
				Bindings.LuauPlayer* ptr = (Bindings.LuauPlayer*)(void*)keyValuePair.Value;
				VRRig vrrig;
				Bindings.LuauVRRigList.TryGetValue(keyValuePair.Key, out vrrig);
				if (!vrrig.IsNotNull())
				{
					LuauHud.Instance.LuauLog("Unknown Rig for player");
				}
				else
				{
					if (keyValuePair.Key == PhotonNetwork.LocalPlayer.ActorNumber)
					{
						ptr->IsMasterClient = PhotonNetwork.LocalPlayer.IsMasterClient;
					}
					Bindings.PlayerFunctions.UpdatePlayer(L, vrrig, ptr);
				}
			}
			Luau.lua_getglobal(L, "AIAgents");
			CustomMapsGameManager instance = CustomMapsGameManager.instance;
			List<GameAgent> list;
			if (instance == null)
			{
				list = null;
			}
			else
			{
				GameAgentManager gameAgentManager = instance.gameAgentManager;
				list = ((gameAgentManager != null) ? gameAgentManager.GetAgents() : null);
			}
			List<GameAgent> list2 = list;
			int num = 0;
			for (;;)
			{
				int num2 = num;
				int? num3 = ((list2 != null) ? new int?(list2.Count) : null);
				if (!((num2 < num3.GetValueOrDefault()) & (num3 != null)))
				{
					break;
				}
				GameAgent gameAgent = list2[num];
				if (!gameAgent.IsNull() && !gameAgent.entity.IsNull())
				{
					IntPtr intPtr;
					if (Bindings.LuauAIAgentList.TryGetValue(gameAgent.entity.GetNetId(), out intPtr))
					{
						Bindings.AIAgentFunctions.UpdateEntity(gameAgent.entity, (Bindings.LuauAIAgent*)(void*)intPtr);
					}
					else if (Bindings.LuauAIAgentList.Count + Bindings.LuauGrabbablesList.Count == Constants.aiAgentLimit)
					{
						Debug.Log("[CustomGameMode::PreTick] Custom Map AI Agent limit has been reached!");
					}
					else
					{
						Bindings.LuauAIAgent* ptr2 = Luau.lua_class_push<Bindings.LuauAIAgent>(L);
						Bindings.AIAgentFunctions.UpdateEntity(gameAgent.entity, ptr2);
						Bindings.LuauAIAgentList[gameAgent.entity.GetNetId()] = (IntPtr)((void*)ptr2);
						Luau.lua_rawseti(L, -2, Bindings.LuauAIAgentList.Count);
					}
				}
				num++;
			}
			Luau.lua_pop(L, 1);
			foreach (KeyValuePair<GameObject, IntPtr> keyValuePair2 in Bindings.LuauGameObjectList)
			{
				GameObject key = keyValuePair2.Key;
				if (key.IsNotNull())
				{
					Transform transform = key.transform;
					Bindings.LuauGameObject* ptr3 = (Bindings.LuauGameObject*)(void*)keyValuePair2.Value;
					Vector3 position = transform.position;
					position = new Vector3((float)Math.Round((double)position.x, 4), (float)Math.Round((double)position.y, 4), (float)Math.Round((double)position.z, 4));
					ptr3->Position = position;
					ptr3->Rotation = transform.rotation;
					ptr3->Scale = transform.localScale;
				}
			}
			Bindings.UpdateInputs();
			CustomGameMode.WasInRoom = PhotonNetwork.InRoom;
		}
		catch (Exception)
		{
		}
		return 0;
	}

	private static void RunGamemodeScript(string script)
	{
		CustomGameMode.gameScriptRunner = new LuauScriptRunner(script, "GameMode", new lua_CFunction(CustomGameMode.GameModeBindings), new lua_CFunction(CustomGameMode.PreTickGamemode), new lua_CFunction(CustomGameMode.AfterTickGamemode));
	}

	private static void RunGamemodeScriptFromFile(string filename)
	{
		CustomGameMode.RunGamemodeScript(File.ReadAllText(Path.Join(Application.persistentDataPath, "Scripts", filename)));
	}

	public static LuauScriptRunner gameScriptRunner;

	public static string LuaScript = "";

	private static bool WasInRoom = false;

	public static bool GameModeInitialized;
}

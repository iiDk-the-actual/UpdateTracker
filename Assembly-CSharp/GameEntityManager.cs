using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using ExitGames.Client.Photon;
using Fusion;
using GorillaExtensions;
using GorillaLocomotion;
using Ionic.Zlib;
using Photon.Pun;
using Photon.Realtime;
using Unity.Collections;
using UnityEngine;

[NetworkBehaviourWeaved(0)]
public class GameEntityManager : NetworkComponent, IMatchmakingCallbacks, IInRoomCallbacks, IRequestableOwnershipGuardCallbacks, ITickSystemTick
{
	public event GameEntityManager.ZoneStartEvent onZoneStart;

	public event GameEntityManager.ZoneClearEvent onZoneClear;

	public static GameEntityManager activeManager { get; private set; }

	public bool TickRunning { get; set; }

	public bool PendingTableData { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		this.entities = new List<GameEntity>(64);
		this.gameEntityData = new List<GameEntityData>(64);
		this.netIdToIndex = new Dictionary<int, int>(16384);
		this.netIds = new NativeArray<int>(16384, Unity.Collections.Allocator.Persistent, NativeArrayOptions.ClearMemory);
		this.createdItemTypeCount = new Dictionary<int, int>();
		this.OnEntityRemoved = (Action<GameEntity>)Delegate.Combine(this.OnEntityRemoved, new Action<GameEntity>(CustomGameMode.OnGameEntityRemoved));
		this.zoneStateData = new GameEntityManager.ZoneStateData
		{
			zoneStateRequests = new List<GameEntityManager.ZoneStateRequest>(),
			zonePlayers = new List<Player>(),
			recievedStateBytes = new byte[15360],
			numRecievedStateBytes = 0
		};
		this.guard.AddCallbackTarget(this);
		this.netIdsForCreate = new List<int>();
		this.entityTypeIdsForCreate = new List<int>();
		this.packedPositionsForCreate = new List<long>();
		this.packedRotationsForCreate = new List<int>();
		this.createDataForCreate = new List<long>();
		this.netIdsForDelete = new List<int>();
		this.netIdsForState = new List<int>();
		this.statesForState = new List<long>();
		this.zoneComponents = new List<IGameEntityZoneComponent>(8);
		if (this.ghostReactorManager != null)
		{
			this.zoneComponents.Add(this.ghostReactorManager);
		}
		if (this.customMapsManager != null)
		{
			this.zoneComponents.Add(this.customMapsManager);
		}
		if (this.superInfectionManager != null)
		{
			this.zoneComponents.Add(this.superInfectionManager);
		}
		this.BuildFactory();
		GameEntityManager.allManagers.Add(this);
	}

	internal override void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		base.OnEnable();
		TickSystem<object>.AddTickCallback(this);
		VRRigCache.OnRigDeactivated += this.OnRigDeactivated;
	}

	internal override void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		base.OnDisable();
		TickSystem<object>.RemoveTickCallback(this);
		VRRigCache.OnRigDeactivated -= this.OnRigDeactivated;
	}

	private void OnDestroy()
	{
		NetworkBehaviourUtils.InternalOnDestroy(this);
		this.netIds.Dispose();
		GameEntityManager.allManagers.Remove(this);
	}

	public static GameEntityManager GetManagerForZone(GTZone zone)
	{
		for (int i = 0; i < GameEntityManager.allManagers.Count; i++)
		{
			if (GameEntityManager.allManagers[i].zone == zone)
			{
				return GameEntityManager.allManagers[i];
			}
		}
		return null;
	}

	public void Tick()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		this.UpdateZoneState();
		float time = Time.time;
		for (int i = 0; i < this.entities.Count; i++)
		{
			GameEntity gameEntity = this.entities[i];
			if (gameEntity != null && gameEntity.LastTickTime + gameEntity.MinTimeBetweenTicks < time && gameEntity.isActiveAndEnabled)
			{
				Action onTick = gameEntity.OnTick;
				if (onTick != null)
				{
					onTick();
				}
				gameEntity.LastTickTime = time;
			}
		}
		if (!this.IsAuthority())
		{
			return;
		}
		if (this.netIdsForCreate.Count > 0 && Time.time > this.lastCreateSent + this.createCooldown)
		{
			this.lastCreateSent = Time.time;
			this.photonView.RPC("CreateItemRPC", RpcTarget.Others, new object[]
			{
				this.netIdsForCreate.ToArray(),
				this.entityTypeIdsForCreate.ToArray(),
				this.packedPositionsForCreate.ToArray(),
				this.packedRotationsForCreate.ToArray(),
				this.createDataForCreate.ToArray()
			});
			this.netIdsForCreate.Clear();
			this.entityTypeIdsForCreate.Clear();
			this.packedPositionsForCreate.Clear();
			this.packedRotationsForCreate.Clear();
			this.createDataForCreate.Clear();
		}
		if (this.netIdsForDelete.Count > 0 && Time.time > this.lastDestroySent + this.destroyCooldown)
		{
			this.lastDestroySent = Time.time;
			this.photonView.RPC("DestroyItemRPC", RpcTarget.Others, new object[] { this.netIdsForDelete.ToArray() });
			this.netIdsForDelete.Clear();
		}
		if (this.netIdsForState.Count > 0 && Time.time > this.lastStateSent + this.stateCooldown)
		{
			this.lastDestroySent = Time.time;
			this.photonView.RPC("ApplyStateRPC", RpcTarget.All, new object[]
			{
				this.netIdsForState.ToArray(),
				this.statesForState.ToArray()
			});
			this.netIdsForState.Clear();
			this.statesForState.Clear();
		}
	}

	public GameEntityId AddGameEntity(GameEntity gameEntity)
	{
		return this.AddGameEntity(this.CreateNetId(), gameEntity);
	}

	public GameEntityId AddGameEntity(int netId, GameEntity gameEntity)
	{
		int num = this.FindNewEntityIndex();
		this.entities[num] = gameEntity;
		GameEntityData gameEntityData = default(GameEntityData);
		this.gameEntityData.Add(gameEntityData);
		gameEntity.id = new GameEntityId
		{
			index = num
		};
		this.netIdToIndex[netId] = num;
		this.netIds[num] = netId;
		Action<GameEntity> onEntityAdded = this.OnEntityAdded;
		if (onEntityAdded != null)
		{
			onEntityAdded(gameEntity);
		}
		return gameEntity.id;
	}

	private int FindNewEntityIndex()
	{
		for (int i = 0; i < this.entities.Count; i++)
		{
			if (this.entities[i] == null)
			{
				return i;
			}
		}
		this.entities.Add(null);
		return this.entities.Count - 1;
	}

	public void RemoveGameEntity(GameEntity entity)
	{
		int index = entity.id.index;
		if (index < 0 || index >= this.entities.Count)
		{
			return;
		}
		if (this.entities[index] == entity)
		{
			this.entities[index] = null;
		}
		else
		{
			for (int i = 0; i < this.entities.Count; i++)
			{
				if (this.entities[i] == entity)
				{
					this.entities[i] = null;
					break;
				}
			}
		}
		Action<GameEntity> onEntityRemoved = this.OnEntityRemoved;
		if (onEntityRemoved == null)
		{
			return;
		}
		onEntityRemoved(entity);
	}

	public List<GameEntity> GetGameEntities()
	{
		return this.entities;
	}

	public bool IsValidNetId(int netId)
	{
		int num;
		return this.netIdToIndex.TryGetValue(netId, out num) && num >= 0 && num < this.entities.Count;
	}

	public int FindOpenIndex()
	{
		for (int i = 0; i < this.netIds.Length; i++)
		{
			if (this.netIds[i] != -1)
			{
				return i;
			}
		}
		return -1;
	}

	public GameEntityId GetEntityIdFromNetId(int netId)
	{
		int num;
		if (this.netIdToIndex.TryGetValue(netId, out num))
		{
			return new GameEntityId
			{
				index = num
			};
		}
		return GameEntityId.Invalid;
	}

	public int GetNetIdFromEntityId(GameEntityId id)
	{
		if (id.index < 0 || id.index >= this.netIds.Length)
		{
			return -1;
		}
		return this.netIds[id.index];
	}

	public virtual bool IsAuthority()
	{
		return !NetworkSystem.Instance.InRoom || this.guard.isTrulyMine;
	}

	public bool IsAuthorityPlayer(NetPlayer player)
	{
		return player != null && this.IsAuthorityPlayer(player.GetPlayerRef());
	}

	public bool IsAuthorityPlayer(Player player)
	{
		return player != null && this.guard.actualOwner != null && player == this.guard.actualOwner.GetPlayerRef();
	}

	public bool IsZoneAuthority()
	{
		return this.IsAuthority();
	}

	public bool HasAuthority()
	{
		return this.GetAuthorityPlayer() != null;
	}

	public Player GetAuthorityPlayer()
	{
		if (this.guard.actualOwner != null)
		{
			return this.guard.actualOwner.GetPlayerRef();
		}
		return null;
	}

	public virtual bool IsZoneActive()
	{
		return this.zoneStateData.state == GameEntityManager.ZoneState.Active;
	}

	public bool IsPositionInZone(Vector3 pos)
	{
		return this.zoneLimit == null || this.zoneLimit.bounds.Contains(pos);
	}

	public virtual bool IsValidClientRPC(Player sender)
	{
		return this.IsAuthorityPlayer(sender) && (this.IsZoneActive() || sender == PhotonNetwork.LocalPlayer);
	}

	public bool IsValidClientRPC(Player sender, int entityNetId)
	{
		return this.IsValidClientRPC(sender) && this.IsValidNetId(entityNetId);
	}

	public bool IsValidClientRPC(Player sender, int entityNetId, Vector3 pos)
	{
		return this.IsValidClientRPC(sender, entityNetId) && this.IsPositionInZone(pos);
	}

	public bool IsValidClientRPC(Player sender, Vector3 pos)
	{
		return this.IsValidClientRPC(sender) && this.IsPositionInZone(pos);
	}

	public bool IsValidAuthorityRPC(Player sender)
	{
		return this.IsAuthority() && (this.IsZoneActive() || sender == PhotonNetwork.LocalPlayer);
	}

	public bool IsValidAuthorityRPC(Player sender, int entityNetId)
	{
		return this.IsValidAuthorityRPC(sender) && this.IsValidNetId(entityNetId);
	}

	public bool IsValidAuthorityRPC(Player sender, int entityNetId, Vector3 pos)
	{
		return this.IsValidAuthorityRPC(sender, entityNetId) && this.IsPositionInZone(pos);
	}

	public bool IsValidAuthorityRPC(Player sender, Vector3 pos)
	{
		return this.IsValidAuthorityRPC(sender) && this.IsPositionInZone(pos);
	}

	public bool IsValidEntity(GameEntityId id)
	{
		return this.GetGameEntity(id) != null;
	}

	public GameEntity GetGameEntity(GameEntityId id)
	{
		if (!id.IsValid())
		{
			return null;
		}
		return this.GetGameEntity(id.index);
	}

	public GameEntity GetGameEntityFromNetId(int netId)
	{
		int num;
		if (this.netIdToIndex.TryGetValue(netId, out num))
		{
			return this.GetGameEntity(num);
		}
		return null;
	}

	private GameEntity GetGameEntity(int index)
	{
		if (index == -1)
		{
			return null;
		}
		if (index < 0 || index >= this.entities.Count)
		{
			return null;
		}
		return this.entities[index];
	}

	public T GetGameComponent<T>(GameEntityId id) where T : Component
	{
		GameEntity gameEntity = this.GetGameEntity(id);
		if (gameEntity == null)
		{
			return default(T);
		}
		return gameEntity.GetComponent<T>();
	}

	public bool IsEntityValidToMigrate(GameEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		Vector3 position = VRRig.LocalRig.transform.position;
		int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
		bool flag = true;
		int num = 0;
		while (num < this.zoneComponents.Count && flag)
		{
			flag &= this.zoneComponents[num].ValidateMigratedGameEntity(this.GetNetIdFromEntityId(entity.id), entity.typeId, position, Quaternion.identity, entity.createData, actorNumber);
			num++;
		}
		return flag;
	}

	private void BuildFactory()
	{
		using (Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder(true))
		{
			string text = "[GameEntityManager]  BuildFactory: Entity names and typeIds for manager \"" + base.name + "\":";
			utf16ValueStringBuilder.AppendLine(text);
			foreach (IGameEntityZoneComponent gameEntityZoneComponent in this.zoneComponents)
			{
				IFactoryItemProvider factoryItemProvider = gameEntityZoneComponent as IFactoryItemProvider;
				if (factoryItemProvider != null)
				{
					foreach (GameEntity gameEntity in factoryItemProvider.GetFactoryItems())
					{
						if (!this.tempFactoryItems.Contains(gameEntity))
						{
							this.tempFactoryItems.Add(gameEntity);
						}
					}
				}
			}
			this.itemPrefabFactory = new Dictionary<int, GameObject>(1024);
			this.priceLookupByEntityId = new Dictionary<int, int>();
			for (int i = 0; i < this.tempFactoryItems.Count; i++)
			{
				GameObject gameObject = this.tempFactoryItems[i].gameObject;
				int staticHash = gameObject.name.GetStaticHash();
				if (gameObject.GetComponent<GRToolLantern>())
				{
					this.priceLookupByEntityId.Add(staticHash, 50);
				}
				else if (gameObject.GetComponent<GRToolCollector>())
				{
					this.priceLookupByEntityId.Add(staticHash, 50);
				}
				this.itemPrefabFactory.Add(staticHash, gameObject);
				utf16ValueStringBuilder.AppendFormat<string, int>("    - name=\"{0}\", typeId={1}\n", gameObject.name, staticHash);
				if (utf16ValueStringBuilder.Length > 5000)
				{
					utf16ValueStringBuilder.Append("... (continued in next log message) ...");
					utf16ValueStringBuilder.Clear();
					if (i + 1 < this.tempFactoryItems.Count)
					{
						utf16ValueStringBuilder.Append(text);
						utf16ValueStringBuilder.Append(" ... CONTINUED FROM PREVIOUS ...\n");
					}
				}
			}
		}
	}

	private int CreateNetId()
	{
		int num = this.nextNetId;
		this.nextNetId++;
		return num;
	}

	public GameEntityId RequestCreateItem(int entityTypeId, Vector3 position, Quaternion rotation, long createData)
	{
		if (!this.IsZoneAuthority() || !this.IsZoneActive() || !this.IsPositionInZone(position))
		{
			return GameEntityId.Invalid;
		}
		long num = BitPackUtils.PackWorldPosForNetwork(position);
		int num2 = BitPackUtils.PackQuaternionForNetwork(rotation);
		int num3 = this.CreateNetId();
		this.netIdsForCreate.Add(num3);
		this.entityTypeIdsForCreate.Add(entityTypeId);
		this.packedPositionsForCreate.Add(num);
		this.packedRotationsForCreate.Add(num2);
		this.createDataForCreate.Add(createData);
		return this.CreateAndInitItemLocal(num3, entityTypeId, position, rotation, createData);
	}

	[PunRPC]
	public void CreateItemRPC(int[] netId, int[] entityTypeId, long[] packedPos, int[] packedRot, long[] createData, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.CreateItem))
		{
			return;
		}
		if (netId == null || entityTypeId == null || packedPos == null || createData == null || netId.Length != entityTypeId.Length || netId.Length != packedPos.Length || netId.Length != packedRot.Length || netId.Length != createData.Length)
		{
			return;
		}
		for (int i = 0; i < netId.Length; i++)
		{
			Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(packedPos[i]);
			Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(packedRot[i]);
			float num = 10000f;
			if (!(in vector).IsValid(in num) || !(in quaternion).IsValid() || !this.FactoryHasEntity(entityTypeId[i]) || !this.IsPositionInZone(vector))
			{
				return;
			}
			this.CreateAndInitItemLocal(netId[i], entityTypeId[i], vector, quaternion, createData[i]);
		}
	}

	public void RequestCreateItems(List<GameEntityCreateData> entityData)
	{
		Debug.Log(string.Format("GameEntityManager RequestCreateItems List.Count: {0}", entityData.Count));
		if (!this.IsZoneAuthority() || !this.IsZoneActive())
		{
			GTDev.LogError<string>(string.Format("[GameEntityManager::RequestCreateItems] Cannot create items. Zone Auth: {0} ", this.IsZoneAuthority()) + string.Format("| Zone Active: {0}", this.IsZoneActive()), null);
			return;
		}
		GameEntityManager.ClearByteBuffer(GameEntityManager.tempSerializeGameState);
		MemoryStream memoryStream = new MemoryStream(GameEntityManager.tempSerializeGameState);
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(entityData.Count);
		for (int i = 0; i < entityData.Count; i++)
		{
			GameEntityCreateData gameEntityCreateData = entityData[i];
			int num = this.CreateNetId();
			long num2 = BitPackUtils.PackWorldPosForNetwork(gameEntityCreateData.position);
			int num3 = BitPackUtils.PackQuaternionForNetwork(gameEntityCreateData.rotation);
			binaryWriter.Write(num);
			binaryWriter.Write(gameEntityCreateData.entityTypeId);
			binaryWriter.Write(num2);
			binaryWriter.Write(num3);
			binaryWriter.Write(gameEntityCreateData.createData);
		}
		long position = memoryStream.Position;
		byte[] array = GZipStream.CompressBuffer(GameEntityManager.tempSerializeGameState);
		this.photonView.RPC("CreateItemsRPC", RpcTarget.All, new object[]
		{
			(int)this.zone,
			array
		});
	}

	[PunRPC]
	public void CreateItemsRPC(int zoneId, byte[] stateData, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender) || stateData == null || stateData.Length >= 15360 || this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.CreateItems))
		{
			return;
		}
		try
		{
			byte[] array = GZipStream.UncompressBuffer(stateData);
			int num = array.Length;
			using (MemoryStream memoryStream = new MemoryStream(array))
			{
				using (BinaryReader binaryReader = new BinaryReader(memoryStream))
				{
					int num2 = binaryReader.ReadInt32();
					for (int i = 0; i < num2; i++)
					{
						int num3 = binaryReader.ReadInt32();
						int num4 = binaryReader.ReadInt32();
						long num5 = binaryReader.ReadInt64();
						int num6 = binaryReader.ReadInt32();
						long num7 = binaryReader.ReadInt64();
						Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(num5);
						Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(num6);
						float num8 = 10000f;
						if ((in vector).IsValid(in num8) && (in quaternion).IsValid() && this.FactoryHasEntity(num4) && this.IsPositionInZone(vector))
						{
							this.CreateAndInitItemLocal(num3, num4, vector, quaternion, num7);
						}
					}
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void JoinWithItems(List<GameEntity> entities)
	{
		if (entities.Count == 0)
		{
			return;
		}
		GameEntityManager.ClearByteBuffer(GameEntityManager.tempSerializeGameState);
		MemoryStream memoryStream = new MemoryStream(GameEntityManager.tempSerializeGameState);
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		int num = 0;
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i] != null)
			{
				num++;
			}
		}
		binaryWriter.Write(num);
		for (int j = 0; j < entities.Count; j++)
		{
			GameEntity gameEntity = entities[j];
			if (!(gameEntity == null))
			{
				int typeId = gameEntity.typeId;
				long num2 = BitPackUtils.PackWorldPosForNetwork(gameEntity.transform.localPosition);
				int num3 = BitPackUtils.PackQuaternionForNetwork(gameEntity.transform.localRotation);
				long num4 = gameEntity.createData;
				for (int k = 0; k < this.zoneComponents.Count; k++)
				{
					num4 = this.zoneComponents[k].ProcessMigratedGameEntityCreateData(gameEntity, num4);
				}
				byte b;
				switch (gameEntity.snappedJoint)
				{
				default:
					b = ((gameEntity.heldByHandIndex == 0) ? 1 : 0);
					break;
				case SnapJointType.ArmL:
					b = 3;
					break;
				case SnapJointType.ArmR:
					b = 2;
					break;
				}
				binaryWriter.Write(typeId);
				binaryWriter.Write(num2);
				binaryWriter.Write(num3);
				binaryWriter.Write(num4);
				binaryWriter.Write(b);
			}
		}
		long position = memoryStream.Position;
		byte[] array = GZipStream.CompressBuffer(GameEntityManager.tempSerializeGameState);
		this.photonView.RPC("JoinWithItemsRPC", this.GetAuthorityPlayer(), new object[]
		{
			array,
			new int[0],
			PhotonNetwork.LocalPlayer.ActorNumber
		});
	}

	[PunRPC]
	public void PlayerLeftZoneRPC(PhotonMessageInfo info)
	{
		GamePlayer gamePlayer = GamePlayer.GetGamePlayer(info.Sender);
		if (NetworkSystem.Instance.SessionIsPrivate)
		{
			gamePlayer.DidJoinWithItems = false;
		}
		foreach (GameEntityId gameEntityId in gamePlayer.IterateHeldAndSnappedItems(this))
		{
			if (!this.netIdsForDelete.Contains(this.GetNetIdFromEntityId(gameEntityId)))
			{
				this.netIdsForDelete.Add(this.GetNetIdFromEntityId(gameEntityId));
			}
			this.DestroyItemLocal(gameEntityId);
		}
		Action onPlayerLeftZone = gamePlayer.OnPlayerLeftZone;
		if (onPlayerLeftZone == null)
		{
			return;
		}
		onPlayerLeftZone();
	}

	[PunRPC]
	public void JoinWithItemsRPC(byte[] stateData, int[] netIds, int joiningActorNum, PhotonMessageInfo info)
	{
		GamePlayer joiningPlayer = GamePlayer.GetGamePlayer(joiningActorNum);
		bool isAuthority = this.IsAuthority();
		if (isAuthority)
		{
			if (!this.IsValidAuthorityRPC(info.Sender))
			{
				return;
			}
		}
		else if (!this.IsValidClientRPC(info.Sender))
		{
			return;
		}
		if (joiningPlayer == null || (!isAuthority && this.GetAuthorityPlayer() != info.Sender) || (isAuthority && info.Sender.ActorNumber != joiningActorNum) || stateData == null || stateData.Length >= 15360 || joiningPlayer.DidJoinWithItems)
		{
			return;
		}
		if (!this.IsInZone())
		{
			return;
		}
		if (isAuthority)
		{
			joiningPlayer.DidJoinWithItems = true;
		}
		Action createItemsCallback = null;
		createItemsCallback = delegate
		{
			try
			{
				GamePlayer joiningPlayer2 = joiningPlayer;
				joiningPlayer2.OnPlayerInitialized = (Action)Delegate.Remove(joiningPlayer2.OnPlayerInitialized, createItemsCallback);
				byte[] array = GZipStream.UncompressBuffer(stateData);
				int num = array.Length;
				using (MemoryStream memoryStream = new MemoryStream(array))
				{
					using (BinaryReader binaryReader = new BinaryReader(memoryStream))
					{
						int num2 = binaryReader.ReadInt32();
						if (num2 <= 4)
						{
							if (isAuthority || netIds.Length == num2)
							{
								if (isAuthority)
								{
									netIds = new int[num2];
									for (int i = 0; i < num2; i++)
									{
										netIds[i] = this.CreateNetId();
									}
								}
								for (int j = 0; j < num2; j++)
								{
									int num3 = netIds[j];
									int num4 = binaryReader.ReadInt32();
									long num5 = binaryReader.ReadInt64();
									int num6 = binaryReader.ReadInt32();
									long num7 = binaryReader.ReadInt64();
									byte b = binaryReader.ReadByte();
									Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(num5);
									Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(num6);
									float num8 = 10000f;
									if ((in vector).IsValid(in num8) && (in quaternion).IsValid() && this.FactoryHasEntity(num4) && this.IsPositionInZone(vector))
									{
										bool flag = true;
										int num9 = 0;
										while (num9 < this.zoneComponents.Count && flag)
										{
											flag &= this.zoneComponents[num9].ValidateMigratedGameEntity(num3, num4, joiningPlayer.rig.transform.position, Quaternion.identity, num7, joiningActorNum);
											num9++;
										}
										if (flag)
										{
											GameEntityId gameEntityId = this.CreateAndInitItemLocal(num3, num4, joiningPlayer.rig.transform.position, Quaternion.identity, num7);
											bool flag2 = false;
											SnapJointType snapJointType;
											switch (b)
											{
											default:
												snapJointType = SnapJointType.None;
												break;
											case 1:
												snapJointType = SnapJointType.None;
												flag2 = true;
												break;
											case 2:
												snapJointType = SnapJointType.ArmR;
												break;
											case 3:
												snapJointType = SnapJointType.ArmL;
												flag2 = true;
												break;
											}
											if (snapJointType != SnapJointType.None)
											{
												this.SnapEntityLocal(gameEntityId, flag2, vector, quaternion, (int)snapJointType, joiningPlayer.rig.OwningNetPlayer);
											}
											else
											{
												this.GrabEntityOnCreate(gameEntityId, flag2, vector, quaternion, joiningPlayer.rig.OwningNetPlayer);
											}
										}
									}
								}
								if (isAuthority)
								{
									this.photonView.RPC("JoinWithItemsRPC", RpcTarget.Others, new object[] { stateData, netIds, joiningActorNum });
								}
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
		};
		if (joiningPlayer.AdditionalDataInitialized)
		{
			createItemsCallback();
			return;
		}
		GamePlayer joiningPlayer3 = joiningPlayer;
		joiningPlayer3.OnPlayerInitialized = (Action)Delegate.Combine(joiningPlayer3.OnPlayerInitialized, createItemsCallback);
	}

	public bool FactoryHasEntity(int entityTypeId)
	{
		GameObject gameObject;
		return this.itemPrefabFactory.TryGetValue(entityTypeId, out gameObject);
	}

	public GameObject FactoryPrefabById(int entityTypeId)
	{
		GameObject gameObject;
		if (this.itemPrefabFactory.TryGetValue(entityTypeId, out gameObject))
		{
			return gameObject;
		}
		return null;
	}

	public bool PriceLookup(int entityTypeId, out int price)
	{
		if (this.priceLookupByEntityId.TryGetValue(entityTypeId, out price))
		{
			return true;
		}
		price = -1;
		return false;
	}

	private void ValidateThatNetIdIsNotAlreadyUsed(int netId, int newTypeId)
	{
		for (int i = 0; i < this.netIds.Length; i++)
		{
			if (i < this.entities.Count && this.netIds[i] == netId)
			{
				this.entities[i] == null;
			}
		}
	}

	public GameEntityId CreateAndInitItemLocal(int netId, int entityTypeId, Vector3 position, Quaternion rotation, long createData)
	{
		GameEntity gameEntity = this.CreateItemLocal(netId, entityTypeId, position, rotation);
		if (gameEntity == null)
		{
			return GameEntityId.Invalid;
		}
		this.InitItemLocal(gameEntity, createData);
		return gameEntity.id;
	}

	public GameEntity CreateItemLocal(int netId, int entityTypeId, Vector3 position, Quaternion rotation)
	{
		this.nextNetId = Mathf.Max(netId + 1, this.nextNetId);
		GameObject gameObject;
		if (!this.itemPrefabFactory.TryGetValue(entityTypeId, out gameObject))
		{
			return null;
		}
		if (!this.createdItemTypeCount.ContainsKey(entityTypeId))
		{
			this.createdItemTypeCount[entityTypeId] = 0;
		}
		if (this.createdItemTypeCount[entityTypeId] > 100)
		{
			return null;
		}
		Dictionary<int, int> dictionary = this.createdItemTypeCount;
		int num = dictionary[entityTypeId];
		dictionary[entityTypeId] = num + 1;
		GameEntity componentInChildren = global::UnityEngine.Object.Instantiate<GameObject>(gameObject, position, rotation).GetComponentInChildren<GameEntity>();
		this.AddGameEntity(netId, componentInChildren);
		componentInChildren.Create(this, entityTypeId);
		return componentInChildren;
	}

	public void InitItemLocal(GameEntity entity, long createData)
	{
		entity.Init(createData);
		for (int i = 0; i < this.zoneComponents.Count; i++)
		{
			this.zoneComponents[i].OnCreateGameEntity(entity);
		}
	}

	public void RequestDestroyItem(GameEntityId entityId)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		if (!this.netIdsForDelete.Contains(this.GetNetIdFromEntityId(entityId)))
		{
			this.netIdsForDelete.Add(this.GetNetIdFromEntityId(entityId));
		}
		this.DestroyItemLocal(entityId);
	}

	public void RequestDestroyItems(List<GameEntityId> entityIds)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < entityIds.Count; i++)
		{
			list.Add(this.GetNetIdFromEntityId(entityIds[i]));
		}
		this.photonView.RPC("DestroyItemRPC", RpcTarget.All, new object[] { list.ToArray() });
	}

	[PunRPC]
	public void DestroyItemRPC(int[] entityNetId, PhotonMessageInfo info)
	{
		if (entityNetId == null || this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.DestroyItem))
		{
			return;
		}
		for (int i = 0; i < entityNetId.Length; i++)
		{
			if (!this.IsValidClientRPC(info.Sender, entityNetId[i]))
			{
				return;
			}
			this.DestroyItemLocal(this.GetEntityIdFromNetId(entityNetId[i]));
		}
	}

	public void DestroyItemLocal(GameEntityId entityId)
	{
		GameEntity gameEntity = this.GetGameEntity(entityId);
		if (gameEntity == null)
		{
			return;
		}
		if (!this.createdItemTypeCount.ContainsKey(gameEntity.typeId))
		{
			this.createdItemTypeCount[gameEntity.typeId] = 1;
		}
		Dictionary<int, int> dictionary = this.createdItemTypeCount;
		int typeId = gameEntity.typeId;
		int num = dictionary[typeId];
		dictionary[typeId] = num - 1;
		GamePlayer gamePlayer;
		if (GamePlayer.TryGetGamePlayer(gameEntity.heldByActorNumber, out gamePlayer))
		{
			gamePlayer.ClearGrabbedIfHeld(gameEntity.id);
			if (gamePlayer.IsLocal())
			{
				GamePlayerLocal.instance.ClearGrabbedIfHeld(gameEntity.id);
			}
		}
		GamePlayer gamePlayer2;
		if (GamePlayer.TryGetGamePlayer(gameEntity.snappedByActorNumber, out gamePlayer2))
		{
			gamePlayer2.ClearSnappedIfSnapped(gameEntity.id);
		}
		this.RemoveGameEntity(gameEntity);
		global::UnityEngine.Object.Destroy(gameEntity.gameObject);
	}

	public void RequestState(GameEntityId entityId, long newState)
	{
		if (this.IsAuthority())
		{
			this.RequestStateAuthority(entityId, newState);
			return;
		}
		this.photonView.RPC("RequestStateRPC", this.GetAuthorityPlayer(), new object[]
		{
			this.GetNetIdFromEntityId(entityId),
			newState
		});
	}

	private void RequestStateAuthority(GameEntityId entityId, long newState)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		int netIdFromEntityId = this.GetNetIdFromEntityId(entityId);
		if (!this.IsValidNetId(netIdFromEntityId))
		{
			return;
		}
		if (this.netIdsForState.Contains(netIdFromEntityId))
		{
			this.statesForState[this.netIdsForState.IndexOf(netIdFromEntityId)] = newState;
			return;
		}
		this.netIdsForState.Add(netIdFromEntityId);
		this.statesForState.Add(newState);
	}

	[PunRPC]
	public void RequestStateRPC(int entityNetId, long newState, PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender, entityNetId))
		{
			return;
		}
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(info.Sender, out gamePlayer) || !gamePlayer.netStateLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		GameEntityId entityIdFromNetId = this.GetEntityIdFromNetId(entityNetId);
		GameEntity gameEntity = this.GetGameEntity(entityIdFromNetId);
		if (gameEntity == null || gameEntity.IsNull())
		{
			return;
		}
		bool flag = false;
		GRToolClub component = gameEntity.GetComponent<GRToolClub>();
		GRToolCollector component2 = gameEntity.GetComponent<GRToolCollector>();
		GRToolRevive component3 = gameEntity.GetComponent<GRToolRevive>();
		GRToolLantern component4 = gameEntity.GetComponent<GRToolLantern>();
		GRToolFlash component5 = gameEntity.GetComponent<GRToolFlash>();
		GRToolDirectionalShield component6 = gameEntity.GetComponent<GRToolDirectionalShield>();
		GRToolShieldGun component7 = gameEntity.GetComponent<GRToolShieldGun>();
		if (component == null && component2 == null && component3 == null && component4 == null && component5 == null && component6 == null && component7 == null)
		{
			flag = this.IsAuthorityPlayer(info.Sender);
		}
		bool flag2 = gamePlayer.IsHoldingEntity(entityIdFromNetId, false) || gamePlayer.IsHoldingEntity(entityIdFromNetId, true);
		bool flag3 = gameEntity.lastHeldByActorNumber == info.Sender.ActorNumber;
		if (!flag && (flag2 || flag3))
		{
			if (component4 != null)
			{
				flag = component4.CanChangeState(newState);
			}
			if (component5 != null)
			{
				flag = component5.CanChangeState(newState);
			}
			if (component != null || component2 != null || component3 != null || component6 != null || component7 != null)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			bool flag4 = gameEntity.snappedByActorNumber == gamePlayer.rig.OwningNetPlayer.ActorNumber;
			if (gameEntity.canHoldingPlayerUpdateState && flag2)
			{
				flag = true;
			}
			else if (gameEntity.canLastHoldingPlayerUpdateState && flag3)
			{
				flag = true;
			}
			else if (gameEntity.canSnapPlayerUpdateState && flag4)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (this.netIdsForState.Contains(entityNetId))
			{
				this.statesForState[this.netIdsForState.IndexOf(entityNetId)] = newState;
				return;
			}
			this.netIdsForState.Add(entityNetId);
			this.statesForState.Add(newState);
		}
	}

	[PunRPC]
	public void ApplyStateRPC(int[] netId, long[] newState, PhotonMessageInfo info)
	{
		if (netId == null || newState == null || netId.Length != newState.Length || this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.ApplyState))
		{
			return;
		}
		for (int i = 0; i < netId.Length; i++)
		{
			if (!this.IsValidClientRPC(info.Sender, netId[i]))
			{
				return;
			}
			GameEntityId entityIdFromNetId = this.GetEntityIdFromNetId(netId[i]);
			GameEntity gameEntity = this.entities[entityIdFromNetId.index];
			if (gameEntity != null)
			{
				gameEntity.SetState(newState[i]);
			}
		}
	}

	public void RequestGrabEntity(GameEntityId gameEntityId, bool isLeftHand, Vector3 localPosition, Quaternion localRotation)
	{
		bool inRoom = PhotonNetwork.InRoom;
		if (!this.IsAuthority() || !inRoom)
		{
			this.GrabEntityLocal(gameEntityId, isLeftHand, localPosition, localRotation, NetPlayer.Get(PhotonNetwork.LocalPlayer));
		}
		if (inRoom)
		{
			long num = BitPackUtils.PackHandPosRotForNetwork(localPosition, localRotation);
			this.photonView.RPC("RequestGrabEntityRPC", this.GetAuthorityPlayer(), new object[]
			{
				this.GetNetIdFromEntityId(gameEntityId),
				isLeftHand,
				num
			});
			PhotonNetwork.SendAllOutgoingCommands();
		}
	}

	[PunRPC]
	public void RequestGrabEntityRPC(int entityNetId, bool isLeftHand, long packedPosRot, PhotonMessageInfo info)
	{
		if (!this.IsValidAuthorityRPC(info.Sender, entityNetId))
		{
			return;
		}
		Vector3 vector;
		Quaternion quaternion;
		BitPackUtils.UnpackHandPosRotFromNetwork(packedPosRot, out vector, out quaternion);
		float num = 10000f;
		if (!(in vector).IsValid(in num) || !(in quaternion).IsValid() || vector.sqrMagnitude > 6400f)
		{
			return;
		}
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(info.Sender, out gamePlayer) || !this.IsPlayerHandNearEntity(gamePlayer, entityNetId, isLeftHand, false, 16f) || this.IsValidEntity(gamePlayer.GetGameEntityId(isLeftHand)) || !gamePlayer.netGrabLimiter.CheckCallTime(Time.time) || gamePlayer.IsHoldingEntity(this, isLeftHand))
		{
			return;
		}
		GameEntity gameEntity = this.GetGameEntity(this.GetEntityIdFromNetId(entityNetId));
		if (gameEntity == null)
		{
			return;
		}
		if (!this.ValidateGrab(gameEntity, info.Sender.ActorNumber, isLeftHand))
		{
			return;
		}
		this.photonView.RPC("GrabEntityRPC", RpcTarget.All, new object[] { entityNetId, isLeftHand, packedPosRot, info.Sender });
		PhotonNetwork.SendAllOutgoingCommands();
	}

	[PunRPC]
	public void GrabEntityRPC(int entityNetId, bool isLeftHand, long packedPosRot, Player grabbedByPlayer, PhotonMessageInfo info)
	{
		if (!this.IsValidClientRPC(info.Sender, entityNetId) || this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.GrabEntity))
		{
			return;
		}
		Vector3 vector;
		Quaternion quaternion;
		BitPackUtils.UnpackHandPosRotFromNetwork(packedPosRot, out vector, out quaternion);
		float num = 10000f;
		if (!(in vector).IsValid(in num) || !(in quaternion).IsValid() || vector.sqrMagnitude > 6400f)
		{
			return;
		}
		this.GrabEntityLocal(this.GetEntityIdFromNetId(entityNetId), isLeftHand, vector, quaternion, NetPlayer.Get(grabbedByPlayer));
	}

	private void GrabEntityLocal(GameEntityId gameEntityId, bool isLeftHand, Vector3 localPosition, Quaternion localRotation, NetPlayer grabbedByPlayer)
	{
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(grabbedByPlayer.ActorNumber), out rigContainer))
		{
			return;
		}
		GameEntity gameEntity = this.entities[gameEntityId.index];
		if (gameEntityId.index < 0 || gameEntityId.index >= this.entities.Count)
		{
			return;
		}
		if (gameEntity == null)
		{
			return;
		}
		if (grabbedByPlayer == null)
		{
			return;
		}
		int handIndex = GamePlayer.GetHandIndex(isLeftHand);
		if (grabbedByPlayer.IsLocal && gameEntity.heldByActorNumber == grabbedByPlayer.ActorNumber && gameEntity.heldByHandIndex == handIndex)
		{
			return;
		}
		this.TryDetachCompletely(gameEntity);
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(grabbedByPlayer.ActorNumber, out gamePlayer))
		{
			return;
		}
		GamePlayer gamePlayer2;
		if (GamePlayer.TryGetGamePlayer(gameEntity.heldByActorNumber, out gamePlayer2))
		{
			int num = gamePlayer2.FindHandIndex(gameEntityId);
			bool flag = gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
			gamePlayer2.ClearGrabbedIfHeld(gameEntityId);
			if (num != -1 && flag)
			{
				GamePlayerLocal.instance.ClearGrabbed(num);
			}
		}
		Transform handTransform = GamePlayer.GetHandTransform(rigContainer.Rig, handIndex);
		Rigidbody component = gameEntity.GetComponent<Rigidbody>();
		if (component != null)
		{
			if (grabbedByPlayer.IsLocal)
			{
				component.constraints = RigidbodyConstraints.FreezeAll;
				component.isKinematic = false;
			}
			else
			{
				component.constraints = RigidbodyConstraints.None;
				component.isKinematic = true;
			}
		}
		gameEntity.transform.SetParent(handTransform);
		gameEntity.transform.SetLocalPositionAndRotation(localPosition, localRotation);
		gameEntity.heldByActorNumber = grabbedByPlayer.ActorNumber;
		gameEntity.heldByHandIndex = handIndex;
		gameEntity.lastHeldByActorNumber = gameEntity.heldByActorNumber;
		gamePlayer.SetGrabbed(gameEntityId, handIndex, this);
		if (grabbedByPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			GamePlayerLocal.instance.SetGrabbed(gameEntityId, GamePlayer.GetHandIndex(isLeftHand));
			GamePlayerLocal.instance.PlayCatchFx(isLeftHand);
		}
		GameSnappable component2 = gameEntity.GetComponent<GameSnappable>();
		if (component2 != null && component2.snappedToJoint != null && component2.snappedToJoint.jointType != SnapJointType.None)
		{
			SuperInfectionSnapPoint superInfectionSnapPoint = SuperInfectionSnapPointManager.FindSnapPoint(gamePlayer, component2.snappedToJoint.jointType);
			if (superInfectionSnapPoint == null)
			{
				superInfectionSnapPoint = component2.snappedToJoint;
			}
			superInfectionSnapPoint.Unsnapped();
			component2.OnUnsnap();
			Action onUnsnapped = gameEntity.OnUnsnapped;
			if (onUnsnapped != null)
			{
				onUnsnapped();
			}
			gameEntity.snappedByActorNumber = -1;
			gameEntity.snappedJoint = SnapJointType.None;
		}
		gameEntity.PlayCatchFx();
		Action onGrabbed = gameEntity.OnGrabbed;
		if (onGrabbed != null)
		{
			onGrabbed();
		}
		CustomGameMode.OnEntityGrabbed(gameEntity, true);
	}

	public void GrabEntityOnCreate(GameEntityId gameEntityId, bool isLeftHand, Vector3 localPosition, Quaternion localRotation, NetPlayer grabbedByPlayer)
	{
		if (grabbedByPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			GamePlayerLocal.instance.gamePlayer.DeleteGrabbedEntityLocal(GamePlayer.GetHandIndex(isLeftHand));
		}
		this.GrabEntityLocal(gameEntityId, isLeftHand, localPosition, localRotation, grabbedByPlayer);
	}

	public GameEntityId TryGrabLocal(Vector3 handPosition, bool isLeftHand, out Vector3 closestPointOnBoundingBox)
	{
		GameEntityManager.<>c__DisplayClass146_0 CS$<>8__locals1;
		CS$<>8__locals1.handPosition = handPosition;
		float num = 0f;
		float num2 = 0.1f;
		float num3 = 0.25f;
		CS$<>8__locals1.minimumObjectExtent = 0.04f;
		int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
		Vector3 rigidbodyVelocity = GTPlayer.Instance.RigidbodyVelocity;
		CS$<>8__locals1.bestEntity = GameEntityId.Invalid;
		CS$<>8__locals1.bestDist = float.MaxValue;
		CS$<>8__locals1.closestPoint = CS$<>8__locals1.handPosition;
		for (int i = 0; i < this.entities.Count; i++)
		{
			CS$<>8__locals1.entity = this.entities[i];
			if (this.ValidateGrab(CS$<>8__locals1.entity, actorNumber, isLeftHand))
			{
				float num4 = 0.75f;
				float sqrMagnitude = (CS$<>8__locals1.handPosition - CS$<>8__locals1.entity.transform.position).sqrMagnitude;
				if (sqrMagnitude <= num4 * num4)
				{
					Vector3 vector = CS$<>8__locals1.entity.GetVelocity() - rigidbodyVelocity;
					float magnitude = vector.magnitude;
					CS$<>8__locals1.slopForSpeed = Mathf.Clamp(magnitude * num2, 0f, num3);
					CS$<>8__locals1.slopProjection = vector.normalized * CS$<>8__locals1.slopForSpeed;
					num = CS$<>8__locals1.entity.pickupRangeFromSurface;
					this.renderSearchList.Clear();
					CS$<>8__locals1.entity.GetComponentsInChildren<MeshFilter>(false, this.renderSearchList);
					foreach (MeshFilter meshFilter in this.renderSearchList)
					{
						if (!(this.GetParentEntity<GameEntity>(meshFilter.transform) != CS$<>8__locals1.entity))
						{
							GameEntityManager.<TryGrabLocal>g__TestAgainstBounds|146_0(meshFilter.transform, meshFilter.sharedMesh.bounds, ref CS$<>8__locals1);
						}
					}
					this.renderSearchListSkinned.Clear();
					CS$<>8__locals1.entity.GetComponentsInChildren<SkinnedMeshRenderer>(false, this.renderSearchListSkinned);
					foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.renderSearchListSkinned)
					{
						if (!(this.GetParentEntity<GameEntity>(skinnedMeshRenderer.transform) != CS$<>8__locals1.entity))
						{
							GameEntityManager.<TryGrabLocal>g__TestAgainstBounds|146_0(skinnedMeshRenderer.transform, skinnedMeshRenderer.localBounds, ref CS$<>8__locals1);
						}
					}
					if (this.renderSearchList.Count == 0 && this.renderSearchListSkinned.Count == 0)
					{
						float num5 = Mathf.Sqrt(sqrMagnitude);
						if (num5 < CS$<>8__locals1.bestDist)
						{
							CS$<>8__locals1.bestDist = num5;
							CS$<>8__locals1.bestEntity = CS$<>8__locals1.entity.id;
							CS$<>8__locals1.closestPoint = CS$<>8__locals1.entity.transform.position;
						}
					}
				}
			}
		}
		closestPointOnBoundingBox = CS$<>8__locals1.closestPoint;
		if (CS$<>8__locals1.bestDist > num)
		{
			return GameEntityId.Invalid;
		}
		return CS$<>8__locals1.bestEntity;
	}

	private void DrawDebugStar(Vector3 position, float radius)
	{
		for (int i = 0; i < 20; i++)
		{
			Debug.DrawLine(position, position + Random.onUnitSphere * radius, Color.red, 10f);
		}
	}

	private static bool SegmentHitsBounds(Bounds bounds, Vector3 a, Vector3 b, out Vector3 hitPoint, out float distance)
	{
		hitPoint = default(Vector3);
		distance = float.MaxValue;
		Vector3 vector = b - a;
		float magnitude = vector.magnitude;
		if (magnitude <= Mathf.Epsilon)
		{
			if (bounds.Contains(a))
			{
				distance = 0f;
				hitPoint = a;
				return true;
			}
			return false;
		}
		else
		{
			Ray ray = new Ray(a, vector / magnitude);
			if (bounds.IntersectRay(ray, out distance) && distance <= magnitude)
			{
				hitPoint = a + ray.direction * distance;
				return true;
			}
			return false;
		}
	}

	public bool GetEntitiesWithComponentInRadius<T>(Vector3 center, float radius, bool checkRootOnly, List<T> nearbyEntities)
	{
		float num = radius * radius;
		for (int i = 0; i < this.entities.Count; i++)
		{
			GameEntity gameEntity = this.entities[i];
			if (!(gameEntity == null))
			{
				T t;
				if (checkRootOnly)
				{
					t = gameEntity.GetComponent<T>();
				}
				else
				{
					t = gameEntity.GetComponentInChildren<T>();
				}
				if (t != null && (this.entities[i].transform.position - center).sqrMagnitude < num)
				{
					nearbyEntities.Add(t);
				}
			}
		}
		return nearbyEntities.Count > 0;
	}

	private bool ValidateGrab(GameEntity gameEntity, int playerActorNumber, bool isLeftHand)
	{
		if (gameEntity == null || !gameEntity.pickupable)
		{
			return false;
		}
		if (gameEntity.onlyGrabActorNumber != -1 && gameEntity.onlyGrabActorNumber != playerActorNumber)
		{
			return false;
		}
		if (gameEntity.heldByActorNumber != -1 && gameEntity.heldByActorNumber != playerActorNumber && GamePlayer.GetGamePlayer(gameEntity.heldByActorNumber) != null)
		{
			return false;
		}
		if (gameEntity.snappedByActorNumber != -1 && gameEntity.snappedByActorNumber != playerActorNumber && GamePlayer.GetGamePlayer(gameEntity.snappedByActorNumber) != null)
		{
			return false;
		}
		GameSnappable component = gameEntity.GetComponent<GameSnappable>();
		if (component != null && !component.CanGrabWithHand(isLeftHand))
		{
			return false;
		}
		if (this.IsValidEntity(gameEntity.attachedToEntityId))
		{
			GameEntity gameEntity2 = this.GetGameEntity(gameEntity.attachedToEntityId);
			if (gameEntity2 != null)
			{
				if (gameEntity2.snappedByActorNumber != -1 && gameEntity2.snappedByActorNumber != playerActorNumber && GamePlayer.GetGamePlayer(gameEntity2.snappedByActorNumber) != null)
				{
					return false;
				}
				GameSnappable component2 = gameEntity2.GetComponent<GameSnappable>();
				if (component2 != null && !component2.CanGrabWithHand(isLeftHand))
				{
					return false;
				}
			}
		}
		return true;
	}

	private T GetParentEntity<T>(Transform transform) where T : MonoBehaviour
	{
		while (transform != null)
		{
			T component = transform.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			transform = transform.parent;
		}
		return default(T);
	}

	public void RequestThrowEntity(GameEntityId entityId, bool isLeftHand, Vector3 headPosition, Vector3 velocity, Vector3 angVelocity)
	{
		GameEntity gameEntity = this.GetGameEntity(entityId);
		if (gameEntity == null)
		{
			return;
		}
		Vector3 vector = gameEntity.transform.position;
		Quaternion rotation = gameEntity.transform.rotation;
		Rigidbody component = gameEntity.GetComponent<Rigidbody>();
		if (component != null)
		{
			Vector3 vector2 = gameEntity.transform.TransformPoint(component.centerOfMass);
			Vector3 vector3 = vector2 - headPosition;
			float magnitude = vector3.magnitude;
			if (magnitude > 0f)
			{
				vector3 /= magnitude;
				RaycastHit raycastHit;
				if (Physics.SphereCast(headPosition, 0.05f, vector3, out raycastHit, magnitude + 0.1f, 513, QueryTriggerInteraction.Ignore))
				{
					component.GetComponentsInChildren<Collider>(this._collidersList);
					Vector3 vector4 = component.position + -raycastHit.normal * 1000f;
					float num = float.MaxValue;
					bool flag = false;
					Plane plane = new Plane(raycastHit.normal, raycastHit.point);
					foreach (Collider collider in this._collidersList)
					{
						if (collider.enabled && !collider.isTrigger)
						{
							Vector3 vector5 = collider.ClosestPoint(vector4);
							float num2 = Mathf.Abs(plane.GetDistanceToPoint(vector5));
							if (num2 < num)
							{
								num = num2;
								flag = true;
							}
						}
					}
					if (flag)
					{
						vector += raycastHit.normal * num;
					}
					else
					{
						float num3 = Mathf.Max(raycastHit.distance - 0.2f, 0f);
						Vector3 vector6 = headPosition + vector3 * num3;
						vector += vector6 - vector2;
					}
				}
			}
		}
		bool inRoom = PhotonNetwork.InRoom;
		if (!this.IsAuthority() || !inRoom)
		{
			this.ThrowEntityLocal(entityId, isLeftHand, vector, rotation, velocity, angVelocity, NetPlayer.Get(PhotonNetwork.LocalPlayer));
		}
		if (inRoom)
		{
			this.photonView.RPC("RequestThrowEntityRPC", this.GetAuthorityPlayer(), new object[]
			{
				this.GetNetIdFromEntityId(entityId),
				isLeftHand,
				vector,
				rotation,
				velocity,
				angVelocity
			});
			PhotonNetwork.SendAllOutgoingCommands();
		}
	}

	[PunRPC]
	public void RequestThrowEntityRPC(int entityNetId, bool isLeftHand, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, PhotonMessageInfo info)
	{
		if (this.IsValidAuthorityRPC(info.Sender, entityNetId))
		{
			float num = 10000f;
			if ((in position).IsValid(in num) && (in rotation).IsValid())
			{
				float num2 = 10000f;
				if ((in velocity).IsValid(in num2))
				{
					float num3 = 10000f;
					if ((in angVelocity).IsValid(in num3) && velocity.sqrMagnitude <= 1600f && this.IsPositionInZone(position))
					{
						GamePlayer gamePlayer;
						if (!GamePlayer.TryGetGamePlayer(info.Sender, out gamePlayer) || !GameEntityManager.IsPlayerHandNearPosition(gamePlayer, position, isLeftHand, false, 16f) || !gamePlayer.IsHoldingEntity(this.GetEntityIdFromNetId(entityNetId), isLeftHand) || !gamePlayer.netThrowLimiter.CheckCallTime(Time.time))
						{
							return;
						}
						this.photonView.RPC("ThrowEntityRPC", RpcTarget.All, new object[] { entityNetId, isLeftHand, position, rotation, velocity, angVelocity, info.Sender, info.SentServerTime });
						PhotonNetwork.SendAllOutgoingCommands();
						return;
					}
				}
			}
		}
	}

	[PunRPC]
	public void ThrowEntityRPC(int entityNetId, bool isLeftHand, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, Player thrownByPlayer, double throwTime, PhotonMessageInfo info)
	{
		if (this.IsValidClientRPC(info.Sender, entityNetId, position) && !this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.ThrowEntity))
		{
			float num = 10000f;
			if ((in position).IsValid(in num) && (in rotation).IsValid())
			{
				float num2 = 10000f;
				if ((in velocity).IsValid(in num2))
				{
					float num3 = 10000f;
					if ((in angVelocity).IsValid(in num3) && velocity.sqrMagnitude <= 1600f)
					{
						NetPlayer netPlayer = NetPlayer.Get(thrownByPlayer);
						if (netPlayer.IsLocal && !this.IsAuthority())
						{
							return;
						}
						this.ThrowEntityLocal(this.GetEntityIdFromNetId(entityNetId), isLeftHand, position, rotation, velocity, angVelocity, netPlayer);
						return;
					}
				}
			}
		}
	}

	private void ThrowEntityLocal(GameEntityId entityId, bool isLeftHand, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, NetPlayer thrownByPlayer)
	{
		if (entityId.index < 0 || entityId.index >= this.entities.Count)
		{
			return;
		}
		GameEntity gameEntity = this.entities[entityId.index];
		if (gameEntity == null)
		{
			return;
		}
		if (thrownByPlayer == null)
		{
			return;
		}
		gameEntity.transform.SetParent(null);
		gameEntity.transform.SetLocalPositionAndRotation(position, rotation);
		Rigidbody component = gameEntity.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
			component.constraints = RigidbodyConstraints.None;
			component.position = position;
			component.rotation = rotation;
			component.linearVelocity = velocity;
			component.angularVelocity = angVelocity;
		}
		gameEntity.heldByActorNumber = -1;
		gameEntity.heldByHandIndex = -1;
		gameEntity.attachedToEntityId = GameEntityId.Invalid;
		bool flag = thrownByPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
		int handIndex = GamePlayer.GetHandIndex(isLeftHand);
		RigContainer rigContainer;
		if (flag)
		{
			GamePlayerLocal.instance.gamePlayer.ClearGrabbed(handIndex);
			GamePlayerLocal.instance.ClearGrabbed(handIndex);
			GamePlayerLocal.instance.PlayThrowFx(isLeftHand);
		}
		else if (VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(thrownByPlayer.ActorNumber), out rigContainer))
		{
			GamePlayer gamePlayerRef = rigContainer.Rig.GamePlayerRef;
			if (gamePlayerRef != null)
			{
				gamePlayerRef.ClearGrabbedIfHeld(entityId);
				gamePlayerRef.ClearSnappedIfSnapped(entityId);
			}
		}
		gameEntity.PlayThrowFx();
		Action onReleased = gameEntity.OnReleased;
		if (onReleased != null)
		{
			onReleased();
		}
		CustomGameMode.OnEntityGrabbed(gameEntity, false);
		GRBadge component2 = gameEntity.GetComponent<GRBadge>();
		if (component2 != null)
		{
			GRPlayer grplayer = GRPlayer.Get(thrownByPlayer.ActorNumber);
			if (grplayer != null)
			{
				grplayer.AttachBadge(component2);
			}
		}
	}

	public void RequestSnapEntity(GameEntityId entityId, bool isLeftHand, SnapJointType jointType)
	{
		GameEntity gameEntity = this.GetGameEntity(entityId);
		if (gameEntity == null)
		{
			return;
		}
		Vector3 position = gameEntity.transform.position;
		Quaternion rotation = gameEntity.transform.rotation;
		if (!this.IsAuthority())
		{
			this.SnapEntityLocal(entityId, isLeftHand, position, rotation, (int)jointType, NetPlayer.Get(PhotonNetwork.LocalPlayer));
		}
		this.photonView.RPC("RequestSnapEntityRPC", this.GetAuthorityPlayer(), new object[]
		{
			this.GetNetIdFromEntityId(entityId),
			isLeftHand,
			position,
			rotation,
			(int)jointType
		});
		PhotonNetwork.SendAllOutgoingCommands();
	}

	[PunRPC]
	public void RequestSnapEntityRPC(int entityNetId, bool isLeftHand, Vector3 position, Quaternion rotation, int jointType, PhotonMessageInfo info)
	{
		if (this.IsValidAuthorityRPC(info.Sender, entityNetId))
		{
			float num = 10000f;
			if ((in position).IsValid(in num) && (in rotation).IsValid() && this.IsPositionInZone(position))
			{
				GamePlayer gamePlayer = GamePlayer.GetGamePlayer(info.Sender);
				if (gamePlayer == null || !GameEntityManager.IsPlayerHandNearPosition(gamePlayer, position, isLeftHand, false, 16f) || !gamePlayer.IsHoldingEntity(this.GetEntityIdFromNetId(entityNetId), isLeftHand) || !gamePlayer.netSnapLimiter.CheckCallTime(Time.time))
				{
					return;
				}
				this.photonView.RPC("SnapEntityRPC", RpcTarget.All, new object[] { entityNetId, isLeftHand, position, rotation, jointType, info.Sender, info.SentServerTime });
				PhotonNetwork.SendAllOutgoingCommands();
				return;
			}
		}
	}

	[PunRPC]
	public void SnapEntityRPC(int entityNetId, bool isLeftHand, Vector3 position, Quaternion rotation, int jointType, Player thrownByPlayer, double snapTime, PhotonMessageInfo info)
	{
		if (this.IsValidClientRPC(info.Sender, entityNetId, position) && !this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.ThrowEntity))
		{
			float num = 10000f;
			if ((in position).IsValid(in num) && (in rotation).IsValid())
			{
				if (!this.IsAuthority() && thrownByPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
				{
					return;
				}
				this.SnapEntityLocal(this.GetEntityIdFromNetId(entityNetId), isLeftHand, position, rotation, jointType, NetPlayer.Get(thrownByPlayer));
				return;
			}
		}
	}

	private void SnapEntityLocal(GameEntityId gameEntityId, bool isLeftHand, Vector3 position, Quaternion rotation, int jointType, NetPlayer snappedByPlayer)
	{
		if (gameEntityId.index < 0 || gameEntityId.index >= this.entities.Count)
		{
			return;
		}
		GameEntity gameEntity = this.entities[gameEntityId.index];
		if (gameEntity == null)
		{
			return;
		}
		if (snappedByPlayer == null)
		{
			return;
		}
		if (snappedByPlayer.IsLocal && gameEntity.heldByActorNumber != snappedByPlayer.ActorNumber && gameEntity.lastHeldByActorNumber == snappedByPlayer.ActorNumber)
		{
			return;
		}
		GamePlayer gamePlayer = null;
		this.TryDetachCompletely(gameEntity);
		SuperInfectionSnapPoint superInfectionSnapPoint;
		if (jointType == 64)
		{
			gameEntity.GetComponent<GameSnappable>();
			superInfectionSnapPoint = SuperInfectionSnapPointManager.FindSnapPoint(gamePlayer, (SnapJointType)jointType);
		}
		else
		{
			gamePlayer = GamePlayer.GetGamePlayer(snappedByPlayer.ActorNumber);
			superInfectionSnapPoint = SuperInfectionSnapPointManager.FindSnapPoint(gamePlayer, (SnapJointType)jointType);
			int num = -1;
			if (jointType == 1)
			{
				num = GamePlayer.GetHandIndex(true);
			}
			if (jointType == 4)
			{
				num = GamePlayer.GetHandIndex(false);
			}
			if (jointType == 128)
			{
				num = GamePlayer.GetHandIndex(true);
			}
			if (jointType == 256)
			{
				num = GamePlayer.GetHandIndex(false);
			}
			if (num != -1)
			{
				gamePlayer.SetSnapped(gameEntityId, num, this);
			}
		}
		if (superInfectionSnapPoint == null)
		{
			return;
		}
		if (superInfectionSnapPoint.HasSnapped())
		{
			GameEntity snappedEntity = superInfectionSnapPoint.GetSnappedEntity();
			snappedEntity.transform.SetParent(null);
			snappedEntity.transform.SetLocalPositionAndRotation(position, rotation);
			Rigidbody component = snappedEntity.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.isKinematic = false;
				component.constraints = RigidbodyConstraints.None;
				component.position = position;
				component.rotation = rotation;
				component.linearVelocity = Vector3.up * 5f;
			}
			snappedEntity.heldByActorNumber = -1;
			snappedEntity.heldByHandIndex = -1;
			snappedEntity.snappedByActorNumber = -1;
			snappedEntity.snappedJoint = SnapJointType.None;
			snappedEntity.PlayThrowFx();
			Action onReleased = snappedEntity.OnReleased;
			if (onReleased != null)
			{
				onReleased();
			}
		}
		superInfectionSnapPoint.Snapped(gameEntity);
		gameEntity.transform.SetParent(superInfectionSnapPoint.transform);
		gameEntity.transform.SetLocalPositionAndRotation(position, rotation);
		Rigidbody component2 = gameEntity.GetComponent<Rigidbody>();
		if (component2 != null)
		{
			component2.isKinematic = true;
		}
		Vector3 zero = Vector3.zero;
		Quaternion identity = Quaternion.identity;
		GameSnappable component3 = gameEntity.GetComponent<GameSnappable>();
		if (component3 != null)
		{
			component3.GetSnapOffset((SnapJointType)jointType, out zero, out identity);
		}
		gameEntity.transform.localPosition = zero;
		gameEntity.transform.localRotation = identity;
		gameEntity.snappedByActorNumber = snappedByPlayer.ActorNumber;
		gameEntity.snappedJoint = (SnapJointType)jointType;
		if (component3 != null)
		{
			component3.OnSnap();
		}
		Action onSnapped = gameEntity.OnSnapped;
		if (onSnapped != null)
		{
			onSnapped();
		}
		gameEntity.PlaySnapFx();
	}

	public void SnapEntityOnCreate(GameEntityId gameEntityId, bool isLeftHand, Vector3 localPosition, Quaternion localRotation, int jointType, NetPlayer grabbedByPlayer)
	{
		this.SnapEntityLocal(gameEntityId, isLeftHand, localPosition, localRotation, jointType, grabbedByPlayer);
	}

	private void TryUnsnapLocal(GameEntity gameEntity)
	{
		if (gameEntity == null)
		{
			return;
		}
		GamePlayer gamePlayer = GamePlayer.GetGamePlayer(gameEntity.snappedByActorNumber);
		if (gamePlayer != null)
		{
			gamePlayer.ClearSnappedIfSnapped(gameEntity.id);
		}
		GameSnappable component = gameEntity.GetComponent<GameSnappable>();
		if (component != null && component.snappedToJoint != null && component.snappedToJoint.jointType != SnapJointType.None)
		{
			SuperInfectionSnapPoint superInfectionSnapPoint = SuperInfectionSnapPointManager.FindSnapPoint(gamePlayer, component.snappedToJoint.jointType);
			if (superInfectionSnapPoint == null)
			{
				superInfectionSnapPoint = component.snappedToJoint;
			}
			component.OnUnsnap();
			superInfectionSnapPoint.Unsnapped();
			Action onUnsnapped = gameEntity.OnUnsnapped;
			if (onUnsnapped != null)
			{
				onUnsnapped();
			}
		}
		gameEntity.snappedByActorNumber = -1;
		gameEntity.snappedJoint = SnapJointType.None;
	}

	public void RequestAttachEntity(GameEntityId entityId, GameEntityId attachToEntityId, int slotId, Vector3 localPosition, Quaternion localRotation)
	{
		if (this.GetGameEntity(entityId) == null)
		{
			return;
		}
		if (!this.IsAuthority())
		{
			this.AttachEntityLocal(entityId, attachToEntityId, slotId, localPosition, localRotation);
		}
		this.photonView.RPC("RequestAttachEntityRPC", this.GetAuthorityPlayer(), new object[]
		{
			this.GetNetIdFromEntityId(entityId),
			this.GetNetIdFromEntityId(attachToEntityId),
			slotId,
			localPosition,
			localRotation
		});
		PhotonNetwork.SendAllOutgoingCommands();
	}

	public void RequestAttachEntityAuthority(GameEntityId entityId, GameEntityId attachToEntityId, int slotId, Vector3 localPosition, Quaternion localRotation)
	{
		if (this.GetGameEntity(entityId) == null)
		{
			return;
		}
		if (!this.IsAuthority())
		{
			return;
		}
		this.photonView.RPC("AttachEntityRPC", RpcTarget.All, new object[]
		{
			this.GetNetIdFromEntityId(entityId),
			this.GetNetIdFromEntityId(attachToEntityId),
			slotId,
			localPosition,
			localRotation,
			null,
			PhotonNetwork.Time
		});
		PhotonNetwork.SendAllOutgoingCommands();
	}

	[PunRPC]
	public void RequestAttachEntityRPC(int entityNetId, int attachToEntityNetId, int slotId, Vector3 localPosition, Quaternion localRotation, PhotonMessageInfo info)
	{
		bool flag = !this.IsValidNetId(attachToEntityNetId);
		if (this.IsValidAuthorityRPC(info.Sender, entityNetId))
		{
			float num = 10000f;
			if ((in localPosition).IsValid(in num) && (in localRotation).IsValid())
			{
				if (!flag)
				{
					if (localPosition.sqrMagnitude > 4f || !this.IsEntityNearEntity(entityNetId, attachToEntityNetId, 16f))
					{
						return;
					}
				}
				else if (!this.IsPositionInZone(localPosition))
				{
					return;
				}
				GameEntity gameEntityFromNetId = this.GetGameEntityFromNetId(entityNetId);
				if (gameEntityFromNetId == null)
				{
					return;
				}
				GameDockable component = gameEntityFromNetId.GetComponent<GameDockable>();
				if (component == null)
				{
					return;
				}
				GameEntity gameEntityFromNetId2 = this.GetGameEntityFromNetId(attachToEntityNetId);
				if (gameEntityFromNetId2 != null)
				{
					GameDock component2 = gameEntityFromNetId2.GetComponent<GameDock>();
					if (component2 == null)
					{
						return;
					}
					if (!component2.CanDock(component))
					{
						return;
					}
				}
				GamePlayer gamePlayer = GamePlayer.GetGamePlayer(info.Sender);
				if (gamePlayer == null || !gamePlayer.IsHoldingEntity(this.GetEntityIdFromNetId(entityNetId)) || !gamePlayer.netSnapLimiter.CheckCallTime(Time.time))
				{
					return;
				}
				this.photonView.RPC("AttachEntityRPC", RpcTarget.All, new object[] { entityNetId, attachToEntityNetId, slotId, localPosition, localRotation, info.Sender, info.SentServerTime });
				PhotonNetwork.SendAllOutgoingCommands();
				return;
			}
		}
	}

	[PunRPC]
	public void AttachEntityRPC(int entityNetId, int attachToEntityNetId, int slotId, Vector3 localPosition, Quaternion localRotation, Player attachedByPlayer, double snapTime, PhotonMessageInfo info)
	{
		if (this.IsValidClientRPC(info.Sender, entityNetId) && this.IsValidNetId(attachToEntityNetId) && !this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.ThrowEntity))
		{
			float num = 10000f;
			if ((in localPosition).IsValid(in num) && (in localRotation).IsValid())
			{
				if (!this.IsAuthority() && attachedByPlayer != null && attachedByPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
				{
					return;
				}
				this.AttachEntityLocal(this.GetEntityIdFromNetId(entityNetId), this.GetEntityIdFromNetId(attachToEntityNetId), slotId, localPosition, localRotation);
				return;
			}
		}
	}

	private void AttachEntityLocal(GameEntityId gameEntityId, GameEntityId attachToEntityId, int slotId, Vector3 localPosition, Quaternion localRotation)
	{
		if (gameEntityId.index < 0 || gameEntityId.index >= this.entities.Count)
		{
			return;
		}
		GameEntity gameEntity = this.entities[gameEntityId.index];
		if (gameEntity == null)
		{
			return;
		}
		GameEntity gameEntity2 = this.entities[attachToEntityId.index];
		this.TryDetachCompletely(gameEntity);
		bool flag = gameEntity2 == null;
		Transform transform = ((gameEntity2 == null) ? null : gameEntity2.transform);
		gameEntity.transform.SetParent(transform);
		gameEntity.transform.SetLocalPositionAndRotation(localPosition, localRotation);
		gameEntity.attachedToEntityId = (flag ? GameEntityId.Invalid : gameEntity2.id);
		Rigidbody component = gameEntity.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = !flag;
			component.constraints = RigidbodyConstraints.None;
		}
		GameDockable component2 = gameEntity.GetComponent<GameDockable>();
		if (gameEntity2 != null)
		{
			Action onAttached = gameEntity.OnAttached;
			if (onAttached != null)
			{
				onAttached();
			}
			GameDock component3 = gameEntity2.GetComponent<GameDock>();
			if (component3 != null)
			{
				component3.OnDock(gameEntity, gameEntity2);
				if (component2 != null)
				{
					component2.OnDock(gameEntity, gameEntity2);
				}
			}
		}
	}

	private void TryDetachLocal(GameEntity gameEntity)
	{
		if (gameEntity == null)
		{
			return;
		}
		if (gameEntity.attachedToEntityId != GameEntityId.Invalid)
		{
			GameEntity gameEntity2 = this.entities[gameEntity.attachedToEntityId.index];
			if (gameEntity2 != null)
			{
				GameDock component = gameEntity2.GetComponent<GameDock>();
				if (component != null)
				{
					component.OnUndock(gameEntity, gameEntity2);
					GameDockable component2 = gameEntity.GetComponent<GameDockable>();
					if (component2 != null)
					{
						component2.OnUndock(gameEntity, gameEntity2);
					}
				}
			}
		}
		if (gameEntity.attachedToEntityId != GameEntityId.Invalid)
		{
			Action onDetached = gameEntity.OnDetached;
			if (onDetached != null)
			{
				onDetached();
			}
		}
		gameEntity.attachedToEntityId = GameEntityId.Invalid;
	}

	public void TryDetachCompletely(GameEntity gameEntity)
	{
		this.TryRemoveFromHandLocal(gameEntity);
		this.TryUnsnapLocal(gameEntity);
		this.TryDetachLocal(gameEntity);
	}

	private void TryRemoveFromHandLocal(GameEntity gameEntity)
	{
		GameEntityId id = gameEntity.id;
		int heldByActorNumber = gameEntity.heldByActorNumber;
		GamePlayer gamePlayer = GamePlayer.GetGamePlayer(heldByActorNumber);
		if (gamePlayer != null)
		{
			bool flag = heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
			gamePlayer.ClearGrabbedIfHeld(id);
			if (flag)
			{
				GamePlayerLocal.instance.ClearGrabbedIfHeld(id);
			}
			Action onReleased = gameEntity.OnReleased;
			if (onReleased != null)
			{
				onReleased();
			}
		}
		gameEntity.heldByActorNumber = -1;
		gameEntity.heldByHandIndex = -1;
	}

	public void AttachEntityOnCreate(GameEntityId gameEntityId, bool isLeftHand, Vector3 localPosition, Quaternion localRotation, int jointType, NetPlayer grabbedByPlayer)
	{
		this.SnapEntityLocal(gameEntityId, isLeftHand, localPosition, localRotation, jointType, grabbedByPlayer);
	}

	public void RequestHit(GameHitData hit)
	{
		GameHittable gameComponent = this.GetGameComponent<GameHittable>(hit.hitEntityId);
		if (gameComponent == null)
		{
			return;
		}
		gameComponent.ApplyHit(hit);
		base.SendRPC("RequestHitRPC", this.GetAuthorityPlayer(), new object[]
		{
			this.GetNetIdFromEntityId(hit.hitEntityId),
			this.GetNetIdFromEntityId(hit.hitByEntityId),
			hit.hitTypeId,
			hit.hitEntityPosition,
			hit.hitPosition,
			hit.hitImpulse
		});
	}

	[PunRPC]
	public void RequestHitRPC(int hittableNetId, int hitByNetId, int hitTypeId, Vector3 entityPosition, Vector3 hitPosition, Vector3 hitImpulse, PhotonMessageInfo info)
	{
		float num = 10000f;
		if ((in entityPosition).IsValid(in num))
		{
			float num2 = 10000f;
			if ((in hitPosition).IsValid(in num2))
			{
				float num3 = 10000f;
				if ((in hitImpulse).IsValid(in num3) && this.IsValidAuthorityRPC(info.Sender, hittableNetId, entityPosition) && this.IsPositionInZone(hitPosition))
				{
					GamePlayer gamePlayer;
					if (!GamePlayer.TryGetGamePlayer(info.Sender, out gamePlayer) || !gamePlayer.netImpulseLimiter.CheckCallTime(Time.time))
					{
						return;
					}
					GameEntityId entityIdFromNetId = this.GetEntityIdFromNetId(hittableNetId);
					GameHittable gameComponent = this.GetGameComponent<GameHittable>(entityIdFromNetId);
					if (gameComponent == null)
					{
						return;
					}
					GameHitData gameHitData = new GameHitData
					{
						hitTypeId = hitTypeId,
						hitEntityId = entityIdFromNetId,
						hitByEntityId = this.GetEntityIdFromNetId(hitByNetId),
						hitEntityPosition = entityPosition,
						hitPosition = hitPosition,
						hitImpulse = hitImpulse
					};
					if (!gameComponent.IsHitValid(gameHitData))
					{
						return;
					}
					base.SendRPC("ApplyHitRPC", RpcTarget.All, new object[] { hittableNetId, hitByNetId, hitTypeId, entityPosition, hitPosition, hitImpulse, info.Sender });
					return;
				}
			}
		}
	}

	[PunRPC]
	public void ApplyHitRPC(int hittableNetId, int hitByNetId, int hitTypeId, Vector3 entityPosition, Vector3 hitPosition, Vector3 hitImpulse, Player player, PhotonMessageInfo info)
	{
		float num = 10000f;
		if ((in hitPosition).IsValid(in num))
		{
			float num2 = 10000f;
			if ((in hitImpulse).IsValid(in num2) && this.IsValidClientRPC(info.Sender, hittableNetId, entityPosition) && !this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.HitEntity) && player != null)
			{
				if (player.IsLocal)
				{
					return;
				}
				if (this.GetGameEntity(this.GetEntityIdFromNetId(hittableNetId)) == null)
				{
					return;
				}
				hitImpulse = Vector3.ClampMagnitude(hitImpulse, 100f);
				GameEntityId entityIdFromNetId = this.GetEntityIdFromNetId(hittableNetId);
				GameHitData gameHitData = new GameHitData
				{
					hitTypeId = hitTypeId,
					hitEntityId = entityIdFromNetId,
					hitByEntityId = this.GetEntityIdFromNetId(hitByNetId),
					hitEntityPosition = entityPosition,
					hitPosition = hitPosition,
					hitImpulse = hitImpulse,
					hitAmount = 0
				};
				GameEntity gameEntity = this.GetGameEntity(this.GetEntityIdFromNetId(hitByNetId));
				GameHittable gameComponent = this.GetGameComponent<GameHittable>(entityIdFromNetId);
				if (gameEntity != null)
				{
					GameHitter component = gameEntity.GetComponent<GameHitter>();
					if (component != null)
					{
						gameHitData.hitAmount = component.CalcHitAmount((GameHitType)hitTypeId, gameComponent, gameEntity);
					}
				}
				if (gameComponent != null)
				{
					gameComponent.ApplyHit(gameHitData);
				}
				return;
			}
		}
	}

	public bool IsPlayerHandNearEntity(GamePlayer player, int entityNetId, bool isLeftHand, bool checkBothHands, float acceptableRadius = 16f)
	{
		GameEntityId entityIdFromNetId = this.GetEntityIdFromNetId(entityNetId);
		GameEntity gameEntity = this.GetGameEntity(entityIdFromNetId);
		return !(gameEntity == null) && GameEntityManager.IsPlayerHandNearPosition(player, gameEntity.transform.position, isLeftHand, checkBothHands, acceptableRadius);
	}

	public static bool IsPlayerHandNearPosition(GamePlayer player, Vector3 worldPosition, bool isLeftHand, bool checkBothHands, float acceptableRadius = 16f)
	{
		bool flag = true;
		if (player != null && player.rig != null)
		{
			if (isLeftHand || checkBothHands)
			{
				flag = (worldPosition - player.rig.leftHandTransform.position).sqrMagnitude < acceptableRadius * acceptableRadius;
			}
			if (!isLeftHand || checkBothHands)
			{
				float sqrMagnitude = (worldPosition - player.rig.rightHandTransform.position).sqrMagnitude;
				flag = flag && sqrMagnitude < acceptableRadius * acceptableRadius;
			}
		}
		return flag;
	}

	public bool IsEntityNearEntity(int entityNetId, int otherEntityNetId, float acceptableRadius = 16f)
	{
		GameEntityId entityIdFromNetId = this.GetEntityIdFromNetId(otherEntityNetId);
		GameEntity gameEntity = this.GetGameEntity(entityIdFromNetId);
		return !(gameEntity == null) && this.IsEntityNearPosition(entityNetId, gameEntity.transform.position, acceptableRadius);
	}

	public bool IsEntityNearPosition(int entityNetId, Vector3 position, float acceptableRadius = 16f)
	{
		GameEntityId entityIdFromNetId = this.GetEntityIdFromNetId(entityNetId);
		GameEntity gameEntity = this.GetGameEntity(entityIdFromNetId);
		return !(gameEntity == null) && Vector3.SqrMagnitude(gameEntity.transform.position - position) < acceptableRadius * acceptableRadius;
	}

	public static bool ValidateDataType<T>(object obj, out T dataAsType)
	{
		if (obj is T)
		{
			dataAsType = (T)((object)obj);
			return true;
		}
		dataAsType = default(T);
		return false;
	}

	private void ClearZone(bool ignoreHeldGadgets = false)
	{
		if (ignoreHeldGadgets)
		{
			List<GameEntity> list = GamePlayerLocal.instance.gamePlayer.HeldAndSnappedEntities(null);
			Vector3 position = VRRig.LocalRig.transform.position;
			int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i] == null || list[i].manager != this)
				{
					list.RemoveAt(i);
				}
				else
				{
					GameEntity gameEntity = list[i];
					bool flag = true;
					int netIdFromEntityId = this.GetNetIdFromEntityId(gameEntity.id);
					int num = 0;
					while (num < this.zoneComponents.Count && flag)
					{
						flag &= this.zoneComponents[num].ValidateMigratedGameEntity(netIdFromEntityId, gameEntity.typeId, position, Quaternion.identity, gameEntity.createData, actorNumber);
						num++;
					}
					if (!flag)
					{
						list.RemoveAt(i);
					}
				}
			}
			for (int j = this.entities.Count - 1; j >= 0; j--)
			{
				if (!(this.entities[j] == null) && !list.Contains(this.entities[j]))
				{
					this.DestroyItemLocal(this.entities[j].id);
				}
			}
			GamePlayerLocal.instance.gamePlayer.DidJoinWithItems = false;
		}
		else
		{
			for (int k = 0; k < this.entities.Count; k++)
			{
				if (this.entities[k] != null && this.entities[k].manager == this)
				{
					this.DestroyItemLocal(this.entities[k].id);
				}
			}
			GamePlayer gamePlayerRef = VRRig.LocalRig.GamePlayerRef;
			if (gamePlayerRef != null)
			{
				gamePlayerRef.ClearZone(this);
			}
		}
		for (int l = 0; l < this.entities.Count; l++)
		{
			if (this.entities[l] != null && this.entities[l].manager != this)
			{
				this.entities[l] = null;
			}
		}
		foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
		{
			GamePlayer component = vrrig.GetComponent<GamePlayer>();
			if (!component.IsLocal())
			{
				component.ClearZone(this);
			}
		}
		this.gameEntityData.Clear();
		for (int m = 0; m < this.zoneComponents.Count; m++)
		{
			this.zoneComponents[m].OnZoneClear(this.zoneClearReason);
		}
	}

	public int SerializeGameState(int zoneId, byte[] bytes, int maxBytes)
	{
		MemoryStream memoryStream = new MemoryStream(bytes);
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		for (int i = 0; i < this.zoneComponents.Count; i++)
		{
			this.zoneComponents[i].SerializeZoneData(binaryWriter);
		}
		GameEntityManager.tempEntitiesToSerialize.Clear();
		for (int j = 0; j < this.entities.Count; j++)
		{
			GameEntity gameEntity = this.entities[j];
			if (!(gameEntity == null))
			{
				GameEntityManager.tempEntitiesToSerialize.Add(gameEntity);
			}
		}
		binaryWriter.Write(GameEntityManager.tempEntitiesToSerialize.Count);
		for (int k = 0; k < GameEntityManager.tempEntitiesToSerialize.Count; k++)
		{
			GameEntity gameEntity2 = GameEntityManager.tempEntitiesToSerialize[k];
			if (!(gameEntity2 == null))
			{
				int netIdFromEntityId = this.GetNetIdFromEntityId(gameEntity2.id);
				binaryWriter.Write(netIdFromEntityId);
				binaryWriter.Write(gameEntity2.typeId);
				long num = BitPackUtils.PackWorldPosForNetwork(gameEntity2.transform.position);
				int num2 = BitPackUtils.PackQuaternionForNetwork(gameEntity2.transform.rotation);
				binaryWriter.Write(num);
				binaryWriter.Write(num2);
			}
		}
		for (int l = 0; l < GameEntityManager.tempEntitiesToSerialize.Count; l++)
		{
			GameEntity gameEntity3 = GameEntityManager.tempEntitiesToSerialize[l];
			if (!(gameEntity3 == null))
			{
				int netIdFromEntityId2 = this.GetNetIdFromEntityId(gameEntity3.id);
				binaryWriter.Write(netIdFromEntityId2);
				binaryWriter.Write(gameEntity3.createData);
				binaryWriter.Write(gameEntity3.GetState());
				int num3 = -1;
				GameEntity gameEntity4 = this.GetGameEntity(gameEntity3.attachedToEntityId);
				if (gameEntity4 != null)
				{
					num3 = this.GetNetIdFromEntityId(gameEntity4.id);
				}
				binaryWriter.Write(num3);
				if (num3 != -1)
				{
					long num4 = BitPackUtils.PackHandPosRotForNetwork(gameEntity3.transform.localPosition, gameEntity3.transform.localRotation);
					binaryWriter.Write(num4);
				}
				GameAgent component = gameEntity3.GetComponent<GameAgent>();
				bool flag = component != null;
				binaryWriter.Write(flag);
				if (flag)
				{
					long num5 = BitPackUtils.PackWorldPosForNetwork(component.navAgent.destination);
					binaryWriter.Write(num5);
					NetPlayer targetPlayer = component.targetPlayer;
					int num6 = ((targetPlayer != null) ? targetPlayer.ActorNumber : (-1));
					binaryWriter.Write(num6);
				}
				byte b = (byte)gameEntity3.entitySerialize.Count;
				binaryWriter.Write(b);
				for (int m = 0; m < (int)b; m++)
				{
					gameEntity3.entitySerialize[m].OnGameEntitySerialize(binaryWriter);
				}
				for (int n = 0; n < this.zoneComponents.Count; n++)
				{
					this.zoneComponents[n].SerializeZoneEntityData(binaryWriter, gameEntity3);
				}
			}
		}
		GameEntityManager.tempRigs.Clear();
		GameEntityManager.tempRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(GameEntityManager.tempRigs);
		for (int num7 = GameEntityManager.tempRigs.Count - 1; num7 >= 0; num7--)
		{
			if (GameEntityManager.tempRigs[num7].OwningNetPlayer == null)
			{
				GameEntityManager.tempRigs.RemoveAt(num7);
			}
		}
		int count = GameEntityManager.tempRigs.Count;
		binaryWriter.Write(count);
		for (int num8 = 0; num8 < GameEntityManager.tempRigs.Count; num8++)
		{
			VRRig vrrig = GameEntityManager.tempRigs[num8];
			NetPlayer owningNetPlayer = vrrig.OwningNetPlayer;
			binaryWriter.Write(owningNetPlayer.ActorNumber);
			GamePlayer gamePlayerRef = vrrig.GamePlayerRef;
			bool flag2 = gamePlayerRef != null;
			binaryWriter.Write(flag2);
			if (flag2)
			{
				gamePlayerRef.SerializeNetworkState(binaryWriter, owningNetPlayer, this);
				for (int num9 = 0; num9 < this.zoneComponents.Count; num9++)
				{
					this.zoneComponents[num9].SerializeZonePlayerData(binaryWriter, owningNetPlayer.ActorNumber);
				}
			}
		}
		return (int)memoryStream.Position;
	}

	public void DeserializeTableState(byte[] bytes, int numBytes)
	{
		if (numBytes <= 0)
		{
			return;
		}
		GameEntityManager.tempAttachments.Clear();
		using (MemoryStream memoryStream = new MemoryStream(bytes))
		{
			using (BinaryReader binaryReader = new BinaryReader(memoryStream))
			{
				for (int i = 0; i < this.zoneComponents.Count; i++)
				{
					this.zoneComponents[i].DeserializeZoneData(binaryReader);
				}
				int num = binaryReader.ReadInt32();
				for (int j = 0; j < num; j++)
				{
					int num2 = binaryReader.ReadInt32();
					int num3 = binaryReader.ReadInt32();
					long num4 = binaryReader.ReadInt64();
					int num5 = binaryReader.ReadInt32();
					Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(num4);
					Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(num5);
					this.CreateItemLocal(num2, num3, vector, quaternion);
				}
				int k = 0;
				while (k < num)
				{
					int num6 = binaryReader.ReadInt32();
					long num7 = binaryReader.ReadInt64();
					long num8 = binaryReader.ReadInt64();
					GameEntity gameEntityFromNetId = this.GetGameEntityFromNetId(num6);
					if (gameEntityFromNetId != null)
					{
						this.InitItemLocal(gameEntityFromNetId, num7);
						gameEntityFromNetId.SetState(num8);
					}
					int num9 = binaryReader.ReadInt32();
					if (num9 == -1)
					{
						goto IL_014A;
					}
					long num10 = binaryReader.ReadInt64();
					if (!(gameEntityFromNetId == null))
					{
						Vector3 vector2;
						Quaternion quaternion2;
						BitPackUtils.UnpackHandPosRotFromNetwork(num10, out vector2, out quaternion2);
						GameEntityManager.tempAttachments.Add(new GameEntityManager.AttachmentData
						{
							entityNetId = num6,
							attachToEntityNetId = num9,
							localPosition = vector2,
							localRotation = quaternion2
						});
						goto IL_014A;
					}
					IL_0200:
					k++;
					continue;
					IL_014A:
					if (binaryReader.ReadBoolean())
					{
						long num11 = binaryReader.ReadInt64();
						int num12 = binaryReader.ReadInt32();
						Vector3 vector3 = BitPackUtils.UnpackWorldPosFromNetwork(num11);
						GameAgent component = gameEntityFromNetId.GetComponent<GameAgent>();
						if (component != null)
						{
							if (component.IsOnNavMesh())
							{
								component.navAgent.destination = vector3;
							}
							component.targetPlayer = NetworkSystem.Instance.GetPlayer(num12);
						}
					}
					byte b = binaryReader.ReadByte();
					for (int l = 0; l < (int)b; l++)
					{
						gameEntityFromNetId.entitySerialize[l].OnGameEntityDeserialize(binaryReader);
					}
					for (int m = 0; m < this.zoneComponents.Count; m++)
					{
						this.zoneComponents[m].DeserializeZoneEntityData(binaryReader, gameEntityFromNetId);
					}
					goto IL_0200;
				}
				int num13 = binaryReader.ReadInt32();
				for (int n = 0; n < num13; n++)
				{
					int num14 = binaryReader.ReadInt32();
					if (binaryReader.ReadBoolean())
					{
						GamePlayer gamePlayer;
						GamePlayer.TryGetGamePlayer(num14, out gamePlayer);
						GamePlayer.DeserializeNetworkState(binaryReader, gamePlayer, this);
						for (int num15 = 0; num15 < this.zoneComponents.Count; num15++)
						{
							this.zoneComponents[num15].DeserializeZonePlayerData(binaryReader, num14);
						}
					}
				}
				for (int num16 = 0; num16 < GameEntityManager.tempAttachments.Count; num16++)
				{
					GameEntityManager.AttachmentData attachmentData = GameEntityManager.tempAttachments[num16];
					GameEntityId entityIdFromNetId = this.GetEntityIdFromNetId(attachmentData.entityNetId);
					GameEntityId entityIdFromNetId2 = this.GetEntityIdFromNetId(attachmentData.attachToEntityNetId);
					if (!(entityIdFromNetId == entityIdFromNetId2))
					{
						this.AttachEntityLocal(entityIdFromNetId, entityIdFromNetId2, 0, attachmentData.localPosition, attachmentData.localRotation);
					}
				}
			}
		}
	}

	private void UpdateZoneState()
	{
		GameEntityManager.tempRigs.Clear();
		GameEntityManager.tempRigs.Add(VRRig.LocalRig);
		VRRigCache.Instance.GetAllUsedRigs(GameEntityManager.tempRigs);
		this.UpdateAuthority(GameEntityManager.tempRigs);
		if (this.IsAuthority())
		{
			this.UpdateClientsFromAuthority(GameEntityManager.tempRigs);
			this.UpdateZoneStateAuthority();
		}
		else
		{
			this.UpdateZoneStateClient();
		}
		for (int i = this.zoneStateData.zonePlayers.Count - 1; i >= 0; i--)
		{
			if (this.zoneStateData.zonePlayers[i] == null)
			{
				this.zoneStateData.zonePlayers.RemoveAt(i);
			}
		}
	}

	private void UpdateAuthority(List<VRRig> allRigs)
	{
		if (!PhotonNetwork.InRoom && base.IsMine)
		{
			if (!this.IsAuthority())
			{
				this.guard.SetOwnership(NetworkSystem.Instance.LocalPlayer, false, false);
				return;
			}
		}
		else if (this.IsAuthority() && !this.IsInZone())
		{
			Player player = null;
			if (this.useRandomCheckForAuthority)
			{
				int num = 0;
				while (player == null)
				{
					if (num >= 10)
					{
						break;
					}
					num++;
					int num2 = Random.Range(0, allRigs.Count);
					VRRig vrrig = allRigs[num2];
					GamePlayer gamePlayer;
					if (GamePlayer.TryGetGamePlayer(vrrig, out gamePlayer) && !(gamePlayer.rig == null) && gamePlayer.rig.OwningNetPlayer != null && !gamePlayer.rig.isLocal && vrrig.zoneEntity.currentZone == this.zone)
					{
						player = gamePlayer.rig.OwningNetPlayer.GetPlayerRef();
					}
				}
			}
			else
			{
				for (int i = 0; i < allRigs.Count; i++)
				{
					VRRig vrrig2 = allRigs[i];
					GamePlayer gamePlayer2;
					if (GamePlayer.TryGetGamePlayer(vrrig2, out gamePlayer2) && !(gamePlayer2.rig == null) && gamePlayer2.rig.OwningNetPlayer != null && !gamePlayer2.rig.isLocal && vrrig2.zoneEntity.currentZone == this.zone)
					{
						player = gamePlayer2.rig.OwningNetPlayer.GetPlayerRef();
					}
				}
			}
			if (player != null && player != null)
			{
				this.guard.TransferOwnership(player, "");
			}
		}
	}

	private void UpdateClientsFromAuthority(List<VRRig> allRigs)
	{
		if (!this.IsInZone())
		{
			return;
		}
		for (int i = 0; i < this.zoneStateData.zoneStateRequests.Count; i++)
		{
			GameEntityManager.ZoneStateRequest zoneStateRequest = this.zoneStateData.zoneStateRequests[i];
			if (zoneStateRequest.player != null && zoneStateRequest.zone == this.zone)
			{
				this.SendZoneStateToPlayerOrTarget(zoneStateRequest.zone, zoneStateRequest.player, RpcTarget.MasterClient);
				zoneStateRequest.completed = true;
				this.zoneStateData.zoneStateRequests[i] = zoneStateRequest;
				this.zoneStateData.zoneStateRequests.RemoveAt(i);
				return;
			}
			this.zoneStateData.zoneStateRequests.RemoveAt(i);
			i--;
		}
	}

	public void TestSerializeTableState()
	{
		GameEntityManager.ClearByteBuffer(GameEntityManager.tempSerializeGameState);
		int num = this.SerializeGameState((int)this.zone, GameEntityManager.tempSerializeGameState, 15360);
		byte[] array = GZipStream.CompressBuffer(GameEntityManager.tempSerializeGameState);
		Debug.LogFormat("Test Serialize Game State Buffer Size Uncompressed {0}", new object[] { num });
		Debug.LogFormat("Test Serialize Game State Buffer Size Compressed {0}", new object[] { array.Length });
	}

	public static void ClearByteBuffer(byte[] buffer)
	{
		int num = buffer.Length;
		for (int i = 0; i < num; i++)
		{
			buffer[i] = 0;
		}
	}

	private void SendZoneStateToPlayerOrTarget(GTZone zone, Player player, RpcTarget target)
	{
		GameEntityManager.ClearByteBuffer(GameEntityManager.tempSerializeGameState);
		this.SerializeGameState((int)zone, GameEntityManager.tempSerializeGameState, 15360);
		byte[] array = GZipStream.CompressBuffer(GameEntityManager.tempSerializeGameState);
		byte[] array2 = new byte[512];
		int i = 0;
		int num = 0;
		int num2 = array.Length;
		while (i < num2)
		{
			int num3 = Mathf.Min(512, num2 - i);
			Array.Copy(array, i, array2, 0, num3);
			if (player != null)
			{
				this.photonView.RPC("SendTableDataRPC", player, new object[] { num, num2, array2 });
			}
			else
			{
				this.photonView.RPC("SendTableDataRPC", target, new object[] { num, num2, array2 });
			}
			i += num3;
			num++;
		}
	}

	[PunRPC]
	public void SendTableDataRPC(int packetNum, int totalBytes, byte[] bytes, PhotonMessageInfo info)
	{
		if (!this.IsAuthorityPlayer(info.Sender) || this.m_RpcSpamChecks.IsSpamming(GameEntityManager.RPC.SendTableData) || bytes == null || bytes.Length >= 15360)
		{
			return;
		}
		if (this.zoneStateData.state != GameEntityManager.ZoneState.WaitingForState)
		{
			return;
		}
		if (packetNum == 0)
		{
			this.zoneStateData.numRecievedStateBytes = 0;
			for (int i = 0; i < this.zoneStateData.recievedStateBytes.Length; i++)
			{
				this.zoneStateData.recievedStateBytes[i] = 0;
			}
		}
		Array.Copy(bytes, 0, this.zoneStateData.recievedStateBytes, this.zoneStateData.numRecievedStateBytes, bytes.Length);
		this.zoneStateData.numRecievedStateBytes += bytes.Length;
		if (this.zoneStateData.numRecievedStateBytes >= totalBytes)
		{
			if (this.superInfectionManager != null && this.superInfectionManager.zoneSuperInfection == null)
			{
				this.PendingTableData = true;
				return;
			}
			this.ResolveTableData();
		}
	}

	public void ResolveTableData()
	{
		this.PendingTableData = false;
		if (GameEntityManager.activeManager.IsNotNull() && GameEntityManager.activeManager != this)
		{
			GameEntityManager.activeManager.zoneClearReason = ZoneClearReason.LeaveZone;
			GameEntityManager.activeManager.ClearZone(false);
		}
		this.ClearZone(false);
		try
		{
			byte[] array = GZipStream.UncompressBuffer(this.zoneStateData.recievedStateBytes);
			int num = array.Length;
			this.DeserializeTableState(array, num);
			this.SetZoneState(GameEntityManager.ZoneState.Active);
			for (int i = 0; i < this.zoneComponents.Count; i++)
			{
				this.zoneComponents[i].OnZoneInit();
			}
		}
		catch (Exception)
		{
		}
	}

	private void UpdateZoneStateAuthority()
	{
		GamePlayer gamePlayer = GamePlayerLocal.instance.gamePlayer;
		if (gamePlayer == null || gamePlayer.rig == null || gamePlayer.rig.OwningNetPlayer == null)
		{
			return;
		}
		if (!this.IsInZone())
		{
			if (this.zoneStateData.state != GameEntityManager.ZoneState.WaitingToEnterZone)
			{
				this.zoneClearReason = ZoneClearReason.LeaveZone;
				this.SetZoneState(GameEntityManager.ZoneState.WaitingToEnterZone);
				return;
			}
			if (this.entities.Count > 0 && this.ShouldClearZone())
			{
				this.zoneClearReason = ZoneClearReason.LeaveZone;
				this.ClearZone(false);
				return;
			}
		}
		GameEntityManager.ZoneState state = this.zoneStateData.state;
		if (state > GameEntityManager.ZoneState.WaitingForState)
		{
			return;
		}
		if (this.IsInZone() && PhotonNetwork.InRoom)
		{
			this.SetZoneState(GameEntityManager.ZoneState.Active);
			for (int i = 0; i < this.zoneComponents.Count; i++)
			{
				this.zoneComponents[i].OnZoneCreate();
			}
			for (int j = 0; j < this.zoneComponents.Count; j++)
			{
				this.zoneComponents[j].OnZoneInit();
			}
		}
	}

	private void UpdateZoneStateClient()
	{
		GamePlayer gamePlayer = GamePlayerLocal.instance.gamePlayer;
		if (gamePlayer == null || gamePlayer.rig == null || gamePlayer.rig.OwningNetPlayer == null)
		{
			return;
		}
		if (!this.IsInZone())
		{
			if (this.zoneStateData.state != GameEntityManager.ZoneState.WaitingToEnterZone)
			{
				this.zoneClearReason = ZoneClearReason.LeaveZone;
				this.SetZoneState(GameEntityManager.ZoneState.WaitingToEnterZone);
				return;
			}
			if (this.entities.Count > 0 && this.ShouldClearZone())
			{
				this.zoneClearReason = ZoneClearReason.LeaveZone;
				this.ClearZone(false);
				return;
			}
		}
		GameEntityManager.ZoneState state = this.zoneStateData.state;
		if (state != GameEntityManager.ZoneState.WaitingToEnterZone)
		{
			if (state != GameEntityManager.ZoneState.WaitingToRequestState)
			{
				return;
			}
			if (Time.timeAsDouble - this.zoneStateData.stateStartTime > 1.0)
			{
				this.SetZoneState(GameEntityManager.ZoneState.WaitingForState);
				this.photonView.RPC("RequestZoneStateRPC", this.GetAuthorityPlayer(), new object[] { (int)this.zone });
				this.JoinWithItems(GamePlayerLocal.instance.gamePlayer.HeldAndSnappedEntities(null));
			}
		}
		else if (this.HasAuthority() && this.IsInZone() && !this.IsAuthority())
		{
			this.SetZoneState(GameEntityManager.ZoneState.WaitingToRequestState);
			return;
		}
	}

	private bool IsInZone()
	{
		bool flag = true;
		for (int i = 0; i < this.zoneComponents.Count; i++)
		{
			flag &= this.zoneComponents[i].IsZoneReady();
		}
		return flag;
	}

	private bool ShouldClearZone()
	{
		bool flag = false;
		for (int i = 0; i < this.zoneComponents.Count; i++)
		{
			flag |= this.zoneComponents[i].ShouldClearZone();
		}
		return flag;
	}

	private void SetZoneState(GameEntityManager.ZoneState newState)
	{
		if (newState == this.zoneStateData.state)
		{
			return;
		}
		this.zoneStateData.state = newState;
		this.zoneStateData.stateStartTime = Time.timeAsDouble;
		switch (this.zoneStateData.state)
		{
		case GameEntityManager.ZoneState.WaitingToEnterZone:
			if (!this.IsAuthority())
			{
				this.photonView.RPC("PlayerLeftZoneRPC", this.GetAuthorityPlayer(), Array.Empty<object>());
			}
			this.ClearZone(!this.ShouldClearZone() && this.zoneClearReason != ZoneClearReason.MigrateGameEntityZone);
			return;
		case GameEntityManager.ZoneState.WaitingToRequestState:
			break;
		case GameEntityManager.ZoneState.WaitingForState:
		{
			this.zoneStateData.numRecievedStateBytes = 0;
			for (int i = 0; i < this.zoneStateData.recievedStateBytes.Length; i++)
			{
				this.zoneStateData.recievedStateBytes[i] = 0;
			}
			return;
		}
		case GameEntityManager.ZoneState.Active:
			if (!(GameEntityManager.activeManager == this))
			{
				GameEntityManager activeManager = GameEntityManager.activeManager;
				GameEntityManager.activeManager = this;
				GamePlayerLocal.instance.MigrateToEntityManager(this);
				if (activeManager.IsNotNull())
				{
					activeManager.zoneClearReason = ZoneClearReason.MigrateGameEntityZone;
					activeManager.SetZoneState(GameEntityManager.ZoneState.WaitingToEnterZone);
				}
			}
			break;
		default:
			return;
		}
	}

	public void DebugSendState()
	{
		this.SetZoneState(GameEntityManager.ZoneState.WaitingToRequestState);
	}

	[PunRPC]
	public void RequestZoneStateRPC(int zoneId, PhotonMessageInfo info)
	{
		if (!this.IsAuthority())
		{
			return;
		}
		if (zoneId != (int)this.zone || this.zoneStateData.zoneStateRequests == null)
		{
			return;
		}
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(info.Sender, out gamePlayer))
		{
			return;
		}
		if (!gamePlayer.newJoinZoneLimiter.CheckCallTime(Time.time))
		{
			return;
		}
		for (int i = 0; i < this.zoneStateData.zoneStateRequests.Count; i++)
		{
			if (this.zoneStateData.zoneStateRequests[i].player == info.Sender)
			{
				return;
			}
		}
		this.zoneStateData.zoneStateRequests.Add(new GameEntityManager.ZoneStateRequest
		{
			player = info.Sender,
			zone = this.zone,
			completed = false
		});
	}

	public override void WriteDataFusion()
	{
	}

	public override void ReadDataFusion()
	{
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.superInfectionManager != null)
		{
			this.superInfectionManager.WriteDataPUN(stream, info);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.superInfectionManager != null)
		{
			this.superInfectionManager.ReadDataPUN(stream, info);
		}
	}

	void IMatchmakingCallbacks.OnJoinedRoom()
	{
		this.zoneClearReason = ZoneClearReason.JoinZone;
		this.SetZoneState(GameEntityManager.ZoneState.WaitingToEnterZone);
	}

	void IMatchmakingCallbacks.OnLeftRoom()
	{
		this.zoneClearReason = ZoneClearReason.Disconnect;
		this.SetZoneState(GameEntityManager.ZoneState.WaitingToEnterZone);
	}

	void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnCreatedRoom()
	{
	}

	void IMatchmakingCallbacks.OnPreLeavingRoom()
	{
	}

	void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
	}

	void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	void IInRoomCallbacks.OnPlayerLeftRoom(Player leavingPlayer)
	{
	}

	void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	public void OnRigDeactivated(RigContainer container)
	{
		if (this != GameEntityManager.activeManager)
		{
			return;
		}
		GamePlayer component = container.GetComponent<GamePlayer>();
		if (this.IsAuthority())
		{
			this.RequestDestroyItems(component.HeldAndSnappedItems(this));
		}
		component.ResetData();
	}

	public void OnOwnershipTransferred(NetPlayer toPlayer, NetPlayer fromPlayer)
	{
	}

	public bool OnOwnershipRequest(NetPlayer fromPlayer)
	{
		return false;
	}

	public void OnMyOwnerLeft()
	{
	}

	public bool OnMasterClientAssistedTakeoverRequest(NetPlayer fromPlayer, NetPlayer toPlayer)
	{
		return false;
	}

	public void OnMyCreatorLeft()
	{
	}

	[CompilerGenerated]
	internal static void <TryGrabLocal>g__TestAgainstBounds|146_0(Transform t, Bounds bounds, ref GameEntityManager.<>c__DisplayClass146_0 A_2)
	{
		Vector3 vector = t.InverseTransformPoint(A_2.handPosition);
		Vector3 vector2 = t.InverseTransformPoint(A_2.handPosition + A_2.slopProjection);
		float magnitude = bounds.extents.magnitude;
		Vector3 extents = bounds.extents;
		float num = Mathf.Min(magnitude / 2f, A_2.minimumObjectExtent);
		bounds.extents = new Vector3(Mathf.Max(bounds.extents.x, num), Mathf.Max(bounds.extents.y, num), Mathf.Max(bounds.extents.z, num));
		extents != bounds.extents;
		Vector3 vector3;
		float num2;
		Vector3 vector4;
		float num3;
		if (GameEntityManager.SegmentHitsBounds(bounds, vector, vector2, out vector3, out num2))
		{
			vector4 = ((num2 <= 0f) ? Vector3.zero : t.TransformVector(vector - vector3));
			num3 = vector4.magnitude - A_2.slopForSpeed;
		}
		else
		{
			vector4 = t.TransformVector(vector - bounds.ClosestPoint(vector));
			num3 = vector4.magnitude;
		}
		if (num3 < A_2.bestDist)
		{
			A_2.bestDist = num3;
			A_2.bestEntity = A_2.entity.id;
			A_2.closestPoint = A_2.handPosition - vector4;
		}
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
	}

	private const string preLog = "[GameEntityManager]  ";

	private const string preErr = "[GameEntityManager]  ERROR!!!  ";

	private const string preErrBeta = "[GameEntityManager]  ERROR!!!  (beta only log) ";

	private const int MAX_STATE_BYTES = 15360;

	private const int MAX_CHUNK_BYTES = 512;

	public const float MAX_LOCAL_MAGNITUDE_SQ = 6400f;

	public const float MAX_DISTANCE_FROM_HAND = 16f;

	public const float MAX_ENTITY_DIST = 16f;

	public const float MAX_THROW_SPEED_SQ = 1600f;

	public const int MAX_ENTITY_COUNT_PER_TYPE = 100;

	public const int INVALID_ID = -1;

	public const int INVALID_INDEX = -1;

	private static List<GameEntityManager> allManagers = new List<GameEntityManager>(8);

	public GTZone zone;

	public PhotonView photonView;

	public RequestableOwnershipGuard guard;

	public Player prevAuthorityPlayer;

	public BoxCollider zoneLimit;

	public bool useRandomCheckForAuthority;

	public GameAgentManager gameAgentManager;

	public GhostReactorManager ghostReactorManager;

	public CustomMapsGameManager customMapsManager;

	public SuperInfectionManager superInfectionManager;

	private List<IGameEntityZoneComponent> zoneComponents;

	private List<GameEntity> entities;

	private List<GameEntityData> gameEntityData;

	public List<GameEntity> tempFactoryItems;

	private Dictionary<int, GameObject> itemPrefabFactory;

	private Dictionary<int, int> priceLookupByEntityId;

	private List<GameEntity> tempEntities = new List<GameEntity>();

	private List<int> netIdsForCreate;

	private List<int> entityTypeIdsForCreate;

	private List<int> packedRotationsForCreate;

	private List<long> packedPositionsForCreate;

	private List<long> createDataForCreate;

	private float createCooldown = 0.24f;

	private float lastCreateSent;

	private List<int> netIdsForDelete;

	private float destroyCooldown = 0.25f;

	private float lastDestroySent;

	private List<int> netIdsForState;

	private List<long> statesForState;

	private float lastStateSent;

	private float stateCooldown;

	private Dictionary<int, int> netIdToIndex;

	private NativeArray<int> netIds;

	private Dictionary<int, int> createdItemTypeCount;

	private ZoneClearReason zoneClearReason;

	[NonSerialized]
	public Action<GameEntity> OnEntityRemoved;

	[NonSerialized]
	public Action<GameEntity> OnEntityAdded;

	private GameEntityManager.ZoneStateData zoneStateData;

	private int nextNetId = 1;

	public CallLimitersList<CallLimiter, GameEntityManager.RPC> m_RpcSpamChecks = new CallLimitersList<CallLimiter, GameEntityManager.RPC>();

	private List<MeshFilter> renderSearchList = new List<MeshFilter>(32);

	private List<SkinnedMeshRenderer> renderSearchListSkinned = new List<SkinnedMeshRenderer>(32);

	private List<Collider> _collidersList = new List<Collider>(16);

	private static List<VRRig> tempRigs = new List<VRRig>(16);

	private static List<GameEntity> tempEntitiesToSerialize = new List<GameEntity>(512);

	private static List<GameEntityManager.AttachmentData> tempAttachments = new List<GameEntityManager.AttachmentData>(512);

	private static byte[] tempSerializeGameState = new byte[15360];

	public delegate void ZoneStartEvent(GTZone zoneId);

	public delegate void ZoneClearEvent(GTZone zoneId);

	private enum ZoneState
	{
		WaitingToEnterZone,
		WaitingToRequestState,
		WaitingForState,
		Active
	}

	private struct ZoneStateRequest
	{
		public Player player;

		public GTZone zone;

		public bool completed;
	}

	private class ZoneStateData
	{
		public GameEntityManager.ZoneState state;

		public double stateStartTime;

		public List<GameEntityManager.ZoneStateRequest> zoneStateRequests;

		public List<Player> zonePlayers;

		public byte[] recievedStateBytes;

		public int numRecievedStateBytes;
	}

	public enum RPC
	{
		CreateItem,
		CreateItems,
		DestroyItem,
		ApplyState,
		GrabEntity,
		ThrowEntity,
		SendTableData,
		HitEntity
	}

	private struct AttachmentData
	{
		public int entityNetId;

		public int attachToEntityNetId;

		public Vector3 localPosition;

		public Quaternion localRotation;
	}
}

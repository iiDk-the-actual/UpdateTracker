using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using GorillaExtensions;
using GorillaGameModes;
using Photon.Pun;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class SuperInfectionManager : MonoBehaviour, IGameEntityZoneComponent, IFactoryItemProvider
{
	private void Awake()
	{
		GameEntityManager gameEntityManager = this.gameEntityManager;
		gameEntityManager.OnEntityAdded = (Action<GameEntity>)Delegate.Combine(gameEntityManager.OnEntityAdded, new Action<GameEntity>(this.OnEntityAdded));
		GameEntityManager gameEntityManager2 = this.gameEntityManager;
		gameEntityManager2.OnEntityRemoved = (Action<GameEntity>)Delegate.Combine(gameEntityManager2.OnEntityRemoved, new Action<GameEntity>(this.OnEntityRemoved));
	}

	public void OnEnableZoneSuperInfection(SuperInfection zone)
	{
		this.zoneSuperInfection = zone;
		if (this.PendingZoneInit)
		{
			this.PendingZoneInit = false;
			this.OnZoneInit();
		}
		if (this.gameEntityManager.PendingTableData)
		{
			this.gameEntityManager.ResolveTableData();
		}
	}

	private void OnEnable()
	{
		SuperInfectionManager.siManagerByZone.TryAdd(this.gameEntityManager.zone, this);
	}

	private void OnDisable()
	{
		SuperInfectionManager.siManagerByZone.Remove(this.gameEntityManager.zone);
	}

	public static SuperInfectionManager GetSIManagerForZone(GTZone targetZone)
	{
		SuperInfectionManager superInfectionManager;
		if (SuperInfectionManager.siManagerByZone.TryGetValue(targetZone, out superInfectionManager))
		{
			return superInfectionManager;
		}
		return null;
	}

	public void OnZoneCreate()
	{
	}

	public void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.zoneSuperInfection == null)
		{
			return;
		}
		if (!this.gameEntityManager.IsAuthority())
		{
			return;
		}
		for (int i = 0; i < this.zoneSuperInfection.siTerminals.Length; i++)
		{
			this.zoneSuperInfection.siTerminals[i].WriteDataPUN(stream, info);
		}
		for (int j = 0; j < this.zoneSuperInfection.siDeposits.Length; j++)
		{
			this.zoneSuperInfection.siDeposits[j].WriteDataPUN(stream, info);
		}
		this.zoneSuperInfection.questBoard.WriteDataPUN(stream, info);
		SuperInfectionManager.tempRigs.Clear();
		VRRigCache.Instance.GetActiveRigs(SuperInfectionManager.tempRigs);
		SuperInfectionManager.tempRigs2.Clear();
		for (int k = 0; k < SuperInfectionManager.tempRigs.Count; k++)
		{
			if (SuperInfectionManager.tempRigs[k].OwningNetPlayer != null)
			{
				SuperInfectionManager.tempRigs2.Add(SuperInfectionManager.tempRigs[k]);
			}
		}
		int count = SuperInfectionManager.tempRigs2.Count;
		stream.SendNext(count);
		for (int l = 0; l < count; l++)
		{
			SIPlayer siplayer = SIPlayer.Get(SuperInfectionManager.tempRigs2[l].OwningNetPlayer.ActorNumber);
			stream.SendNext(siplayer.ActorNr);
			siplayer.WriteDataPUN(stream, info);
		}
	}

	public void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.zoneSuperInfection == null)
		{
			return;
		}
		if (!this.gameEntityManager.IsAuthorityPlayer(info.Sender))
		{
			return;
		}
		for (int i = 0; i < this.zoneSuperInfection.siTerminals.Length; i++)
		{
			this.zoneSuperInfection.siTerminals[i].ReadDataPUN(stream, info);
		}
		for (int j = 0; j < this.zoneSuperInfection.siDeposits.Length; j++)
		{
			this.zoneSuperInfection.siDeposits[j].ReadDataPUN(stream, info);
		}
		this.zoneSuperInfection.questBoard.ReadDataPUN(stream, info);
		int num = (int)stream.ReceiveNext();
		if (num < 0 || num > 10)
		{
			return;
		}
		for (int k = 0; k < num; k++)
		{
			SIPlayer siplayer = SIPlayer.Get((int)stream.ReceiveNext());
			if (siplayer == null)
			{
				return;
			}
			if (!siplayer.ReadDataPUN(stream, info))
			{
				return;
			}
		}
	}

	void IGameEntityZoneComponent.SerializeZoneData(BinaryWriter writer)
	{
		if (this.zoneSuperInfection == null)
		{
			return;
		}
		for (int i = 0; i < this.zoneSuperInfection.siTerminals.Length; i++)
		{
			this.zoneSuperInfection.siTerminals[i].SerializeZoneData(writer);
		}
	}

	void IGameEntityZoneComponent.DeserializeZoneData(BinaryReader reader)
	{
		if (this.zoneSuperInfection == null)
		{
			return;
		}
		for (int i = 0; i < this.zoneSuperInfection.siTerminals.Length; i++)
		{
			this.zoneSuperInfection.siTerminals[i].DeserializeZoneData(reader);
		}
	}

	public void SerializeZoneEntityData(BinaryWriter writer, GameEntity entity)
	{
	}

	public void DeserializeZoneEntityData(BinaryReader reader, GameEntity entity)
	{
	}

	void IGameEntityZoneComponent.SerializeZonePlayerData(BinaryWriter writer, int actorNumber)
	{
		SIPlayer siplayer = SIPlayer.Get(actorNumber);
		siplayer.SerializeNetworkState(writer, siplayer.gamePlayer.rig.OwningNetPlayer);
	}

	void IGameEntityZoneComponent.DeserializeZonePlayerData(BinaryReader reader, int actorNumber)
	{
		SIPlayer siplayer = SIPlayer.Get(actorNumber);
		SIPlayer.DeserializeNetworkStateAndBurn(reader, siplayer, this);
	}

	public bool IsZoneReady()
	{
		return NetworkSystem.Instance.InRoom && this.IsInSuperInfectionMode() && this.zoneSuperInfection.IsNotNull();
	}

	public bool ShouldClearZone()
	{
		return GameMode.ActiveGameMode != null && GameMode.ActiveGameMode.GameType() != GameModeType.SuperInfect;
	}

	public bool IsInSuperInfectionMode()
	{
		return GameMode.ActiveGameMode != null && GameMode.ActiveGameMode.GameType() == GameModeType.SuperInfect;
	}

	public void OnCreateGameEntity(GameEntity entity)
	{
		SIPlayer siplayer = SIPlayer.Get((int)(entity.createData & (long)((ulong)(-1))));
		if (siplayer != null)
		{
			siplayer.activePlayerGadgets.Add(entity.GetNetId());
		}
		SIGadget component = entity.GetComponent<SIGadget>();
		if (component != null)
		{
			SIUpgradeSet siupgradeSet = new SIUpgradeSet((int)(entity.createData >> 32));
			siupgradeSet = component.FilterUpgradeNodes(siupgradeSet);
			component.ApplyUpgradeNodes(siupgradeSet);
			component.RefreshUpgradeVisuals(siupgradeSet);
		}
		foreach (SuperInfectionSnapPoint superInfectionSnapPoint in entity.GetComponentsInChildren<SuperInfectionSnapPoint>(true))
		{
			this.RegisterSnapPoint(superInfectionSnapPoint);
		}
	}

	public void OnZoneClear(ZoneClearReason reason)
	{
		SuperInfection superInfection = this.zoneSuperInfection;
		if (superInfection != null)
		{
			superInfection.OnZoneClear(reason);
		}
		SIPlayer localPlayer = SIPlayer.LocalPlayer;
		if (localPlayer != null)
		{
			localPlayer.Reset();
		}
		SIPlayer.ClearPlayerCache();
		this.allSnapPoints.Clear();
	}

	public void OnZoneInit()
	{
		if (this.zoneSuperInfection == null)
		{
			this.PendingZoneInit = true;
			return;
		}
		SuperInfectionManager.activeSuperInfectionManager = this;
		if (this.gameEntityManager.IsAuthority())
		{
			this.TestSpawnGadget();
		}
		this.zoneSuperInfection.OnZoneInit();
		if (SIPlayer.Get(NetworkSystem.Instance.LocalPlayer.ActorNumber) != null)
		{
			this.progression.Init();
			if (this.progression.ClientReady)
			{
				SIPlayer.SetAndBroadcastProgression();
			}
			else
			{
				this.progression.OnClientReady += this.<OnZoneInit>g__WhenReady|39_0;
			}
		}
		this.allSnapPoints.Clear();
		foreach (GameEntity gameEntity in this.gameEntityManager.GetGameEntities())
		{
			if (!(gameEntity == null))
			{
				foreach (SuperInfectionSnapPoint superInfectionSnapPoint in gameEntity.GetComponentsInChildren<SuperInfectionSnapPoint>(true))
				{
					this.RegisterSnapPoint(superInfectionSnapPoint);
				}
			}
		}
	}

	public void RegisterSnapPoint(SuperInfectionSnapPoint snapPoint)
	{
		List<SuperInfectionSnapPoint> list;
		if (!this.allSnapPoints.TryGetValue(snapPoint.jointType, out list))
		{
			list = (this.allSnapPoints[snapPoint.jointType] = new List<SuperInfectionSnapPoint>());
		}
		list.Add(snapPoint);
	}

	public void UnregisterSnapPoint(SuperInfectionSnapPoint snapPoint)
	{
		if (this.allSnapPoints.ContainsKey(snapPoint.jointType))
		{
			this.allSnapPoints[snapPoint.jointType].Remove(snapPoint);
			if (this.allSnapPoints[snapPoint.jointType].Count == 0)
			{
				this.allSnapPoints.Remove(snapPoint.jointType);
			}
		}
	}

	public IEnumerable<SuperInfectionSnapPoint> GetPoints(SnapJointType jointType)
	{
		foreach (KeyValuePair<SnapJointType, List<SuperInfectionSnapPoint>> keyValuePair in this.allSnapPoints)
		{
			if ((keyValuePair.Key & jointType) != SnapJointType.None)
			{
				foreach (SuperInfectionSnapPoint superInfectionSnapPoint in keyValuePair.Value)
				{
					yield return superInfectionSnapPoint;
				}
				List<SuperInfectionSnapPoint>.Enumerator enumerator2 = default(List<SuperInfectionSnapPoint>.Enumerator);
			}
		}
		Dictionary<SnapJointType, List<SuperInfectionSnapPoint>>.Enumerator enumerator = default(Dictionary<SnapJointType, List<SuperInfectionSnapPoint>>.Enumerator);
		yield break;
		yield break;
	}

	public SuperInfectionSnapPoint FindNearestSnapPoint(SnapJointType jointType, Vector3 origin, float maxDist, bool includeOccupied = false)
	{
		SuperInfectionSnapPoint superInfectionSnapPoint = null;
		float num = maxDist * maxDist;
		foreach (SuperInfectionSnapPoint superInfectionSnapPoint2 in this.GetPoints(jointType))
		{
			if (!(superInfectionSnapPoint2 == null) && superInfectionSnapPoint2.isActiveAndEnabled && (includeOccupied || !superInfectionSnapPoint2.HasSnapped()))
			{
				float sqrMagnitude = (superInfectionSnapPoint2.transform.position - origin).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					superInfectionSnapPoint = superInfectionSnapPoint2;
					num = sqrMagnitude;
				}
			}
		}
		return superInfectionSnapPoint;
	}

	public void CallRPC(SuperInfectionManager.ClientToAuthorityRPC clientToAuthorityRPC, object[] data)
	{
		if (NetworkSystem.Instance.InRoom)
		{
			this.photonView.RPC("SIClientToAuthorityRPC", this.gameEntityManager.GetAuthorityPlayer(), new object[]
			{
				(int)clientToAuthorityRPC,
				data
			});
		}
	}

	public void CallRPC(SuperInfectionManager.ClientToClientRPC clientToClientRPC, object[] data)
	{
		if (NetworkSystem.Instance.InRoom)
		{
			this.photonView.RPC("SIClientToClientRPC", RpcTarget.Others, new object[]
			{
				(int)clientToClientRPC,
				data
			});
		}
	}

	public void CallRPC(SuperInfectionManager.AuthorityToClientRPC authorityToClientRPC, object[] data)
	{
		if (NetworkSystem.Instance.InRoom)
		{
			this.photonView.RPC("SIAuthorityToClientRPC", RpcTarget.Others, new object[]
			{
				(int)authorityToClientRPC,
				data
			});
		}
	}

	public void CallRPC(SuperInfectionManager.AuthorityToClientRPC authorityToClientRPC, int actorNr, object[] data)
	{
		if (NetworkSystem.Instance.InRoom)
		{
			this.photonView.RPC("SIAuthorityToClientRPC", NetworkSystem.Instance.GetNetPlayerByID(actorNr).GetPlayerRef(), new object[]
			{
				(int)authorityToClientRPC,
				data
			});
		}
	}

	[PunRPC]
	public void SIClientToAuthorityRPC(int clientToAuthorityRPCEnum, object[] data, PhotonMessageInfo info)
	{
		if (!this.gameEntityManager.IsValidAuthorityRPC(info.Sender))
		{
			return;
		}
		if (data == null)
		{
			return;
		}
		SIPlayer siplayer = SIPlayer.Get(info.Sender.ActorNumber);
		if (siplayer.IsNull() || !siplayer.clientToAuthorityRPCLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.ProcessClientToAuthorityRPC(clientToAuthorityRPCEnum, data, info);
	}

	public void ProcessClientToAuthorityRPC(int clientToAuthorityRPCEnum, object[] data, PhotonMessageInfo info)
	{
		switch (clientToAuthorityRPCEnum)
		{
		case 0:
		{
			if (data.Length != 4)
			{
				return;
			}
			int num;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num))
			{
				return;
			}
			int num2;
			if (!GameEntityManager.ValidateDataType<int>(data[1], out num2))
			{
				return;
			}
			int num3;
			if (!GameEntityManager.ValidateDataType<int>(data[2], out num3))
			{
				return;
			}
			int num4;
			if (!GameEntityManager.ValidateDataType<int>(data[3], out num4))
			{
				return;
			}
			if (num4 < 0 || num4 >= this.zoneSuperInfection.siTerminals.Length)
			{
				return;
			}
			if (!Enum.IsDefined(typeof(SITouchscreenButton.SITouchscreenButtonType), (SITouchscreenButton.SITouchscreenButtonType)num))
			{
				return;
			}
			if (!Enum.IsDefined(typeof(SICombinedTerminal.TerminalSubFunction), (SICombinedTerminal.TerminalSubFunction)num3))
			{
				return;
			}
			this.zoneSuperInfection.siTerminals[num4].TouchscreenButtonPressed((SITouchscreenButton.SITouchscreenButtonType)num, num2, info.Sender.ActorNumber, (SICombinedTerminal.TerminalSubFunction)num3);
			return;
		}
		case 1:
		{
			if (data.Length != 1)
			{
				return;
			}
			int num5;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num5))
			{
				return;
			}
			if (num5 < 0 || num5 >= this.zoneSuperInfection.siTerminals.Length)
			{
				return;
			}
			SIPlayer siplayer = SIPlayer.Get(info.Sender.ActorNumber);
			if (siplayer == null)
			{
				return;
			}
			SICombinedTerminal sicombinedTerminal = this.zoneSuperInfection.siTerminals[num5];
			if (!siplayer.gamePlayer.rig.IsPositionInRange(sicombinedTerminal.transform.position, 3f))
			{
				return;
			}
			sicombinedTerminal.PlayerHandScanned(info.Sender.ActorNumber);
			return;
		}
		case 2:
		{
			if (data.Length != 2)
			{
				return;
			}
			int num6;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num6))
			{
				return;
			}
			int num7;
			if (!GameEntityManager.ValidateDataType<int>(data[1], out num7))
			{
				return;
			}
			if (num7 < 0 || num7 >= this.zoneSuperInfection.siDeposits.Length)
			{
				return;
			}
			GameEntity gameEntityFromNetId = this.gameEntityManager.GetGameEntityFromNetId(num6);
			if (gameEntityFromNetId == null)
			{
				return;
			}
			SIResourceDeposit siresourceDeposit = this.zoneSuperInfection.siDeposits[num7];
			if ((gameEntityFromNetId.transform.position - siresourceDeposit.transform.position).IsLongerThan(3f))
			{
				return;
			}
			SIResource component = gameEntityFromNetId.GetComponent<SIResource>();
			if (component != null)
			{
				siresourceDeposit.ResourceDeposited(component);
				return;
			}
			break;
		}
		case 3:
		{
			if (data.Length != 2)
			{
				return;
			}
			int num8;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num8))
			{
				return;
			}
			int num9;
			if (!GameEntityManager.ValidateDataType<int>(data[1], out num9))
			{
				return;
			}
			GameEntity gameEntityFromNetId2 = this.gameEntityManager.GetGameEntityFromNetId(num8);
			if (!gameEntityFromNetId2)
			{
				return;
			}
			SIGadget component2 = gameEntityFromNetId2.GetComponent<SIGadget>();
			if (component2)
			{
				component2.ProcessClientToAuthorityRPC(info, num9, null);
				return;
			}
			break;
		}
		case 4:
		{
			if (data.Length != 3)
			{
				return;
			}
			int num10;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num10))
			{
				return;
			}
			int num11;
			if (!GameEntityManager.ValidateDataType<int>(data[1], out num11))
			{
				return;
			}
			object[] array;
			if (!GameEntityManager.ValidateDataType<object[]>(data[2], out array))
			{
				return;
			}
			GameEntity gameEntityFromNetId3 = this.gameEntityManager.GetGameEntityFromNetId(num10);
			if (!gameEntityFromNetId3)
			{
				return;
			}
			SIGadget component3 = gameEntityFromNetId3.GetComponent<SIGadget>();
			if (component3)
			{
				component3.ProcessClientToAuthorityRPC(info, num11, array);
			}
			break;
		}
		default:
			return;
		}
	}

	[PunRPC]
	public void SIAuthorityToClientRPC(int authorityToClientRPCEnum, object[] data, PhotonMessageInfo info)
	{
		if (!this.gameEntityManager.IsValidClientRPC(info.Sender))
		{
			return;
		}
		if (data == null)
		{
			return;
		}
		SIPlayer siplayer = SIPlayer.Get(info.Sender.ActorNumber);
		if (siplayer.IsNull() || !siplayer.authorityToClientRPCLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.ProcessAuthorityToClientRPC(authorityToClientRPCEnum, data, info);
	}

	public void ProcessAuthorityToClientRPC(int authorityToClientRPCEnum, object[] data, PhotonMessageInfo info)
	{
		switch (authorityToClientRPCEnum)
		{
		case 3:
		{
			if (data.Length != 2)
			{
				return;
			}
			int num;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num))
			{
				return;
			}
			int num2;
			if (!GameEntityManager.ValidateDataType<int>(data[1], out num2))
			{
				return;
			}
			GameEntity gameEntityFromNetId = this.gameEntityManager.GetGameEntityFromNetId(num);
			if (!gameEntityFromNetId)
			{
				return;
			}
			SIGadget component = gameEntityFromNetId.GetComponent<SIGadget>();
			if (component)
			{
				component.ProcessAuthorityToClientRPC(info, num2, null);
				return;
			}
			break;
		}
		case 4:
		{
			if (data.Length != 3)
			{
				return;
			}
			int num3;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num3))
			{
				return;
			}
			int num4;
			if (!GameEntityManager.ValidateDataType<int>(data[1], out num4))
			{
				return;
			}
			object[] array;
			if (!GameEntityManager.ValidateDataType<object[]>(data[2], out array))
			{
				return;
			}
			GameEntity gameEntityFromNetId2 = this.gameEntityManager.GetGameEntityFromNetId(num3);
			if (!gameEntityFromNetId2)
			{
				return;
			}
			SIGadget component2 = gameEntityFromNetId2.GetComponent<SIGadget>();
			if (component2)
			{
				component2.ProcessAuthorityToClientRPC(info, num4, array);
				return;
			}
			break;
		}
		case 5:
		{
			if (data.Length != 1)
			{
				return;
			}
			Vector3 vector;
			if (!GameEntityManager.ValidateDataType<Vector3>(data[0], out vector))
			{
				return;
			}
			if (SIPlayer.LocalPlayer)
			{
				SIPlayer.LocalPlayer.TriggerIdolDepositedCelebration(vector);
				return;
			}
			break;
		}
		default:
			return;
		}
	}

	[PunRPC]
	public void SIClientToClientRPC(int clientToClientRPCEnum, object[] data, PhotonMessageInfo info)
	{
		if (data == null)
		{
			return;
		}
		SIPlayer siplayer = SIPlayer.Get(info.Sender.ActorNumber);
		if (siplayer.IsNull() || !siplayer.clientToClientRPCLimiter.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.ProcessClientToClientRPC(clientToClientRPCEnum, data, info);
	}

	public void ProcessClientToClientRPC(int clientToClientRPCEnum, object[] data, PhotonMessageInfo info)
	{
		switch (clientToClientRPCEnum)
		{
		case 0:
		{
			SIPlayer siplayer = SIPlayer.Get(info.Sender.ActorNumber);
			if (siplayer == null)
			{
				return;
			}
			if (data.Length != 8)
			{
				return;
			}
			int[] array;
			if (!GameEntityManager.ValidateDataType<int[]>(data[0], out array))
			{
				return;
			}
			int[] array2;
			if (!GameEntityManager.ValidateDataType<int[]>(data[1], out array2))
			{
				return;
			}
			bool[][] array3;
			if (!GameEntityManager.ValidateDataType<bool[][]>(data[2], out array3))
			{
				return;
			}
			int num;
			if (!GameEntityManager.ValidateDataType<int>(data[3], out num))
			{
				return;
			}
			int num2;
			if (!GameEntityManager.ValidateDataType<int>(data[4], out num2))
			{
				return;
			}
			int num3;
			if (!GameEntityManager.ValidateDataType<int>(data[5], out num3))
			{
				return;
			}
			int[] array4;
			if (!GameEntityManager.ValidateDataType<int[]>(data[6], out array4))
			{
				return;
			}
			int[] array5;
			if (!GameEntityManager.ValidateDataType<int[]>(data[7], out array5))
			{
				return;
			}
			siplayer.UpdateProgression(array, array2, array3, num, num2, num3, array4, array5);
			if (this.zoneSuperInfection != null)
			{
				this.zoneSuperInfection.RefreshStations(info.Sender.ActorNumber);
				return;
			}
			break;
		}
		case 1:
		{
			if (data.Length != 5)
			{
				return;
			}
			if (SIPlayer.Get(info.Sender.ActorNumber) == null)
			{
				return;
			}
			int num4;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num4))
			{
				return;
			}
			Vector3 vector;
			if (GameEntityManager.ValidateDataType<Vector3>(data[1], out vector))
			{
				float num5 = 10000f;
				if ((in vector).IsValid(in num5))
				{
					Vector3 vector2;
					if (GameEntityManager.ValidateDataType<Vector3>(data[2], out vector2))
					{
						num5 = 10000f;
						if ((in vector2).IsValid(in num5))
						{
							Vector3 vector3;
							if (GameEntityManager.ValidateDataType<Vector3>(data[3], out vector3))
							{
								num5 = 10000f;
								if ((in vector3).IsValid(in num5))
								{
									Quaternion quaternion;
									if (!GameEntityManager.ValidateDataType<Quaternion>(data[4], out quaternion) || !(in quaternion).IsValid())
									{
										return;
									}
									GameEntity gameEntityFromNetId = this.gameEntityManager.GetGameEntityFromNetId(num4);
									if (gameEntityFromNetId == null)
									{
										return;
									}
									if (gameEntityFromNetId.heldByActorNumber != info.Sender.ActorNumber && gameEntityFromNetId.snappedByActorNumber != info.Sender.ActorNumber)
									{
										return;
									}
									SIGadgetDashYoyo component = gameEntityFromNetId.GetComponent<SIGadgetDashYoyo>();
									if (component == null)
									{
										return;
									}
									component.RemoteThrowYoYoTarget(vector, vector2, vector3, quaternion);
									return;
								}
							}
							return;
						}
					}
					return;
				}
			}
			return;
		}
		case 2:
		{
			if (data.Length != 2)
			{
				return;
			}
			int num6;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num6))
			{
				return;
			}
			int num7;
			if (!GameEntityManager.ValidateDataType<int>(data[1], out num7))
			{
				return;
			}
			GameEntity gameEntityFromNetId2 = this.gameEntityManager.GetGameEntityFromNetId(num6);
			if (!gameEntityFromNetId2)
			{
				return;
			}
			SIGadget component2 = gameEntityFromNetId2.GetComponent<SIGadget>();
			if (component2)
			{
				component2.ProcessClientToClientRPC(info, num7, null);
				return;
			}
			break;
		}
		case 3:
		{
			if (data.Length != 3)
			{
				return;
			}
			int num8;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num8))
			{
				return;
			}
			int num9;
			if (!GameEntityManager.ValidateDataType<int>(data[1], out num9))
			{
				return;
			}
			object[] array6;
			if (!GameEntityManager.ValidateDataType<object[]>(data[2], out array6))
			{
				return;
			}
			GameEntity gameEntityFromNetId3 = this.gameEntityManager.GetGameEntityFromNetId(num8);
			if (!gameEntityFromNetId3)
			{
				return;
			}
			SIGadget component3 = gameEntityFromNetId3.GetComponent<SIGadget>();
			if (component3)
			{
				component3.ProcessClientToClientRPC(info, num9, array6);
			}
			break;
		}
		default:
			return;
		}
	}

	[ContextMenu("Spawn Debug Object")]
	private void TestSpawnGadget()
	{
		this.testSpawner.Spawn(this.gameEntityManager);
	}

	public IEnumerable<GameEntity> GetFactoryItems()
	{
		return this.techTreeSO.SpawnableEntities;
	}

	private void OnEntityAdded(GameEntity entity)
	{
		SIGadget sigadget;
		if (this.zoneSuperInfection != null && entity.TryGetComponent<SIGadget>(out sigadget))
		{
			this.zoneSuperInfection.AddGadget(sigadget);
		}
	}

	private void OnEntityRemoved(GameEntity entity)
	{
		SIGadget sigadget;
		if (this.zoneSuperInfection != null && entity.TryGetComponent<SIGadget>(out sigadget))
		{
			this.zoneSuperInfection.RemoveGadget(sigadget);
		}
	}

	public bool ValidateMigratedGameEntity(int netId, int entityTypeId, Vector3 position, Quaternion rotation, long createData, int actorNr)
	{
		GameObject gameObject = this.gameEntityManager.FactoryPrefabById(entityTypeId);
		if (gameObject == null)
		{
			return false;
		}
		if (gameObject.GetComponent<SIGadget>() == null)
		{
			return false;
		}
		SIPlayer siplayer = SIPlayer.Get(actorNr);
		if (siplayer == null)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < siplayer.activePlayerGadgets.Count; i++)
		{
			GameEntity gameEntityFromNetId = this.gameEntityManager.GetGameEntityFromNetId(siplayer.activePlayerGadgets[i]);
			if (((gameEntityFromNetId != null) ? gameEntityFromNetId.GetComponent<SIGadget>() : null) != null)
			{
				num++;
			}
		}
		if (num >= siplayer.totalGadgetLimit)
		{
			return false;
		}
		bool flag = false;
		int num2 = 0;
		while (num2 < siplayer.CurrentProgression.techTreeData.Length && !flag)
		{
			for (int j = 0; j < siplayer.CurrentProgression.techTreeData[num2].Length; j++)
			{
				if (siplayer.CurrentProgression.techTreeData[num2][j] && siplayer.progressionSORef.IsValidNode(num2, j))
				{
					SITechTreeNode treeNode = siplayer.progressionSORef.GetTreeNode(num2, j);
					if (treeNode != null && treeNode.IsDispensableGadget && treeNode.unlockedGadgetPrefab.gameObject.name.GetStaticHash() == entityTypeId)
					{
						flag = true;
						break;
					}
				}
			}
			num2++;
		}
		return flag;
	}

	[CompilerGenerated]
	private void <OnZoneInit>g__WhenReady|39_0()
	{
		this.progression.OnClientReady -= this.<OnZoneInit>g__WhenReady|39_0;
		SIPlayer.SetAndBroadcastProgression();
	}

	private const string preLog = "[SuperInfectionManager]  ";

	private const string preErr = "[SuperInfectionManager]  ERROR!!!  ";

	public GameEntityManager gameEntityManager;

	public TestSpawnGadget testSpawner;

	public PhotonView photonView;

	public XSceneRef zoneSuperInfectionRef;

	[NonSerialized]
	public SuperInfection zoneSuperInfection;

	[SerializeField]
	private SITechTreeSO techTreeSO;

	[SerializeField]
	private SIProgression progression;

	public static SuperInfectionManager activeSuperInfectionManager;

	public static Dictionary<GTZone, SuperInfectionManager> siManagerByZone = new Dictionary<GTZone, SuperInfectionManager>();

	private static List<VRRig> tempRigs = new List<VRRig>(10);

	private static List<VRRig> tempRigs2 = new List<VRRig>(10);

	private readonly Dictionary<SnapJointType, List<SuperInfectionSnapPoint>> allSnapPoints = new Dictionary<SnapJointType, List<SuperInfectionSnapPoint>>();

	private const float rpcProximityCheckRange = 3f;

	private bool PendingZoneInit;

	private bool PendingTableData;

	public enum ClientToAuthorityRPC
	{
		CombinedTerminalButtonPress,
		CombinedTerminalHandScan,
		ResourceDepositDeposited,
		CallEntityRPC,
		CallEntityRPCData
	}

	public enum AuthorityToClientRPC
	{
		TechPointGranted,
		ResourceDepositTechPointGranted,
		ResourceDepositTechPointRejected,
		CallEntityRPC,
		CallEntityRPCData,
		TriggerMonkeIdolDepositCelebration
	}

	public enum ClientToClientRPC
	{
		BroadcastProgression,
		LaunchDashYoyo,
		CallEntityRPC,
		CallEntityRPCData
	}
}

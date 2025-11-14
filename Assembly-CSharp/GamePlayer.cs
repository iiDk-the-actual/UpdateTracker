using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Rendering;

public class GamePlayer : MonoBehaviour
{
	public bool DidJoinWithItems { get; set; }

	public bool AdditionalDataInitialized { get; set; }

	private void Awake()
	{
		this.handTransforms = new Transform[2];
		this.handTransforms[0] = this.leftHand;
		this.handTransforms[1] = this.rightHand;
		this.hands = new GamePlayer.HandData[2];
		this.ResetData();
		this.newJoinZoneLimiter = new CallLimiter(10, 10f, 0.5f);
		this.netImpulseLimiter = new CallLimiter(25, 1f, 0.5f);
		this.netGrabLimiter = new CallLimiter(25, 1f, 0.5f);
		this.netThrowLimiter = new CallLimiter(25, 1f, 0.5f);
		this.netStateLimiter = new CallLimiter(25, 1f, 0.5f);
		this.netSnapLimiter = new CallLimiter(25, 1f, 0.5f);
		if (this.snapPointManager == null)
		{
			this.snapPointManager = base.GetComponentInChildren<SuperInfectionSnapPointManager>(true);
			if (this.snapPointManager == null)
			{
				Debug.LogError("[GamePlayer]  ERROR!!!  Snappoints cannot function because the required `SuperInfectionSnapPointManager` could found in children.", this);
			}
		}
	}

	public void Clear()
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.hands[i].grabbedEntityId != GameEntityId.Invalid && this.hands[i].grabbedEntityManager != null)
			{
				this.hands[i].grabbedEntityManager.RequestThrowEntity(this.hands[i].grabbedEntityId, GamePlayer.IsLeftHand(i), GTPlayer.Instance.HeadCenterPosition, Vector3.zero, Vector3.zero);
			}
			this.ClearGrabbed(i);
		}
		for (int j = 0; j < 2; j++)
		{
			if (this.hands[j].snappedEntityId != GameEntityId.Invalid && this.hands[j].snappedEntityManager != null)
			{
				GameEntityId snappedEntityId = this.hands[j].snappedEntityId;
				GameEntityManager snappedEntityManager = this.hands[j].snappedEntityManager;
				snappedEntityManager.RequestGrabEntity(snappedEntityId, !GamePlayer.IsLeftHand(j), Vector3.zero, Quaternion.identity);
				snappedEntityManager.RequestThrowEntity(snappedEntityId, !GamePlayer.IsLeftHand(j), GTPlayer.Instance.HeadCenterPosition, Vector3.zero, Vector3.zero);
			}
			this.ClearSnapped(j);
		}
	}

	public void ResetData()
	{
		for (int i = 0; i < 2; i++)
		{
			this.ClearGrabbed(i);
			this.ClearSnapped(i);
		}
		this.DidJoinWithItems = false;
		this.AdditionalDataInitialized = false;
		this.SetInitializePlayer(false);
	}

	private void OnEnable()
	{
	}

	private void Start()
	{
		GamePlayer.InitializeStaticLookupCaches();
	}

	public void MigrateHeldActorNumbers()
	{
		int actorNumber = this.rig.OwningNetPlayer.ActorNumber;
		for (int i = 0; i < 2; i++)
		{
			if (this.hands[i].grabbedEntityManager != null)
			{
				GameEntity gameEntity = this.hands[i].grabbedEntityManager.GetGameEntity(this.hands[i].grabbedEntityId);
				if (gameEntity != null)
				{
					gameEntity.MigrateHeldBy(actorNumber);
				}
			}
			if (this.hands[i].snappedEntityManager != null)
			{
				GameEntity gameEntity2 = this.hands[i].snappedEntityManager.GetGameEntity(this.hands[i].snappedEntityId);
				if (gameEntity2 != null)
				{
					gameEntity2.MigrateSnappedBy(actorNumber);
				}
			}
		}
	}

	public void SetGrabbed(GameEntityId gameBallId, int handIndex, GameEntityManager gameEntityManager)
	{
		if (gameBallId.IsValid())
		{
			this.ClearGrabbedIfHeld(gameBallId);
		}
		GamePlayer.HandData handData = this.hands[handIndex];
		handData.grabbedEntityId = gameBallId;
		handData.grabbedEntityManager = gameEntityManager;
		this.hands[handIndex] = handData;
	}

	public void SetSnapped(GameEntityId gameBallId, int handIndex, GameEntityManager gameEntityManager)
	{
		if (gameBallId.IsValid())
		{
			this.ClearSnappedIfSnapped(gameBallId);
			this.ClearGrabbedIfHeld(gameBallId);
		}
		GamePlayer.HandData handData = this.hands[handIndex];
		handData.snappedEntityId = gameBallId;
		handData.snappedEntityManager = gameEntityManager;
		this.hands[handIndex] = handData;
	}

	public void ClearZone(GameEntityManager manager)
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.hands[i].grabbedEntityId != GameEntityId.Invalid && this.hands[i].grabbedEntityManager == manager)
			{
				GameEntity gameEntity = this.hands[i].grabbedEntityManager.GetGameEntity(this.hands[i].grabbedEntityId);
				if (gameEntity != null)
				{
					Action onReleased = gameEntity.OnReleased;
					if (onReleased != null)
					{
						onReleased();
					}
				}
				this.ClearGrabbed(i);
			}
			if (this.hands[i].snappedEntityId != GameEntityId.Invalid && this.hands[i].snappedEntityManager == manager)
			{
				GameEntity gameEntity2 = this.hands[i].snappedEntityManager.GetGameEntity(this.hands[i].snappedEntityId);
				if (gameEntity2 != null)
				{
					Action onReleased2 = gameEntity2.OnReleased;
					if (onReleased2 != null)
					{
						onReleased2();
					}
				}
				this.ClearSnapped(i);
			}
		}
		if (NetworkSystem.Instance.SessionIsPrivate)
		{
			this.DidJoinWithItems = false;
		}
	}

	public void ClearGrabbedIfHeld(GameEntityId gameBallId)
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.hands[i].grabbedEntityId == gameBallId)
			{
				this.ClearGrabbed(i);
			}
		}
	}

	public void ClearSnappedIfSnapped(GameEntityId gameBallId)
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.hands[i].snappedEntityId == gameBallId)
			{
				this.ClearSnapped(i);
			}
		}
	}

	public void ClearGrabbed(int handIndex)
	{
		this.SetGrabbed(GameEntityId.Invalid, handIndex, null);
	}

	public void ClearSnapped(int handIndex)
	{
		this.SetSnapped(GameEntityId.Invalid, handIndex, null);
	}

	public bool IsGrabbingDisabled()
	{
		return this.grabbingDisabled;
	}

	public void DisableGrabbing(bool disable)
	{
		this.grabbingDisabled = disable;
	}

	public bool IsHoldingEntity(GameEntityId gameEntityId, bool isLeftHand)
	{
		return this.GetGrabbedGameEntityId(GamePlayer.GetHandIndex(isLeftHand)) == gameEntityId;
	}

	public bool IsHoldingEntity(GameEntityManager gameEntityManager, bool isLeftHand)
	{
		return gameEntityManager.GetGameEntity(this.GetGrabbedGameEntityId(GamePlayer.GetHandIndex(isLeftHand))) != null;
	}

	public bool IsHoldingEntity(GameEntityId gameEntityId)
	{
		return this.GetGrabbedGameEntityId(GamePlayer.GetHandIndex(true)) == gameEntityId || this.GetGrabbedGameEntityId(GamePlayer.GetHandIndex(false)) == gameEntityId;
	}

	public void RequestDropAllSnapped()
	{
		this.Clear();
		this.snapPointManager.DropAllSnappedAuthority();
	}

	public List<GameEntityId> HeldAndSnappedItems(GameEntityManager manager)
	{
		return this.IterateHeldAndSnappedItems(manager).ToList<GameEntityId>();
	}

	public IEnumerable<GameEntityId> IterateHeldAndSnappedItems(GameEntityManager manager)
	{
		int num;
		for (int i = 0; i < 2; i = num)
		{
			if (this.hands[i].grabbedEntityId != GameEntityId.Invalid && this.hands[i].grabbedEntityManager == manager)
			{
				yield return this.hands[i].grabbedEntityId;
			}
			if (this.hands[i].snappedEntityId != GameEntityId.Invalid && this.hands[i].snappedEntityManager == manager)
			{
				yield return this.hands[i].snappedEntityId;
			}
			num = i + 1;
		}
		yield break;
	}

	public List<GameEntity> HeldAndSnappedEntities(GameEntityManager ignoreEntitiesInManager = null)
	{
		return this.IterateHeldAndSnappedEntities(ignoreEntitiesInManager).ToList<GameEntity>();
	}

	public IEnumerable<GameEntity> IterateHeldAndSnappedEntities(GameEntityManager ignoreEntitiesInManager = null)
	{
		int num;
		for (int i = 0; i < 2; i = num)
		{
			if (this.hands[i].grabbedEntityId != GameEntityId.Invalid && this.hands[i].grabbedEntityManager != null && this.hands[i].grabbedEntityManager != ignoreEntitiesInManager)
			{
				GameEntity gameEntity = this.hands[i].grabbedEntityManager.GetGameEntity(this.hands[i].grabbedEntityId);
				yield return gameEntity;
			}
			if (this.hands[i].snappedEntityId != GameEntityId.Invalid && this.hands[i].snappedEntityManager != null && this.hands[i].snappedEntityManager != ignoreEntitiesInManager)
			{
				GameEntity gameEntity2 = this.hands[i].snappedEntityManager.GetGameEntity(this.hands[i].snappedEntityId);
				yield return gameEntity2;
			}
			num = i + 1;
		}
		yield break;
	}

	public void DeleteGrabbedEntityLocal(int handIndex)
	{
		if (this.hands[handIndex].grabbedEntityId != GameEntityId.Invalid && this.hands[handIndex].grabbedEntityManager != null)
		{
			GameEntity gameEntity = this.hands[handIndex].grabbedEntityManager.GetGameEntity(this.hands[handIndex].grabbedEntityId);
			if (gameEntity != null)
			{
				if (gameEntity != null)
				{
					Action onReleased = gameEntity.OnReleased;
					if (onReleased != null)
					{
						onReleased();
					}
				}
				this.hands[handIndex].grabbedEntityManager.DestroyItemLocal(this.hands[handIndex].grabbedEntityId);
			}
		}
	}

	public int MigrateToEntityManager(GameEntityManager newEntityManager)
	{
		int num = 0;
		for (int i = 0; i < this.hands.Length; i++)
		{
			GameEntityId grabbedEntityId = this.hands[i].grabbedEntityId;
			if (grabbedEntityId != GameEntityId.Invalid && this.hands[i].grabbedEntityManager != newEntityManager)
			{
				GameEntity gameEntity = this.hands[i].grabbedEntityManager.GetGameEntity(grabbedEntityId);
				if (gameEntity != null && gameEntity.IsValidToMigrate())
				{
					GameEntityId gameEntityId = gameEntity.MigrateToEntityManager(newEntityManager);
					this.hands[i].grabbedEntityManager = newEntityManager;
					this.hands[i].grabbedEntityId = gameEntityId;
					num++;
				}
			}
			GameEntityId snappedEntityId = this.hands[i].snappedEntityId;
			if (snappedEntityId != GameEntityId.Invalid && this.hands[i].snappedEntityManager != newEntityManager)
			{
				GameEntity gameEntity2 = this.hands[i].snappedEntityManager.GetGameEntity(snappedEntityId);
				if (gameEntity2 != null && gameEntity2.IsValidToMigrate())
				{
					GameEntityId gameEntityId2 = gameEntity2.MigrateToEntityManager(newEntityManager);
					this.hands[i].snappedEntityManager = newEntityManager;
					this.hands[i].snappedEntityId = gameEntityId2;
					num++;
				}
			}
		}
		return num;
	}

	public GameEntityId GetGameEntityId(bool isLeftHand)
	{
		return this.GetGrabbedGameEntityId(GamePlayer.GetHandIndex(isLeftHand));
	}

	public GameEntityId GetGrabbedGameEntityId(int handIndex)
	{
		if (handIndex < 0 || handIndex >= this.hands.Length)
		{
			return GameEntityId.Invalid;
		}
		return this.hands[handIndex].grabbedEntityId;
	}

	public GameEntityId GetGrabbedGameEntityIdAndManager(int handIndex, out GameEntityManager manager)
	{
		if (handIndex < 0 || handIndex >= this.hands.Length)
		{
			manager = null;
			return GameEntityId.Invalid;
		}
		manager = this.hands[handIndex].grabbedEntityManager;
		return this.hands[handIndex].grabbedEntityId;
	}

	public GameEntity GetGrabbedGameEntity(int handIndex)
	{
		if (handIndex < 0 || handIndex >= this.hands.Length || this.hands[handIndex].grabbedEntityManager == null)
		{
			return null;
		}
		return this.hands[handIndex].grabbedEntityManager.GetGameEntity(this.GetGrabbedGameEntityId(handIndex));
	}

	public int FindHandIndex(GameEntityId gameBallId)
	{
		for (int i = 0; i < this.hands.Length; i++)
		{
			if (this.hands[i].grabbedEntityId == gameBallId)
			{
				return i;
			}
		}
		return -1;
	}

	public int FindSnapIndex(GameEntityId gameBallId)
	{
		for (int i = 0; i < this.hands.Length; i++)
		{
			if (this.hands[i].snappedEntityId == gameBallId)
			{
				return i;
			}
		}
		return -1;
	}

	public GameEntityId GetGameBallId()
	{
		for (int i = 0; i < this.hands.Length; i++)
		{
			if (this.hands[i].grabbedEntityId.IsValid())
			{
				return this.hands[i].grabbedEntityId;
			}
		}
		return GameEntityId.Invalid;
	}

	public static bool IsLeftHand(int handIndex)
	{
		return handIndex == 0;
	}

	public static int GetHandIndex(bool leftHand)
	{
		if (!leftHand)
		{
			return 1;
		}
		return 0;
	}

	[Obsolete("Method `GamePlayer.TryGetGamePlayer(Player)` is obsolete, use `TryGetGamePlayer(Player, out GamePlayer)` instead.")]
	public static VRRig GetRig(int actorNumber)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(actorNumber);
		if (player == null)
		{
			return null;
		}
		Room currentRoom = PhotonNetwork.CurrentRoom;
		if (currentRoom != null && currentRoom.GetPlayer(actorNumber, false) == null)
		{
			return null;
		}
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			return null;
		}
		return rigContainer.Rig;
	}

	public static GamePlayer GetGamePlayer(Player player)
	{
		GamePlayer gamePlayer;
		GamePlayer.TryGetGamePlayer(player, out gamePlayer);
		return gamePlayer;
	}

	public static bool TryGetGamePlayer(Player player, out GamePlayer gamePlayer)
	{
		if (player == null)
		{
			gamePlayer = null;
			return false;
		}
		return GamePlayer.TryGetGamePlayer(player.ActorNumber, out gamePlayer);
	}

	[Obsolete("Method `GamePlayer.GetGamePlayer(actorNum)` is obsolete, use `TryGetGamePlayer(actorNum, out GamePlayer)` instead.")]
	public static GamePlayer GetGamePlayer(int actorNumber)
	{
		GamePlayer gamePlayer;
		GamePlayer.TryGetGamePlayer(actorNumber, out gamePlayer);
		return gamePlayer;
	}

	public static bool TryGetGamePlayer(int actorNumber, out GamePlayer out_gamePlayer)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(actorNumber);
		RigContainer rigContainer;
		if (player == null || !VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			out_gamePlayer = null;
			return false;
		}
		return GamePlayer.TryGetGamePlayer(rigContainer.Rig, out out_gamePlayer);
	}

	[Obsolete("Method `GamePlayer.GetGamePlayer(VRRig)` is obsolete, use `TryGetGamePlayer(VRRig, out GamePlayer)` instead.")]
	public static GamePlayer GetGamePlayer(VRRig rig)
	{
		GamePlayer gamePlayer;
		GamePlayer.TryGetGamePlayer(rig, out gamePlayer);
		return gamePlayer;
	}

	public static bool TryGetGamePlayer(VRRig rig, out GamePlayer out_gamePlayer)
	{
		if (rig == null)
		{
			out_gamePlayer = null;
			return false;
		}
		out_gamePlayer = rig.GetComponent<GamePlayer>();
		return out_gamePlayer != null;
	}

	public static GamePlayer GetGamePlayer(Collider collider, bool bodyOnly = false)
	{
		Transform transform = collider.transform;
		while (transform != null)
		{
			GamePlayer component = transform.GetComponent<GamePlayer>();
			if (component != null)
			{
				return component;
			}
			if (bodyOnly)
			{
				break;
			}
			transform = transform.parent;
		}
		return null;
	}

	public static Transform GetHandTransform(VRRig rig, int handIndex)
	{
		GamePlayer gamePlayer;
		if (handIndex >= 0 && handIndex < 2 && GamePlayer.TryGetGamePlayer(rig, out gamePlayer))
		{
			return gamePlayer.handTransforms[handIndex];
		}
		return null;
	}

	public bool IsLocal()
	{
		return GamePlayerLocal.instance != null && GamePlayerLocal.instance.gamePlayer == this;
	}

	public void SerializeNetworkState(BinaryWriter writer, NetPlayer player, GameEntityManager manager)
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.hands[i].grabbedEntityManager == manager)
			{
				int netIdFromEntityId = manager.GetNetIdFromEntityId(this.hands[i].grabbedEntityId);
				writer.Write(netIdFromEntityId);
				long num = 0L;
				if (netIdFromEntityId != -1)
				{
					GameEntity gameEntity = manager.GetGameEntity(this.hands[i].grabbedEntityId);
					if (gameEntity != null)
					{
						num = BitPackUtils.PackHandPosRotForNetwork(gameEntity.transform.localPosition, gameEntity.transform.localRotation);
					}
				}
				writer.Write(num);
			}
			else
			{
				writer.Write(-1);
				writer.Write(0L);
			}
			if (this.hands[i].snappedEntityManager == manager)
			{
				int netIdFromEntityId2 = manager.GetNetIdFromEntityId(this.hands[i].snappedEntityId);
				writer.Write(netIdFromEntityId2);
				long num2 = 0L;
				if (netIdFromEntityId2 != -1)
				{
					GameEntity gameEntity2 = manager.GetGameEntity(this.hands[i].snappedEntityId);
					if (gameEntity2 != null)
					{
						num2 = BitPackUtils.PackHandPosRotForNetwork(gameEntity2.transform.localPosition, gameEntity2.transform.localRotation);
					}
				}
				writer.Write(num2);
			}
			else
			{
				writer.Write(-1);
				writer.Write(0L);
			}
		}
		writer.Write(this.AdditionalDataInitialized);
	}

	public static void DeserializeNetworkState(BinaryReader reader, GamePlayer gamePlayer, GameEntityManager manager)
	{
		for (int i = 0; i < 2; i++)
		{
			int num = reader.ReadInt32();
			long num2 = reader.ReadInt64();
			int num3 = reader.ReadInt32();
			long num4 = reader.ReadInt64();
			if (num != -1)
			{
				GameEntityId entityIdFromNetId = manager.GetEntityIdFromNetId(num);
				if (entityIdFromNetId.IsValid())
				{
					GameEntity gameEntity = manager.GetGameEntity(entityIdFromNetId);
					if (num2 != 0L && !(gameEntity == null))
					{
						Vector3 vector;
						Quaternion quaternion;
						BitPackUtils.UnpackHandPosRotFromNetwork(num2, out vector, out quaternion);
						if (gamePlayer != null && gamePlayer.rig.OwningNetPlayer != null)
						{
							manager.GrabEntityOnCreate(entityIdFromNetId, GamePlayer.IsLeftHand(i), vector, quaternion, gamePlayer.rig.OwningNetPlayer);
						}
					}
				}
			}
			if (num3 != -1)
			{
				GameEntityId entityIdFromNetId2 = manager.GetEntityIdFromNetId(num3);
				if (entityIdFromNetId2.IsValid())
				{
					GameEntity gameEntity2 = manager.GetGameEntity(entityIdFromNetId2);
					if (num4 != 0L && !(gameEntity2 == null))
					{
						Vector3 vector2;
						Quaternion quaternion2;
						BitPackUtils.UnpackHandPosRotFromNetwork(num4, out vector2, out quaternion2);
						if (gamePlayer != null && gamePlayer.rig.OwningNetPlayer != null)
						{
							SnapJointType snapJointType = (GamePlayer.IsLeftHand(i) ? SnapJointType.ArmL : SnapJointType.ArmR);
							manager.SnapEntityOnCreate(entityIdFromNetId2, GamePlayer.IsLeftHand(i), vector2, quaternion2, (int)snapJointType, gamePlayer.rig.OwningNetPlayer);
						}
					}
				}
			}
		}
		bool flag = reader.ReadBoolean();
		if (gamePlayer != null)
		{
			gamePlayer.SetInitializePlayer(flag);
		}
	}

	internal static void InitializeStaticLookupCaches()
	{
		GamePlayer.lookupCache_actorNum_to_gamePlayer = new ValueTuple<int, GamePlayer>[10];
		GamePlayer.lookupCache_rigInstanceId_to_gamePlayer = new ValueTuple<int, GamePlayer>[10];
		if (VRRigCache.isInitialized)
		{
			GamePlayer.UpdateStaticLookupCaches();
		}
	}

	internal static void UpdateStaticLookupCaches()
	{
		if (GamePlayer.lookupCache_actorNum_to_gamePlayer == null)
		{
			return;
		}
		List<VRRig> list;
		using (ListPool<VRRig>.Get(out list))
		{
			if (list.Capacity < 10)
			{
				list.Capacity = 10;
			}
			VRRigCache.Instance.GetActiveRigs(list);
			if (list.Count > GamePlayer.lookupCache_actorNum_to_gamePlayer.Length)
			{
				int num = list.Count * 2;
				Array.Resize<ValueTuple<int, GamePlayer>>(ref GamePlayer.lookupCache_actorNum_to_gamePlayer, num);
				Array.Resize<ValueTuple<int, GamePlayer>>(ref GamePlayer.lookupCache_rigInstanceId_to_gamePlayer, num);
			}
			GamePlayer.staticLookupCachesCount = list.Count;
			if (GamePlayer.staticLookupCachesCount >= 1)
			{
				VRRig vrrig = list[0];
				if (vrrig == null)
				{
					throw new NullReferenceException("[GT/GamePlayer::_VRRigCache_OnActiveRigsChanged]  ERROR!!!  (should never happen) The VRRig at index 0 is expected to be the local rig but is null.");
				}
				int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
				GamePlayer gamePlayer = GamePlayerLocal.instance.gamePlayer;
				GamePlayer.lookupCache_actorNum_to_gamePlayer[0] = new ValueTuple<int, GamePlayer>(actorNumber, gamePlayer);
				GamePlayer.lookupCache_rigInstanceId_to_gamePlayer[0] = new ValueTuple<int, GamePlayer>(vrrig.GetInstanceID(), gamePlayer);
			}
			for (int i = 1; i < GamePlayer.staticLookupCachesCount; i++)
			{
				VRRig vrrig2 = list[i];
				if (vrrig2 == null)
				{
					throw new NullReferenceException("[GT/GamePlayer::_VRRigCache_OnActiveRigsChanged]  ERROR!!!  (should never happen) An entry from `VRRigCache.Instance.GetActiveRigs(activeRigs)` is null but is expected to be ready and all entries not null at this stage.");
				}
				GamePlayer component = vrrig2.GetComponent<GamePlayer>();
				if (component == null)
				{
					throw new NullReferenceException("[GT/GamePlayer::_VRRigCache_OnActiveRigsChanged]  ERROR!!!  (should never happen) Could not get GamePlayer from rig which is expected to be ready at this stage.");
				}
				NetPlayer owningNetPlayer = vrrig2.OwningNetPlayer;
				int num2 = ((owningNetPlayer != null) ? owningNetPlayer.ActorNumber : int.MinValue);
				GamePlayer.lookupCache_actorNum_to_gamePlayer[i] = new ValueTuple<int, GamePlayer>(num2, component);
				GamePlayer.lookupCache_rigInstanceId_to_gamePlayer[i] = new ValueTuple<int, GamePlayer>(vrrig2.GetInstanceID(), component);
			}
			for (int j = GamePlayer.staticLookupCachesCount; j < GamePlayer.lookupCache_actorNum_to_gamePlayer.Length; j++)
			{
				GamePlayer.lookupCache_actorNum_to_gamePlayer[j] = new ValueTuple<int, GamePlayer>(0, null);
				GamePlayer.lookupCache_rigInstanceId_to_gamePlayer[j] = new ValueTuple<int, GamePlayer>(0, null);
			}
		}
	}

	public void SetInitializePlayer(bool initialized)
	{
		bool additionalDataInitialized = this.AdditionalDataInitialized;
		this.AdditionalDataInitialized = initialized;
		if (!additionalDataInitialized && this.AdditionalDataInitialized)
		{
			Action onPlayerInitialized = this.OnPlayerInitialized;
			if (onPlayerInitialized == null)
			{
				return;
			}
			onPlayerInitialized();
		}
	}

	private const string preLog = "[GamePlayer]  ";

	private const string preErr = "[GamePlayer]  ERROR!!!  ";

	public VRRig rig;

	public Transform leftHand;

	public Transform rightHand;

	public SuperInfectionSnapPointManager snapPointManager;

	private Transform[] handTransforms;

	private GamePlayer.HandData[] hands;

	public const int MAX_HANDS = 2;

	public const int LEFT_HAND = 0;

	public const int RIGHT_HAND = 1;

	public CallLimiter newJoinZoneLimiter;

	public CallLimiter netImpulseLimiter;

	public CallLimiter netGrabLimiter;

	public CallLimiter netThrowLimiter;

	public CallLimiter netStateLimiter;

	public CallLimiter netSnapLimiter;

	public Action OnPlayerInitialized;

	public Action OnPlayerLeftZone;

	private bool grabbingDisabled;

	private const bool _k_MATTO__USE_STATIC_CACHE = false;

	[OnEnterPlay_SetNull]
	private static ValueTuple<int, GamePlayer>[] lookupCache_actorNum_to_gamePlayer;

	[OnEnterPlay_SetNull]
	private static ValueTuple<int, GamePlayer>[] lookupCache_rigInstanceId_to_gamePlayer;

	[OnEnterPlay_Set(0)]
	private static int staticLookupCachesCount;

	public const int INVALID_ACTOR_NUMBER = -2147483648;

	private struct HandData
	{
		public GameEntityId grabbedEntityId;

		public GameEntityManager grabbedEntityManager;

		public GameEntityId snappedEntityId;

		public GameEntityManager snappedEntityManager;
	}
}

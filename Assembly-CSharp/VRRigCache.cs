using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaTag;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Rendering;

internal class VRRigCache : MonoBehaviour
{
	public static VRRigCache Instance { get; private set; }

	public Transform NetworkParent
	{
		get
		{
			return this.networkParent;
		}
	}

	public static bool isInitialized { get; private set; }

	public static event Action OnActiveRigsChanged;

	public static event Action OnPostInitialize;

	public static event Action OnPostSpawnRig;

	public static event Action<RigContainer> OnRigActivated;

	public static event Action<RigContainer> OnRigDeactivated;

	public static event Action<RigContainer> OnRigNameChanged;

	private void Awake()
	{
		this.InitializeVRRigCache();
		if (this.localRig != null && this.localRig.Rig != null)
		{
			VRRig rig = this.localRig.Rig;
			rig.OnNameChanged = (Action<RigContainer>)Delegate.Combine(rig.OnNameChanged, VRRigCache.OnRigNameChanged);
			if (this.localRig.Rig.bodyRenderer != null)
			{
				this.localRig.Rig.bodyRenderer.SetupAsLocalPlayerBody();
			}
		}
		TickSystemTimer ensureNetworkObjectTimer = this.m_ensureNetworkObjectTimer;
		ensureNetworkObjectTimer.callback = (Action)Delegate.Combine(ensureNetworkObjectTimer.callback, new Action(this.InstantiateNetworkObject));
	}

	private void OnDestroy()
	{
		if (VRRigCache.Instance == this)
		{
			VRRigCache.Instance = null;
		}
		VRRigCache.isInitialized = false;
		if (this.localRig != null && this.localRig.Rig != null)
		{
			VRRig rig = this.localRig.Rig;
			rig.OnNameChanged = (Action<RigContainer>)Delegate.Remove(rig.OnNameChanged, VRRigCache.OnRigNameChanged);
		}
	}

	public void InitializeVRRigCache()
	{
		if (VRRigCache.isInitialized || ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (VRRigCache.Instance != null && VRRigCache.Instance != this)
		{
			Object.Destroy(this);
			return;
		}
		VRRigCache.Instance = this;
		if (this.rigParent == null)
		{
			this.rigParent = base.transform;
		}
		if (this.networkParent == null)
		{
			this.networkParent = base.transform;
		}
		for (int i = 0; i < this.rigAmount; i++)
		{
			RigContainer rigContainer = this.SpawnRig();
			VRRigCache.freeRigs.Enqueue(rigContainer);
			rigContainer.Rig.BuildInitialize();
			rigContainer.Rig.transform.parent = null;
		}
		VRRigCache.isInitialized = true;
		Action onPostInitialize = VRRigCache.OnPostInitialize;
		if (onPostInitialize != null)
		{
			onPostInitialize();
		}
		Action onPostSpawnRig = VRRigCache.OnPostSpawnRig;
		if (onPostSpawnRig == null)
		{
			return;
		}
		onPostSpawnRig();
	}

	private RigContainer SpawnRig()
	{
		if (this.rigTemplate.activeSelf)
		{
			this.rigTemplate.SetActive(false);
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.rigTemplate, this.rigParent, false);
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<RigContainer>();
	}

	internal bool TryGetVrrig(Player targetPlayer, out RigContainer playerRig)
	{
		return this.TryGetVrrig(NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber), out playerRig);
	}

	internal bool TryGetVrrig(int targetPlayerId, out RigContainer playerRig)
	{
		return this.TryGetVrrig(NetworkSystem.Instance.GetPlayer(targetPlayerId), out playerRig);
	}

	internal bool TryGetVrrig(NetPlayer targetPlayer, out RigContainer playerRig)
	{
		playerRig = null;
		if (ApplicationQuittingState.IsQuitting)
		{
			return false;
		}
		if (targetPlayer == null || targetPlayer.IsNull)
		{
			GTDev.LogError<string>("[GT/VRRigCache]  ERROR!!!  TryGetVrrig: Supplied targetPlayer cannot be null!", null);
			return false;
		}
		if (targetPlayer.IsLocal)
		{
			playerRig = this.localRig;
			return true;
		}
		if (!targetPlayer.InRoom)
		{
			return false;
		}
		if (!VRRigCache.rigsInUse.TryGetValue(targetPlayer, out playerRig))
		{
			if (VRRigCache.freeRigs.Count <= 0)
			{
				return false;
			}
			playerRig = VRRigCache.freeRigs.Dequeue();
			playerRig.Creator = targetPlayer;
			VRRigCache.rigsInUse.Add(targetPlayer, playerRig);
			VRRig rig = playerRig.Rig;
			rig.OnNameChanged = (Action<RigContainer>)Delegate.Remove(rig.OnNameChanged, VRRigCache.OnRigNameChanged);
			VRRig rig2 = playerRig.Rig;
			rig2.OnNameChanged = (Action<RigContainer>)Delegate.Combine(rig2.OnNameChanged, VRRigCache.OnRigNameChanged);
			playerRig.gameObject.SetActive(true);
			playerRig.RigEvents.SendPostEnableEvent();
			GamePlayer.UpdateStaticLookupCaches();
			Action<RigContainer> onRigActivated = VRRigCache.OnRigActivated;
			if (onRigActivated != null)
			{
				onRigActivated(playerRig);
			}
			Action onActiveRigsChanged = VRRigCache.OnActiveRigsChanged;
			if (onActiveRigsChanged != null)
			{
				onActiveRigsChanged();
			}
		}
		return true;
	}

	private void AddRigToGorillaParent(NetPlayer player, VRRig vrrig)
	{
		GorillaParent instance = GorillaParent.instance;
		if (instance == null)
		{
			return;
		}
		if (!instance.vrrigs.Contains(vrrig))
		{
			instance.vrrigs.Add(vrrig);
		}
		if (!instance.vrrigDict.ContainsKey(player))
		{
			instance.vrrigDict.Add(player, vrrig);
			return;
		}
		instance.vrrigDict[player] = vrrig;
	}

	public void OnPlayerEnteredRoom(NetPlayer newPlayer)
	{
		if (newPlayer.ActorNumber == -1)
		{
			Debug.LogError("LocalPlayer returned, vrrig no correctly initialised");
		}
		RigContainer rigContainer;
		if (this.TryGetVrrig(newPlayer, out rigContainer))
		{
			this.AddRigToGorillaParent(newPlayer, rigContainer.Rig);
			GamePlayer.UpdateStaticLookupCaches();
			Action onActiveRigsChanged = VRRigCache.OnActiveRigsChanged;
			if (onActiveRigsChanged == null)
			{
				return;
			}
			onActiveRigsChanged();
		}
	}

	public void OnJoinedRoom()
	{
		foreach (NetPlayer netPlayer in NetworkSystem.Instance.AllNetPlayers)
		{
			RigContainer rigContainer;
			if (this.TryGetVrrig(netPlayer, out rigContainer))
			{
				this.AddRigToGorillaParent(netPlayer, rigContainer.Rig);
			}
		}
		this.m_ensureNetworkObjectTimer.Start();
		GamePlayer.UpdateStaticLookupCaches();
		Action onActiveRigsChanged = VRRigCache.OnActiveRigsChanged;
		if (onActiveRigsChanged == null)
		{
			return;
		}
		onActiveRigsChanged();
	}

	private void RemoveRigFromGorillaParent(NetPlayer player, VRRig vrrig)
	{
		GorillaParent instance = GorillaParent.instance;
		if (instance == null)
		{
			return;
		}
		if (instance.vrrigs.Contains(vrrig))
		{
			instance.vrrigs.Remove(vrrig);
		}
		if (instance.vrrigDict.ContainsKey(player))
		{
			instance.vrrigDict.Remove(player);
		}
	}

	public void OnPlayerLeftRoom(NetPlayer leavingPlayer)
	{
		if (leavingPlayer.IsNull)
		{
			Debug.LogError("Leaving players NetPlayer is Null");
			this.CheckForMissingPlayer();
		}
		RigContainer rigContainer;
		if (!VRRigCache.rigsInUse.TryGetValue(leavingPlayer, out rigContainer))
		{
			this.LogError("failed to find player's vrrig who left " + leavingPlayer.UserId);
			return;
		}
		rigContainer.gameObject.Disable();
		VRRig rig = rigContainer.Rig;
		rig.OnNameChanged = (Action<RigContainer>)Delegate.Remove(rig.OnNameChanged, VRRigCache.OnRigNameChanged);
		VRRigCache.freeRigs.Enqueue(rigContainer);
		VRRigCache.rigsInUse.Remove(leavingPlayer);
		this.RemoveRigFromGorillaParent(leavingPlayer, rigContainer.Rig);
		GamePlayer.UpdateStaticLookupCaches();
		Action<RigContainer> onRigDeactivated = VRRigCache.OnRigDeactivated;
		if (onRigDeactivated != null)
		{
			onRigDeactivated(rigContainer);
		}
		Action onActiveRigsChanged = VRRigCache.OnActiveRigsChanged;
		if (onActiveRigsChanged == null)
		{
			return;
		}
		onActiveRigsChanged();
	}

	private void CheckForMissingPlayer()
	{
		foreach (KeyValuePair<NetPlayer, RigContainer> keyValuePair in VRRigCache.rigsInUse)
		{
			if (keyValuePair.Key == null || keyValuePair.Value == null)
			{
				Debug.LogError("Somehow null reference in rigsInUse");
			}
			else if (!keyValuePair.Key.InRoom)
			{
				keyValuePair.Value.gameObject.Disable();
				VRRig rig = keyValuePair.Value.Rig;
				rig.OnNameChanged = (Action<RigContainer>)Delegate.Remove(rig.OnNameChanged, VRRigCache.OnRigNameChanged);
				VRRigCache.freeRigs.Enqueue(keyValuePair.Value);
				VRRigCache.rigsInUse.Remove(keyValuePair.Key);
				this.RemoveRigFromGorillaParent(keyValuePair.Key, keyValuePair.Value.Rig);
				GamePlayer.UpdateStaticLookupCaches();
				Action<RigContainer> onRigDeactivated = VRRigCache.OnRigDeactivated;
				if (onRigDeactivated != null)
				{
					onRigDeactivated(keyValuePair.Value);
				}
				Action onActiveRigsChanged = VRRigCache.OnActiveRigsChanged;
				if (onActiveRigsChanged != null)
				{
					onActiveRigsChanged();
				}
			}
		}
	}

	public void OnLeftRoom()
	{
		this.m_ensureNetworkObjectTimer.Stop();
		Dictionary<NetPlayer, RigContainer> dictionary;
		using (DictionaryPool<NetPlayer, RigContainer>.Get(out dictionary))
		{
			dictionary.EnsureCapacity(VRRigCache.rigsInUse.Count);
			dictionary.Clear();
			foreach (KeyValuePair<NetPlayer, RigContainer> keyValuePair in VRRigCache.rigsInUse)
			{
				NetPlayer netPlayer;
				RigContainer rigContainer;
				keyValuePair.Deconstruct(out netPlayer, out rigContainer);
				NetPlayer netPlayer2 = netPlayer;
				RigContainer rigContainer2 = rigContainer;
				dictionary.Add(netPlayer2, rigContainer2);
			}
			foreach (KeyValuePair<NetPlayer, RigContainer> keyValuePair in dictionary)
			{
				NetPlayer netPlayer;
				RigContainer rigContainer;
				keyValuePair.Deconstruct(out netPlayer, out rigContainer);
				NetPlayer netPlayer3 = netPlayer;
				RigContainer rigContainer3 = rigContainer;
				if (!(rigContainer3 == null))
				{
					VRRig rig = VRRigCache.rigsInUse[netPlayer3].Rig;
					VRRig rig2 = rigContainer3.Rig;
					rig2.OnNameChanged = (Action<RigContainer>)Delegate.Remove(rig2.OnNameChanged, VRRigCache.OnRigNameChanged);
					rigContainer3.gameObject.Disable();
					VRRigCache.rigsInUse.Remove(netPlayer3);
					this.RemoveRigFromGorillaParent(netPlayer3, rig);
					VRRigCache.freeRigs.Enqueue(rigContainer3);
				}
			}
			GamePlayer.UpdateStaticLookupCaches();
			if (VRRigCache.OnRigDeactivated != null)
			{
				foreach (RigContainer rigContainer4 in dictionary.Values)
				{
					VRRigCache.OnRigDeactivated(rigContainer4);
				}
			}
			Action onActiveRigsChanged = VRRigCache.OnActiveRigsChanged;
			if (onActiveRigsChanged != null)
			{
				onActiveRigsChanged();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal VRRig[] GetAllRigs()
	{
		VRRig[] array = new VRRig[VRRigCache.rigsInUse.Count + VRRigCache.freeRigs.Count];
		int num = 0;
		foreach (RigContainer rigContainer in VRRigCache.rigsInUse.Values)
		{
			array[num] = rigContainer.Rig;
			num++;
		}
		foreach (RigContainer rigContainer2 in VRRigCache.freeRigs)
		{
			array[num] = rigContainer2.Rig;
			num++;
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void GetAllUsedRigs(List<VRRig> rigs)
	{
		if (rigs == null)
		{
			return;
		}
		foreach (RigContainer rigContainer in VRRigCache.rigsInUse.Values)
		{
			rigs.Add(rigContainer.Rig);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void GetActiveRigs(List<VRRig> rigsListToUpdate)
	{
		if (rigsListToUpdate == null)
		{
			return;
		}
		rigsListToUpdate.Clear();
		if (!VRRigCache.isInitialized)
		{
			return;
		}
		rigsListToUpdate.Add(VRRigCache.Instance.localRig.Rig);
		foreach (RigContainer rigContainer in VRRigCache.rigsInUse.Values)
		{
			rigsListToUpdate.Add(rigContainer.Rig);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetAllRigsHash()
	{
		int num = 0;
		foreach (RigContainer rigContainer in VRRigCache.rigsInUse.Values)
		{
			num += rigContainer.GetInstanceID();
		}
		foreach (RigContainer rigContainer2 in VRRigCache.freeRigs)
		{
			num += rigContainer2.GetInstanceID();
		}
		return num;
	}

	internal void InstantiateNetworkObject()
	{
		if (this.localRig.netView.IsNotNull() || !NetworkSystem.Instance.InRoom)
		{
			return;
		}
		PrefabType prefabType;
		if (!VRRigCache.Instance.GetComponent<PhotonPrefabPool>().networkPrefabs.TryGetValue("Player Network Controller", out prefabType) || prefabType.prefab == null)
		{
			Debug.LogError("OnJoinedRoom: Unable to find player prefab to spawn");
			return;
		}
		GameObject gameObject = GTPlayer.Instance.gameObject;
		Color playerColor = this.localRig.Rig.playerColor;
		VRRigCache.rigRGBData[0] = playerColor.r;
		VRRigCache.rigRGBData[1] = playerColor.g;
		VRRigCache.rigRGBData[2] = playerColor.b;
		NetworkSystem.Instance.NetInstantiate(prefabType.prefab, gameObject.transform.position, gameObject.transform.rotation, false, 0, VRRigCache.rigRGBData, null);
	}

	internal void OnVrrigSerializerSuccesfullySpawned()
	{
		GamePlayer.UpdateStaticLookupCaches();
		Action onActiveRigsChanged = VRRigCache.OnActiveRigsChanged;
		if (onActiveRigsChanged == null)
		{
			return;
		}
		onActiveRigsChanged();
	}

	private void LogInfo(string log)
	{
	}

	private void LogWarning(string log)
	{
	}

	private void LogError(string log)
	{
	}

	private const string preLog = "[GT/VRRigCache] ";

	private const string preErr = "[GT/VRRigCache]  ERROR!!!  ";

	private const string preErrBeta = "[GT/VRRigCache]  ERROR!!!  (beta only log) ";

	private const string preErrEd = "[GT/VRRigCache]  ERROR!!!  (editor only log) ";

	public RigContainer localRig;

	[SerializeField]
	private Transform rigParent;

	[SerializeField]
	private Transform networkParent;

	[SerializeField]
	private GameObject rigTemplate;

	private int rigAmount = 9;

	[SerializeField]
	private TickSystemTimer m_ensureNetworkObjectTimer = new TickSystemTimer(0.1f);

	[OnEnterPlay_Clear]
	private static Queue<RigContainer> freeRigs = new Queue<RigContainer>(10);

	[OnEnterPlay_Clear]
	private static Dictionary<NetPlayer, RigContainer> rigsInUse = new Dictionary<NetPlayer, RigContainer>(10);

	private static object[] rigRGBData = new object[] { 0f, 0f, 0f };
}

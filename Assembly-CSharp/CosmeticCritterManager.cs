using System;
using System.Collections.Generic;
using Fusion;
using Photon.Pun;
using UnityEngine;

public class CosmeticCritterManager : NetworkSceneObject, ITickSystemTick
{
	public static CosmeticCritterManager Instance { get; private set; }

	public bool TickRunning { get; set; }

	private new void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
		base.OnEnable();
		TickSystem<object>.AddTickCallback(this);
	}

	private new void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		base.OnDisable();
		TickSystem<object>.RemoveTickCallback(this);
	}

	public void RegisterLocalHoldable(CosmeticCritterHoldable holdable)
	{
		this.localHoldables.Add(holdable);
	}

	public void RegisterIndependentSpawner(CosmeticCritterSpawnerIndependent spawner)
	{
		if (spawner.IsLocal)
		{
			this.localCritterSpawners.AddIfNew(spawner);
			return;
		}
		this.remoteCritterSpawners.AddIfNew(spawner);
	}

	public void UnregisterIndependentSpawner(CosmeticCritterSpawnerIndependent spawner)
	{
		if (spawner.IsLocal)
		{
			this.localCritterSpawners.Remove(spawner);
			return;
		}
		this.remoteCritterSpawners.Remove(spawner);
	}

	public void RegisterCatcher(CosmeticCritterCatcher catcher)
	{
		if (catcher.IsLocal)
		{
			this.localCritterCatchers.AddIfNew(catcher);
			return;
		}
		this.remoteCritterCatchers.AddIfNew(catcher);
	}

	public void UnregisterCatcher(CosmeticCritterCatcher catcher)
	{
		if (catcher.IsLocal)
		{
			this.localCritterCatchers.Remove(catcher);
			return;
		}
		this.remoteCritterCatchers.Remove(catcher);
	}

	public void RegisterTickForEachCritter(Type type, ICosmeticCritterTickForEach target)
	{
		List<ICosmeticCritterTickForEach> list;
		if (!this.tickForEachCritterOfType.TryGetValue(type, out list) || list == null)
		{
			list = new List<ICosmeticCritterTickForEach>();
			this.tickForEachCritterOfType.Add(type, list);
		}
		list.AddIfNew(target);
	}

	public void UnregisterTickForEachCritter(Type type, ICosmeticCritterTickForEach target)
	{
		List<ICosmeticCritterTickForEach> list;
		if (this.tickForEachCritterOfType.TryGetValue(type, out list) && list != null)
		{
			list.Remove(target);
		}
	}

	private void ResetLocalCallLimiters()
	{
		int i = 0;
		while (i < this.localHoldables.Count)
		{
			if (this.localHoldables[i] == null)
			{
				this.localHoldables.RemoveAt(i);
			}
			else
			{
				this.localHoldables[i].ResetCallLimiter();
				i++;
			}
		}
	}

	private void ResetCosmeticCritters(NetPlayer player)
	{
		if (NetworkSystem.Instance.LocalPlayer != player)
		{
			return;
		}
		this.ResetLocalCallLimiters();
		for (int i = 0; i < this.activeCritters.Count; i++)
		{
			this.FreeCritter(this.activeCritters[i]);
		}
	}

	private void Awake()
	{
		if (CosmeticCritterManager.Instance != null && CosmeticCritterManager.Instance != this)
		{
			global::UnityEngine.Object.Destroy(this);
			return;
		}
		CosmeticCritterManager.Instance = this;
		this.localHoldables = new List<CosmeticCritterHoldable>();
		this.localCritterSpawners = new List<CosmeticCritterSpawnerIndependent>();
		this.remoteCritterSpawners = new List<CosmeticCritterSpawnerIndependent>();
		this.localCritterCatchers = new List<CosmeticCritterCatcher>();
		this.remoteCritterCatchers = new List<CosmeticCritterCatcher>();
		this.activeCritters = new List<CosmeticCritter>();
		this.activeCrittersPerType = new Dictionary<Type, int>();
		this.activeCrittersBySeed = new Dictionary<int, CosmeticCritter>();
		this.inactiveCrittersByType = new Dictionary<Type, Stack<CosmeticCritter>>();
		this.tickForEachCritterOfType = new Dictionary<Type, List<ICosmeticCritterTickForEach>>();
		NetworkSystem.Instance.OnPlayerJoined += this.ResetCosmeticCritters;
		NetworkSystem.Instance.OnPlayerLeft += this.ResetCosmeticCritters;
	}

	private void ReuseOrSpawnNewCritter(CosmeticCritterSpawner spawner, int seed, double time)
	{
		Type critterType = spawner.GetCritterType();
		Stack<CosmeticCritter> stack;
		CosmeticCritter cosmeticCritter;
		if (!this.inactiveCrittersByType.TryGetValue(critterType, out stack))
		{
			stack = new Stack<CosmeticCritter>();
			this.inactiveCrittersByType.Add(critterType, stack);
			cosmeticCritter = global::UnityEngine.Object.Instantiate<GameObject>(spawner.GetCritterPrefab(), base.transform).GetComponent<CosmeticCritter>();
		}
		else if (stack.TryPop(out cosmeticCritter))
		{
			cosmeticCritter.gameObject.SetActive(true);
		}
		else
		{
			cosmeticCritter = global::UnityEngine.Object.Instantiate<GameObject>(spawner.GetCritterPrefab(), base.transform).GetComponent<CosmeticCritter>();
		}
		cosmeticCritter.SetSeedSpawnerTypeAndTime(seed, spawner, critterType, time);
		this.activeCritters.Add(cosmeticCritter);
		if (!this.activeCrittersPerType.ContainsKey(critterType))
		{
			this.activeCrittersPerType.Add(critterType, 1);
		}
		else
		{
			Dictionary<Type, int> dictionary = this.activeCrittersPerType;
			Type type = critterType;
			dictionary[type]++;
		}
		this.activeCrittersBySeed.Add(seed, cosmeticCritter);
		Random.State state = Random.state;
		Random.InitState(seed);
		spawner.SetRandomVariables(cosmeticCritter);
		cosmeticCritter.SetRandomVariables();
		Random.state = state;
		spawner.OnSpawn(cosmeticCritter);
		cosmeticCritter.OnSpawn();
	}

	private void FreeCritter(CosmeticCritter critter)
	{
		critter.OnDespawn();
		if (critter.Spawner != null)
		{
			critter.Spawner.OnDespawn(critter);
		}
		critter.gameObject.SetActive(false);
		Type cachedType = critter.CachedType;
		Stack<CosmeticCritter> stack;
		if (!this.inactiveCrittersByType.TryGetValue(cachedType, out stack))
		{
			stack = new Stack<CosmeticCritter>();
			this.inactiveCrittersByType.Add(cachedType, stack);
		}
		stack.Push(critter);
		this.activeCritters.Remove(critter);
		int num;
		if (this.activeCrittersPerType.TryGetValue(cachedType, out num))
		{
			this.activeCrittersPerType[cachedType] = Math.Max(num - 1, 0);
		}
		this.activeCrittersBySeed.Remove(critter.Seed);
	}

	public void Tick()
	{
		for (int i = 0; i < this.activeCritters.Count; i++)
		{
			CosmeticCritter cosmeticCritter = this.activeCritters[i];
			if (cosmeticCritter.Expired())
			{
				this.FreeCritter(cosmeticCritter);
			}
			else
			{
				cosmeticCritter.Tick();
				List<ICosmeticCritterTickForEach> list;
				if (this.tickForEachCritterOfType.TryGetValue(cosmeticCritter.CachedType, out list))
				{
					for (int j = 0; j < list.Count; j++)
					{
						list[j].TickForEachCritter(cosmeticCritter);
					}
				}
				int k = 0;
				while (k < this.localCritterCatchers.Count)
				{
					CosmeticCritterCatcher cosmeticCritterCatcher = this.localCritterCatchers[k];
					CosmeticCritterAction localCatchAction = cosmeticCritterCatcher.GetLocalCatchAction(cosmeticCritter);
					if (localCatchAction != CosmeticCritterAction.None)
					{
						double num = (PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.timeAsDouble);
						cosmeticCritterCatcher.OnCatch(cosmeticCritter, localCatchAction, num);
						if ((localCatchAction & CosmeticCritterAction.Despawn) != CosmeticCritterAction.None)
						{
							this.FreeCritter(cosmeticCritter);
							i--;
						}
						if ((localCatchAction & CosmeticCritterAction.SpawnLinked) != CosmeticCritterAction.None && cosmeticCritterCatcher.GetLinkedSpawner() != null)
						{
							this.ReuseOrSpawnNewCritter(cosmeticCritterCatcher.GetLinkedSpawner(), cosmeticCritter.Seed + 1, num);
						}
						if (PhotonNetwork.InRoom && (localCatchAction & CosmeticCritterAction.RPC) != CosmeticCritterAction.None)
						{
							this.photonView.RPC("CosmeticCritterRPC", RpcTarget.Others, new object[] { localCatchAction, cosmeticCritterCatcher.OwnerID, cosmeticCritter.Seed });
							break;
						}
						break;
					}
					else
					{
						k++;
					}
				}
			}
		}
		for (int l = 0; l < this.localCritterSpawners.Count; l++)
		{
			CosmeticCritterSpawnerIndependent cosmeticCritterSpawnerIndependent = this.localCritterSpawners[l];
			int num2;
			if ((!this.activeCrittersPerType.TryGetValue(cosmeticCritterSpawnerIndependent.GetCritterType(), out num2) || num2 < cosmeticCritterSpawnerIndependent.GetCritter().GetGlobalMaxCritters()) && cosmeticCritterSpawnerIndependent.CanSpawnLocal())
			{
				int num3 = Random.Range(0, int.MaxValue);
				if (!this.activeCrittersBySeed.ContainsKey(num3))
				{
					this.ReuseOrSpawnNewCritter(cosmeticCritterSpawnerIndependent, num3, PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.timeAsDouble);
					if (PhotonNetwork.InRoom)
					{
						this.photonView.RPC("CosmeticCritterRPC", RpcTarget.Others, new object[]
						{
							CosmeticCritterAction.RPC | CosmeticCritterAction.Spawn,
							cosmeticCritterSpawnerIndependent.OwnerID,
							num3
						});
					}
				}
			}
		}
	}

	[PunRPC]
	private void CosmeticCritterRPC(CosmeticCritterAction action, int holdableID, int seed, PhotonMessageInfo info)
	{
		PhotonMessageInfoWrapped photonMessageInfoWrapped = new PhotonMessageInfoWrapped(info);
		GorillaNot.IncrementRPCCall(photonMessageInfoWrapped, "CosmeticCritterRPC");
		if ((action & CosmeticCritterAction.RPC) == CosmeticCritterAction.None)
		{
			return;
		}
		if (action == (CosmeticCritterAction.RPC | CosmeticCritterAction.Spawn))
		{
			this.SpawnCosmeticCritterRPC(holdableID, seed, photonMessageInfoWrapped);
			return;
		}
		this.CatchCosmeticCritterRPC(action, holdableID, seed, photonMessageInfoWrapped);
	}

	private void CatchCosmeticCritterRPC(CosmeticCritterAction catchAction, int catcherID, int seed, PhotonMessageInfoWrapped info)
	{
		CosmeticCritter cosmeticCritter;
		if (!this.activeCrittersBySeed.TryGetValue(seed, out cosmeticCritter))
		{
			return;
		}
		int i = 0;
		while (i < this.remoteCritterCatchers.Count)
		{
			CosmeticCritterCatcher cosmeticCritterCatcher = this.remoteCritterCatchers[i];
			if (cosmeticCritterCatcher.OwnerID == catcherID)
			{
				if (!cosmeticCritterCatcher.OwningPlayerMatches(info))
				{
					return;
				}
				if (cosmeticCritterCatcher.ValidateRemoteCatchAction(cosmeticCritter, catchAction, info.SentServerTime))
				{
					cosmeticCritterCatcher.OnCatch(cosmeticCritter, catchAction, info.SentServerTime);
					if ((catchAction & CosmeticCritterAction.Despawn) != CosmeticCritterAction.None)
					{
						this.FreeCritter(cosmeticCritter);
					}
					int num;
					if ((catchAction & CosmeticCritterAction.SpawnLinked) != CosmeticCritterAction.None && cosmeticCritterCatcher.GetLinkedSpawner() != null && (!this.activeCrittersPerType.TryGetValue(cosmeticCritterCatcher.GetLinkedSpawner().GetCritterType(), out num) || num < cosmeticCritterCatcher.GetLinkedSpawner().GetCritter().GetGlobalMaxCritters() + 1))
					{
						this.ReuseOrSpawnNewCritter(cosmeticCritterCatcher.GetLinkedSpawner(), seed + 1, info.SentServerTime);
					}
				}
				return;
			}
			else
			{
				i++;
			}
		}
	}

	private void SpawnCosmeticCritterRPC(int spawnerID, int seed, PhotonMessageInfoWrapped info)
	{
		if (this.activeCrittersBySeed.ContainsKey(seed))
		{
			return;
		}
		int i = 0;
		while (i < this.remoteCritterSpawners.Count)
		{
			CosmeticCritterSpawnerIndependent cosmeticCritterSpawnerIndependent = this.remoteCritterSpawners[i];
			if (cosmeticCritterSpawnerIndependent.OwnerID == spawnerID)
			{
				if (!cosmeticCritterSpawnerIndependent.OwningPlayerMatches(info))
				{
					return;
				}
				int num;
				if ((!this.activeCrittersPerType.TryGetValue(cosmeticCritterSpawnerIndependent.GetCritterType(), out num) || num < cosmeticCritterSpawnerIndependent.GetCritter().GetGlobalMaxCritters()) && cosmeticCritterSpawnerIndependent.CanSpawnRemote(info.SentServerTime))
				{
					this.ReuseOrSpawnNewCritter(cosmeticCritterSpawnerIndependent, seed, info.SentServerTime);
				}
				return;
			}
			else
			{
				i++;
			}
		}
	}

	private List<CosmeticCritterHoldable> localHoldables;

	private List<CosmeticCritterSpawnerIndependent> localCritterSpawners;

	private List<CosmeticCritterSpawnerIndependent> remoteCritterSpawners;

	private List<CosmeticCritterCatcher> localCritterCatchers;

	private List<CosmeticCritterCatcher> remoteCritterCatchers;

	private List<CosmeticCritter> activeCritters;

	private Dictionary<Type, int> activeCrittersPerType;

	private Dictionary<int, CosmeticCritter> activeCrittersBySeed;

	private Dictionary<Type, Stack<CosmeticCritter>> inactiveCrittersByType;

	private Dictionary<Type, List<ICosmeticCritterTickForEach>> tickForEachCritterOfType;
}

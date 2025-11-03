using System;
using System.Collections.Generic;
using GorillaGameModes;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class SuperInfection : MonoBehaviour, IGorillaSliceableSimple
{
	public bool IsAuthorityAndActive
	{
		get
		{
			return this.siManager.gameEntityManager.IsAuthority() && this.siManager.gameEntityManager.IsZoneActive();
		}
	}

	public float ResourceSpawnInterval
	{
		get
		{
			if (!Application.isPlaying)
			{
				return 0f;
			}
			return this.GetResourceSpawnInterval();
		}
	}

	public float TimeSinceLastSpawn
	{
		get
		{
			return Time.time - this._lastResourceSpawnTime;
		}
	}

	public float TimeToNextSpawn
	{
		get
		{
			if (!Application.isPlaying)
			{
				return 0f;
			}
			if (this._lastResourceSpawnTime <= 0f)
			{
				return 0f;
			}
			return this.GetResourceSpawnInterval() - (Time.time - this._lastResourceSpawnTime);
		}
	}

	private void Awake()
	{
		this.resourceRegions = this.resourceNodeParent.GetComponentsInChildren<SIResourceRegion>(true);
		this._resourcePrefabs = new List<SIResource>();
		foreach (SIResourceRegion siresourceRegion in this.resourceRegions)
		{
			if (!this._resourcePrefabs.Contains(siresourceRegion.resourcePrefab))
			{
				this._resourcePrefabs.Add(siresourceRegion.resourcePrefab);
			}
		}
		this.perRoundResourceRegions = this.perRoundResourceNodeParent.GetComponentsInChildren<SIResourceRegion>(true);
		this.resourceResetHeight = this.resourceResetLoc.position.y;
	}

	public void OnEnable()
	{
		this.siManager = SuperInfectionManager.GetSIManagerForZone(this.zone);
		if (this.siManager != null)
		{
			Debug.Log(string.Format("$OnEnable: {0} zoneSuperInfection = {1}", this.siManager, this));
			this.siManager.OnEnableZoneSuperInfection(this);
		}
		if (this.siManager.isActiveAndEnabled)
		{
			this.DisableStations();
		}
		if (NetworkSystem.Instance != null)
		{
			NetworkSystem.Instance.OnPlayerLeft += this.RemovePlayerGadgetsOnLeave;
		}
		for (int i = 0; i < this.siTerminals.Length; i++)
		{
			this.siTerminals[i].index = i;
		}
		for (int j = 0; j < this.siDeposits.Length; j++)
		{
			this.siDeposits[j].index = j;
		}
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (this.siManager)
		{
			this.siManager.zoneSuperInfection = null;
		}
		this.DisableStations();
		if (NetworkSystem.Instance != null)
		{
			NetworkSystem.Instance.OnPlayerLeft -= this.RemovePlayerGadgetsOnLeave;
		}
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnZoneInit()
	{
		this.EnableStations();
	}

	public void OnZoneClear(ZoneClearReason reason)
	{
		if (reason != ZoneClearReason.JoinZone)
		{
			this.DisableStations();
			SIProgression.Instance.SendTelemetryData();
		}
	}

	private void EnableStations()
	{
		for (int i = 0; i < this.siTerminals.Length; i++)
		{
			this.siTerminals[i].gameObject.SetActive(true);
		}
		for (int j = 0; j < this.siDeposits.Length; j++)
		{
			this.siDeposits[j].gameObject.SetActive(true);
		}
		this.questBoard.gameObject.SetActive(true);
		if (this.purchaseTerminal != null)
		{
			this.purchaseTerminal.gameObject.SetActive(true);
		}
		for (int k = 0; k < this.zoneObjects.Length; k++)
		{
			GameObject gameObject = this.zoneObjects[k];
			if (gameObject != null)
			{
				gameObject.SetActive(true);
			}
			else
			{
				Debug.LogError("[GT/SuperInfection]  ERROR!!!  " + string.Format("null ref at `zoneObjects[{0}]`.", k));
			}
		}
	}

	private void DisableStations()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		for (int i = 0; i < this.siTerminals.Length; i++)
		{
			this.siTerminals[i].gameObject.SetActive(false);
			this.siTerminals[i].Reset();
		}
		for (int j = 0; j < this.siDeposits.Length; j++)
		{
			this.siDeposits[j].gameObject.SetActive(false);
		}
		this.questBoard.gameObject.SetActive(false);
		if (this.purchaseTerminal != null)
		{
			this.purchaseTerminal.gameObject.SetActive(false);
		}
		for (int k = 0; k < this.zoneObjects.Length; k++)
		{
			GameObject gameObject = this.zoneObjects[k];
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
			else
			{
				Debug.LogError("[GT/SuperInfection]  ERROR!!!  " + string.Format("null ref at `zoneObjects[{0}]`.", k));
			}
		}
	}

	public void Update()
	{
		if (!this.IsAuthorityAndActive)
		{
			return;
		}
		if (this.retryCreatePerRoundResources)
		{
			this.CreatePerRoundResources();
		}
		if (Time.time >= this._nextResourceUpdateTime)
		{
			this.GetResourceSpawnInterval();
			foreach (SIResourceRegion siresourceRegion in this.resourceRegions)
			{
				for (int j = siresourceRegion.ItemCount - 1; j >= 0; j--)
				{
					GameEntity gameEntity = siresourceRegion.Items[j];
					if (!gameEntity)
					{
						GTDev.Log<string>(string.Format("Removing null item at {0}", j), null);
						siresourceRegion.Items.RemoveAt(j);
					}
					else if (gameEntity.transform.position.y < this.resourceResetHeight)
					{
						this.siManager.gameEntityManager.RequestDestroyItem(gameEntity.id);
					}
				}
			}
			this.CheckResourceSpawn();
			this._nextResourceUpdateTime = Time.time + 1f;
		}
	}

	private void CheckResourceSpawn()
	{
		if (Time.time >= this.GetNextResourceSpawnTime())
		{
			SIResourceRegion siresourceRegion = null;
			float num = float.MaxValue;
			foreach (SIResourceRegion siresourceRegion2 in this.resourceRegions)
			{
				if (siresourceRegion2.ItemCount < siresourceRegion2.MaxItems && siresourceRegion2.LastSpawnTime < num)
				{
					siresourceRegion = siresourceRegion2;
					num = siresourceRegion2.LastSpawnTime;
				}
			}
			if (!siresourceRegion)
			{
				this._lastResourceSpawnTime = Time.time;
				return;
			}
			ValueTuple<bool, Vector3, Vector3> spawnPointWithNormal = siresourceRegion.GetSpawnPointWithNormal(5);
			if (!spawnPointWithNormal.Item1)
			{
				GTDev.Log<string>(string.Format("[{0}] Couldn't find a valid {1} spawn point in {2}", base.name, siresourceRegion.resourcePrefab.name, siresourceRegion), null);
				return;
			}
			if (siresourceRegion.resourcePrefab == null)
			{
				GTDev.Log<string>("No resourceprefab set for region", null);
				return;
			}
			float spawnPitchVariance = siresourceRegion.resourcePrefab.spawnPitchVariance;
			Quaternion quaternion = Quaternion.Euler(Random.Range(-spawnPitchVariance, spawnPitchVariance), (float)Random.Range(0, 360), Random.Range(-spawnPitchVariance, spawnPitchVariance));
			Quaternion quaternion2 = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, spawnPointWithNormal.Item3), spawnPointWithNormal.Item3) * quaternion;
			GameEntity gameEntity = this.siManager.gameEntityManager.GetGameEntity(this.siManager.gameEntityManager.RequestCreateItem(siresourceRegion.resourcePrefab.gameObject.name.GetStaticHash(), spawnPointWithNormal.Item2, quaternion2, 0L));
			if (gameEntity)
			{
				GTDev.Log<string>(string.Format("Spawned {0} at {1}", gameEntity.name, spawnPointWithNormal.Item2), gameEntity, null);
				siresourceRegion.AddItem(gameEntity);
				siresourceRegion.LastSpawnTime = (this._lastResourceSpawnTime = Time.time);
				return;
			}
			GTDev.LogError<string>(string.Format("Failed to spawn {0} at {1}", siresourceRegion.resourcePrefab.gameObject.name, spawnPointWithNormal.Item2), null);
		}
	}

	private float GetNextResourceSpawnTime()
	{
		if (this._lastResourceSpawnTime <= 0f)
		{
			return 0f;
		}
		return this._lastResourceSpawnTime + this.GetResourceSpawnInterval();
	}

	private float GetResourceSpawnInterval()
	{
		return 3600f / (float)(this.perPlayerHourlyResourceRate * Mathf.Max(GameMode.ParticipatingPlayers.Count, this.minRoomPopulation));
	}

	public void RemovePlayerGadgetsOnLeave(NetPlayer player)
	{
		SIPlayer siplayer = SIPlayer.Get(player.ActorNumber);
		if (siplayer == null)
		{
			return;
		}
		if (this.siManager.gameEntityManager.IsAuthority())
		{
			for (int i = siplayer.activePlayerGadgets.Count - 1; i >= 0; i--)
			{
				this.siManager.gameEntityManager.RequestDestroyItem(this.siManager.gameEntityManager.GetGameEntityFromNetId(siplayer.activePlayerGadgets[i]).id);
			}
		}
		siplayer.activePlayerGadgets.Clear();
	}

	public void RefreshStations(int actorNr)
	{
		for (int i = 0; i < this.siTerminals.Length; i++)
		{
			if (!(this.siTerminals[i].activePlayer == null) && this.siTerminals[i].activePlayer.gameObject.activeInHierarchy && this.siTerminals[i].activePlayer.ActorNr == actorNr)
			{
				this.siTerminals[i].techTree.UpdateState(this.siTerminals[i].techTree.currentState);
				this.siTerminals[i].resourceCollection.UpdateState(this.siTerminals[i].resourceCollection.currentState);
				this.siTerminals[i].dispenser.UpdateState(this.siTerminals[i].dispenser.currentState);
			}
		}
		if (SIPlayer.LocalPlayer.ActorNr == actorNr && this.purchaseTerminal != null)
		{
			this.purchaseTerminal.UpdateCurrentTechPoints();
		}
	}

	public void SliceUpdate()
	{
		if (this.siManager.gameEntityManager.IsAuthority())
		{
			for (int i = this.activeGadgets.Count - 1; i >= 0; i--)
			{
				if (this.activeGadgets[i] == null)
				{
					this.activeGadgets.RemoveAt(i);
				}
				else if (this.activeGadgets[i].transform.position.y < this.resourceResetHeight)
				{
					this.siManager.gameEntityManager.RequestDestroyItem(this.activeGadgets[i].gameEntity.id);
					this.activeGadgets.RemoveAt(i);
				}
			}
		}
	}

	public List<SIResource> ResourcePrefabs
	{
		get
		{
			return this._resourcePrefabs;
		}
	}

	public void AddGadget(SIGadget gadget)
	{
		this.activeGadgets.Add(gadget);
	}

	public void RemoveGadget(SIGadget gadget)
	{
		this.activeGadgets.Remove(gadget);
	}

	public void ResetPerRoundResources()
	{
		this.ClearPerRoundResources();
		this.CreatePerRoundResources();
	}

	private void CreatePerRoundResources()
	{
		if (!this.siManager.gameEntityManager.IsZoneActive())
		{
			this.retryCreatePerRoundResources = true;
			return;
		}
		this.retryCreatePerRoundResources = false;
		foreach (SIResourceRegion siresourceRegion in this.perRoundResourceRegions)
		{
			for (int j = siresourceRegion.ItemCount; j < siresourceRegion.MaxItems; j++)
			{
				ValueTuple<bool, Vector3, Vector3> spawnPointWithNormal = siresourceRegion.GetSpawnPointWithNormal(5);
				Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, spawnPointWithNormal.Item3), spawnPointWithNormal.Item3) * Quaternion.Euler(0f, (float)Random.Range(0, 360), 0f);
				GameEntity gameEntity = this.siManager.gameEntityManager.GetGameEntity(this.siManager.gameEntityManager.RequestCreateItem(siresourceRegion.resourcePrefab.gameObject.name.GetStaticHash(), spawnPointWithNormal.Item2, quaternion, 0L));
				if (gameEntity)
				{
					siresourceRegion.AddItem(gameEntity);
					if (!spawnPointWithNormal.Item1)
					{
						Rigidbody component = gameEntity.GetComponent<Rigidbody>();
						if (component != null)
						{
							component.isKinematic = false;
						}
					}
				}
				else
				{
					GTDev.LogError<string>(string.Format("Failed to spawn {0} at {1}", siresourceRegion.resourcePrefab.gameObject.name, spawnPointWithNormal.Item2), null);
				}
			}
		}
	}

	private void ClearPerRoundResources()
	{
		foreach (SIResourceRegion siresourceRegion in this.perRoundResourceRegions)
		{
			for (int j = siresourceRegion.ItemCount - 1; j >= 0; j--)
			{
				GameEntity gameEntity = siresourceRegion.Items[j];
				if (!gameEntity)
				{
					siresourceRegion.Items.RemoveAt(j);
				}
				else if (gameEntity.lastHeldByActorNumber == 0 || !(SIPlayer.Get(gameEntity.lastHeldByActorNumber) != null))
				{
					this.siManager.gameEntityManager.RequestDestroyItem(gameEntity.id);
				}
			}
		}
	}

	private const string preLog = "[GT/SuperInfection]  ";

	private const string preErr = "[GT/SuperInfection]  ERROR!!!  ";

	public SICombinedTerminal[] siTerminals;

	public SIResourceDeposit[] siDeposits;

	public SIQuestBoard questBoard;

	public SIPurchaseTerminal purchaseTerminal;

	[Tooltip("Add miscellaneous zone objects here.  They'll be disabled when not in this mode.")]
	public GameObject[] zoneObjects;

	public Transform resourceNodeParent;

	public SIResourceRegion[] resourceRegions;

	public int perPlayerHourlyResourceRate = 20;

	[Tooltip("Resource generation rate varies based on population.  We'll assume at least this many players are present.")]
	public int minRoomPopulation = 4;

	public Transform perRoundResourceNodeParent;

	public SIResourceRegion[] perRoundResourceRegions;

	public SuperInfectionManager siManager;

	public Transform resourceResetLoc;

	private float resourceResetHeight;

	public List<SIGadget> activeGadgets = new List<SIGadget>();

	public GTZone zone;

	public SITechTreeSO techTreeSO;

	private bool retryCreatePerRoundResources;

	private float _nextResourceUpdateTime;

	private float _lastResourceSpawnTime;

	private int authorityActorNumber;

	public TextMeshProUGUI authorityName;

	private List<SIResource> _resourcePrefabs;
}

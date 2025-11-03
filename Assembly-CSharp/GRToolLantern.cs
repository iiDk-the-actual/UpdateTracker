using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(GameEntity))]
public class GRToolLantern : MonoBehaviour, IGRSummoningEntity
{
	private void Awake()
	{
		this.trackedEntities = new List<int>();
		this.state = GRToolLantern.State.Off;
		this.gameEntity.OnStateChanged += this.OnStateChanged;
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnReleased = (Action)Delegate.Combine(gameEntity2.OnReleased, new Action(this.OnReleased));
		if (this.tool != null)
		{
			this.tool.onToolUpgraded += this.OnToolUpgraded;
			this.OnToolUpgraded(this.tool);
		}
	}

	private void OnEnable()
	{
		this.TurnOff();
		this.state = GRToolLantern.State.Off;
	}

	private void OnDestroy()
	{
		if (this.providingXRay && this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity3))
		{
			this.DisableXRay();
		}
	}

	private void OnToolUpgraded(GRTool tool)
	{
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity1))
		{
			this.turnOnSound = this.upgrade1TurnOnSound;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity2))
		{
			this.turnOnSound = this.upgrade2TurnOnSound;
			return;
		}
		if (tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity3))
		{
			this.turnOnSound = this.upgrade3TurnOnSound;
		}
	}

	public void OnGrabbed()
	{
	}

	public void OnReleased()
	{
		if (this.WasLastHeldLocal())
		{
			this.DisableXRay();
		}
	}

	private void EnableXRay()
	{
		if (!this.providingXRay && this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity3))
		{
			GRPlayer.GetLocal().xRayVisionRefCount++;
			this.providingXRay = true;
		}
	}

	private void DisableXRay()
	{
		if (this.providingXRay && this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity3))
		{
			GRPlayer.GetLocal().xRayVisionRefCount--;
			this.providingXRay = false;
		}
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		if (this.IsHeldLocal() || this.tool.energy > 0)
		{
			this.OnUpdateAuthority(deltaTime);
			return;
		}
		this.OnUpdateRemote(deltaTime);
	}

	private void OnUpdateAuthority(float dt)
	{
		if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity3))
		{
			bool flag = this.IsHeld();
			this.EnableLights(flag);
		}
		if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity2))
		{
			this.SetState(GRToolLantern.State.On);
			if (Time.timeAsDouble > this.lastFlareDropTime + this.minFlareDropInterval && this.IsButtonHeld() && this.tool.HasEnoughEnergy() && this.trackedEntities.Count < this.maxSpawnedFlares && this.lanternFlarePrefab != null)
			{
				if (this.IsHeldLocal())
				{
					Vector3 vector = base.transform.rotation * this.flareSpawnoffset;
					this.gameEntity.manager.RequestCreateItem(this.lanternFlarePrefab.name.GetStaticHash(), base.transform.position + vector, base.transform.rotation * Quaternion.Euler(10f, 0f, 10f), (long)this.gameEntity.GetNetId());
				}
				this.lastFlareDropTime = Time.timeAsDouble;
				this.tool.UseEnergy();
				this.audioSource.PlayOneShot(this.turnOnSound, this.turnOnSoundVolume);
				return;
			}
		}
		else
		{
			GRToolLantern.State state = this.state;
			if (state != GRToolLantern.State.Off)
			{
				if (state != GRToolLantern.State.On)
				{
					return;
				}
				this.timeOnSpentEnergy -= dt;
				if ((!this.IsButtonHeld() && this.timeOnSpentEnergy <= 0f) || this.tool.energy <= 0)
				{
					this.SetState(GRToolLantern.State.Off);
					this.gameEntity.RequestState(this.gameEntity.id, 0L);
					return;
				}
				if (this.IsButtonHeld() && this.timeOnSpentEnergy <= 0f)
				{
					this.TryConsumeEnergy();
				}
			}
			else if (this.IsButtonHeld() && this.tool.HasEnoughEnergy())
			{
				this.SetState(GRToolLantern.State.On);
				this.gameEntity.RequestState(this.gameEntity.id, 1L);
				return;
			}
		}
	}

	private void TryConsumeEnergy()
	{
		if (this.tool.HasEnoughEnergy())
		{
			this.tool.UseEnergy();
			this.timeOnSpentEnergy = this.timeOnPerEnergyUseDurationSeconds * 10f * (float)this.tool.GetEnergyUseCost() / (float)this.tool.GetEnergyMax();
		}
	}

	private void OnUpdateRemote(float dt)
	{
		GRToolLantern.State state = (GRToolLantern.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
	}

	private void SetState(GRToolLantern.State newState)
	{
		if (this.state == newState)
		{
			return;
		}
		if (!this.CanChangeState((long)newState))
		{
			return;
		}
		this.state = newState;
		GRToolLantern.State state = this.state;
		if (state != GRToolLantern.State.Off)
		{
			if (state == GRToolLantern.State.On)
			{
				this.TurnOn();
				return;
			}
		}
		else
		{
			this.TurnOff();
		}
	}

	private void TurnOn()
	{
		if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity3))
		{
			this.EnableXRay();
		}
		else
		{
			this.EnableLights(true);
		}
		this.audioSource.PlayOneShot(this.turnOnSound, this.turnOnSoundVolume);
		this.onHaptic.PlayIfHeldLocal(this.gameEntity);
		this.timeLastTurnedOn = Time.time;
	}

	private void EnableLights(bool isOn)
	{
		if (this.gameLight.gameObject.activeSelf == isOn)
		{
			return;
		}
		if (this.attributes.HasBeenInitialized())
		{
			this.gameLight.light.intensity = (float)this.attributes.CalculateFinalValueForAttribute(GRAttributeType.LightIntensity);
		}
		this.gameLight.gameObject.SetActive(isOn);
		for (int i = 0; i < this.meshAndMaterials.Count; i++)
		{
			MaterialUtils.SwapMaterial(this.meshAndMaterials[i], !isOn);
		}
	}

	private void TurnOff()
	{
		if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.LanternIntensity3))
		{
			this.DisableXRay();
			return;
		}
		this.EnableLights(false);
	}

	private bool IsHeld()
	{
		return this.gameEntity.IsHeld();
	}

	private bool IsHeldLocal()
	{
		return this.gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
	}

	private bool WasLastHeldLocal()
	{
		return this.gameEntity.lastHeldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
	}

	private bool IsButtonHeld()
	{
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(this.gameEntity.heldByActorNumber, out gamePlayer))
		{
			return false;
		}
		int num = gamePlayer.FindHandIndex(this.gameEntity.id);
		if (num == -1)
		{
			return false;
		}
		if (!GamePlayer.IsLeftHand(num))
		{
			return gamePlayer.rig.rightIndex.calcT > 0.25f;
		}
		return gamePlayer.rig.leftIndex.calcT > 0.25f;
	}

	private void OnStateChanged(long prevState, long nextState)
	{
	}

	public bool CanChangeState(long newStateIndex)
	{
		if (newStateIndex < 0L || newStateIndex >= 2L)
		{
			return false;
		}
		GRToolLantern.State state = (GRToolLantern.State)newStateIndex;
		if (state != GRToolLantern.State.Off)
		{
			return state == GRToolLantern.State.On && this.tool.energy > 0;
		}
		return Time.time > this.timeLastTurnedOn + this.minOnDuration || this.tool.energy <= 0;
	}

	public void AddTrackedEntity(GameEntity entityToTrack)
	{
		int netId = entityToTrack.GetNetId();
		this.trackedEntities.AddIfNew(netId);
	}

	public void RemoveTrackedEntity(GameEntity entityToRemove)
	{
		int netId = entityToRemove.GetNetId();
		if (this.trackedEntities.Contains(netId))
		{
			this.trackedEntities.Remove(netId);
		}
	}

	public void OnSummonedEntityInit(GameEntity entity)
	{
		this.AddTrackedEntity(entity);
	}

	public void OnSummonedEntityDestroy(GameEntity entity)
	{
		this.RemoveTrackedEntity(entity);
	}

	public GameEntity gameEntity;

	public GRTool tool;

	public GameLight gameLight;

	public GRAttributes attributes;

	[SerializeField]
	private float timeOnPerEnergyUseDurationSeconds = 2f;

	[SerializeField]
	private int minEnergyPerUse = 1;

	[SerializeField]
	private float turnOnSoundVolume;

	[SerializeField]
	private AudioClip turnOnSound;

	[SerializeField]
	private AudioClip upgrade1TurnOnSound;

	[SerializeField]
	private AudioClip upgrade2TurnOnSound;

	[SerializeField]
	private AudioClip upgrade3TurnOnSound;

	[SerializeField]
	private AudioSource audioSource;

	public List<MeshAndMaterials> meshAndMaterials;

	[Header("Haptic")]
	public AbilityHaptic onHaptic;

	private float timeOnSpentEnergy;

	private float timeLastTurnedOn;

	private float minOnDuration = 0.5f;

	private GRToolLantern.State state;

	private List<int> trackedEntities;

	private double lastFlareDropTime;

	public double minFlareDropInterval = 1.0;

	public GameEntity lanternFlarePrefab;

	public int maxSpawnedFlares = 10;

	private bool providingXRay;

	public Vector3 flareSpawnoffset = Vector3.zero;

	private enum State
	{
		Off,
		On,
		Count
	}
}

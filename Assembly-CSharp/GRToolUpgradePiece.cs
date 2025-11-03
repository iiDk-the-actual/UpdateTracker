using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GRToolUpgradePiece : MonoBehaviour, IGameEntityComponent
{
	private void Start()
	{
		MeshFilter componentInChildren = base.GetComponentInChildren<MeshFilter>();
		if (componentInChildren != null)
		{
			this.meshCollider.sharedMesh = componentInChildren.sharedMesh;
		}
	}

	private void EnableProcAnimLoop()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnTick = (Action)Delegate.Combine(gameEntity.OnTick, new Action(this.Tick));
		if (!this.humAudioSource.isPlaying)
		{
			this.humAudioSource.volume = 0f;
			this.humAudioSource.GTPlay();
		}
	}

	private void DisableProcAnimLoop()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnTick = (Action)Delegate.Remove(gameEntity.OnTick, new Action(this.Tick));
		this.SwitchMagnetizedTarget(null);
		this.childVisualTransform.localPosition = Vector3.zero;
		this.childVisualTransform.localRotation = Quaternion.identity;
		this.childVisualTransform.localScale = Vector3.one;
		this.humAudioSource.Stop();
		if (this.attractParticleSystem != null)
		{
			this.attractParticleSystem.Stop();
		}
	}

	private void SwitchMagnetizedTarget(GameEntity entity)
	{
		this.currentMagnetizingTool = entity;
	}

	private void Tick()
	{
		Vector3 position = base.transform.position;
		List<GameEntity> gameEntities = this.gameEntity.manager.GetGameEntities();
		int num = this.gameEntityListCheckIndex;
		int num2 = ((this.toolSearchesPerFrame < gameEntities.Count) ? this.toolSearchesPerFrame : gameEntities.Count);
		GRTool grtool = ((this.currentMagnetizingTool != null) ? this.currentMagnetizingTool.GetComponent<GRTool>() : null);
		GRTool.Upgrade upgrade = ((grtool != null) ? grtool.FindMatchingUpgrade(this.matchingUpgrade) : null);
		float num3 = ((grtool != null) ? grtool.GetPointDistanceToUpgrade(position, upgrade) : 1E+10f);
		if (num3 > this.minDistToStartMagnetize)
		{
			this.SwitchMagnetizedTarget(null);
			grtool = null;
			upgrade = null;
			num3 = 1E+10f;
		}
		for (int i = 0; i < num2; i++)
		{
			num = (num + 1) % gameEntities.Count;
			GameEntity gameEntity = gameEntities[num];
			if (!(gameEntity == null))
			{
				GRTool component = gameEntity.GetComponent<GRTool>();
				if (component != null && gameEntity.heldByActorNumber != -1)
				{
					GRTool.Upgrade upgrade2 = component.FindMatchingUpgrade(this.matchingUpgrade);
					if (upgrade2 != null)
					{
						float pointDistanceToUpgrade = component.GetPointDistanceToUpgrade(position, upgrade2);
						if (pointDistanceToUpgrade > 0f && pointDistanceToUpgrade < num3 && pointDistanceToUpgrade < this.minDistToStartMagnetize)
						{
							this.SwitchMagnetizedTarget(gameEntity);
							grtool = component;
							upgrade = upgrade2;
							num3 = pointDistanceToUpgrade;
						}
					}
				}
			}
		}
		this.gameEntityListCheckIndex = num;
		if (grtool != null)
		{
			Transform upgradeAttachTransform = grtool.GetUpgradeAttachTransform(upgrade);
			if (num3 >= this.minDistToSnap)
			{
				float num4 = Mathf.Clamp01(num3 / this.minDistToStartMagnetize);
				this.humAudioSource.volume = Mathf.Lerp(this.magnetizingLoopMaxVolume, this.magnetizingLoopMinVolume, num4);
				float num5 = this.shakeMaxAmount * (1f - num4);
				float num6 = Mathf.Clamp01((this.visualDistanceCurve != null) ? this.visualDistanceCurve.Evaluate(num4) : num4);
				this.shakePhase += Time.deltaTime * this.shakeFrequency;
				if (this.shakePhase > 6.2831855f)
				{
					this.shakePhase -= 6.2831855f;
				}
				Transform transform = base.transform;
				if (this.childVisualTransform != null)
				{
					Vector3 vector = Vector3.Lerp(upgradeAttachTransform.position, transform.position, num6);
					Quaternion quaternion = Quaternion.Slerp(upgradeAttachTransform.rotation, transform.rotation, num6);
					Vector3 vector2 = Vector3.Lerp(upgradeAttachTransform.localScale, transform.localScale, num6);
					vector2.x /= transform.localScale.x;
					vector2.y /= transform.localScale.y;
					vector2.z /= transform.localScale.y;
					quaternion *= Quaternion.Euler(new Vector3(num5 * Mathf.Sin(this.shakePhase), num5 * Mathf.Cos(this.shakePhase), 0f));
					this.childVisualTransform.position = vector;
					this.childVisualTransform.rotation = quaternion;
					this.childVisualTransform.localScale = vector2;
				}
				if (this.attractParticleSystem != null)
				{
					if (!this.attractParticleSystem.isPlaying)
					{
						this.attractParticleSystem.Play();
					}
					this.attractParticleSystem.emission.enabled = true;
				}
				this.forceField.transform.position = upgradeAttachTransform.position;
				return;
			}
			this.humAudioSource.volume = 0f;
			if (this.attractParticleSystem != null)
			{
				this.attractParticleSystem.Stop();
			}
			this.childVisualTransform.position = upgradeAttachTransform.position;
			this.childVisualTransform.rotation = upgradeAttachTransform.rotation;
			this.childVisualTransform.localScale = new Vector3(upgradeAttachTransform.localScale.x / base.transform.localScale.x, upgradeAttachTransform.localScale.y / base.transform.localScale.y, upgradeAttachTransform.localScale.z / base.transform.localScale.z);
			if (this.currentMagnetizingTool != null)
			{
				GhostReactor instance = GhostReactor.instance;
				if (instance != null)
				{
					instance.grManager.ToolSnapRequestUpgrade(this.gameEntity.GetNetId(), this.matchingUpgrade, this.currentMagnetizingTool.GetComponent<GameEntity>().GetNetId());
					return;
				}
			}
		}
		else
		{
			if (this.attractParticleSystem != null)
			{
				this.attractParticleSystem.emission.enabled = false;
			}
			this.humAudioSource.volume = 0f;
		}
	}

	private void OnEnable()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.GrabbedByPlayer));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnReleased = (Action)Delegate.Combine(gameEntity2.OnReleased, new Action(this.ReleasedByPlayer));
	}

	private void OnDisable()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Remove(gameEntity.OnGrabbed, new Action(this.GrabbedByPlayer));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnReleased = (Action)Delegate.Remove(gameEntity2.OnReleased, new Action(this.ReleasedByPlayer));
	}

	public void GrabbedByPlayer()
	{
		if (this.gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			GRPlayer grplayer = GRPlayer.Get(this.gameEntity.heldByActorNumber);
			if (grplayer)
			{
				grplayer.GrabbedItem(this.gameEntity.id, base.gameObject.name);
			}
		}
		this.EnableProcAnimLoop();
	}

	public void ReleasedByPlayer()
	{
		this.DisableProcAnimLoop();
	}

	public void OnEntityInit()
	{
		GhostReactor.ToolEntityCreateData toolEntityCreateData = GhostReactor.ToolEntityCreateData.Unpack(this.gameEntity.createData);
		GhostReactorManager ghostReactorManager = GhostReactorManager.Get(this.gameEntity);
		if (ghostReactorManager != null)
		{
			GRToolUpgradePurchaseStationFull toolUpgradeStationFullForIndex = ghostReactorManager.GetToolUpgradeStationFullForIndex(toolEntityCreateData.stationIndex);
			if (toolUpgradeStationFullForIndex != null)
			{
				toolUpgradeStationFullForIndex.InitLinkedEntity(this.gameEntity);
			}
		}
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	public GameEntity gameEntity;

	public GRToolProgressionManager.ToolParts matchingUpgrade;

	private int gameEntityListCheckIndex;

	private GameEntity currentMagnetizingTool;

	public AnimationCurve visualDistanceCurve;

	public float shakeMaxAmount = 10f;

	public float shakeFrequency = 100f;

	public Transform childVisualTransform;

	public AudioSource humAudioSource;

	public AudioSource audioSource;

	public AudioClip snapAudioClip;

	public MeshCollider meshCollider;

	public ParticleSystem attractParticleSystem;

	public ParticleSystemForceField forceField;

	public float minDistToStartMagnetize = 0.5f;

	public float minDistToSnap;

	public float magnetizingLoopMinVolume = 0.2f;

	public float magnetizingLoopMaxVolume = 1f;

	public float snapAudioVolume = 1f;

	private int toolSearchesPerFrame = 5;

	private float shakePhase;
}

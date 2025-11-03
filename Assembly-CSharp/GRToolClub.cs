using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class GRToolClub : MonoBehaviourTick, IGameHitter, IGameEntityDebugComponent, IGameEntityComponent
{
	private void Awake()
	{
		this.retractableSection.localPosition = new Vector3(0f, 0f, 0f);
	}

	public new void OnEnable()
	{
		base.OnEnable();
		this.SetExtendedAmount(0f);
		this.gameHitter.hitFx = this.noPowerFx;
		this.gameHitter.damageAttribute = this.noPowerAttribute;
		this.SetState(GRToolClub.State.Idle);
	}

	public void OnEntityInit()
	{
		if (this.tool != null)
		{
			this.tool.onToolUpgraded += this.OnToolUpgraded;
			this.OnToolUpgraded(this.tool);
		}
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	private void OnToolUpgraded(GRTool tool)
	{
	}

	private void EnableImpactVFXForCurrentUpgradeLevel()
	{
		if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.BatonDamage1))
		{
			this.gameHitter.hitFx = this.upgrade1ImpactVFX;
			return;
		}
		if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.BatonDamage2))
		{
			this.gameHitter.hitFx = this.upgrade2ImpactVFX;
			return;
		}
		if (this.tool.HasUpgradeInstalled(GRToolProgressionManager.ToolParts.BatonDamage3))
		{
			this.gameHitter.hitFx = this.upgrade3ImpactVFX;
			return;
		}
		this.gameHitter.hitFx = this.poweredImpactFx;
	}

	public override void Tick()
	{
		float deltaTime = Time.deltaTime;
		if (this.gameEntity.IsHeld())
		{
			if (this.gameEntity.IsHeldByLocalPlayer())
			{
				this.OnUpdateAuthority(deltaTime);
			}
			else
			{
				this.OnUpdateRemote(deltaTime);
			}
		}
		else
		{
			this.SetState(GRToolClub.State.Idle);
		}
		this.OnUpdateShared(deltaTime);
	}

	private void OnUpdateAuthority(float dt)
	{
		GRToolClub.State state = this.state;
		if (state != GRToolClub.State.Idle)
		{
			if (state != GRToolClub.State.Extended)
			{
				return;
			}
			if (!this.IsButtonHeld() || !this.tool.HasEnoughEnergy())
			{
				this.SetState(GRToolClub.State.Idle);
			}
		}
		else if (this.IsButtonHeld() && this.tool.HasEnoughEnergy())
		{
			this.SetState(GRToolClub.State.Extended);
			return;
		}
	}

	private void OnUpdateRemote(float dt)
	{
		GRToolClub.State state = (GRToolClub.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
	}

	private void OnUpdateShared(float dt)
	{
		GRToolClub.State state = this.state;
		if (state != GRToolClub.State.Idle)
		{
			if (state != GRToolClub.State.Extended)
			{
				return;
			}
			if (this.extendedAmount < 1f)
			{
				float num = Mathf.MoveTowards(this.extendedAmount, 1f, 1f / this.extensionTime * Time.deltaTime);
				this.SetExtendedAmount(num);
			}
		}
		else if (this.extendedAmount > 0f)
		{
			float num2 = Mathf.MoveTowards(this.extendedAmount, 0f, 1f / this.extensionTime * Time.deltaTime);
			this.SetExtendedAmount(num2);
			return;
		}
	}

	private void SetExtendedAmount(float newExtendedAmount)
	{
		this.extendedAmount = newExtendedAmount;
		float num = Mathf.Lerp(this.retractableSectionMin, this.retractableSectionMax, this.extendedAmount);
		this.retractableSection.localPosition = new Vector3(0f, num, 0f);
	}

	private void SetState(GRToolClub.State newState)
	{
		if (this.state == newState)
		{
			return;
		}
		GRToolClub.State state = this.state;
		if (state != GRToolClub.State.Idle)
		{
		}
		this.state = newState;
		state = this.state;
		if (state != GRToolClub.State.Idle)
		{
			if (state == GRToolClub.State.Extended)
			{
				this.idleCollider.enabled = false;
				this.extendedCollider.enabled = true;
				for (int i = 0; i < this.meshAndMaterials.Count; i++)
				{
					MaterialUtils.SwapMaterial(this.meshAndMaterials[i], false);
				}
				this.humAudioSource.Play();
				this.dullLight.SetActive(true);
				this.audioSource.PlayOneShot(this.extendAudio, this.extendVolume);
				for (int j = 0; j < this.humParticleEffects.Count; j++)
				{
					this.humParticleEffects[j].gameObject.SetActive(true);
				}
				this.EnableImpactVFXForCurrentUpgradeLevel();
				this.gameHitter.damageAttribute = this.poweredAttribute;
				this.openHaptic.PlayIfHeldLocal(this.gameEntity);
			}
		}
		else
		{
			this.extendedCollider.enabled = false;
			this.idleCollider.enabled = true;
			for (int k = 0; k < this.meshAndMaterials.Count; k++)
			{
				MaterialUtils.SwapMaterial(this.meshAndMaterials[k], true);
			}
			this.humAudioSource.Stop();
			this.dullLight.SetActive(false);
			this.audioSource.PlayOneShot(this.retractAudio, this.retractVolume);
			for (int l = 0; l < this.humParticleEffects.Count; l++)
			{
				this.humParticleEffects[l].gameObject.SetActive(false);
			}
			this.gameHitter.hitFx = this.noPowerFx;
			this.gameHitter.damageAttribute = this.noPowerAttribute;
			this.closeHaptic.PlayIfHeldLocal(this.gameEntity);
		}
		if (this.gameEntity.IsHeldByLocalPlayer())
		{
			this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
		}
	}

	private bool IsButtonHeld()
	{
		if (!this.gameEntity.IsHeldByLocalPlayer())
		{
			return false;
		}
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(this.gameEntity.heldByActorNumber, out gamePlayer))
		{
			return false;
		}
		int num = gamePlayer.FindHandIndex(this.gameEntity.id);
		return num != -1 && ControllerInputPoller.TriggerFloat(GamePlayer.IsLeftHand(num) ? XRNode.LeftHand : XRNode.RightHand) > 0.25f;
	}

	public void OnSuccessfulHit(GameHitData hitData)
	{
		if (this.state == GRToolClub.State.Extended)
		{
			this.tool.UseEnergy();
		}
	}

	public void GetDebugTextLines(out List<string> strings)
	{
		strings = new List<string>();
		strings.Add(string.Format("Knockback: <color=\"yellow\">x{0}<color=\"white\">", this.gameHitter.knockbackMultiplier));
	}

	public GameEntity gameEntity;

	public GameHitter gameHitter;

	public GRTool tool;

	public Rigidbody rigidBody;

	public AudioSource audioSource;

	public AudioSource humAudioSource;

	public List<ParticleSystem> humParticleEffects = new List<ParticleSystem>();

	public GRAttributes attributes;

	public AudioClip extendAudio;

	public float extendVolume = 0.5f;

	public AudioClip retractAudio;

	public float retractVolume = 0.5f;

	public GameHitFx noPowerFx;

	public GameHitFx poweredImpactFx;

	public GameHitFx upgrade1ImpactVFX;

	public GameHitFx upgrade2ImpactVFX;

	public GameHitFx upgrade3ImpactVFX;

	public GRAttributeType noPowerAttribute;

	public GRAttributeType poweredAttribute;

	public float minHitSpeed = 2.25f;

	public GameObject dullLight;

	public List<MeshAndMaterials> meshAndMaterials;

	public Transform retractableSection;

	public Collider idleCollider;

	public Collider extendedCollider;

	public float retractableSectionMin = -0.31f;

	public float retractableSectionMax;

	public float extensionTime = 0.15f;

	[Header("Haptic")]
	public AbilityHaptic openHaptic;

	public AbilityHaptic closeHaptic;

	private float extendedAmount;

	private GRToolClub.State state;

	private enum State
	{
		Idle,
		Extended
	}
}

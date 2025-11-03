using System;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(GameEntity))]
public class GRToolRevive : MonoBehaviour
{
	private void Awake()
	{
		this.state = GRToolRevive.State.Idle;
	}

	private void OnEnable()
	{
		this.StopRevive();
		this.state = GRToolRevive.State.Idle;
	}

	private void OnDestroy()
	{
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		if (this.gameEntity.IsHeldByLocalPlayer())
		{
			this.OnUpdateAuthority(deltaTime);
			return;
		}
		this.OnUpdateRemote(deltaTime);
	}

	private void OnUpdateAuthority(float dt)
	{
		switch (this.state)
		{
		case GRToolRevive.State.Idle:
			if (this.tool.HasEnoughEnergy() && this.IsButtonHeld())
			{
				this.SetStateAuthority(GRToolRevive.State.Reviving);
				return;
			}
			break;
		case GRToolRevive.State.Reviving:
			this.stateTimeRemaining -= dt;
			if (this.stateTimeRemaining <= 0f)
			{
				this.SetStateAuthority(GRToolRevive.State.Cooldown);
				return;
			}
			break;
		case GRToolRevive.State.Cooldown:
			if (!this.IsButtonHeld())
			{
				this.SetStateAuthority(GRToolRevive.State.Idle);
			}
			break;
		default:
			return;
		}
	}

	private void OnUpdateRemote(float dt)
	{
		GRToolRevive.State state = (GRToolRevive.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
	}

	private void SetStateAuthority(GRToolRevive.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(GRToolRevive.State newState)
	{
		if (this.state == newState)
		{
			return;
		}
		if (this.state == GRToolRevive.State.Reviving)
		{
			this.StopRevive();
		}
		this.state = newState;
		GRToolRevive.State state = this.state;
		if (state != GRToolRevive.State.Idle)
		{
			if (state == GRToolRevive.State.Reviving)
			{
				this.StartRevive();
				this.stateTimeRemaining = this.reviveDuration;
				return;
			}
		}
		else
		{
			this.stateTimeRemaining = -1f;
		}
	}

	private void StartRevive()
	{
		this.reviveFx.SetActive(true);
		this.audioSource.volume = this.reviveSoundVolume;
		this.audioSource.clip = this.reviveSound;
		this.audioSource.Play();
		this.tool.UseEnergy();
		this.onHaptic.PlayIfHeldLocal(this.gameEntity);
		if (this.gameEntity.IsAuthority())
		{
			int num = Physics.SphereCastNonAlloc(this.shootFrom.position, 0.5f, this.shootFrom.rotation * Vector3.forward, this.tempHitResults, this.reviveDistance, this.playerLayerMask);
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = this.tempHitResults[i];
				Rigidbody attachedRigidbody = raycastHit.collider.attachedRigidbody;
				if (!(attachedRigidbody == null))
				{
					GRPlayer component = attachedRigidbody.GetComponent<GRPlayer>();
					if (component != null && component.State != GRPlayer.GRPlayerState.Alive)
					{
						GhostReactorManager.Get(this.gameEntity).RequestPlayerStateChange(component, GRPlayer.GRPlayerState.Alive);
						return;
					}
				}
			}
		}
	}

	private void StopRevive()
	{
		this.reviveFx.SetActive(false);
		this.audioSource.Stop();
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

	public GameEntity gameEntity;

	public GRTool tool;

	[SerializeField]
	private Transform shootFrom;

	[SerializeField]
	private LayerMask playerLayerMask;

	[SerializeField]
	private float reviveDistance = 1.5f;

	[SerializeField]
	private GameObject reviveFx;

	[SerializeField]
	private float reviveSoundVolume;

	[SerializeField]
	private AudioClip reviveSound;

	[SerializeField]
	private float reviveDuration = 0.75f;

	[SerializeField]
	private AudioSource audioSource;

	[Header("Haptic")]
	public AbilityHaptic onHaptic;

	private GRToolRevive.State state;

	private float stateTimeRemaining;

	private RaycastHit[] tempHitResults = new RaycastHit[128];

	private enum State
	{
		Idle,
		Reviving,
		Cooldown
	}
}

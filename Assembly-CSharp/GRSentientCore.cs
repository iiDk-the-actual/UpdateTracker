using System;
using CjLib;
using GorillaLocomotion;
using Photon.Pun;
using Unity.XR.CoreUtils;
using UnityEngine;

public class GRSentientCore : MonoBehaviour, IGRSleepableEntity
{
	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
	}

	public float WakeUpRadius
	{
		get
		{
			return this.wakeupRadius;
		}
	}

	private void Start()
	{
		this.rb = base.GetComponent<Rigidbody>();
		GhostReactor.instance.sleepableEntities.Add(this);
		this.gameEntity.OnStateChanged += this.OnStateChanged;
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnReleased = (Action)Delegate.Combine(gameEntity2.OnReleased, new Action(this.OnReleased));
		GameEntity gameEntity3 = this.gameEntity;
		gameEntity3.OnSnapped = (Action)Delegate.Combine(gameEntity3.OnSnapped, new Action(this.OnSnapped));
		GameEntity gameEntity4 = this.gameEntity;
		gameEntity4.OnDetached = (Action)Delegate.Combine(gameEntity4.OnDetached, new Action(this.OnDetached));
		this.Sleep();
	}

	private void OnDestroy()
	{
		if (GhostReactor.instance != null)
		{
			GhostReactor.instance.sleepableEntities.Remove(this);
		}
		if (this.gameEntity != null)
		{
			this.gameEntity.OnStateChanged -= this.OnStateChanged;
			GameEntity gameEntity = this.gameEntity;
			gameEntity.OnGrabbed = (Action)Delegate.Remove(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
			GameEntity gameEntity2 = this.gameEntity;
			gameEntity2.OnReleased = (Action)Delegate.Remove(gameEntity2.OnReleased, new Action(this.OnReleased));
			GameEntity gameEntity3 = this.gameEntity;
			gameEntity3.OnSnapped = (Action)Delegate.Remove(gameEntity3.OnSnapped, new Action(this.OnSnapped));
			GameEntity gameEntity4 = this.gameEntity;
			gameEntity4.OnDetached = (Action)Delegate.Remove(gameEntity4.OnDetached, new Action(this.OnDetached));
		}
	}

	public bool IsSleeping()
	{
		return this.gameEntity.GetState() == 0L;
	}

	public void WakeUp()
	{
		if (this.gameEntity.IsAuthority() && this.IsSleeping())
		{
			this.gameEntity.RequestState(this.gameEntity.id, 1L);
		}
		if (this.localState == GRSentientCore.SentientCoreState.Asleep)
		{
			this.localState = GRSentientCore.SentientCoreState.Awake;
			this.localStateStartTime = Time.time;
		}
		this.sleepRequested = false;
		base.enabled = true;
	}

	public void Sleep()
	{
		this.sleepRequested = true;
	}

	private void OnStateChanged(long prevState, long nextState)
	{
		if ((int)nextState == 0)
		{
			this.sleepRequested = false;
		}
		else if (!base.enabled)
		{
			this.WakeUp();
		}
		this.SetState((GRSentientCore.SentientCoreState)nextState);
	}

	private void OnGrabbed()
	{
		this.WakeUp();
		this.SetState(GRSentientCore.SentientCoreState.Held);
		this.timeUntilNextAlert = Mathf.Min(this.timeUntilFirstAlert, this.timeUntilNextAlert);
	}

	private void OnReleased()
	{
		this.SetState(GRSentientCore.SentientCoreState.Dropped);
	}

	private void OnSnapped()
	{
		this.SetState(GRSentientCore.SentientCoreState.AttachedToPlayer);
	}

	private void OnDetached()
	{
		this.SetState(GRSentientCore.SentientCoreState.Dropped);
	}

	private void Update()
	{
		if (this.debugDraw)
		{
			DebugUtil.DrawSphere(base.transform.position, 0.15f, 12, 12, Color.cyan, true, DebugUtil.Style.Wireframe);
		}
		if (this.gameEntity.IsAuthority())
		{
			this.AuthorityUpdate();
		}
		this.SharedUpdate();
	}

	private void AuthorityUpdate()
	{
		if (this.trailFX != null)
		{
			if (this.gameEntity.snappedByActorNumber != -1 || this.gameEntity.heldByActorNumber != -1)
			{
				if (this.trailFX.isPlaying)
				{
					this.trailFX.Stop();
				}
			}
			else if (!this.trailFX.isPlaying)
			{
				this.trailFX.Play();
			}
		}
		switch (this.localState)
		{
		case GRSentientCore.SentientCoreState.Asleep:
		case GRSentientCore.SentientCoreState.JumpAnticipation:
		case GRSentientCore.SentientCoreState.Jumping:
		case GRSentientCore.SentientCoreState.HeldAlert:
		case GRSentientCore.SentientCoreState.Dropped:
			break;
		case GRSentientCore.SentientCoreState.Awake:
			if (this.sleepRequested)
			{
				this.sleepRequested = false;
				this.SetState(GRSentientCore.SentientCoreState.Asleep);
			}
			if (this.gameEntity.heldByActorNumber != -1)
			{
				this.SetState(GRSentientCore.SentientCoreState.Held);
				return;
			}
			if (!this.sleepRequested && Time.time > this.localStateStartTime + this.jumpCooldownTime)
			{
				this.AuthorityInitiateJump();
				return;
			}
			break;
		case GRSentientCore.SentientCoreState.JumpInitiated:
			if (this.sleepRequested)
			{
				this.sleepRequested = false;
				this.SetState(GRSentientCore.SentientCoreState.Asleep);
				return;
			}
			break;
		case GRSentientCore.SentientCoreState.Held:
			this.timeUntilNextAlert -= Time.deltaTime;
			if (this.timeUntilNextAlert < 0f)
			{
				this.timeUntilNextAlert = Random.Range(this.timeRangeBetweenAlerts.x, this.timeRangeBetweenAlerts.y);
				this.SetState(GRSentientCore.SentientCoreState.HeldAlert);
				return;
			}
			break;
		case GRSentientCore.SentientCoreState.AttachedToPlayer:
			this.timeUntilNextAlert -= Time.deltaTime;
			if (this.timeUntilNextAlert < 0f)
			{
				this.timeUntilNextAlert = Random.Range(this.timeRangeBetweenAlerts.x, this.timeRangeBetweenAlerts.y);
				this.alertEnemiesSound.Play(null);
				GRNoiseEventManager.instance.AddNoiseEvent(base.transform.position, this.alertNoiseEventMagnitude, this.enemyAlertDuration);
			}
			break;
		default:
			return;
		}
	}

	private void SharedUpdate()
	{
		switch (this.localState)
		{
		default:
			base.enabled = false;
			return;
		case GRSentientCore.SentientCoreState.Awake:
			if (this.visualCore != null && this.visualCore.transform.localScale != Vector3.one)
			{
				this.visualCore.transform.localScale = Vector3.one;
				this.visualCore.transform.localPosition = Vector3.zero;
				this.visualCore.transform.localRotation = Quaternion.identity;
				return;
			}
			break;
		case GRSentientCore.SentientCoreState.JumpAnticipation:
			if (this.debugDraw)
			{
				this.DrawJumpPath(Color.yellow);
			}
			if (Time.time <= this.jumpStartTime)
			{
				Vector3 normalized = (this.surfaceNormal + this.jumpDirection).normalized;
				float num = (this.jumpStartTime - Time.time) / this.jumpAnticipationTime * 0.25f + 0.75f;
				float num2 = Mathf.Sqrt(1f / num);
				this.visualCore.transform.localScale = new Vector3(num2, num, num2);
				this.visualCore.transform.position = this.visualCore.parent.position - normalized * (1f - num) * this.radius;
				this.visualCore.transform.rotation = Quaternion.FromToRotation(Vector3.up, normalized);
				return;
			}
			this.SetState(GRSentientCore.SentientCoreState.Jumping);
			this.jumpSound.Play(null);
			if (this.visualCore != null)
			{
				this.visualCore.transform.localScale = Vector3.one;
				this.visualCore.transform.localPosition = Vector3.zero;
				this.visualCore.transform.localRotation = Quaternion.identity;
				return;
			}
			break;
		case GRSentientCore.SentientCoreState.Jumping:
		{
			if (this.debugDraw)
			{
				this.DrawJumpPath(Color.yellow);
			}
			float deltaTime = Time.deltaTime;
			Vector3 vector = base.transform.position + this.jumpVelocity * deltaTime;
			Vector3 vector2 = (this.useSurfaceNormalForGravityDirection ? (-this.surfaceNormal) : Vector3.down);
			this.jumpVelocity += vector2 * (this.jumpGravityAccel * deltaTime);
			float magnitude = this.jumpVelocity.magnitude;
			if (magnitude > this.maxSpeed && this.maxSpeed > 0f)
			{
				this.jumpVelocity *= this.maxSpeed / magnitude;
			}
			float magnitude2 = (vector - base.transform.position).magnitude;
			Vector3 vector3 = ((magnitude2 > 0.001f) ? ((vector - base.transform.position) / magnitude2) : Vector3.zero);
			RaycastHit raycastHit;
			if (Physics.SphereCast(new Ray(base.transform.position, vector3), this.radius, out raycastHit, magnitude2, GTPlayer.Instance.locomotionEnabledLayers.value, QueryTriggerInteraction.Ignore))
			{
				vector = base.transform.position + vector3 * raycastHit.distance;
				this.surfaceNormal = raycastHit.normal;
				this.SetState(GRSentientCore.SentientCoreState.Awake);
				this.landSound.Play(null);
			}
			base.transform.position = vector;
			return;
		}
		case GRSentientCore.SentientCoreState.Held:
		{
			GRPlayer grplayer = GRPlayer.Get(this.gameEntity.heldByActorNumber);
			if (grplayer != null)
			{
				grplayer.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.TimeChaosExposure, Time.deltaTime);
			}
			this.isPlayingAlert = false;
			return;
		}
		case GRSentientCore.SentientCoreState.HeldAlert:
			if (!this.isPlayingAlert)
			{
				this.isPlayingAlert = true;
				this.alertEnemiesSound.Play(null);
				GRNoiseEventManager.instance.AddNoiseEvent(base.transform.position, this.alertNoiseEventMagnitude, this.enemyAlertDuration);
			}
			if (Time.time - this.localStateStartTime > this.enemyAlertDuration)
			{
				this.SetState(GRSentientCore.SentientCoreState.Held);
				return;
			}
			break;
		case GRSentientCore.SentientCoreState.AttachedToPlayer:
		{
			GRPlayer grplayer2 = GRPlayer.Get(this.gameEntity.snappedByActorNumber);
			if (grplayer2 != null)
			{
				grplayer2.IncrementSynchronizedSessionStat(GRPlayer.SynchronizedSessionStat.TimeChaosExposure, Time.deltaTime);
				return;
			}
			break;
		}
		case GRSentientCore.SentientCoreState.Dropped:
		{
			float deltaTime2 = Time.deltaTime;
			Vector3 vector4 = base.transform.position + this.rb.linearVelocity * deltaTime2;
			float magnitude3 = (vector4 - base.transform.position).magnitude;
			Vector3 vector5 = ((magnitude3 > 0.001f) ? ((vector4 - base.transform.position) / magnitude3) : Vector3.zero);
			RaycastHit raycastHit2;
			if (Physics.SphereCast(new Ray(base.transform.position, vector5), this.radius, out raycastHit2, magnitude3, GTPlayer.Instance.locomotionEnabledLayers.value, QueryTriggerInteraction.Ignore))
			{
				vector4 = base.transform.position + vector5 * raycastHit2.distance;
				this.surfaceNormal = raycastHit2.normal;
				base.transform.position = vector4;
				this.rb.isKinematic = true;
				this.SetState(GRSentientCore.SentientCoreState.Awake);
			}
			break;
		}
		}
	}

	private void SetState(GRSentientCore.SentientCoreState nextState)
	{
		if (this.localState != nextState)
		{
			this.localState = nextState;
			this.localStateStartTime = Time.time;
			if (this.gameEntity.IsAuthority())
			{
				this.gameEntity.RequestState(this.gameEntity.id, (long)nextState);
			}
		}
	}

	public void PerformJump(Vector3 startPos, Vector3 normal, Vector3 direction, double jumpNetworkTime)
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		if (!base.enabled || this.IsSleeping())
		{
			this.WakeUp();
		}
		base.transform.position = startPos;
		float num = Mathf.Clamp((float)(jumpNetworkTime - PhotonNetwork.Time), 0f, this.jumpAnticipationTime);
		this.jumpStartTime = Time.time + num;
		this.jumpDirection = direction;
		this.jumpDirection.Normalize();
		this.jumpStartPosition = startPos;
		this.surfaceNormal = normal;
		this.jumpVelocity = this.jumpDirection * this.jumpSpeed;
		this.SetState(GRSentientCore.SentientCoreState.JumpAnticipation);
	}

	private void DrawJumpPath(Color pathColor)
	{
		DebugUtil.DrawLine(this.jumpStartPosition, this.jumpStartPosition + this.surfaceNormal * 0.15f, Color.cyan, true);
		float num = 0.016666f;
		int num2 = 100;
		Vector3 vector = this.jumpStartPosition;
		Vector3 vector2 = this.jumpDirection * this.jumpSpeed;
		for (int i = 0; i < num2; i++)
		{
			Vector3 vector3 = vector + vector2 * num;
			vector2 += -this.surfaceNormal * (this.jumpGravityAccel * num);
			float magnitude = (vector3 - vector).magnitude;
			Vector3 vector4 = ((magnitude > 0.001f) ? ((vector3 - vector) / magnitude) : Vector3.zero);
			RaycastHit raycastHit;
			if (Physics.SphereCast(new Ray(vector, vector4), this.radius, out raycastHit, magnitude, GTPlayer.Instance.locomotionEnabledLayers.value, QueryTriggerInteraction.Ignore))
			{
				vector3 = raycastHit.point;
				DebugUtil.DrawLine(vector, vector3, pathColor, true);
				DebugUtil.DrawLine(vector3, vector3 + raycastHit.normal * 0.15f, Color.cyan, true);
				DebugUtil.DrawSphere(raycastHit.point, 0.1f, 12, 12, pathColor, true, DebugUtil.Style.Wireframe);
				return;
			}
			DebugUtil.DrawLine(vector, vector3, pathColor, true);
			vector = vector3;
		}
	}

	public void AuthorityInitiateJump()
	{
		if (!this.gameEntity.IsAuthority())
		{
			return;
		}
		Vector3 insideUnitSphere = Random.insideUnitSphere;
		if (Vector3.Dot(insideUnitSphere, this.surfaceNormal) > 0.99f)
		{
			insideUnitSphere = new Vector3(this.surfaceNormal.y, this.surfaceNormal.z, this.surfaceNormal.x);
		}
		float num = Random.Range(this.jumpAngleMinMax.x, this.jumpAngleMinMax.y);
		Vector3 vector = Quaternion.AngleAxis(90f - num, Vector3.Cross(this.surfaceNormal, insideUnitSphere)) * this.surfaceNormal;
		vector.Normalize();
		this.SetState(GRSentientCore.SentientCoreState.JumpInitiated);
		this.gameEntity.manager.ghostReactorManager.RequestSentientCorePerformJump(this.gameEntity, base.transform.position, this.surfaceNormal, vector, this.jumpAnticipationTime);
	}

	public GameEntity gameEntity;

	public Vector2 jumpAngleMinMax = new Vector2(30f, 60f);

	public float jumpSpeed = 3f;

	public float jumpGravityAccel = 10f;

	public float maxSpeed = 5f;

	public float radius = 0.14f;

	public float jumpAnticipationTime = 1f;

	public float jumpCooldownTime = 2f;

	public bool useSurfaceNormalForGravityDirection = true;

	public Vector2 timeRangeBetweenAlerts = new Vector2(7f, 12f);

	public float timeUntilFirstAlert = 0.5f;

	public float alertNoiseEventMagnitude = 1f;

	public AbilitySound jumpSound;

	public AbilitySound landSound;

	public AbilitySound alertEnemiesSound;

	public float wakeupRadius = 3f;

	public bool debugDraw;

	public Transform visualCore;

	public ParticleSystem trailFX;

	private Vector3 surfaceNormal = Vector3.up;

	private Vector3 jumpDirection = Vector3.up;

	private Vector3 jumpStartPosition;

	private Vector3 jumpVelocity;

	private float jumpStartTime;

	private Rigidbody rb;

	private float timeUntilNextAlert = 7f;

	private float enemyAlertDuration = 1f;

	private bool isPlayingAlert;

	private bool sleepRequested;

	[ReadOnly]
	public GRSentientCore.SentientCoreState localState = GRSentientCore.SentientCoreState.Awake;

	private float localStateStartTime;

	public enum SentientCoreState
	{
		Asleep,
		Awake,
		JumpInitiated,
		JumpAnticipation,
		Jumping,
		Held,
		HeldAlert,
		AttachedToPlayer,
		Dropped
	}
}

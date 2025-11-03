using System;
using UnityEngine;

public class GameBall : MonoBehaviour
{
	public bool IsLaunched
	{
		get
		{
			return this._launched;
		}
	}

	private void Awake()
	{
		this.id = GameBallId.Invalid;
		if (this.rigidBody == null)
		{
			this.rigidBody = base.GetComponent<Rigidbody>();
		}
		if (this.collider == null)
		{
			this.collider = base.GetComponent<Collider>();
		}
		if (this.disc && this.rigidBody != null)
		{
			this.rigidBody.maxAngularVelocity = 28f;
		}
		this.heldByActorNumber = -1;
		this.lastHeldByTeamId = -1;
		this.onlyGrabTeamId = -1;
		this._monkeBall = base.GetComponent<MonkeBall>();
	}

	private void FixedUpdate()
	{
		if (this.rigidBody == null)
		{
			return;
		}
		if (this._launched)
		{
			this._launchedTimer += Time.fixedDeltaTime;
			if (this.collider.isTrigger && this._launchedTimer > 1f && this.rigidBody.linearVelocity.y <= 0f)
			{
				this._launched = false;
				this.collider.isTrigger = false;
			}
		}
		Vector3 vector = -Physics.gravity * (1f - this.gravityMult);
		this.rigidBody.AddForce(vector * this.rigidBody.mass, ForceMode.Force);
		this._catchSoundDecay -= Time.deltaTime;
	}

	public void WasLaunched()
	{
		this._launched = true;
		this.collider.isTrigger = true;
		this._launchedTimer = 0f;
	}

	public Vector3 GetVelocity()
	{
		if (this.rigidBody == null)
		{
			return Vector3.zero;
		}
		return this.rigidBody.linearVelocity;
	}

	public void SetVelocity(Vector3 velocity)
	{
		this.rigidBody.linearVelocity = velocity;
	}

	public void PlayCatchFx()
	{
		if (this.audioSource != null && this._catchSoundDecay <= 0f)
		{
			this.audioSource.clip = this.catchSound;
			this.audioSource.volume = this.catchSoundVolume;
			this.audioSource.Play();
			this._catchSoundDecay = 0.1f;
		}
	}

	public void PlayThrowFx()
	{
		if (this.audioSource != null)
		{
			this.audioSource.clip = this.throwSound;
			this.audioSource.volume = this.throwSoundVolume;
			this.audioSource.Play();
		}
	}

	public void PlayBounceFX()
	{
		if (this.audioSource != null)
		{
			this.audioSource.clip = this.groundSound;
			this.audioSource.volume = this.groundSoundVolume;
			this.audioSource.Play();
		}
	}

	public void SetHeldByTeamId(int teamId)
	{
		this.lastHeldByTeamId = teamId;
	}

	private bool IsGamePlayer(Collider collider)
	{
		return GameBallPlayer.GetGamePlayer(collider, false) != null;
	}

	public void SetVisualOffset(bool detach)
	{
		if (this._monkeBall != null)
		{
			this._monkeBall.SetVisualOffset(detach);
		}
	}

	public GameBallId id;

	public float gravityMult = 1f;

	public bool disc;

	public Vector3 localDiscUp;

	public AudioSource audioSource;

	public AudioClip catchSound;

	public float catchSoundVolume;

	private float _catchSoundDecay;

	public AudioClip throwSound;

	public float throwSoundVolume;

	public AudioClip groundSound;

	public float groundSoundVolume;

	[SerializeField]
	private Rigidbody rigidBody;

	[SerializeField]
	private Collider collider;

	public int heldByActorNumber;

	public int lastHeldByActorNumber;

	public int lastHeldByTeamId;

	public int onlyGrabTeamId;

	private bool _launched;

	private float _launchedTimer;

	public MonkeBall _monkeBall;
}

using System;
using Photon.Pun;
using UnityEngine;

public class MonkeBall : MonoBehaviourTick
{
	private void Start()
	{
		this.Refresh();
	}

	public override void Tick()
	{
		this.UpdateVisualOffset();
		if (!PhotonNetwork.IsMasterClient)
		{
			if (this._resyncPosition)
			{
				this._resyncDelay -= Time.deltaTime;
				if (this._resyncDelay <= 0f)
				{
					this._resyncPosition = false;
					GameBallManager.Instance.RequestSetBallPosition(this.gameBall.id);
				}
			}
			if (this._positionFailsafe)
			{
				if (base.transform.position.y < -500f || (GameBallManager.Instance.transform.position - base.transform.position).sqrMagnitude > 6400f)
				{
					if (PhotonNetwork.IsConnected)
					{
						GameBallManager.Instance.RequestSetBallPosition(this.gameBall.id);
					}
					else
					{
						base.transform.position = GameBallManager.Instance.transform.position;
					}
					this._positionFailsafe = false;
					this._positionFailsafeTimer = 3f;
					return;
				}
			}
			else
			{
				this._positionFailsafeTimer -= Time.deltaTime;
				if (this._positionFailsafeTimer <= 0f)
				{
					this._positionFailsafe = true;
				}
			}
			return;
		}
		if (this.gameBall.onlyGrabTeamId != -1 && Time.timeAsDouble >= this.restrictTeamGrabEndTime)
		{
			MonkeBallGame.Instance.RequestRestrictBallToTeam(this.gameBall.id, -1);
		}
		if (this.AlreadyDropped())
		{
			this._droppedTimer += Time.deltaTime;
			if (this._droppedTimer >= 7.5f)
			{
				this._droppedTimer = 0f;
				GameBallManager.Instance.RequestTeleportBall(this.gameBall.id, base.transform.position, base.transform.rotation, this._rigidBody.linearVelocity, this._rigidBody.angularVelocity);
			}
		}
		if (this._justGrabbed)
		{
			this._justGrabbedTimer -= Time.deltaTime;
			if (this._justGrabbedTimer <= 0f)
			{
				this._justGrabbed = false;
			}
		}
		if (this._resyncPosition)
		{
			this._resyncDelay -= Time.deltaTime;
			if (this._resyncDelay <= 0f)
			{
				this._resyncPosition = false;
				GameBallManager.Instance.RequestTeleportBall(this.gameBall.id, base.transform.position, base.transform.rotation, this._rigidBody.linearVelocity, this._rigidBody.angularVelocity);
			}
		}
		if (this._positionFailsafe)
		{
			if (base.transform.position.y < -250f || (GameBallManager.Instance.transform.position - base.transform.position).sqrMagnitude > 6400f)
			{
				MonkeBallGame.Instance.LaunchBallNeutral(this.gameBall.id);
				this._positionFailsafe = false;
				this._positionFailsafeTimer = 3f;
				return;
			}
		}
		else
		{
			this._positionFailsafeTimer -= Time.deltaTime;
			if (this._positionFailsafeTimer <= 0f)
			{
				this._positionFailsafe = true;
			}
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (this.AlreadyDropped() || this._justGrabbed)
		{
			return;
		}
		if (MonkeBall.IsGamePlayer(collision.collider))
		{
			return;
		}
		this.alreadyDropped = true;
		this._droppedTimer = 0f;
		this.gameBall.PlayBounceFX();
		if (!PhotonNetwork.IsMasterClient)
		{
			if (this._rigidBody.linearVelocity.sqrMagnitude > 1f)
			{
				this._resyncPosition = true;
				this._resyncDelay = 1.5f;
			}
			int lastHeldByActorNumber = this.gameBall.lastHeldByActorNumber;
			int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
			return;
		}
		if (this._rigidBody.linearVelocity.sqrMagnitude > 1f)
		{
			this._resyncPosition = true;
			this._resyncDelay = 0.5f;
		}
		if (this._launchAfterScore)
		{
			this._launchAfterScore = false;
			MonkeBallGame.Instance.RequestRestrictBallToTeamOnScore(this.gameBall.id, MonkeBallGame.Instance.GetOtherTeam(this.gameBall.lastHeldByTeamId));
			return;
		}
		MonkeBallGame.Instance.RequestRestrictBallToTeam(this.gameBall.id, MonkeBallGame.Instance.GetOtherTeam(this.gameBall.lastHeldByTeamId));
	}

	public void TriggerDelayedResync()
	{
		this._resyncPosition = true;
		if (PhotonNetwork.IsMasterClient)
		{
			this._resyncDelay = 0.5f;
			return;
		}
		this._resyncDelay = 1.5f;
	}

	public void SetRigidbodyDiscrete()
	{
		this._rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
	}

	public void SetRigidbodyContinuous()
	{
		this._rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
	}

	public static MonkeBall Get(GameBall ball)
	{
		if (ball == null)
		{
			return null;
		}
		return ball.GetComponent<MonkeBall>();
	}

	public bool AlreadyDropped()
	{
		return this.alreadyDropped;
	}

	public void OnGrabbed()
	{
		this.alreadyDropped = false;
		this._justGrabbed = true;
		this._justGrabbedTimer = 0.1f;
		this._resyncPosition = false;
	}

	public void OnSwitchHeldByTeam(int teamId)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			MonkeBallGame.Instance.RequestRestrictBallToTeam(this.gameBall.id, teamId);
		}
	}

	public void ClearCannotGrabTeamId()
	{
		this.gameBall.onlyGrabTeamId = -1;
		this.restrictTeamGrabEndTime = -1.0;
		this.Refresh();
	}

	public bool RestrictBallToTeam(int teamId, float duration)
	{
		if (teamId == this.gameBall.onlyGrabTeamId && Time.timeAsDouble + (double)duration < this.restrictTeamGrabEndTime)
		{
			return false;
		}
		this.gameBall.onlyGrabTeamId = teamId;
		this.restrictTeamGrabEndTime = Time.timeAsDouble + (double)duration;
		this.Refresh();
		return true;
	}

	private void Refresh()
	{
		if (this.gameBall.onlyGrabTeamId == -1)
		{
			this.mainRenderer.material = this.defaultMaterial;
			return;
		}
		this.mainRenderer.material = this.teamMaterial[this.gameBall.onlyGrabTeamId];
	}

	private static bool IsGamePlayer(Collider collider)
	{
		return GameBallPlayer.GetGamePlayer(collider, false) != null;
	}

	public void SetVisualOffset(bool detach)
	{
		if (detach)
		{
			this.lastVisiblePosition = this.mainRenderer.transform.position;
			this._visualOffset = true;
			this._timeOffset = Time.time;
			this.mainRenderer.transform.SetParent(null, true);
			return;
		}
		this.ReattachVisuals();
	}

	private void ReattachVisuals()
	{
		if (!this._visualOffset)
		{
			return;
		}
		this.mainRenderer.transform.SetParent(base.transform);
		this.mainRenderer.transform.localPosition = Vector3.zero;
		this.mainRenderer.transform.localRotation = Quaternion.identity;
		this._visualOffset = false;
	}

	private void UpdateVisualOffset()
	{
		if (this._visualOffset)
		{
			this.mainRenderer.transform.position = Vector3.Lerp(this.mainRenderer.transform.position, this._rigidBody.position, Mathf.Clamp((Time.time - this._timeOffset) / this.maxLerpTime, this.offsetLerp, 1f));
			if ((this.mainRenderer.transform.position - this._rigidBody.position).sqrMagnitude < this._offsetThreshold)
			{
				this.ReattachVisuals();
			}
		}
	}

	public GameBall gameBall;

	public MeshRenderer mainRenderer;

	public Material defaultMaterial;

	public Material[] teamMaterial;

	public double restrictTeamGrabEndTime;

	public bool alreadyDropped;

	private bool _justGrabbed;

	private float _justGrabbedTimer;

	private bool _launchAfterScore;

	private float _droppedTimer;

	private bool _resyncPosition;

	private float _resyncDelay;

	private bool _visualOffset;

	private float _offsetThreshold = 0.05f;

	private float _timeOffset;

	public float maxLerpTime = 0.5f;

	public float offsetLerp = 0.2f;

	private bool _positionFailsafe = true;

	private float _positionFailsafeTimer;

	public Vector3 lastVisiblePosition;

	[SerializeField]
	private Rigidbody _rigidBody;
}

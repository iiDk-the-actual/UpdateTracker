using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NetworkView))]
public class ThrowableSetDressing : TransferrableObject
{
	public bool inInitialPose { get; private set; } = true;

	public override bool ShouldBeKinematic()
	{
		return this.inInitialPose || base.ShouldBeKinematic();
	}

	protected override void Awake()
	{
		base.Awake();
		this.netView = base.GetComponent<NetworkView>();
	}

	protected override void Start()
	{
		base.Start();
		this.respawnAtPos = base.transform.position;
		this.respawnAtRot = base.transform.rotation;
		this.currentState = TransferrableObject.PositionState.Dropped;
	}

	public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
	{
		base.OnGrab(pointGrabbed, grabbingHand);
		this.inInitialPose = false;
		this.StopRespawnTimer();
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		this.StartRespawnTimer(-1f);
		return true;
	}

	public override void DropItem()
	{
		base.DropItem();
		this.StartRespawnTimer(-1f);
	}

	private void StopRespawnTimer()
	{
		if (this.respawnTimer != null)
		{
			base.StopCoroutine(this.respawnTimer);
			this.respawnTimer = null;
		}
	}

	public void SetWillTeleport()
	{
		this.worldShareableInstance.SetWillTeleport();
	}

	public void StartRespawnTimer(float overrideTimer = -1f)
	{
		float num = ((overrideTimer != -1f) ? overrideTimer : this.respawnTimerDuration);
		this.StopRespawnTimer();
		if (this.respawnTimerDuration != 0f && (!this.netView.IsValid || this.netView.IsMine))
		{
			this.respawnTimer = base.StartCoroutine(this.RespawnTimerCoroutine(num));
		}
	}

	private IEnumerator RespawnTimerCoroutine(float timerDuration)
	{
		yield return new WaitForSeconds(timerDuration);
		if (base.InHand())
		{
			yield break;
		}
		this.SetWillTeleport();
		base.transform.position = this.respawnAtPos;
		base.transform.rotation = this.respawnAtRot;
		this.inInitialPose = true;
		this.rigidbodyInstance.isKinematic = true;
		yield break;
	}

	public float respawnTimerDuration;

	[Tooltip("set this only if this set dressing is using as an ingredient for the magic cauldron - Halloween")]
	public MagicIngredientType IngredientTypeSO;

	private float _respawnTimestamp;

	[SerializeField]
	private CapsuleCollider capsuleCollider;

	private NetworkView netView;

	private Vector3 respawnAtPos;

	private Quaternion respawnAtRot;

	private Coroutine respawnTimer;
}

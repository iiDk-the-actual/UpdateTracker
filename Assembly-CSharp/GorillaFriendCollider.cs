using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaNetworking;
using UnityEngine;

public class GorillaFriendCollider : MonoBehaviour, IGorillaSliceableSimple
{
	public void Awake()
	{
		this.thisCapsule = base.GetComponent<CapsuleCollider>();
		this.thisBox = base.GetComponent<BoxCollider>();
		this.jiggleAmount = Random.Range(0f, 1f);
		this.tagAndBodyLayerMask = LayerMask.GetMask(new string[] { "Gorilla Tag Collider" }) | LayerMask.GetMask(new string[] { "Gorilla Body Collider" });
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	private void AddUserID(in string userID)
	{
		if (this.playerIDsCurrentlyTouching.Contains(userID))
		{
			return;
		}
		this.playerIDsCurrentlyTouching.Add(userID);
	}

	public void SliceUpdate()
	{
		float time = Time.time;
		if (this._nextUpdateTime < 0f)
		{
			this._nextUpdateTime = time + 1f + this.jiggleAmount;
			return;
		}
		if (time < this._nextUpdateTime)
		{
			return;
		}
		this._nextUpdateTime = time + 1f;
		if (NetworkSystem.Instance.InRoom || this.runCheckWhileNotInRoom)
		{
			this.RefreshPlayersInSphere();
		}
	}

	public void RefreshPlayersInSphere()
	{
		this.playerIDsCurrentlyTouching.Clear();
		if (this.thisBox != null)
		{
			this.collisions = Physics.OverlapBoxNonAlloc(this.thisBox.transform.position, this.thisBox.size / 2f, this.overlapColliders, this.thisBox.transform.rotation, this.tagAndBodyLayerMask);
		}
		else
		{
			this.collisions = Physics.OverlapSphereNonAlloc(base.transform.position, this.thisCapsule.radius, this.overlapColliders, this.tagAndBodyLayerMask);
		}
		this.collisions = Mathf.Min(this.collisions, this.overlapColliders.Length);
		if (this.collisions > 0)
		{
			for (int i = 0; i < this.collisions; i++)
			{
				this.otherCollider = this.overlapColliders[i];
				if (!(this.otherCollider == null) && !(this.otherCollider.attachedRigidbody == null))
				{
					this.otherColliderGO = this.otherCollider.attachedRigidbody.gameObject;
					this.collidingRig = this.otherColliderGO.GetComponent<VRRig>();
					if (this.collidingRig == null || this.collidingRig.creator == null || this.collidingRig.creator.IsNull || string.IsNullOrEmpty(this.collidingRig.creator.UserId))
					{
						GTPlayer component = this.otherColliderGO.GetComponent<GTPlayer>();
						if (component == null || NetworkSystem.Instance.LocalPlayer == null)
						{
							goto IL_0264;
						}
						if (this.thisCapsule != null && this.applyCapsuleYLimits)
						{
							float y = component.bodyCollider.transform.position.y;
							if (y < this.capsuleColliderYLimits.x || y > this.capsuleColliderYLimits.y)
							{
								goto IL_0264;
							}
						}
						string text = NetworkSystem.Instance.LocalPlayer.UserId;
						this.AddUserID(in text);
					}
					else
					{
						if (this.thisCapsule != null && this.applyCapsuleYLimits)
						{
							float y2 = this.collidingRig.bodyTransform.transform.position.y;
							if (y2 < this.capsuleColliderYLimits.x || y2 > this.capsuleColliderYLimits.y)
							{
								goto IL_0264;
							}
						}
						string text = this.collidingRig.creator.UserId;
						this.AddUserID(in text);
					}
					this.overlapColliders[i] = null;
				}
				IL_0264:;
			}
			if (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.LocalPlayer != null && this.playerIDsCurrentlyTouching.Contains(NetworkSystem.Instance.LocalPlayer.UserId) && GorillaComputer.instance.friendJoinCollider != this)
			{
				GorillaComputer.instance.allowedMapsToJoin = this.myAllowedMapsToJoin;
				GorillaComputer.instance.friendJoinCollider = this;
				GorillaComputer.instance.UpdateScreen();
			}
			this.otherCollider = null;
			this.otherColliderGO = null;
			this.collidingRig = null;
		}
	}

	public List<string> playerIDsCurrentlyTouching = new List<string>();

	private CapsuleCollider thisCapsule;

	private BoxCollider thisBox;

	[Tooltip("If using a capsule collider, the player position can be checked against these minimum and maximum Y limits (world position) to make it behave more like a cylinder check")]
	public bool applyCapsuleYLimits;

	[Tooltip("If the player's Y world position is lower than Limits.x or higher than Limits.y, they will not be considered \"Inside\" the friend collider")]
	public Vector2 capsuleColliderYLimits = Vector2.zero;

	public bool runCheckWhileNotInRoom;

	public string[] myAllowedMapsToJoin;

	private readonly Collider[] overlapColliders = new Collider[20];

	private int tagAndBodyLayerMask;

	private float jiggleAmount;

	private Collider otherCollider;

	private GameObject otherColliderGO;

	private VRRig collidingRig;

	private int collisions;

	private WaitForSeconds wait1Sec = new WaitForSeconds(1f);

	public bool manualRefreshOnly;

	private float _nextUpdateTime = -1f;
}

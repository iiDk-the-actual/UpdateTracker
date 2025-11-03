using System;
using UnityEngine;

public class GorillaHandNode : MonoBehaviour
{
	public bool isGripping
	{
		get
		{
			return this.PollGrip();
		}
	}

	public bool isLeftHand
	{
		get
		{
			return this._isLeftHand;
		}
	}

	public bool isRightHand
	{
		get
		{
			return this._isRightHand;
		}
	}

	private void Awake()
	{
		this.Setup();
	}

	private bool PollGrip()
	{
		if (this.rig == null)
		{
			return false;
		}
		bool flag = this.PollThumb() >= 0.25f;
		bool flag2 = this.PollIndex() >= 0.25f;
		bool flag3 = this.PollMiddle() >= 0.25f;
		return flag && flag2 && flag3;
	}

	private void Setup()
	{
		if (this.rig == null)
		{
			this.rig = base.GetComponentInParent<VRRig>();
		}
		if (this.rigidbody == null)
		{
			this.rigidbody = base.GetComponent<Rigidbody>();
		}
		if (this.collider == null)
		{
			this.collider = base.GetComponent<Collider>();
		}
		if (this.rig)
		{
			this.vrIndex = (this._isLeftHand ? this.rig.leftIndex : this.rig.rightIndex);
			this.vrThumb = (this._isLeftHand ? this.rig.leftThumb : this.rig.rightThumb);
			this.vrMiddle = (this._isLeftHand ? this.rig.leftMiddle : this.rig.rightMiddle);
		}
		this._isLeftHand = base.name.Contains("left", StringComparison.OrdinalIgnoreCase);
		this._isRightHand = base.name.Contains("right", StringComparison.OrdinalIgnoreCase);
		int num = 0;
		num |= 1024;
		num |= 2097152;
		num |= 16777216;
		base.gameObject.SetTag(this._isLeftHand ? UnityTag.GorillaHandLeft : UnityTag.GorillaHandRight);
		base.gameObject.SetLayer(UnityLayer.GorillaHand);
		this.rigidbody.includeLayers = num;
		this.rigidbody.excludeLayers = ~num;
		this.rigidbody.isKinematic = true;
		this.rigidbody.useGravity = false;
		this.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		this.collider.isTrigger = true;
		this.collider.includeLayers = num;
		this.collider.excludeLayers = ~num;
	}

	private void OnTriggerStay(Collider other)
	{
	}

	private float PollIndex()
	{
		return Mathf.Clamp01(this.vrIndex.calcT / 0.88f);
	}

	private float PollMiddle()
	{
		return this.vrIndex.calcT;
	}

	private float PollThumb()
	{
		return this.vrIndex.calcT;
	}

	public VRRig rig;

	public Collider collider;

	public Rigidbody rigidbody;

	[Space]
	[NonSerialized]
	public VRMapIndex vrIndex;

	[NonSerialized]
	public VRMapThumb vrThumb;

	[NonSerialized]
	public VRMapMiddle vrMiddle;

	[Space]
	public GorillaHandSocket attachedToSocket;

	[Space]
	[SerializeField]
	private bool _isLeftHand;

	[SerializeField]
	private bool _isRightHand;

	public bool ignoreSockets;
}

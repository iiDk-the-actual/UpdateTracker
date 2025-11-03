using System;
using GorillaExtensions;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class StickyHand : MonoBehaviour, ISpawnable
{
	bool ISpawnable.IsSpawned { get; set; }

	public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

	void ISpawnable.OnSpawn(VRRig rig)
	{
		this.myRig = rig;
		this.isLocal = rig.isLocal;
		this.flatHand.enabled = false;
		this.defaultLocalPosition = this.stringParent.transform.InverseTransformPoint(this.rb.transform.position);
		int num = ((this.CosmeticSelectedSide == ECosmeticSelectSide.Left) ? 1 : 2);
		this.stateBitIndex = VRRig.WearablePackedStatesBitWriteInfos[num].index;
	}

	void ISpawnable.OnDespawn()
	{
	}

	private void Update()
	{
		if (this.isLocal)
		{
			if (this.rb.isKinematic && (this.rb.transform.position - this.stringParent.transform.position).IsLongerThan(this.stringDetachLength))
			{
				this.Unstick();
			}
			else if (!this.rb.isKinematic && (this.rb.transform.position - this.stringParent.transform.position).IsLongerThan(this.stringTeleportLength))
			{
				this.rb.transform.position = this.stringParent.transform.TransformPoint(this.defaultLocalPosition);
			}
			this.myRig.WearablePackedStates = GTBitOps.WriteBit(this.myRig.WearablePackedStates, this.stateBitIndex, this.rb.isKinematic);
			return;
		}
		if (GTBitOps.ReadBit(this.myRig.WearablePackedStates, this.stateBitIndex) != this.rb.isKinematic)
		{
			if (this.rb.isKinematic)
			{
				this.Unstick();
				return;
			}
			this.Stick();
		}
	}

	private void Stick()
	{
		this.thwackSound.Play();
		this.flatHand.enabled = true;
		this.regularHand.enabled = false;
		this.rb.isKinematic = true;
	}

	private void Unstick()
	{
		this.schlupSound.Play();
		this.rb.isKinematic = false;
		this.flatHand.enabled = false;
		this.regularHand.enabled = true;
	}

	private void OnCollisionStay(Collision collision)
	{
		if (!this.isLocal || this.rb.isKinematic)
		{
			return;
		}
		if ((this.rb.transform.position - this.stringParent.transform.position).IsLongerThan(this.stringMaxAttachLength))
		{
			return;
		}
		this.Stick();
		Vector3 point = collision.contacts[0].point;
		Vector3 normal = collision.contacts[0].normal;
		this.rb.transform.rotation = Quaternion.LookRotation(normal, this.rb.transform.up);
		Vector3 vector = this.rb.transform.position - point;
		vector -= Vector3.Dot(vector, normal) * normal;
		this.rb.transform.position = point + vector + this.surfaceOffsetDistance * normal;
	}

	[SerializeField]
	private MeshRenderer flatHand;

	[SerializeField]
	private MeshRenderer regularHand;

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private GameObject stringParent;

	[SerializeField]
	private float surfaceOffsetDistance;

	[SerializeField]
	private float stringMaxAttachLength;

	[SerializeField]
	private float stringDetachLength;

	[SerializeField]
	private float stringTeleportLength;

	[SerializeField]
	private SoundBankPlayer thwackSound;

	[SerializeField]
	private SoundBankPlayer schlupSound;

	private VRRig myRig;

	private bool isLocal;

	private int stateBitIndex;

	private Vector3 defaultLocalPosition;
}

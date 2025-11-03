using System;
using GorillaExtensions;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class HoseSimulator : MonoBehaviour, ISpawnable
{
	bool ISpawnable.IsSpawned { get; set; }

	ECosmeticSelectSide ISpawnable.CosmeticSelectedSide { get; set; }

	void ISpawnable.OnDespawn()
	{
	}

	void ISpawnable.OnSpawn(VRRig rig)
	{
		this.anchors = rig.cosmeticReferences.Get(this.startAnchorRef).GetComponent<HoseSimulatorAnchors>();
		if (this.skinnedMeshRenderer != null)
		{
			Bounds localBounds = this.skinnedMeshRenderer.localBounds;
			localBounds.extents = this.localBoundsOverride;
			this.skinnedMeshRenderer.localBounds = localBounds;
		}
		this.hoseSectionLengths = new float[this.hoseBones.Length - 1];
		this.hoseBonePositions = new Vector3[this.hoseBones.Length];
		this.hoseBoneVelocities = new Vector3[this.hoseBones.Length];
		for (int i = 0; i < this.hoseSectionLengths.Length; i++)
		{
			float num = 1f;
			this.hoseSectionLengths[i] = num;
			this.totalHoseLength += num;
		}
	}

	private void LateUpdate()
	{
		if (this.myHoldable.InLeftHand())
		{
			this.isLeftHanded = true;
		}
		else if (this.myHoldable.InRightHand())
		{
			this.isLeftHanded = false;
		}
		for (int i = 0; i < this.miscBones.Length; i++)
		{
			Transform transform = (this.isLeftHanded ? this.anchors.miscAnchorsLeft[i] : this.anchors.miscAnchorsRight[i]);
			this.miscBones[i].transform.position = transform.position;
			this.miscBones[i].transform.rotation = transform.rotation;
		}
		this.startAnchor = (this.isLeftHanded ? this.anchors.leftAnchorPoint : this.anchors.rightAnchorPoint);
		float x = this.myHoldable.transform.lossyScale.x;
		float num = 0f;
		Vector3 position = this.startAnchor.position;
		Vector3 vector = position + this.startAnchor.forward * this.startStiffness * x;
		Vector3 position2 = this.endAnchor.position;
		Vector3 vector2 = position2 - this.endAnchor.forward * this.endStiffness * x;
		for (int j = 0; j < this.hoseBones.Length; j++)
		{
			float num2 = num / this.totalHoseLength;
			Vector3 vector3 = BezierUtils.BezierSolve(num2, position, vector, vector2, position2);
			Vector3 vector4 = BezierUtils.BezierSolve(num2 + 0.1f, position, vector, vector2, position2);
			if (this.firstUpdate)
			{
				this.hoseBones[j].transform.position = vector3;
				this.hoseBonePositions[j] = vector3;
				this.hoseBoneVelocities[j] = Vector3.zero;
			}
			else
			{
				this.hoseBoneVelocities[j] *= this.damping;
				this.hoseBonePositions[j] += this.hoseBoneVelocities[j] * Time.deltaTime;
				float num3 = this.hoseBoneMaxDisplacement[j] * x;
				if ((vector3 - this.hoseBonePositions[j]).IsLongerThan(num3))
				{
					Vector3 vector5 = vector3 + (this.hoseBonePositions[j] - vector3).normalized * num3;
					this.hoseBoneVelocities[j] += (vector5 - this.hoseBonePositions[j]) / Time.deltaTime;
					this.hoseBonePositions[j] = vector5;
				}
				this.hoseBones[j].transform.position = this.hoseBonePositions[j];
			}
			this.hoseBones[j].transform.rotation = Quaternion.LookRotation(vector4 - vector3, this.endAnchor.transform.up);
			if (j < this.hoseSectionLengths.Length)
			{
				num += this.hoseSectionLengths[j];
			}
		}
		this.firstUpdate = false;
	}

	private void OnDrawGizmosSelected()
	{
		if (this.hoseBonePositions != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLineStrip(this.hoseBonePositions, false);
		}
	}

	[SerializeField]
	private SkinnedMeshRenderer skinnedMeshRenderer;

	[SerializeField]
	private Vector3 localBoundsOverride;

	[SerializeField]
	private Transform[] miscBones;

	[SerializeField]
	private Transform[] hoseBones;

	[SerializeField]
	private float[] hoseBoneMaxDisplacement;

	[SerializeField]
	private CosmeticRefID startAnchorRef;

	private Transform startAnchor;

	[SerializeField]
	private float startStiffness = 0.5f;

	[SerializeField]
	private Transform endAnchor;

	[SerializeField]
	private float endStiffness = 0.5f;

	private Vector3[] hoseBonePositions;

	private Vector3[] hoseBoneVelocities;

	[SerializeField]
	private float damping = 0.97f;

	private float[] hoseSectionLengths;

	private float totalHoseLength;

	private bool firstUpdate = true;

	private HoseSimulatorAnchors anchors;

	[SerializeField]
	private TransferrableObject myHoldable;

	private bool isLeftHanded;
}

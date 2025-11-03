using System;
using UnityEngine;

public class FreeHoverboardInstance : MonoBehaviour
{
	public Rigidbody Rigidbody { get; private set; }

	public Color boardColor { get; private set; }

	private void Awake()
	{
		this.Rigidbody = base.GetComponent<Rigidbody>();
		Material[] sharedMaterials = this.boardMesh.sharedMaterials;
		this.colorMaterial = new Material(sharedMaterials[1]);
		sharedMaterials[1] = this.colorMaterial;
		this.boardMesh.sharedMaterials = sharedMaterials;
	}

	public void SetColor(Color col)
	{
		this.colorMaterial.color = col;
		this.boardColor = col;
	}

	private void Update()
	{
		RaycastHit raycastHit;
		if (Physics.SphereCast(new Ray(base.transform.TransformPoint(this.sphereCastCenter), base.transform.TransformVector(Vector3.down)), this.sphereCastRadius, out raycastHit, 1f, this.hoverRaycastMask.value))
		{
			this.hasHoverPoint = true;
			this.hoverPoint = raycastHit.point;
			this.hoverNormal = raycastHit.normal;
			return;
		}
		this.hasHoverPoint = false;
	}

	private void FixedUpdate()
	{
		if (this.hasHoverPoint)
		{
			float num = Vector3.Dot(base.transform.TransformPoint(this.sphereCastCenter) - this.hoverPoint, this.hoverNormal);
			if (num < this.hoverHeight)
			{
				base.transform.position += this.hoverNormal * (this.hoverHeight - num);
				this.Rigidbody.linearVelocity = Vector3.ProjectOnPlane(this.Rigidbody.linearVelocity, this.hoverNormal);
				Vector3 vector = Quaternion.Inverse(base.transform.rotation) * this.Rigidbody.angularVelocity;
				vector.x *= this.avelocityDragWhileHovering;
				vector.z *= this.avelocityDragWhileHovering;
				this.Rigidbody.angularVelocity = base.transform.rotation * vector;
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(base.transform.forward, this.hoverNormal), this.hoverNormal), this.hoverRotationLerp);
			}
		}
	}

	public int ownerActorNumber;

	public int boardIndex;

	[SerializeField]
	private Vector3 sphereCastCenter;

	[SerializeField]
	private float sphereCastRadius;

	[SerializeField]
	private LayerMask hoverRaycastMask;

	[SerializeField]
	private float hoverHeight;

	[SerializeField]
	private float hoverRotationLerp;

	[SerializeField]
	private float avelocityDragWhileHovering;

	[SerializeField]
	private MeshRenderer boardMesh;

	private Material colorMaterial;

	private bool hasHoverPoint;

	private Vector3 hoverPoint;

	private Vector3 hoverNormal;
}

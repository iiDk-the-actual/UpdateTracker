using System;
using UnityEngine;

public class YorickLook : MonoBehaviour
{
	private void Awake()
	{
		this.overlapRigs = new VRRig[10];
	}

	private void LateUpdate()
	{
		if (NetworkSystem.Instance.InRoom)
		{
			if (this.rigs.Length != NetworkSystem.Instance.RoomPlayerCount)
			{
				this.rigs = VRRigCache.Instance.GetAllRigs();
			}
		}
		else if (this.rigs.Length != 1)
		{
			this.rigs = new VRRig[1];
			this.rigs[0] = VRRig.LocalRig;
		}
		float num = -1f;
		float num2 = Mathf.Cos(this.lookAtAngleDegrees / 180f * 3.1415927f);
		int num3 = 0;
		for (int i = 0; i < this.rigs.Length; i++)
		{
			Vector3 vector = this.rigs[i].tagSound.transform.position - base.transform.position;
			if (vector.magnitude <= this.lookRadius)
			{
				float num4 = Vector3.Dot(-base.transform.up, vector.normalized);
				if (num4 > num2)
				{
					this.overlapRigs[num3++] = this.rigs[i];
				}
			}
		}
		this.lookTarget = null;
		for (int j = 0; j < num3; j++)
		{
			Vector3 vector = (this.overlapRigs[j].tagSound.transform.position - base.transform.position).normalized;
			float num4 = Vector3.Dot(base.transform.forward, vector);
			if (num4 > num)
			{
				num = num4;
				this.lookTarget = this.overlapRigs[j].tagSound.transform;
			}
		}
		Vector3 vector2 = -base.transform.up;
		Vector3 vector3 = -base.transform.up;
		if (this.lookTarget != null)
		{
			vector2 = (this.lookTarget.position - this.leftEye.position).normalized;
			vector3 = (this.lookTarget.position - this.rightEye.position).normalized;
		}
		Vector3 vector4 = Vector3.RotateTowards(this.leftEye.rotation * Vector3.forward, vector2, this.rotSpeed * 3.1415927f, 0f);
		Vector3 vector5 = Vector3.RotateTowards(this.rightEye.rotation * Vector3.forward, vector3, this.rotSpeed * 3.1415927f, 0f);
		this.leftEye.rotation = Quaternion.LookRotation(vector4);
		this.rightEye.rotation = Quaternion.LookRotation(vector5);
	}

	public Transform leftEye;

	public Transform rightEye;

	public Transform lookTarget;

	public float lookRadius = 0.5f;

	public VRRig[] rigs = new VRRig[10];

	public VRRig[] overlapRigs;

	public float rotSpeed = 1f;

	public float lookAtAngleDegrees = 60f;
}

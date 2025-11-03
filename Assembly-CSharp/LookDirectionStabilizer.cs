using System;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using Unity.Cinemachine;
using UnityEngine;

public class LookDirectionStabilizer : MonoBehaviour, ISpawnable
{
	public bool IsSpawned { get; set; }

	public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

	void ISpawnable.OnSpawn(VRRig rig)
	{
		this.myRig = rig;
	}

	void ISpawnable.OnDespawn()
	{
	}

	private void Update()
	{
		Transform rigTarget = this.myRig.head.rigTarget;
		if (rigTarget.forward.y < 0f)
		{
			Quaternion quaternion = Quaternion.LookRotation(rigTarget.up.ProjectOntoPlane(Vector3.up));
			Quaternion rotation = base.transform.parent.rotation;
			float num = Vector3.Dot(rigTarget.up, Vector3.up);
			base.transform.rotation = Quaternion.Lerp(rotation, quaternion, Mathf.InverseLerp(1f, 0.7f, num));
			return;
		}
		base.transform.localRotation = Quaternion.identity;
	}

	private VRRig myRig;
}

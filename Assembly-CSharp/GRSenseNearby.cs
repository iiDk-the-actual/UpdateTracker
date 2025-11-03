using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

[Serializable]
public class GRSenseNearby
{
	public void Setup(Transform headTransform)
	{
		this.rigsNearby = new List<VRRig>();
		this.headTransform = headTransform;
	}

	public void UpdateNearby(List<VRRig> allRigs, GRSenseLineOfSight senseLineOfSight)
	{
		Vector3 position = this.headTransform.position;
		Vector3 vector = this.headTransform.rotation * Vector3.forward;
		this.RemoveNotNearby(position);
		this.AddNearby(position, vector, allRigs);
		this.RemoveNoLineOfSight(position, senseLineOfSight);
	}

	public bool IsAnyoneNearby()
	{
		return !GhostReactorManager.AggroDisabled && this.rigsNearby != null && this.rigsNearby.Count > 0;
	}

	public static Vector3 GetRigTestLocation(VRRig rig)
	{
		return rig.transform.position;
	}

	public void AddNearby(Vector3 position, Vector3 forward, List<VRRig> allRigs)
	{
		float num = this.range * this.range;
		float num2 = Mathf.Cos(this.fov * 0.017453292f);
		for (int i = 0; i < allRigs.Count; i++)
		{
			VRRig vrrig = allRigs[i];
			GRPlayer component = vrrig.GetComponent<GRPlayer>();
			if (component.State != GRPlayer.GRPlayerState.Ghost && !component.InStealthMode && !this.rigsNearby.Contains(vrrig))
			{
				Vector3 vector = GRSenseNearby.GetRigTestLocation(vrrig) - position;
				float sqrMagnitude = vector.sqrMagnitude;
				if (sqrMagnitude <= num)
				{
					if (sqrMagnitude > 0f)
					{
						float num3 = Mathf.Sqrt(sqrMagnitude);
						if (Vector3.Dot(vector / num3, forward) < num2)
						{
							goto IL_00AB;
						}
					}
					this.rigsNearby.Add(vrrig);
				}
			}
			IL_00AB:;
		}
	}

	public void RemoveNotNearby(Vector3 position)
	{
		float num = this.exitRange * this.exitRange;
		int i = 0;
		while (i < this.rigsNearby.Count)
		{
			VRRig vrrig = this.rigsNearby[i];
			if (!(vrrig != null))
			{
				goto IL_0058;
			}
			GRPlayer component = vrrig.GetComponent<GRPlayer>();
			if ((GRSenseNearby.GetRigTestLocation(vrrig) - position).sqrMagnitude > num || component.State == GRPlayer.GRPlayerState.Ghost || component.InStealthMode)
			{
				goto IL_0058;
			}
			IL_0068:
			i++;
			continue;
			IL_0058:
			this.rigsNearby.RemoveAt(i);
			i--;
			goto IL_0068;
		}
	}

	public void RemoveNoLineOfSight(Vector3 headPos, GRSenseLineOfSight senseLineOfSight)
	{
		for (int i = 0; i < this.rigsNearby.Count; i++)
		{
			Vector3 rigTestLocation = GRSenseNearby.GetRigTestLocation(this.rigsNearby[i]);
			if (!senseLineOfSight.HasLineOfSight(headPos, rigTestLocation))
			{
				this.rigsNearby.RemoveAt(i);
				i--;
			}
		}
	}

	public VRRig PickClosest(out float outDistanceSq)
	{
		Vector3 position = this.headTransform.position;
		float num = float.MaxValue;
		VRRig vrrig = null;
		for (int i = 0; i < this.rigsNearby.Count; i++)
		{
			float sqrMagnitude = (GRSenseNearby.GetRigTestLocation(this.rigsNearby[i]) - position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				vrrig = this.rigsNearby[i];
			}
		}
		outDistanceSq = num;
		return vrrig;
	}

	public float range;

	public float exitRange;

	public float fov;

	[ReadOnly]
	public List<VRRig> rigsNearby;

	private Transform headTransform;
}

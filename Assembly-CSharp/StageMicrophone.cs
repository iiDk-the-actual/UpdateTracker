using System;
using GorillaExtensions;
using UnityEngine;

public class StageMicrophone : MonoBehaviour
{
	private void Awake()
	{
		StageMicrophone.Instance = this;
	}

	public bool IsPlayerAmplified(VRRig player)
	{
		return (player.GetMouthPosition() - base.transform.position).IsShorterThan(this.PickupRadius);
	}

	public float GetPlayerSpatialBlend(VRRig player)
	{
		if (!this.IsPlayerAmplified(player))
		{
			return 0.9f;
		}
		return this.AmplifiedSpatialBlend;
	}

	public static StageMicrophone Instance;

	[SerializeField]
	private float PickupRadius;

	[SerializeField]
	private float AmplifiedSpatialBlend;
}

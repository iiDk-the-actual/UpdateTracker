using System;
using Photon.Pun;
using UnityEngine;

public class NonCosmeticItemProvider : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		GorillaTriggerColliderHandIndicator component = other.GetComponent<GorillaTriggerColliderHandIndicator>();
		if (component != null)
		{
			GorillaGameManager.instance.FindPlayerVRRig(NetworkSystem.Instance.LocalPlayer).netView.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, new object[] { true, component.isLeftHand });
		}
	}

	public GTZone zone;

	[Tooltip("only for honeycomb")]
	public bool useCondition;

	public int conditionThreshold;

	public NonCosmeticItemProvider.ItemType itemType;

	public enum ItemType
	{
		honeycomb
	}
}

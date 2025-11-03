using System;
using GorillaLocomotion;
using GorillaNetworking;
using UnityEngine;

public class ForceDisableHoverboardTrigger : MonoBehaviour
{
	public void OnTriggerEnter(Collider other)
	{
		if (other == GTPlayer.Instance.headCollider)
		{
			this.wasEnabled = GTPlayer.Instance.isHoverAllowed;
			GTPlayer.Instance.SetHoverAllowed(false, true);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (!this.reEnableOnExit || !this.wasEnabled)
		{
			return;
		}
		if (this.reEnableOnlyInVStump && !GorillaComputer.instance.IsPlayerInVirtualStump())
		{
			return;
		}
		if (other == GTPlayer.Instance.headCollider)
		{
			GTPlayer.Instance.SetHoverAllowed(true, false);
		}
	}

	[Tooltip("If TRUE and the Hoverboard was enabled when the player entered this trigger, it will be re-enabled when they exit.")]
	public bool reEnableOnExit = true;

	public bool reEnableOnlyInVStump = true;

	private bool wasEnabled;
}

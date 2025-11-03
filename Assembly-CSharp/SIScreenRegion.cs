using System;
using System.Collections.Generic;
using UnityEngine;

public class SIScreenRegion : MonoBehaviour
{
	public bool HasPressedButton
	{
		get
		{
			return this._hasPressedButton;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		GorillaTriggerColliderHandIndicator componentInParent = other.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
		if (componentInParent != null)
		{
			this.handIndicators.Add(componentInParent);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		GorillaTriggerColliderHandIndicator componentInParent = other.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
		if (componentInParent != null)
		{
			this.handIndicators.Remove(componentInParent);
			if (this.handIndicators.Count == 0)
			{
				this.ClearPressedIndicator();
			}
		}
	}

	public void RegisterButtonPress()
	{
		if (this.handIndicators.Count > 0)
		{
			this._hasPressedButton = true;
		}
	}

	private void ClearPressedIndicator()
	{
		this._hasPressedButton = false;
	}

	private HashSet<GorillaTriggerColliderHandIndicator> handIndicators = new HashSet<GorillaTriggerColliderHandIndicator>();

	private bool _hasPressedButton;
}

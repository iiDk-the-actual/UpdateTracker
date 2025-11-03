using System;
using System.Collections.Generic;
using UnityEngine;

public class SIExclusionZone : MonoBehaviour
{
	private void OnDisable()
	{
		foreach (SIGadget sigadget in this.gadgetsInZone)
		{
			if (sigadget != null)
			{
				sigadget.LeaveExclusionZone(this);
			}
		}
		this.gadgetsInZone.Clear();
	}

	private void OnTriggerEnter(Collider other)
	{
		SIGadget componentInParent = other.GetComponentInParent<SIGadget>();
		if (componentInParent == null)
		{
			return;
		}
		if (!this.gadgetsInZone.Contains(componentInParent))
		{
			this.gadgetsInZone.Add(componentInParent);
		}
		componentInParent.ApplyExclusionZone(this);
	}

	private void OnTriggerExit(Collider other)
	{
		SIGadget componentInParent = other.GetComponentInParent<SIGadget>();
		if (componentInParent == null)
		{
			return;
		}
		if (this.gadgetsInZone.Contains(componentInParent))
		{
			componentInParent.LeaveExclusionZone(this);
			this.gadgetsInZone.Remove(componentInParent);
		}
	}

	public void ClearGadget(SIGadget gadget)
	{
		this.gadgetsInZone.Remove(gadget);
	}

	private List<SIGadget> gadgetsInZone = new List<SIGadget>();
}

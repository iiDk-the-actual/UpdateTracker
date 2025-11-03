using System;
using System.Collections.Generic;
using UnityEngine;

public class SITouchscreen : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		this.OnTriggerStay(other);
	}

	private void OnTriggerStay(Collider other)
	{
		Transform indicator = this.GetIndicator(other);
		if (indicator != null)
		{
			this.controllingTransform = indicator;
			this.lastTouched = Time.time;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (this.controllingTransform == null || this.GetIndicator(other) != this.controllingTransform)
		{
			return;
		}
		this.controllingTransform = null;
	}

	private Transform GetIndicator(Collider other)
	{
		if (this.notFingerTouchDict.Contains(other))
		{
			return null;
		}
		GorillaTriggerColliderHandIndicator componentInParent;
		if (!this.fingerTouchDict.TryGetValue(other, out componentInParent))
		{
			componentInParent = other.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
			if (componentInParent == null)
			{
				this.notFingerTouchDict.Add(other);
				return null;
			}
			this.fingerTouchDict.Add(other, componentInParent);
		}
		return componentInParent.transform;
	}

	public Transform controllingTransform;

	public float lastTouched;

	public Vector3 lastPosition;

	private Dictionary<Collider, GorillaTriggerColliderHandIndicator> fingerTouchDict = new Dictionary<Collider, GorillaTriggerColliderHandIndicator>();

	private HashSet<Collider> notFingerTouchDict = new HashSet<Collider>();
}

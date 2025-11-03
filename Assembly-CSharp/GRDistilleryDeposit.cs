using System;
using UnityEngine;

public class GRDistilleryDeposit : MonoBehaviour
{
	private void Start()
	{
		this._distillery = base.GetComponentInParent<GRDistillery>();
	}

	private void OnTriggerEnter(Collider other)
	{
	}

	public float hapticStrength;

	public float hapticDuration;

	private GRDistillery _distillery;
}

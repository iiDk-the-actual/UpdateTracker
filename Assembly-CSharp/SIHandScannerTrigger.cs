using System;
using UnityEngine;
using UnityEngine.Events;

public class SIHandScannerTrigger : MonoBehaviour, IClickable
{
	private void Awake()
	{
		if (this.parentScanner == null)
		{
			this.parentScanner = base.GetComponentInParent<SIHandScanner>();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		SIScannableHand component = other.GetComponent<SIScannableHand>();
		if (component == null)
		{
			return;
		}
		this.OnPlayerScanned(component.parentPlayer);
	}

	private void OnPlayerScanned(SIPlayer player)
	{
		this.parentScanner.HandScanned(player);
		this.onHandScanned.Invoke();
	}

	public void Click(bool leftHand = false)
	{
		this.OnPlayerScanned(VRRig.LocalRig.GetComponent<SIPlayer>());
	}

	public SIHandScanner parentScanner;

	public UnityEvent onHandScanned;
}

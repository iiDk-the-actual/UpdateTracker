using System;
using UnityEngine;

public class SIScannableHand : MonoBehaviour
{
	private void Awake()
	{
		this.parentPlayer = base.GetComponentInParent<SIPlayer>();
	}

	public SIPlayer parentPlayer;
}

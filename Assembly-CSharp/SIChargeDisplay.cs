using System;
using UnityEngine;

public class SIChargeDisplay : MonoBehaviour
{
	public void UpdateDisplay(int chargeCount)
	{
		for (int i = 0; i < this.chargeDisplay.Length; i++)
		{
			this.chargeDisplay[i].material = ((i < chargeCount) ? this.chargedMat : this.unchargedMat);
		}
	}

	[SerializeField]
	private MeshRenderer[] chargeDisplay;

	[SerializeField]
	private Material chargedMat;

	[SerializeField]
	private Material unchargedMat;
}

using System;
using UnityEngine;

[Serializable]
public class SizeLayerMask
{
	public int Mask
	{
		get
		{
			int num = 0;
			if (this.affectLayerA)
			{
				num |= 1;
			}
			if (this.affectLayerB)
			{
				num |= 2;
			}
			if (this.affectLayerC)
			{
				num |= 4;
			}
			if (this.affectLayerD)
			{
				num |= 8;
			}
			return num;
		}
	}

	[SerializeField]
	private bool affectLayerA = true;

	[SerializeField]
	private bool affectLayerB = true;

	[SerializeField]
	private bool affectLayerC = true;

	[SerializeField]
	private bool affectLayerD = true;
}

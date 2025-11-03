using System;
using UnityEngine;

public class DistanceMeasure : MonoBehaviour
{
	private void Awake()
	{
		if (this.from == null)
		{
			this.from = base.transform;
		}
		if (this.to == null)
		{
			this.to = base.transform;
		}
	}

	public Transform from;

	public Transform to;
}

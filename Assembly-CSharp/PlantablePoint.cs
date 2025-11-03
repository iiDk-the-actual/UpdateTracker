using System;
using UnityEngine;

public class PlantablePoint : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if ((this.floorMask & (1 << other.gameObject.layer)) != 0)
		{
			this.plantableObject.SetPlanted(true);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if ((this.floorMask & (1 << other.gameObject.layer)) != 0)
		{
			this.plantableObject.SetPlanted(false);
		}
	}

	public bool shouldBeSet;

	public LayerMask floorMask;

	public PlantableObject plantableObject;
}

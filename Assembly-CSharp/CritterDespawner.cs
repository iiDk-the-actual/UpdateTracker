using System;
using UnityEngine;

public class CritterDespawner : MonoBehaviour
{
	public void DespawnAllCritters()
	{
		CrittersManager.instance.QueueDespawnAllCritters();
	}
}

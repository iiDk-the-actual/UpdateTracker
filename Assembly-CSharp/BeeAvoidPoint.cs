using System;
using UnityEngine;

public class BeeAvoidPoint : MonoBehaviour
{
	private void Start()
	{
		BeeSwarmManager.RegisterAvoidPoint(base.gameObject);
		FlockingManager.RegisterAvoidPoint(base.gameObject);
	}

	private void OnDestroy()
	{
		BeeSwarmManager.UnregisterAvoidPoint(base.gameObject);
		FlockingManager.UnregisterAvoidPoint(base.gameObject);
	}
}

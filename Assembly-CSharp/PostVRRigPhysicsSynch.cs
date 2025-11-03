using System;
using System.Collections.Generic;
using UnityEngine;

public class PostVRRigPhysicsSynch : MonoBehaviour
{
	private void LateUpdate()
	{
		Physics.SyncTransforms();
	}

	public static void AddSyncTarget(AutoSyncTransforms body)
	{
		PostVRRigPhysicsSynch.k_syncList.Add(body);
	}

	public static void RemoveSyncTarget(AutoSyncTransforms body)
	{
		PostVRRigPhysicsSynch.k_syncList.Remove(body);
	}

	private static readonly List<AutoSyncTransforms> k_syncList = new List<AutoSyncTransforms>(5);
}

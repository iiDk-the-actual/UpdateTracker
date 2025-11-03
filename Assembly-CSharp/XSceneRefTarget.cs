using System;
using UnityEngine;

public class XSceneRefTarget : MonoBehaviour
{
	private void Awake()
	{
		this.Register(false);
	}

	private void Reset()
	{
		this.UniqueID = XSceneRefTarget.CreateNewID();
	}

	private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			this.Register(false);
		}
	}

	public void Register(bool force = false)
	{
		if (this.UniqueID == this.lastRegisteredID && !force)
		{
			return;
		}
		if (this.lastRegisteredID != -1)
		{
			XSceneRefGlobalHub.Unregister(this.lastRegisteredID, this);
		}
		XSceneRefGlobalHub.Register(this.UniqueID, this);
		this.lastRegisteredID = this.UniqueID;
	}

	private void OnDestroy()
	{
		XSceneRefGlobalHub.Unregister(this.UniqueID, this);
	}

	private void AssignNewID()
	{
		this.UniqueID = XSceneRefTarget.CreateNewID();
		this.Register(false);
	}

	public static int CreateNewID()
	{
		int num = (int)((DateTime.Now - XSceneRefTarget.epoch).TotalSeconds * 8.0 % 2147483646.0) + 1;
		if (num <= XSceneRefTarget.lastAssignedID)
		{
			XSceneRefTarget.lastAssignedID++;
			return XSceneRefTarget.lastAssignedID;
		}
		XSceneRefTarget.lastAssignedID = num;
		return num;
	}

	public int UniqueID;

	[NonSerialized]
	private int lastRegisteredID = -1;

	private static DateTime epoch = new DateTime(2024, 1, 1);

	private static int lastAssignedID;
}

using System;
using GorillaTag;
using UnityEngine;

public class ZoneRootRegister : MonoBehaviour
{
	private void Awake()
	{
		this.watchableSlot.Value = base.gameObject;
	}

	private void OnDestroy()
	{
		this.watchableSlot.Value = null;
	}

	[SerializeField]
	private WatchableGameObjectSO watchableSlot;
}

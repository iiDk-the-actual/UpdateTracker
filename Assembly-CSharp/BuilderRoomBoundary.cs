using System;
using System.Collections.Generic;
using GorillaTagScripts;
using UnityEngine;

public class BuilderRoomBoundary : GorillaTriggerBox
{
	private void Awake()
	{
		foreach (SizeChangerTrigger sizeChangerTrigger in this.enableOnEnterTrigger)
		{
			sizeChangerTrigger.OnEnter += this.OnEnteredBoundary;
		}
		this.disableOnExitTrigger.OnExit += this.OnExitedBoundary;
	}

	private void OnDestroy()
	{
		foreach (SizeChangerTrigger sizeChangerTrigger in this.enableOnEnterTrigger)
		{
			sizeChangerTrigger.OnEnter -= this.OnEnteredBoundary;
		}
		this.disableOnExitTrigger.OnExit -= this.OnExitedBoundary;
	}

	public void OnEnteredBoundary(Collider other)
	{
		if (other.attachedRigidbody == null)
		{
			return;
		}
		this.rigRef = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
		if (this.rigRef == null || !this.rigRef.isOfflineVRRig)
		{
			return;
		}
		BuilderTable builderTable;
		if (!BuilderTable.TryGetBuilderTableForZone(this.rigRef.zoneEntity.currentZone, out builderTable))
		{
			return;
		}
		if (builderTable.isTableMutable)
		{
			this.rigRef.EnableBuilderResizeWatch(true);
		}
	}

	public void OnExitedBoundary(Collider other)
	{
		if (other.attachedRigidbody == null)
		{
			return;
		}
		this.rigRef = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
		if (this.rigRef == null || !this.rigRef.isOfflineVRRig)
		{
			return;
		}
		this.rigRef.EnableBuilderResizeWatch(false);
	}

	[SerializeField]
	private List<SizeChangerTrigger> enableOnEnterTrigger;

	[SerializeField]
	private SizeChangerTrigger disableOnExitTrigger;

	private VRRig rigRef;
}

using System;
using UnityEngine;

public class ZoneEntityBSP : MonoBehaviour, IGorillaSliceableSimple
{
	public VRRig entityRig
	{
		get
		{
			return this._entityRig;
		}
	}

	public GTZone currentZone
	{
		get
		{
			ZoneDef zoneDef = this.currentNode;
			if (zoneDef == null)
			{
				return GTZone.none;
			}
			return zoneDef.zoneId;
		}
	}

	public GTSubZone currentSubZone
	{
		get
		{
			ZoneDef zoneDef = this.currentNode;
			if (zoneDef == null)
			{
				return GTSubZone.none;
			}
			return zoneDef.subZoneId;
		}
	}

	public GroupJoinZoneAB GroupZone
	{
		get
		{
			ZoneDef zoneDef = this.currentNode;
			if (zoneDef == null)
			{
				return default(GroupJoinZoneAB);
			}
			return zoneDef.groupZoneAB;
		}
	}

	private void Start()
	{
		if (!this._entityRig.isOfflineVRRig)
		{
			this._emitTelemetry = false;
		}
		this.SliceUpdate();
	}

	public virtual void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.FixedUpdate);
	}

	public virtual void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.FixedUpdate);
	}

	public void SliceUpdate()
	{
		if (this.isUpdateDisabled)
		{
			return;
		}
		ZoneDef zoneDef = ZoneGraphBSP.Instance.FindZoneAtPoint(base.transform.position);
		if (!zoneDef.IsSameZone(this.currentNode))
		{
			this.lastExitedNode = this.currentNode;
			this.currentNode = zoneDef;
			this.lastEnteredNode = zoneDef;
			if (this._emitTelemetry)
			{
				ZoneDef zoneDef2 = this.lastEnteredNode;
				if (zoneDef2 != null && zoneDef2.trackEnter)
				{
					GorillaTelemetry.EnqueueZoneEvent(this.lastEnteredNode, GTZoneEventType.zone_enter);
				}
				ZoneDef zoneDef3 = this.lastExitedNode;
				if (zoneDef3 != null && zoneDef3.trackExit)
				{
					GorillaTelemetry.EnqueueZoneEvent(this.lastExitedNode, GTZoneEventType.zone_exit);
					return;
				}
			}
		}
		else if (this._emitTelemetry)
		{
			ZoneDef zoneDef4 = this.currentNode;
			if (zoneDef4 != null && zoneDef4.trackStay)
			{
				GorillaTelemetry.EnqueueZoneEvent(this.currentNode, GTZoneEventType.zone_stay);
			}
		}
	}

	public void EnableZoneChanges()
	{
		this.isUpdateDisabled = false;
	}

	public void DisableZoneChanges()
	{
		this.isUpdateDisabled = true;
	}

	[Space]
	[SerializeField]
	private bool _emitTelemetry = true;

	[Space]
	[SerializeField]
	private VRRig _entityRig;

	[Space]
	[NonSerialized]
	public ZoneDef currentNode;

	[NonSerialized]
	public ZoneDef lastEnteredNode;

	[NonSerialized]
	public ZoneDef lastExitedNode;

	private bool isUpdateDisabled;
}

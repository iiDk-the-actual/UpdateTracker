using System;
using System.Collections;
using System.Collections.Generic;
using GorillaNetworking;
using UnityEngine;

public class ZoneEntity : MonoBehaviour, IGorillaSliceableSimple
{
	public string entityTag
	{
		get
		{
			return this._entityTag;
		}
	}

	public int entityID
	{
		get
		{
			int num = this._entityID.GetValueOrDefault();
			if (this._entityID == null)
			{
				num = base.GetInstanceID();
				this._entityID = new int?(num);
			}
			return this._entityID.Value;
		}
	}

	public VRRig entityRig
	{
		get
		{
			return this._entityRig;
		}
	}

	public SphereCollider collider
	{
		get
		{
			return this._collider;
		}
	}

	public GroupJoinZoneAB GroupZone
	{
		get
		{
			return (this.currentGroupZone & ~this.currentExcludeGroupZone) | this.previousGroupZone;
		}
	}

	public virtual void OnEnable()
	{
		this.insideBoxes.Clear();
		ZoneGraph.Register(this);
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.FixedUpdate);
	}

	public virtual void OnDisable()
	{
		this.insideBoxes.Clear();
		ZoneGraph.Unregister(this);
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.FixedUpdate);
	}

	public void SliceUpdate()
	{
		if (this.layerMask == -1)
		{
			this.layerMask = LayerMask.GetMask(new string[] { "Zone" });
		}
		int num = Physics.OverlapSphereNonAlloc(base.transform.TransformPoint(this.collider.center), this.collider.radius * base.transform.lossyScale.x, this.colliders, this.layerMask, QueryTriggerInteraction.Collide);
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < num; i++)
		{
			Collider collider = this.colliders[i];
			int instanceID = collider.GetInstanceID();
			hashSet.Add(instanceID);
			if (this.currentlyEnteredColliderIds.TryAdd(instanceID, collider))
			{
				this.ManualTriggerEnter(collider);
			}
			else
			{
				this.TriggerStayManualInvoke(collider);
			}
		}
		Queue<int> queue = new Queue<int>();
		foreach (KeyValuePair<int, Collider> keyValuePair in this.currentlyEnteredColliderIds)
		{
			if (!hashSet.Contains(keyValuePair.Key))
			{
				this.ManualTriggerExit(keyValuePair.Value);
				queue.Enqueue(keyValuePair.Key);
			}
		}
		int num2;
		while (queue.TryDequeue(out num2))
		{
			this.currentlyEnteredColliderIds.Remove(num2);
		}
	}

	public void EnableZoneChanges()
	{
		this._collider.enabled = true;
		if (this.disabledZoneChangesOnTriggerStayCoroutine != null)
		{
			base.StopCoroutine(this.disabledZoneChangesOnTriggerStayCoroutine);
			this.disabledZoneChangesOnTriggerStayCoroutine = null;
		}
	}

	public void DisableZoneChanges()
	{
		this._collider.enabled = false;
		if (this.insideBoxes.Count > 0 && this.disabledZoneChangesOnTriggerStayCoroutine == null)
		{
			this.disabledZoneChangesOnTriggerStayCoroutine = base.StartCoroutine(this.DisabledZoneCollider_OnTriggerStay());
		}
	}

	private IEnumerator DisabledZoneCollider_OnTriggerStay()
	{
		ZoneGraph instance = ZoneGraph.Instance;
		if (instance != null)
		{
			instance.CheckCompiledMaps();
		}
		for (;;)
		{
			foreach (BoxCollider boxCollider in this.insideBoxes)
			{
				this.TriggerStayManualInvoke(boxCollider);
			}
			yield return null;
		}
		yield break;
	}

	private void ManualTriggerEnter(Collider c)
	{
		this.OnZoneTrigger(GTZoneEventType.zone_enter, c);
	}

	private void ManualTriggerExit(Collider c)
	{
		this.OnZoneTrigger(GTZoneEventType.zone_exit, c);
	}

	protected virtual void TriggerStayManualInvoke(Collider c)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		BoxCollider boxCollider = c as BoxCollider;
		if (boxCollider == null)
		{
			return;
		}
		ZoneDef zoneDef = ZoneGraph.ColliderToZoneDef(boxCollider);
		if (Time.time >= this.groupZoneClearAtTimestamp)
		{
			this.previousGroupZone = this.currentGroupZone & ~this.currentExcludeGroupZone;
			this.currentGroupZone = zoneDef.groupZoneAB;
			this.currentExcludeGroupZone = zoneDef.excludeGroupZoneAB;
			this.groupZoneClearAtTimestamp = Time.time + this.groupZoneClearInterval;
		}
		else
		{
			this.currentGroupZone |= zoneDef.groupZoneAB;
			this.currentExcludeGroupZone |= zoneDef.excludeGroupZoneAB;
		}
		if (!this.gLastStayPoll.HasElapsed(1f, true))
		{
			return;
		}
		this.OnZoneTrigger(GTZoneEventType.zone_stay, boxCollider);
	}

	protected virtual void OnZoneTrigger(GTZoneEventType zoneEvent, Collider c)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		BoxCollider boxCollider = c as BoxCollider;
		if (boxCollider == null)
		{
			return;
		}
		ZoneDef zoneDef = ZoneGraph.ColliderToZoneDef(boxCollider);
		this.OnZoneTrigger(zoneEvent, zoneDef, boxCollider);
	}

	private void OnZoneTrigger(GTZoneEventType zoneEvent, ZoneDef zone, BoxCollider box)
	{
		bool flag = false;
		switch (zoneEvent)
		{
		case GTZoneEventType.zone_enter:
		{
			if (zone.zoneId != this.lastEnteredNode.zoneId)
			{
				this.sinceZoneEntered = 0;
			}
			this.lastEnteredNode = ZoneGraph.ColliderToNode(box);
			ZoneDef zoneDef = ZoneGraph.ColliderToZoneDef(box);
			this.insideBoxes.Add(box);
			if (zoneDef.priority > this.currentZonePriority)
			{
				this.currentZone = zone.zoneId;
				this.currentSubZone = zone.subZoneId;
				this.currentZonePriority = zoneDef.priority;
			}
			if (zone.subZoneId == GTSubZone.store_register)
			{
				GorillaTelemetry.PostShopEvent(this._entityRig, GTShopEventType.register_visit, CosmeticsController.instance.currentCart);
			}
			flag = zone.trackEnter;
			break;
		}
		case GTZoneEventType.zone_exit:
			this.lastExitedNode = ZoneGraph.ColliderToNode(box);
			this.insideBoxes.Remove(box);
			if (this.currentZone == this.lastExitedNode.zoneId)
			{
				int num = 0;
				ZoneDef zoneDef2 = null;
				foreach (BoxCollider boxCollider in this.insideBoxes)
				{
					ZoneDef zoneDef3 = ZoneGraph.ColliderToZoneDef(boxCollider);
					if (zoneDef3.priority > num)
					{
						zoneDef2 = zoneDef3;
						num = zoneDef3.priority;
					}
				}
				if (zoneDef2 != null)
				{
					this.currentZone = zoneDef2.zoneId;
					this.currentSubZone = zoneDef2.subZoneId;
					this.currentZonePriority = zoneDef2.priority;
				}
				else
				{
					this.currentZone = GTZone.none;
					this.currentSubZone = GTSubZone.none;
					this.currentZonePriority = 0;
				}
			}
			if (this.currentZone == GTZone.forest && this.currentSubZone == GTSubZone.tree_room)
			{
				zone.subZoneId = GTSubZone.none;
			}
			flag = zone.trackExit;
			break;
		case GTZoneEventType.zone_stay:
		{
			bool flag2 = this.sinceZoneEntered.secondsElapsedInt >= this._zoneStayEventInterval;
			if (flag2)
			{
				this.sinceZoneEntered = 0;
			}
			flag = zone.trackStay && flag2;
			break;
		}
		}
		if (!this._emitTelemetry)
		{
			return;
		}
		if (!flag)
		{
			return;
		}
		if (!this._entityRig.isOfflineVRRig)
		{
			return;
		}
		GorillaTelemetry.EnqueueZoneEvent(zone.zoneId, zone.subZoneId, zoneEvent);
		GorillaTelemetry.LastZone = zone.zoneId;
		GorillaTelemetry.LastSubZone = zone.subZoneId;
		GorillaTelemetry.LastZoneEventType = zoneEvent;
	}

	public static int Compare<T>(T x, T y) where T : ZoneEntity
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (x == null)
		{
			return 1;
		}
		if (y == null)
		{
			return -1;
		}
		return x.entityID.CompareTo(y.entityID);
	}

	[Space]
	[NonSerialized]
	private int? _entityID;

	[SerializeField]
	private string _entityTag;

	[Space]
	[SerializeField]
	private bool _emitTelemetry = true;

	[SerializeField]
	private int _zoneStayEventInterval = 300;

	[Space]
	[SerializeField]
	private VRRig _entityRig;

	[SerializeField]
	private SphereCollider _collider;

	[Space]
	[NonSerialized]
	public GTZone currentZone = GTZone.none;

	[NonSerialized]
	public GTSubZone currentSubZone;

	[NonSerialized]
	private GroupJoinZoneAB currentGroupZone = 0;

	[NonSerialized]
	private GroupJoinZoneAB previousGroupZone = 0;

	[NonSerialized]
	private GroupJoinZoneAB currentExcludeGroupZone = 0;

	private HashSet<BoxCollider> insideBoxes = new HashSet<BoxCollider>();

	private int currentZonePriority;

	private float groupZoneClearAtTimestamp;

	private float groupZoneClearInterval = 0.1f;

	private Coroutine disabledZoneChangesOnTriggerStayCoroutine;

	[Space]
	[NonSerialized]
	public ZoneNode currentNode = ZoneNode.Null;

	[NonSerialized]
	public ZoneNode lastEnteredNode = ZoneNode.Null;

	[NonSerialized]
	public ZoneNode lastExitedNode = ZoneNode.Null;

	[Space]
	[NonSerialized]
	private TimeSince sinceZoneEntered = 0;

	private readonly Dictionary<int, Collider> currentlyEnteredColliderIds = new Dictionary<int, Collider>();

	private Collider[] colliders = new Collider[20];

	public const string ZONE_LAYER = "Zone";

	private LayerMask layerMask = -1;

	private TimeSince gLastStayPoll = 0;
}

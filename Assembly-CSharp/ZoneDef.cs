using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ZoneDef : MonoBehaviour
{
	public GroupJoinZoneAB groupZoneAB
	{
		get
		{
			return new GroupJoinZoneAB
			{
				a = this.groupZone,
				b = this.groupZoneB
			};
		}
	}

	public GroupJoinZoneAB excludeGroupZoneAB
	{
		get
		{
			return new GroupJoinZoneAB
			{
				a = this.excludeGroupZone,
				b = this.excludeGroupZoneB
			};
		}
	}

	public GTZone zoneId;

	[FormerlySerializedAs("subZoneType")]
	[FormerlySerializedAs("subZone")]
	public GTSubZone subZoneId;

	public GroupJoinZoneA groupZone;

	public GroupJoinZoneB groupZoneB;

	public GroupJoinZoneA excludeGroupZone;

	public GroupJoinZoneB excludeGroupZoneB;

	[Space]
	public bool trackEnter = true;

	public bool trackExit;

	public bool trackStay = true;

	public int priority = 1;

	[Space]
	public BoxCollider[] colliders = new BoxCollider[0];

	[Space]
	public ZoneNode[] nodes = new ZoneNode[0];

	[Space]
	public Bounds bounds;

	[Space]
	public ZoneDef[] zoneOverlaps = new ZoneDef[0];
}

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

	public bool IsSameZone(ZoneDef other)
	{
		return !(other == null) && this.zoneId == other.zoneId && this.subZoneId == other.subZoneId;
	}

	public GTZone zoneId;

	[FormerlySerializedAs("subZoneType")]
	[FormerlySerializedAs("subZone")]
	public GTSubZone subZoneId;

	public GroupJoinZoneA groupZone;

	public GroupJoinZoneB groupZoneB;

	public int trackStayIntervalSec = 30;

	[Space]
	public bool trackEnter = true;

	public bool trackExit;

	public bool trackStay = true;
}

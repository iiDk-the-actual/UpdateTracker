using System;

[Serializable]
public struct ZoneKey : IEquatable<ZoneKey>, IComparable<ZoneKey>, IComparable
{
	public int intValue
	{
		get
		{
			return ZoneKey.ToIntValue(this.zoneId, this.subZoneId);
		}
	}

	public string zoneName
	{
		get
		{
			return this.zoneId.GetName<GTZone>();
		}
	}

	public string subZoneName
	{
		get
		{
			return this.subZoneId.GetName<GTSubZone>();
		}
	}

	public ZoneKey(GTZone zone, GTSubZone subZone)
	{
		this.zoneId = zone;
		this.subZoneId = subZone;
	}

	public override int GetHashCode()
	{
		return this.intValue;
	}

	public override string ToString()
	{
		return string.Concat(new string[] { "ZoneKey { ", this.zoneName, " : ", this.subZoneName, " }" });
	}

	public static ZoneKey GetKey(GTZone zone, GTSubZone subZone)
	{
		return new ZoneKey(zone, subZone);
	}

	public static int ToIntValue(GTZone zone, GTSubZone subZone)
	{
		if (zone == GTZone.none && subZone == GTSubZone.none)
		{
			return 0;
		}
		return StaticHash.Compute(zone.GetLongValue<GTZone>(), subZone.GetLongValue<GTSubZone>());
	}

	public bool Equals(ZoneKey other)
	{
		return this.intValue == other.intValue && this.zoneId == other.zoneId && this.subZoneId == other.subZoneId;
	}

	public override bool Equals(object obj)
	{
		if (obj is ZoneKey)
		{
			ZoneKey zoneKey = (ZoneKey)obj;
			return this.Equals(zoneKey);
		}
		return false;
	}

	public static bool operator ==(ZoneKey x, ZoneKey y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(ZoneKey x, ZoneKey y)
	{
		return !x.Equals(y);
	}

	public int CompareTo(ZoneKey other)
	{
		int num = this.intValue.CompareTo(other.intValue);
		if (num == 0)
		{
			num = string.CompareOrdinal(this.zoneName, other.zoneName);
		}
		if (num == 0)
		{
			num = string.CompareOrdinal(this.subZoneName, other.subZoneName);
		}
		return num;
	}

	public int CompareTo(object obj)
	{
		if (obj is ZoneKey)
		{
			ZoneKey zoneKey = (ZoneKey)obj;
			return this.CompareTo(zoneKey);
		}
		return 1;
	}

	public static bool operator <(ZoneKey x, ZoneKey y)
	{
		return x.CompareTo(y) < 0;
	}

	public static bool operator >(ZoneKey x, ZoneKey y)
	{
		return x.CompareTo(y) > 0;
	}

	public static bool operator <=(ZoneKey x, ZoneKey y)
	{
		return x.CompareTo(y) <= 0;
	}

	public static bool operator >=(ZoneKey x, ZoneKey y)
	{
		return x.CompareTo(y) >= 0;
	}

	public static explicit operator int(ZoneKey key)
	{
		return key.intValue;
	}

	public GTZone zoneId;

	public GTSubZone subZoneId;

	public static readonly ZoneKey Null = new ZoneKey(GTZone.none, GTSubZone.none);
}

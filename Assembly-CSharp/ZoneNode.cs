using System;
using Cysharp.Text;
using UnityEngine;

[Serializable]
public struct ZoneNode : IEquatable<ZoneNode>
{
	public static ZoneNode Null { get; } = new ZoneNode
	{
		zoneId = GTZone.none,
		subZoneId = GTSubZone.none,
		isValid = false
	};

	public int zoneKey
	{
		get
		{
			return StaticHash.Compute((int)this.zoneId, (int)this.subZoneId);
		}
	}

	public bool ContainsPoint(Vector3 point)
	{
		return MathUtils.OrientedBoxContains(point, this.center, this.size, this.orientation);
	}

	public int SphereOverlap(Vector3 position, float radius)
	{
		return MathUtils.OrientedBoxSphereOverlap(position, radius, this.center, this.size, this.orientation);
	}

	public override string ToString()
	{
		if (this.subZoneId != GTSubZone.none)
		{
			return ZString.Concat<GTZone, string, GTSubZone>(this.zoneId, ".", this.subZoneId);
		}
		return ZString.Concat<GTZone>(this.zoneId);
	}

	public override int GetHashCode()
	{
		int zoneKey = this.zoneKey;
		int hashCode = this.center.QuantizedId128().GetHashCode();
		int hashCode2 = this.size.QuantizedId128().GetHashCode();
		int hashCode3 = this.orientation.QuantizedId128().GetHashCode();
		return StaticHash.Compute(zoneKey, hashCode, hashCode2, hashCode3);
	}

	public static bool operator ==(ZoneNode x, ZoneNode y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(ZoneNode x, ZoneNode y)
	{
		return !x.Equals(y);
	}

	public bool Equals(ZoneNode other)
	{
		return this.zoneId == other.zoneId && this.subZoneId == other.subZoneId && this.center.Approx(other.center, 1E-05f) && this.size.Approx(other.size, 1E-05f) && this.orientation.Approx(other.orientation, 1E-06f);
	}

	public override bool Equals(object obj)
	{
		if (obj is ZoneNode)
		{
			ZoneNode zoneNode = (ZoneNode)obj;
			return this.Equals(zoneNode);
		}
		return false;
	}

	public GTZone zoneId;

	public GTSubZone subZoneId;

	public Vector3 center;

	public Vector3 size;

	public Quaternion orientation;

	public Bounds AABB;

	public bool isValid;
}

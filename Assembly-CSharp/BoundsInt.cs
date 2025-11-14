using System;
using UnityEngine;

[Serializable]
public struct BoundsInt
{
	public BoundsInt(Vector3Int min, Vector3Int max)
	{
		this.min = min;
		this.max = max;
	}

	public BoundsInt(Vector3 center, Vector3 size)
	{
		Vector3 vector = size * 0.5f;
		this.min = global::BoundsInt.FloatToInt(center - vector);
		this.max = global::BoundsInt.FloatToInt(center + vector);
	}

	public Vector3Int center
	{
		get
		{
			return (this.min + this.max) / 2;
		}
	}

	public Vector3Int size
	{
		get
		{
			return this.max - this.min;
		}
	}

	public Vector3 centerFloat
	{
		get
		{
			return global::BoundsInt.IntToFloat(this.center);
		}
	}

	public Vector3 sizeFloat
	{
		get
		{
			return global::BoundsInt.IntToFloat(this.size);
		}
	}

	public static Vector3Int FloatToInt(Vector3 v)
	{
		return new Vector3Int(Mathf.RoundToInt(v.x * 1000f), Mathf.RoundToInt(v.y * 1000f), Mathf.RoundToInt(v.z * 1000f));
	}

	public static Vector3 IntToFloat(Vector3Int v)
	{
		return new Vector3((float)v.x / 1000f, (float)v.y / 1000f, (float)v.z / 1000f);
	}

	public static global::BoundsInt FromBounds(Bounds bounds)
	{
		return new global::BoundsInt(bounds.center, bounds.size);
	}

	public Bounds ToBounds()
	{
		return new Bounds(this.centerFloat, this.sizeFloat);
	}

	public void SetMinMax(Vector3Int min, Vector3Int max)
	{
		this.min = min;
		this.max = max;
	}

	public void SetMinMax(Vector3 min, Vector3 max)
	{
		this.min = global::BoundsInt.FloatToInt(min);
		this.max = global::BoundsInt.FloatToInt(max);
	}

	public void Encapsulate(global::BoundsInt other)
	{
		this.min = new Vector3Int(Mathf.Min(this.min.x, other.min.x), Mathf.Min(this.min.y, other.min.y), Mathf.Min(this.min.z, other.min.z));
		this.max = new Vector3Int(Mathf.Max(this.max.x, other.max.x), Mathf.Max(this.max.y, other.max.y), Mathf.Max(this.max.z, other.max.z));
	}

	public void Expand(float amount)
	{
		int num = Mathf.RoundToInt(amount * 1000f);
		Vector3Int vector3Int = new Vector3Int(num, num, num);
		this.min -= vector3Int;
		this.max += vector3Int;
	}

	public bool Intersects(global::BoundsInt other)
	{
		return this.min.x < other.max.x && this.max.x > other.min.x && this.min.y < other.max.y && this.max.y > other.min.y && this.min.z < other.max.z && this.max.z > other.min.z;
	}

	public bool Contains(global::BoundsInt other)
	{
		return this.min.x <= other.min.x && this.min.y <= other.min.y && this.min.z <= other.min.z && this.max.x >= other.max.x && this.max.y >= other.max.y && this.max.z >= other.max.z;
	}

	public bool Contains(Vector3 point)
	{
		Vector3Int vector3Int = global::BoundsInt.FloatToInt(point);
		return vector3Int.x >= this.min.x && vector3Int.x <= this.max.x && vector3Int.y >= this.min.y && vector3Int.y <= this.max.y && vector3Int.z >= this.min.z && vector3Int.z <= this.max.z;
	}

	public global::BoundsInt GetIntersection(global::BoundsInt other)
	{
		Vector3Int vector3Int = new Vector3Int(Mathf.Max(this.min.x, other.min.x), Mathf.Max(this.min.y, other.min.y), Mathf.Max(this.min.z, other.min.z));
		Vector3Int vector3Int2 = new Vector3Int(Mathf.Min(this.max.x, other.max.x), Mathf.Min(this.max.y, other.max.y), Mathf.Min(this.max.z, other.max.z));
		if (vector3Int.x > vector3Int2.x || vector3Int.y > vector3Int2.y || vector3Int.z > vector3Int2.z)
		{
			return new global::BoundsInt(Vector3Int.zero, Vector3Int.zero);
		}
		return new global::BoundsInt(vector3Int, vector3Int2);
	}

	public long Volume()
	{
		Vector3Int size = this.size;
		return (long)size.x * (long)size.y * (long)size.z;
	}

	public float VolumeFloat()
	{
		return (float)this.Volume() / 1E+09f;
	}

	public static bool operator ==(global::BoundsInt a, global::BoundsInt b)
	{
		return a.min == b.min && a.max == b.max;
	}

	public static bool operator !=(global::BoundsInt a, global::BoundsInt b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj is global::BoundsInt)
		{
			global::BoundsInt boundsInt = (global::BoundsInt)obj;
			return this == boundsInt;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this.min.GetHashCode() ^ (this.max.GetHashCode() << 2);
	}

	public override string ToString()
	{
		return string.Format("BoundsInt(min: {0}, max: {1})", this.min, this.max);
	}

	private const int SCALE_FACTOR = 1000;

	public Vector3Int min;

	public Vector3Int max;
}

using System;
using MathGeoLib;
using UnityEngine;

[Serializable]
public struct BoundsInfo
{
	public Vector3 sizeComputed
	{
		get
		{
			return Vector3.Scale(this.size, this.scale) * this.inflate;
		}
	}

	public Vector3 sizeComputedAA
	{
		get
		{
			return Vector3.Scale(this.sizeAA, this.scaleAA) * this.inflateAA;
		}
	}

	public static BoundsInfo ComputeBounds(Vector3[] vertices)
	{
		if (vertices.Length == 0)
		{
			return default(BoundsInfo);
		}
		OrientedBoundingBox orientedBoundingBox = OrientedBoundingBox.BruteEnclosing(vertices);
		Vector4 vector = orientedBoundingBox.Axis1;
		Vector4 vector2 = orientedBoundingBox.Axis2;
		Vector4 vector3 = orientedBoundingBox.Axis3;
		Vector4 vector4 = new Vector4(0f, 0f, 0f, 1f);
		BoundsInfo boundsInfo = default(BoundsInfo);
		boundsInfo.center = orientedBoundingBox.Center;
		boundsInfo.size = orientedBoundingBox.Extent * 2f;
		boundsInfo.rotation = new Matrix4x4(vector, vector2, vector3, vector4).rotation;
		boundsInfo.scale = Vector3.one;
		boundsInfo.inflate = 1f;
		Bounds bounds = GeometryUtility.CalculateBounds(vertices, Matrix4x4.identity);
		boundsInfo.centerAA = bounds.center;
		boundsInfo.sizeAA = bounds.size;
		boundsInfo.scaleAA = Vector3.one;
		boundsInfo.inflateAA = 1f;
		return boundsInfo;
	}

	public static BoxCollider CreateBoxCollider(BoundsInfo bounds)
	{
		int hashCode = bounds.center.QuantizedId128().GetHashCode();
		int hashCode2 = bounds.size.QuantizedId128().GetHashCode();
		int hashCode3 = bounds.rotation.QuantizedId128().GetHashCode();
		int num = StaticHash.Compute(hashCode, hashCode2, hashCode3);
		Transform transform = new GameObject(string.Format("BoxCollider_{0:X8}", num)).transform;
		transform.position = bounds.center;
		transform.rotation = bounds.rotation;
		BoxCollider boxCollider = transform.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = bounds.sizeComputed;
		return boxCollider;
	}

	public static BoxCollider CreateBoxColliderAA(BoundsInfo bounds)
	{
		int hashCode = bounds.center.QuantizedId128().GetHashCode();
		int hashCode2 = bounds.size.QuantizedId128().GetHashCode();
		int num = StaticHash.Compute(hashCode, hashCode2);
		Transform transform = new GameObject(string.Format("BoxCollider_{0:X8}", num)).transform;
		transform.position = bounds.centerAA;
		BoxCollider boxCollider = transform.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = bounds.sizeComputedAA;
		return boxCollider;
	}

	public Vector3 center;

	public Vector3 size;

	public Quaternion rotation;

	public Vector3 scale;

	public float inflate;

	[Space]
	public Vector3 centerAA;

	public Vector3 sizeAA;

	public Vector3 scaleAA;

	public float inflateAA;
}

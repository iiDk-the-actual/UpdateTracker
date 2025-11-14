using System;
using UnityEngine;

public static class BoxColliderUtils
{
	public static Matrix4x4 GetWorldToNormalizedBoxMatrix(BoxCollider boxCollider)
	{
		Transform transform = boxCollider.transform;
		Vector3 center = boxCollider.center;
		Vector3 size = boxCollider.size;
		Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
		Matrix4x4 matrix4x = Matrix4x4.Translate(-center);
		return Matrix4x4.Scale(new Vector3((size.x != 0f) ? (2f / size.x) : 1f, (size.y != 0f) ? (2f / size.y) : 1f, (size.z != 0f) ? (2f / size.z) : 1f)) * matrix4x * worldToLocalMatrix;
	}

	public static bool DoesBoxContainPoint(BoxCollider boxCollider, Vector3 worldPoint)
	{
		Vector3 vector = BoxColliderUtils.GetWorldToNormalizedBoxMatrix(boxCollider).MultiplyPoint3x4(worldPoint);
		return Mathf.Abs(vector.x) <= 1f && Mathf.Abs(vector.y) <= 1f && Mathf.Abs(vector.z) <= 1f;
	}

	public static bool DoesBoxContainBox(BoxCollider containerBox, BoxCollider containedBox)
	{
		Transform transform = containedBox.transform;
		Vector3 vector = transform.TransformPoint(containedBox.center);
		Vector3 vector2 = containedBox.size * 0.5f;
		Vector3 vector3 = transform.TransformVector(new Vector3(vector2.x, 0f, 0f));
		Vector3 vector4 = transform.TransformVector(new Vector3(0f, vector2.y, 0f));
		Vector3 vector5 = transform.TransformVector(new Vector3(0f, 0f, vector2.z));
		return BoxColliderUtils.DoesBoxContainPoint(containerBox, vector - vector3 - vector4 - vector5) && BoxColliderUtils.DoesBoxContainPoint(containerBox, vector + vector3 - vector4 - vector5) && BoxColliderUtils.DoesBoxContainPoint(containerBox, vector - vector3 + vector4 - vector5) && BoxColliderUtils.DoesBoxContainPoint(containerBox, vector + vector3 + vector4 - vector5) && BoxColliderUtils.DoesBoxContainPoint(containerBox, vector - vector3 - vector4 + vector5) && BoxColliderUtils.DoesBoxContainPoint(containerBox, vector + vector3 - vector4 + vector5) && BoxColliderUtils.DoesBoxContainPoint(containerBox, vector - vector3 + vector4 + vector5) && BoxColliderUtils.DoesBoxContainPoint(containerBox, vector + vector3 + vector4 + vector5);
	}

	public static bool DoesBoxContainRegion(BoxCollider box, global::BoundsInt regionBounds)
	{
		Matrix4x4 worldToNormalizedBoxMatrix = BoxColliderUtils.GetWorldToNormalizedBoxMatrix(box);
		Vector3 vector = global::BoundsInt.IntToFloat(regionBounds.min);
		Vector3 vector2 = global::BoundsInt.IntToFloat(regionBounds.max);
		foreach (Vector3 vector3 in new Vector3[]
		{
			new Vector3(vector.x, vector.y, vector.z),
			new Vector3(vector2.x, vector.y, vector.z),
			new Vector3(vector.x, vector2.y, vector.z),
			new Vector3(vector2.x, vector2.y, vector.z),
			new Vector3(vector.x, vector.y, vector2.z),
			new Vector3(vector2.x, vector.y, vector2.z),
			new Vector3(vector.x, vector2.y, vector2.z),
			new Vector3(vector2.x, vector2.y, vector2.z)
		})
		{
			Vector3 vector4 = worldToNormalizedBoxMatrix.MultiplyPoint3x4(vector3);
			if (Mathf.Abs(vector4.x) > 1f || Mathf.Abs(vector4.y) > 1f || Mathf.Abs(vector4.z) > 1f)
			{
				return false;
			}
		}
		return true;
	}
}

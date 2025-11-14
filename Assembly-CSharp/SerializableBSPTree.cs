using System;
using UnityEngine;

[Serializable]
public class SerializableBSPTree
{
	public ZoneDef FindZone(Vector3 point)
	{
		if (this.nodes == null || this.rootIndex < 0 || this.rootIndex >= this.nodes.Length)
		{
			return null;
		}
		return this.FindZoneRecursive(point, this.rootIndex);
	}

	private ZoneDef FindZoneRecursive(Vector3 point, int nodeIndex)
	{
		if (nodeIndex < 0 || nodeIndex >= this.nodes.Length)
		{
			return null;
		}
		SerializableBSPNode serializableBSPNode = this.nodes[nodeIndex];
		if (serializableBSPNode.axis == SerializableBSPNode.Axis.Zone)
		{
			return this.zones[serializableBSPNode.zoneIndex];
		}
		if (serializableBSPNode.axis != SerializableBSPNode.Axis.MatrixChain && serializableBSPNode.axis != SerializableBSPNode.Axis.MatrixFinal)
		{
			float axisValue = this.GetAxisValue(point, serializableBSPNode.axis);
			ZoneDef zoneDef;
			if (axisValue < serializableBSPNode.splitValue)
			{
				zoneDef = this.FindZoneRecursive(point, (int)serializableBSPNode.leftChildIndex);
			}
			else
			{
				zoneDef = this.FindZoneRecursive(point, (int)serializableBSPNode.rightChildIndex);
			}
			if (zoneDef == null && Mathf.Abs(axisValue - serializableBSPNode.splitValue) < 2f)
			{
				if (axisValue < serializableBSPNode.splitValue)
				{
					zoneDef = this.FindZoneRecursive(point, (int)serializableBSPNode.rightChildIndex);
				}
				else
				{
					zoneDef = this.FindZoneRecursive(point, (int)serializableBSPNode.leftChildIndex);
				}
			}
			return zoneDef;
		}
		if (serializableBSPNode.matrixIndex < 0)
		{
			if (serializableBSPNode.axis != SerializableBSPNode.Axis.MatrixFinal)
			{
				return this.FindZoneRecursive(point, serializableBSPNode.outsideChildIndex);
			}
			if (serializableBSPNode.outsideChildIndex >= 0 && serializableBSPNode.outsideChildIndex < this.zones.Length)
			{
				return this.zones[serializableBSPNode.outsideChildIndex];
			}
			return null;
		}
		else
		{
			MatrixZonePair matrixZonePair = this.matrices[serializableBSPNode.matrixIndex];
			Vector3 vector = matrixZonePair.matrix.MultiplyPoint3x4(point);
			if (Mathf.Abs(vector.x) <= 1f && Mathf.Abs(vector.y) <= 1f && Mathf.Abs(vector.z) <= 1f)
			{
				if (matrixZonePair.zoneIndex >= 0 && matrixZonePair.zoneIndex < this.zones.Length)
				{
					return this.zones[matrixZonePair.zoneIndex];
				}
				return null;
			}
			else
			{
				if (serializableBSPNode.axis != SerializableBSPNode.Axis.MatrixFinal)
				{
					return this.FindZoneRecursive(point, serializableBSPNode.outsideChildIndex);
				}
				if (serializableBSPNode.outsideChildIndex >= 0 && serializableBSPNode.outsideChildIndex < this.zones.Length)
				{
					return this.zones[serializableBSPNode.outsideChildIndex];
				}
				return null;
			}
		}
	}

	private float GetAxisValue(Vector3 point, SerializableBSPNode.Axis axis)
	{
		switch (axis)
		{
		case SerializableBSPNode.Axis.X:
			return point.x;
		case SerializableBSPNode.Axis.Y:
			return point.y;
		case SerializableBSPNode.Axis.Z:
			return point.z;
		case SerializableBSPNode.Axis.MatrixChain:
			return 0f;
		case SerializableBSPNode.Axis.MatrixFinal:
			return 0f;
		case SerializableBSPNode.Axis.Zone:
			return 0f;
		default:
			return 0f;
		}
	}

	[SerializeField]
	public SerializableBSPNode[] nodes;

	[SerializeField]
	public MatrixZonePair[] matrices;

	[SerializeField]
	public ZoneDef[] zones;

	[SerializeField]
	public int rootIndex = -1;
}

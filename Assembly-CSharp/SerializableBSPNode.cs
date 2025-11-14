using System;
using UnityEngine;

[Serializable]
public struct SerializableBSPNode
{
	public int matrixIndex
	{
		get
		{
			return (int)this.leftChildIndex;
		}
	}

	public int outsideChildIndex
	{
		get
		{
			return (int)this.rightChildIndex;
		}
	}

	public int zoneIndex
	{
		get
		{
			return (int)this.leftChildIndex;
		}
	}

	[SerializeField]
	public SerializableBSPNode.Axis axis;

	[SerializeField]
	public float splitValue;

	[SerializeField]
	public short leftChildIndex;

	[SerializeField]
	public short rightChildIndex;

	public enum Axis
	{
		X,
		Y,
		Z,
		MatrixChain,
		MatrixFinal,
		Zone
	}
}

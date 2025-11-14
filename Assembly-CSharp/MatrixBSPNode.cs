using System;
using UnityEngine;

[Serializable]
public struct MatrixBSPNode
{
	[SerializeField]
	public int matrixIndex;

	[SerializeField]
	public int outsideChildIndex;
}

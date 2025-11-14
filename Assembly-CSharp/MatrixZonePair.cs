using System;
using UnityEngine;

[Serializable]
public struct MatrixZonePair
{
	[SerializeField]
	public Matrix4x4 matrix;

	[SerializeField]
	public int zoneIndex;
}

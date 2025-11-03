using System;
using UnityEngine;

public class BitPackDebug : MonoBehaviour
{
	public bool debugPos;

	public Vector3 pos;

	public Vector3 min = Vector3.one * -2f;

	public Vector3 max = Vector3.one * 2f;

	public float rad = 4f;

	[Space]
	public bool debug32;

	public uint packed;

	public Vector3 unpacked;

	[Space]
	public bool debug16;

	public ushort packed16;

	public Vector3 unpacked16;
}

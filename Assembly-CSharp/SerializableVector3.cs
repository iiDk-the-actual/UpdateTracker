using System;
using UnityEngine;

[Serializable]
public struct SerializableVector3
{
	public SerializableVector3(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public static implicit operator SerializableVector3(Vector3 v)
	{
		return new SerializableVector3(v.x, v.y, v.z);
	}

	public static implicit operator Vector3(SerializableVector3 v)
	{
		return new Vector3(v.x, v.y, v.z);
	}

	public float x;

	public float y;

	public float z;
}

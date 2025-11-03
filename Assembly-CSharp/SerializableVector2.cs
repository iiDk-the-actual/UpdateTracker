using System;
using UnityEngine;

[Serializable]
public struct SerializableVector2
{
	public SerializableVector2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public static implicit operator SerializableVector2(Vector2 v)
	{
		return new SerializableVector2(v.x, v.y);
	}

	public static implicit operator Vector2(SerializableVector2 v)
	{
		return new Vector2(v.x, v.y);
	}

	public float x;

	public float y;
}

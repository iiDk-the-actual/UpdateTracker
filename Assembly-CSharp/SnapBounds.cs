using System;
using System.IO;
using UnityEngine;

[Serializable]
public struct SnapBounds
{
	public SnapBounds(Vector2Int min, Vector2Int max)
	{
		this.min = min;
		this.max = max;
	}

	public SnapBounds(int minX, int minY, int maxX, int maxY)
	{
		this.min = new Vector2Int(minX, minY);
		this.max = new Vector2Int(maxX, maxY);
	}

	public void Clear()
	{
		this.min = new Vector2Int(int.MinValue, int.MinValue);
		this.max = new Vector2Int(int.MinValue, int.MinValue);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(this.min.x);
		writer.Write(this.min.y);
		writer.Write(this.max.x);
		writer.Write(this.max.y);
	}

	public void Read(BinaryReader reader)
	{
		this.min.x = reader.ReadInt32();
		this.min.y = reader.ReadInt32();
		this.max.x = reader.ReadInt32();
		this.max.y = reader.ReadInt32();
	}

	public Vector2Int min;

	public Vector2Int max;
}

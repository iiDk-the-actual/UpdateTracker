using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class GTVector3Extensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 X_Z(this Vector3 vector)
	{
		return new Vector3(vector.x, 0f, vector.z);
	}

	public static Vector3 Sum(this IEnumerable<Vector3> vecs)
	{
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < vecs.Count<Vector3>(); i++)
		{
			vector += vecs.ElementAt(i);
		}
		return vector;
	}

	public static Vector3 Average(this IEnumerable<Vector3> vecs)
	{
		return vecs.Sum() / (float)vecs.Count<Vector3>();
	}
}

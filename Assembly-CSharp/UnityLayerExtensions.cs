using System;
using UnityEngine;

public static class UnityLayerExtensions
{
	public static int ToLayerMask(this UnityLayer self)
	{
		return 1 << (int)self;
	}

	public static int ToLayerIndex(this UnityLayer self)
	{
		return (int)self;
	}

	public static bool IsOnLayer(this GameObject obj, UnityLayer layer)
	{
		return obj.layer == (int)layer;
	}

	public static void SetLayer(this GameObject obj, UnityLayer layer)
	{
		obj.layer = (int)layer;
	}

	public static void SetLayerRecursively(this GameObject obj, UnityLayer layer)
	{
		obj.layer = (int)layer;
		foreach (object obj2 in obj.transform)
		{
			((Transform)obj2).gameObject.SetLayerRecursively(layer);
		}
	}
}

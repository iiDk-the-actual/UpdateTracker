using System;
using System.Collections.Generic;
using UnityEngine;

public static class TransformUtils
{
	public static int ComputePathHashByInstance(Transform t)
	{
		if (t == null)
		{
			return 0;
		}
		int num = 0;
		Transform transform = t;
		while (transform != null)
		{
			num = StaticHash.Compute(num, transform.GetHashCode());
			transform = transform.parent;
		}
		return num;
	}

	public static Hash128 ComputePathHash(Transform t)
	{
		if (t == null)
		{
			return default(Hash128);
		}
		Hash128 hash = default(Hash128);
		Transform transform = t;
		while (transform != null)
		{
			Hash128 hash2 = Hash128.Compute(transform.name);
			HashUtilities.AppendHash(ref hash2, ref hash);
			transform = transform.parent;
		}
		return hash;
	}

	public static string GetScenePath(Transform t)
	{
		if (t == null)
		{
			return null;
		}
		string text = t.name;
		Transform transform = t.parent;
		while (transform != null)
		{
			text = transform.name + "/" + text;
			transform = transform.parent;
		}
		return text;
	}

	public static string GetScenePathReverse(Transform t)
	{
		if (t == null)
		{
			return null;
		}
		string text = t.name;
		Transform transform = t.parent;
		Queue<string> queue = new Queue<string>(16);
		while (transform != null)
		{
			queue.Enqueue(transform.name);
			transform = transform.parent;
		}
		while (queue.Count > 0)
		{
			text = text + "/" + queue.Dequeue();
		}
		return text;
	}

	private const string kFwdSlash = "/";
}

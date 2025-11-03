using System;
using UnityEngine;

public static class PoolUtils
{
	public static int GameObjHashCode(GameObject obj)
	{
		return obj.tag.GetHashCode();
	}
}

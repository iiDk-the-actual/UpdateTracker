using System;
using UnityEngine;

public static class ObjectExtensions
{
	public static void Destroy(this Object target)
	{
		Object.Destroy(target);
	}
}

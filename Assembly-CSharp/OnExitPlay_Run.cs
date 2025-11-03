using System;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class OnExitPlay_Run : OnExitPlay_Attribute
{
	public override void OnEnterPlay(MethodInfo method)
	{
		if (!method.IsStatic)
		{
			Debug.LogError(string.Format("Can't Run non-static method {0}.{1}", method.DeclaringType, method.Name));
			return;
		}
		method.Invoke(null, new object[0]);
	}
}

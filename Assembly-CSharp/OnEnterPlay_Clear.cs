using System;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class OnEnterPlay_Clear : OnEnterPlay_Attribute
{
	public override void OnEnterPlay(FieldInfo field)
	{
		if (!field.IsStatic)
		{
			Debug.LogError(string.Format("Can't Clear non-static field {0}.{1}", field.DeclaringType, field.Name));
			return;
		}
		MethodInfo method = field.FieldType.GetMethod("Clear");
		object value = field.GetValue(null);
		if (value != null)
		{
			method.Invoke(value, new object[0]);
		}
	}
}

using System;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class OnEnterPlay_SetNull : OnEnterPlay_Attribute
{
	public override void OnEnterPlay(FieldInfo field)
	{
		if (!field.IsStatic)
		{
			Debug.LogError(string.Format("Can't SetNull non-static field {0}.{1}", field.DeclaringType, field.Name));
			return;
		}
		field.SetValue(null, null);
	}
}

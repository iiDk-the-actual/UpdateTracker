using System;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class OnEnterPlay_Set : OnEnterPlay_Attribute
{
	public OnEnterPlay_Set(object value)
	{
		this.value = value;
	}

	public override void OnEnterPlay(FieldInfo field)
	{
		if (!field.IsStatic)
		{
			Debug.LogError(string.Format("Can't Set non-static field {0}.{1}", field.DeclaringType, field.Name));
			return;
		}
		if (field.FieldType == typeof(ushort))
		{
			field.SetValue(null, Convert.ToUInt16(this.value));
			return;
		}
		field.SetValue(null, this.value);
	}

	private object value;
}

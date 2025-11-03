using System;
using System.Reflection;

public class OnPlayChange_BaseAttribute : Attribute
{
	public virtual void OnEnterPlay(FieldInfo field)
	{
	}

	public virtual void OnEnterPlay(MethodInfo method)
	{
	}
}

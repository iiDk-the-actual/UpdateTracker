using System;
using System.Diagnostics;

[Conditional("UNITY_EDITOR")]
public class DarkBoxAttribute : Attribute
{
	public DarkBoxAttribute()
	{
	}

	public DarkBoxAttribute(bool withBorders)
	{
		this.withBorders = withBorders;
	}

	public readonly bool withBorders;
}

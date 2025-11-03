using System;
using System.Diagnostics;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.All)]
public class InlineAttribute : Attribute
{
	public InlineAttribute(bool keepLabel = false, bool asGroup = false)
	{
		this.keepLabel = keepLabel;
		this.asGroup = asGroup;
	}

	public readonly bool keepLabel;

	public readonly bool asGroup;
}

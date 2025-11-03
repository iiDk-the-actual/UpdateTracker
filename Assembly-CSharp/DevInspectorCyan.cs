using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DevInspectorCyan : DevInspectorColor
{
	public DevInspectorCyan()
		: base("#5ff")
	{
	}
}

using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DevInspectorYellow : DevInspectorColor
{
	public DevInspectorYellow()
		: base("#ff5")
	{
	}
}

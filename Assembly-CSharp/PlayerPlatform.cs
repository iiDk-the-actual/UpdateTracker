using System;
using System.Runtime.Serialization;

public enum PlayerPlatform
{
	[EnumMember(Value = "meta")]
	Meta,
	[EnumMember(Value = "steam")]
	Steam,
	[EnumMember(Value = "sony")]
	Sony
}

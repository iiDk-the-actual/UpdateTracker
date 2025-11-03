using System;

[Flags]
public enum GestureAlignment : uint
{
	None = 0U,
	TowardFace = 128U,
	AwayFromFace = 256U,
	WorldUp = 512U,
	WorldDown = 1024U
}

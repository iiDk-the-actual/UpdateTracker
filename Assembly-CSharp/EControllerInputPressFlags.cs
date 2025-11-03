using System;

[Flags]
public enum EControllerInputPressFlags
{
	None = 0,
	Index = 1,
	Grip = 2,
	Primary = 4,
	Secondary = 8
}

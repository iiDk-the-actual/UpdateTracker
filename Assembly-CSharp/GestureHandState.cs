using System;

[Flags]
public enum GestureHandState : uint
{
	None = 0U,
	IsLeft = 1U,
	IsRight = 2U,
	Open = 4U,
	Closed = 8U
}

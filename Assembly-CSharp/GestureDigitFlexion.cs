using System;

[Flags]
public enum GestureDigitFlexion : uint
{
	None = 0U,
	Open = 16U,
	Closed = 32U,
	Bent = 64U
}

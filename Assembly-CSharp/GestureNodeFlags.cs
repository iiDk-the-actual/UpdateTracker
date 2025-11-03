using System;

[Flags]
public enum GestureNodeFlags : uint
{
	None = 0U,
	HandLeft = 1U,
	HandRight = 2U,
	HandOpen = 4U,
	HandClosed = 8U,
	DigitOpen = 16U,
	DigitClosed = 32U,
	DigitBent = 64U,
	TowardFace = 128U,
	AwayFromFace = 256U,
	AxisWorldUp = 512U,
	AxisWorldDown = 1024U
}

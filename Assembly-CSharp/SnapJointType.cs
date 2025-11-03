using System;

[Flags]
public enum SnapJointType
{
	None = 0,
	ArmL = 1,
	ArmR = 4,
	Chest = 8,
	Back = 16,
	Head = 32,
	Holster = 64,
	ForearmL = 128,
	ForearmR = 256,
	AuxHead = 512,
	AuxBody1 = 1024,
	AuxBody2 = 2048,
	AuxShoulderL = 4096,
	AuxShoulderR = 8192,
	Max = 16384
}

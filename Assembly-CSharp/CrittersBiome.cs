using System;

[Flags]
public enum CrittersBiome
{
	Forest = 1,
	Mountain = 2,
	Desert = 4,
	Grassland = 8,
	Cave = 16,
	IntroArea = 1073741824,
	Any = -1
}

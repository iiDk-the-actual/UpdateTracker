using System;
using UnityEngine;

public static class Id128Ext
{
	public static Id128 ToId128(this Hash128 h)
	{
		return new Id128(h);
	}

	public static Id128 ToId128(this Guid g)
	{
		return new Id128(g);
	}
}

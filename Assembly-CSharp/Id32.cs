using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public struct Id32
{
	public Id32(string idString)
	{
		if (idString == null)
		{
			throw new ArgumentNullException("idString");
		}
		if (string.IsNullOrWhiteSpace(idString.Trim()))
		{
			throw new ArgumentNullException("idString");
		}
		this._id = XXHash32.Compute(idString, 0U);
	}

	public unsafe static implicit operator int(Id32 i32)
	{
		return *Unsafe.As<Id32, int>(ref i32);
	}

	public static implicit operator Id32(string s)
	{
		return Id32.ComputeID(s);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Id32 ComputeID(string s)
	{
		int num = Id32.ComputeHash(s);
		return *Unsafe.As<int, Id32>(ref num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ComputeHash(string s)
	{
		if (s == null)
		{
			return 0;
		}
		s = s.Trim();
		if (string.IsNullOrWhiteSpace(s))
		{
			return 0;
		}
		return XXHash32.Compute(s, 0U);
	}

	public override int GetHashCode()
	{
		return this._id;
	}

	public override string ToString()
	{
		return string.Format("{{ {0} : {1} }}", "Id32", this._id);
	}

	[SerializeField]
	private int _id;
}

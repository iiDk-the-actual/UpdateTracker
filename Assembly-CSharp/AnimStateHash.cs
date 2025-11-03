using System;
using UnityEngine;

[Serializable]
public struct AnimStateHash
{
	public static implicit operator AnimStateHash(string s)
	{
		return new AnimStateHash
		{
			_hash = Animator.StringToHash(s)
		};
	}

	public static implicit operator int(AnimStateHash ash)
	{
		return ash._hash;
	}

	[SerializeField]
	private int _hash;
}

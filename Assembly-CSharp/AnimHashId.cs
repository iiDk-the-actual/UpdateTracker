using System;
using UnityEngine;

[Serializable]
public struct AnimHashId
{
	public string text
	{
		get
		{
			return this._text;
		}
	}

	public int hash
	{
		get
		{
			return this._hash;
		}
	}

	public AnimHashId(string text)
	{
		this._text = text;
		this._hash = Animator.StringToHash(text);
	}

	public override string ToString()
	{
		return this._text;
	}

	public override int GetHashCode()
	{
		return this._hash;
	}

	public static implicit operator int(AnimHashId h)
	{
		return h._hash;
	}

	public static implicit operator AnimHashId(string s)
	{
		return new AnimHashId(s);
	}

	[SerializeField]
	private string _text;

	[NonSerialized]
	private int _hash;
}

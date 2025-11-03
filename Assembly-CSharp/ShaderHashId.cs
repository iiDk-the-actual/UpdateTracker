using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct ShaderHashId : IEquatable<ShaderHashId>
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

	public ShaderHashId(string text)
	{
		this._text = text;
		this._hash = Shader.PropertyToID(text);
	}

	public override string ToString()
	{
		return this._text;
	}

	public override int GetHashCode()
	{
		return this._hash;
	}

	public static implicit operator int(ShaderHashId h)
	{
		return h._hash;
	}

	public static implicit operator ShaderHashId(string s)
	{
		return new ShaderHashId(s);
	}

	public bool Equals(ShaderHashId other)
	{
		return this._hash == other._hash;
	}

	public override bool Equals(object obj)
	{
		if (obj is ShaderHashId)
		{
			ShaderHashId shaderHashId = (ShaderHashId)obj;
			return this.Equals(shaderHashId);
		}
		return false;
	}

	public static bool operator ==(ShaderHashId x, ShaderHashId y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(ShaderHashId x, ShaderHashId y)
	{
		return !x.Equals(y);
	}

	[FormerlySerializedAs("_hashText")]
	[SerializeField]
	private string _text;

	[NonSerialized]
	private int _hash;
}

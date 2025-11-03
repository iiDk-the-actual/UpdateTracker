using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class UberShaderProperty
{
	public T GetValue<T>(Material target)
	{
		switch (this.type)
		{
		case ShaderPropertyType.Color:
			return UberShaderProperty.ValueAs<Color, T>(target.GetColor(this.nameID));
		case ShaderPropertyType.Vector:
			return UberShaderProperty.ValueAs<Vector4, T>(target.GetVector(this.nameID));
		case ShaderPropertyType.Float:
		case ShaderPropertyType.Range:
			return UberShaderProperty.ValueAs<float, T>(target.GetFloat(this.nameID));
		case ShaderPropertyType.Texture:
			return UberShaderProperty.ValueAs<Texture, T>(target.GetTexture(this.nameID));
		case ShaderPropertyType.Int:
			return UberShaderProperty.ValueAs<int, T>(target.GetInt(this.nameID));
		default:
			return default(T);
		}
	}

	public void SetValue<T>(Material target, T value)
	{
		switch (this.type)
		{
		case ShaderPropertyType.Color:
			target.SetColor(this.nameID, UberShaderProperty.ValueAs<T, Color>(value));
			break;
		case ShaderPropertyType.Vector:
			target.SetVector(this.nameID, UberShaderProperty.ValueAs<T, Vector4>(value));
			break;
		case ShaderPropertyType.Float:
		case ShaderPropertyType.Range:
			target.SetFloat(this.nameID, UberShaderProperty.ValueAs<T, float>(value));
			break;
		case ShaderPropertyType.Texture:
			target.SetTexture(this.nameID, UberShaderProperty.ValueAs<T, Texture>(value));
			break;
		case ShaderPropertyType.Int:
			target.SetInt(this.nameID, UberShaderProperty.ValueAs<T, int>(value));
			break;
		}
		if (!this.isKeywordToggle)
		{
			return;
		}
		bool flag = false;
		ShaderPropertyType shaderPropertyType = this.type;
		if (shaderPropertyType != ShaderPropertyType.Float)
		{
			if (shaderPropertyType == ShaderPropertyType.Int)
			{
				flag = UberShaderProperty.ValueAs<T, int>(value) >= 1;
			}
		}
		else
		{
			flag = UberShaderProperty.ValueAs<T, float>(value) >= 0.5f;
		}
		if (flag)
		{
			target.EnableKeyword(this.keyword);
			return;
		}
		target.DisableKeyword(this.keyword);
	}

	public void Enable(Material target)
	{
		ShaderPropertyType shaderPropertyType = this.type;
		if (shaderPropertyType != ShaderPropertyType.Float)
		{
			if (shaderPropertyType == ShaderPropertyType.Int)
			{
				target.SetInt(this.nameID, 1);
			}
		}
		else
		{
			target.SetFloat(this.nameID, 1f);
		}
		if (this.isKeywordToggle)
		{
			target.EnableKeyword(this.keyword);
		}
	}

	public void Disable(Material target)
	{
		ShaderPropertyType shaderPropertyType = this.type;
		if (shaderPropertyType != ShaderPropertyType.Float)
		{
			if (shaderPropertyType == ShaderPropertyType.Int)
			{
				target.SetInt(this.nameID, 0);
			}
		}
		else
		{
			target.SetFloat(this.nameID, 0f);
		}
		if (this.isKeywordToggle)
		{
			target.DisableKeyword(this.keyword);
		}
	}

	public bool TryGetKeywordState(Material target, out bool enabled)
	{
		enabled = false;
		if (!this.isKeywordToggle)
		{
			return false;
		}
		enabled = target.IsKeywordEnabled(this.keyword);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static TOut ValueAs<TIn, TOut>(TIn value)
	{
		return *Unsafe.As<TIn, TOut>(ref value);
	}

	public int index;

	public int nameID;

	public string name;

	public ShaderPropertyType type;

	public ShaderPropertyFlags flags;

	public Vector2 rangeLimits;

	public string[] attributes;

	public bool isKeywordToggle;

	public string keyword;
}

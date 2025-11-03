using System;
using UnityEngine;

public struct TexFormatInfo
{
	public TexFormatInfo(Texture2D tex2d)
	{
		this.width = tex2d.width;
		this.height = tex2d.height;
		this.format = tex2d.format;
		this.filterMode = tex2d.filterMode;
		this.isLinearColor = !tex2d.isDataSRGB;
		this.mipmapCount = tex2d.mipmapCount;
		this.isValid = true;
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			"TexFormatInfo(isValid: ",
			this.isValid.ToString(),
			", width: ",
			this.width.ToString(),
			", height: ",
			this.height.ToString(),
			", format: ",
			this.format.ToString(),
			", filterMode: ",
			this.filterMode.ToString(),
			", isLinearColor: ",
			this.isLinearColor.ToString(),
			", mipmapCount: ",
			this.mipmapCount.ToString(),
			")"
		});
	}

	public bool isValid;

	public int width;

	public int height;

	public TextureFormat format;

	public FilterMode filterMode;

	public int mipmapCount;

	public bool isLinearColor;
}

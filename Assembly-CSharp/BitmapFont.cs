using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BitmapFont : ScriptableObject
{
	private void OnEnable()
	{
		this._charToSymbol = this.symbols.ToDictionary((BitmapFont.SymbolData s) => s.character, (BitmapFont.SymbolData s) => s);
	}

	public void RenderToTexture(Texture2D target, string text)
	{
		if (text == null)
		{
			text = string.Empty;
		}
		int num = target.width * target.height;
		if (this._empty.Length != num)
		{
			this._empty = new Color[num];
			for (int i = 0; i < this._empty.Length; i++)
			{
				this._empty[i] = Color.black;
			}
		}
		target.SetPixels(this._empty);
		int length = text.Length;
		int num2 = 1;
		int width = this.fontImage.width;
		int height = this.fontImage.height;
		for (int j = 0; j < length; j++)
		{
			char c = text[j];
			BitmapFont.SymbolData symbolData = this._charToSymbol[c];
			int width2 = symbolData.width;
			int height2 = symbolData.height;
			int x = symbolData.x;
			int y = symbolData.y;
			Graphics.CopyTexture(this.fontImage, 0, 0, x, height - (y + height2), width2, height2, target, 0, 0, num2, 2 + symbolData.yoffset);
			num2 += width2 + 1;
		}
		target.Apply(false);
	}

	public Texture2D fontImage;

	public TextAsset fontJson;

	public int symbolPixelsPerUnit = 1;

	public string characterMap;

	[Space]
	public BitmapFont.SymbolData[] symbols = new BitmapFont.SymbolData[0];

	private Dictionary<char, BitmapFont.SymbolData> _charToSymbol;

	private Color[] _empty = new Color[0];

	[Serializable]
	public struct SymbolData
	{
		public char character;

		[Space]
		public int id;

		public int width;

		public int height;

		public int x;

		public int y;

		public int xadvance;

		public int yoffset;
	}
}

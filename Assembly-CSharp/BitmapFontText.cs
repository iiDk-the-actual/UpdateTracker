using System;
using UnityEngine;

public class BitmapFontText : MonoBehaviour
{
	private void Awake()
	{
		this.Init();
		this.Render();
	}

	public void Render()
	{
		this.font.RenderToTexture(this.texture, this.uppercaseOnly ? this.text.ToUpperInvariant() : this.text);
	}

	public void Init()
	{
		this.texture = new Texture2D(this.textArea.x, this.textArea.y, this.font.fontImage.format, false);
		this.texture.filterMode = FilterMode.Point;
		this.material = new Material(this.renderer.sharedMaterial);
		this.material.mainTexture = this.texture;
		this.renderer.sharedMaterial = this.material;
	}

	public string text;

	public bool uppercaseOnly;

	public Vector2Int textArea;

	[Space]
	public Renderer renderer;

	public Texture2D texture;

	public Material material;

	public BitmapFont font;
}

using System;
using System.Collections;
using UnityEngine;

public class TextureSlideshow : MonoBehaviour
{
	private void Awake()
	{
		this._renderer = base.GetComponent<Renderer>();
		this._renderer.material.mainTexture = this.textures[0];
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.runSlideshow());
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
	}

	private IEnumerator runSlideshow()
	{
		yield return new WaitForSecondsRealtime(this.prePause);
		int i = 0;
		for (;;)
		{
			yield return new WaitForSecondsRealtime(Random.Range(this.minMaxPause.x, this.minMaxPause.y));
			this._renderer.material.mainTexture = this.textures[i];
			i = (i + 1) % this.textures.Length;
		}
		yield break;
	}

	private Renderer _renderer;

	[SerializeField]
	private Texture[] textures;

	[SerializeField]
	private Vector2 minMaxPause;

	[SerializeField]
	private float prePause = 1f;
}

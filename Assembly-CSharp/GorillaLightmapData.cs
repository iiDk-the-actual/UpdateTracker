using System;
using UnityEngine;

public class GorillaLightmapData : MonoBehaviour
{
	public void Awake()
	{
		this.lights = new Color[this.lightTextures.Length][];
		this.dirs = new Color[this.dirTextures.Length][];
		for (int i = 0; i < this.dirTextures.Length; i++)
		{
			float value = Random.value;
			Debug.Log(value.ToString() + " before load " + Time.realtimeSinceStartup.ToString());
			this.dirs[i] = this.dirTextures[i].GetPixels();
			this.lights[i] = this.lightTextures[i].GetPixels();
			Debug.Log(value.ToString() + " after load " + Time.realtimeSinceStartup.ToString());
		}
	}

	[SerializeField]
	public Texture2D[] dirTextures;

	[SerializeField]
	public Texture2D[] lightTextures;

	public Color[][] lights;

	public Color[][] dirs;

	public bool done;
}

using System;
using UnityEngine;

public class DJDeckEqualizer : MonoBehaviour
{
	private void Start()
	{
		this.inputColorHash = this.inputColorProperty;
		this.material = this.display.material;
	}

	private void Update()
	{
		Color color = default(Color);
		color.r = 0.25f;
		color.g = 0.25f;
		color.b = 0.5f;
		for (int i = 0; i < this.redTracks.Length; i++)
		{
			AudioSource audioSource = this.redTracks[i];
			if (audioSource.isPlaying)
			{
				color.r = Mathf.Lerp(0.25f, 1f, this.redTrackCurves[i].Evaluate(audioSource.time));
				break;
			}
		}
		for (int j = 0; j < this.greenTracks.Length; j++)
		{
			AudioSource audioSource2 = this.greenTracks[j];
			if (audioSource2.isPlaying)
			{
				color.g = Mathf.Lerp(0.25f, 1f, this.greenTrackCurves[j].Evaluate(audioSource2.time));
				break;
			}
		}
		this.material.SetColor(this.inputColorHash, color);
	}

	[SerializeField]
	private MeshRenderer display;

	[SerializeField]
	private AnimationCurve[] redTrackCurves;

	[SerializeField]
	private AnimationCurve[] greenTrackCurves;

	[SerializeField]
	private AudioSource[] redTracks;

	[SerializeField]
	private AudioSource[] greenTracks;

	private Material material;

	[SerializeField]
	private string inputColorProperty;

	private ShaderHashId inputColorHash;
}

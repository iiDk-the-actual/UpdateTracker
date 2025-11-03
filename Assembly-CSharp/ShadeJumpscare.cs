using System;
using GorillaExtensions;
using UnityEngine;

public class ShadeJumpscare : MonoBehaviour
{
	private void Awake()
	{
		this.audioSource = base.GetComponent<AudioSource>();
	}

	private void OnEnable()
	{
		this.startTime = Time.time;
		this.startAngle = Random.value * 360f;
		this.audioSource.clip = this.audioClips.GetRandomItem<AudioClip>();
		this.audioSource.GTPlay();
	}

	private void Update()
	{
		float num = Time.time - this.startTime;
		float num2 = num / this.animationTime;
		this.shadeTransform.SetPositionAndRotation(base.transform.position + new Vector3(0f, this.shadeHeightFunction.Evaluate(num2), 0f), Quaternion.Euler(0f, this.startAngle + num * this.shadeRotationSpeed, 0f));
		float num3 = this.shadeScaleFunction.Evaluate(num2);
		this.shadeTransform.localScale = new Vector3(num3, num3 * this.shadeYScaleMultFunction.Evaluate(num2), num3);
		this.audioSource.volume = this.soundVolumeFunction.Evaluate(num2);
	}

	[SerializeField]
	private Transform shadeTransform;

	[SerializeField]
	private float animationTime;

	[SerializeField]
	private float shadeRotationSpeed = 1f;

	[SerializeField]
	private AnimationCurve shadeHeightFunction;

	[SerializeField]
	private AnimationCurve shadeScaleFunction;

	[SerializeField]
	private AnimationCurve shadeYScaleMultFunction;

	[SerializeField]
	private AnimationCurve soundVolumeFunction;

	[SerializeField]
	private AudioClip[] audioClips;

	private AudioSource audioSource;

	private float startTime;

	private float startAngle;
}

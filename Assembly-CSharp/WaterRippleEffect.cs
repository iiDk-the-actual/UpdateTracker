using System;
using GorillaLocomotion.Swimming;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class WaterRippleEffect : MonoBehaviour
{
	private void Awake()
	{
		this.animator = base.GetComponent<Animator>();
		this.renderer = base.GetComponent<SpriteRenderer>();
		this.ripplePlaybackSpeedHash = Animator.StringToHash(this.ripplePlaybackSpeedName);
	}

	public void Destroy()
	{
		this.waterVolume = null;
		ObjectPools.instance.Destroy(base.gameObject);
	}

	public void PlayEffect(WaterVolume volume = null)
	{
		this.waterVolume = volume;
		this.rippleStartTime = Time.time;
		this.animator.SetFloat(this.ripplePlaybackSpeedHash, this.ripplePlaybackSpeed);
		if (this.waterVolume != null && this.waterVolume.Parameters != null)
		{
			this.renderer.color = this.waterVolume.Parameters.rippleSpriteColor;
		}
		Color color = this.renderer.color;
		color.a = 1f;
		this.renderer.color = color;
	}

	private void Update()
	{
		if (this.waterVolume != null && !this.waterVolume.isStationary && this.waterVolume.surfacePlane != null)
		{
			Vector3 vector = Vector3.Dot(base.transform.position - this.waterVolume.surfacePlane.position, this.waterVolume.surfacePlane.up) * this.waterVolume.surfacePlane.up;
			base.transform.position = base.transform.position - vector;
		}
		float num = Mathf.Clamp01((Time.time - this.rippleStartTime - this.fadeOutDelay) / this.fadeOutTime);
		Color color = this.renderer.color;
		color.a = 1f - num;
		this.renderer.color = color;
		if (num >= 1f - Mathf.Epsilon)
		{
			this.Destroy();
			return;
		}
	}

	[SerializeField]
	private float ripplePlaybackSpeed = 1f;

	[SerializeField]
	private float fadeOutDelay = 0.5f;

	[SerializeField]
	private float fadeOutTime = 1f;

	private string ripplePlaybackSpeedName = "RipplePlaybackSpeed";

	private int ripplePlaybackSpeedHash;

	private float rippleStartTime = -1f;

	private Animator animator;

	private SpriteRenderer renderer;

	private WaterVolume waterVolume;
}

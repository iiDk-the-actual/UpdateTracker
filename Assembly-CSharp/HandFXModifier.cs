using System;
using UnityEngine;

public class HandFXModifier : FXModifier
{
	private void Awake()
	{
		this.originalScale = base.transform.localScale;
	}

	private void OnDisable()
	{
		base.transform.localScale = this.originalScale;
	}

	public override void UpdateScale(float scale, Color color)
	{
		scale = Mathf.Clamp(scale, this.minScale, this.maxScale);
		base.transform.localScale = this.originalScale * scale;
	}

	private Vector3 originalScale;

	[SerializeField]
	private float minScale;

	[SerializeField]
	private float maxScale;

	[SerializeField]
	private ParticleSystem dustBurst;

	[SerializeField]
	private ParticleSystem dustLinger;
}

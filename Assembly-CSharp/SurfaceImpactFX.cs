using System;
using UnityEngine;

public class SurfaceImpactFX : MonoBehaviour
{
	public void Awake()
	{
		if (this.particleFX == null)
		{
			this.particleFX = base.GetComponent<ParticleSystem>();
		}
		if (this.particleFX == null)
		{
			Debug.LogError("SurfaceImpactFX: No ParticleSystem found! Disabling component.", this);
			base.enabled = false;
			return;
		}
		this.fxMainModule = this.particleFX.main;
	}

	public void SetScale(float scale)
	{
		this.fxMainModule.gravityModifierMultiplier = this.startingGravityModifier * scale;
		base.transform.localScale = this.startingScale * scale;
	}

	public ParticleSystem particleFX;

	public float startingGravityModifier;

	public Vector3 startingScale = Vector3.one;

	private ParticleSystem.MainModule fxMainModule;
}

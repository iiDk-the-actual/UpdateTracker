using System;
using UnityEngine;

public class GorillaColorizableParticle : GorillaColorizableBase
{
	public override void SetColor(Color color)
	{
		ParticleSystem.MainModule main = this.particleSystem.main;
		Color color2 = new Color(Mathf.Pow(color.r, this.gradientColorPower), Mathf.Pow(color.g, this.gradientColorPower), Mathf.Pow(color.b, this.gradientColorPower), color.a);
		main.startColor = new ParticleSystem.MinMaxGradient(this.useLinearColor ? color.linear : color, this.useLinearColor ? color2.linear : color2);
	}

	public ParticleSystem particleSystem;

	public float gradientColorPower = 2f;

	public bool useLinearColor = true;
}

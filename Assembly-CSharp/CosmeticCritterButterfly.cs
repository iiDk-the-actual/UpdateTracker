using System;
using UnityEngine;

public class CosmeticCritterButterfly : CosmeticCritter
{
	public ParticleSystem.EmitParams GetEmitParams
	{
		get
		{
			return this.emitParams;
		}
	}

	public void SetStartPos(Vector3 initialPos)
	{
		this.startPosition = initialPos;
	}

	public override void SetRandomVariables()
	{
		this.direction = Random.insideUnitSphere;
		this.emitParams.startColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
		this.particleSystem.Emit(this.emitParams, 1);
	}

	public override void Tick()
	{
		base.transform.position = this.startPosition + (float)base.GetAliveTime() * this.speed * this.direction;
	}

	[Tooltip("The speed this Butterfly will move at.")]
	[SerializeField]
	private float speed = 1f;

	[Tooltip("Emit one particle from this particle system when spawning.")]
	[SerializeField]
	private ParticleSystem particleSystem;

	private Vector3 startPosition;

	private Vector3 direction;

	private ParticleSystem.EmitParams emitParams;
}

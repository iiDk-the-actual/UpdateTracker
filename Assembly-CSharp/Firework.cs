using System;
using System.Linq;
using UnityEngine;

public class Firework : MonoBehaviour
{
	private void Launch()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this._controller)
		{
			this._controller.Launch(this);
		}
	}

	private void OnValidate()
	{
		if (!this._controller)
		{
			this._controller = base.GetComponentInParent<FireworksController>();
		}
		if (!this._controller)
		{
			return;
		}
		Firework[] array = this._controller.fireworks;
		if (array.Contains(this))
		{
			return;
		}
		array = (from x in array.Concat(new Firework[] { this })
			where x != null
			select x).ToArray<Firework>();
		this._controller.fireworks = array;
	}

	private void OnDrawGizmos()
	{
		if (!this._controller)
		{
			return;
		}
		this._controller.RenderGizmo(this, Color.cyan);
	}

	private void OnDrawGizmosSelected()
	{
		if (!this._controller)
		{
			return;
		}
		this._controller.RenderGizmo(this, Color.yellow);
	}

	[SerializeField]
	private FireworksController _controller;

	[Space]
	public Transform origin;

	public Transform target;

	[Space]
	public Color colorOrigin = Color.cyan;

	public Color colorTarget = Color.magenta;

	[Space]
	public AudioSource sourceOrigin;

	public AudioSource sourceTarget;

	[Space]
	public ParticleSystem trail;

	[Space]
	public ParticleSystem[] explosions;

	[Space]
	public bool doTrail = true;

	public bool doTrailAudio = true;

	public bool doExplosion = true;
}

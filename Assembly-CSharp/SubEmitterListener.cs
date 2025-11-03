using System;
using UnityEngine;
using UnityEngine.Events;

public class SubEmitterListener : MonoBehaviour
{
	private void OnEnable()
	{
		if (this.target == null)
		{
			this.Disable();
			return;
		}
		ParticleSystem.SubEmittersModule subEmitters = this.target.subEmitters;
		if (this.subEmitterIndex < 0)
		{
			this.subEmitterIndex = 0;
		}
		this._canListen = subEmitters.subEmittersCount > 0 && this.subEmitterIndex <= subEmitters.subEmittersCount - 1;
		if (!this._canListen)
		{
			this.Disable();
			return;
		}
		this.subEmitter = this.target.subEmitters.GetSubEmitterSystem(this.subEmitterIndex);
		ParticleSystem.MainModule main = this.subEmitter.main;
		this.interval = main.startLifetime.constantMax * main.startLifetimeMultiplier;
	}

	private void OnDisable()
	{
		this._listenOnce = false;
		this._listening = false;
	}

	public void ListenStart()
	{
		if (this._listening)
		{
			return;
		}
		if (this._canListen)
		{
			this.Enable();
			this._listening = true;
		}
	}

	public void ListenStop()
	{
		this.Disable();
	}

	public void ListenOnce()
	{
		if (this._listening)
		{
			return;
		}
		this.Enable();
		if (this._canListen)
		{
			this.Enable();
			this._listenOnce = true;
			this._listening = true;
		}
	}

	private void Update()
	{
		if (!this._canListen)
		{
			return;
		}
		if (!this._listening)
		{
			return;
		}
		if (this.subEmitter.particleCount > 0 && this._sinceLastEmit >= this.interval * this.intervalScale)
		{
			this._sinceLastEmit = 0f;
			this.OnSubEmit();
			if (this._listenOnce)
			{
				this.Disable();
			}
		}
	}

	protected virtual void OnSubEmit()
	{
		UnityEvent unityEvent = this.onSubEmit;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	public void Enable()
	{
		if (!base.enabled)
		{
			base.enabled = true;
		}
	}

	public void Disable()
	{
		if (base.enabled)
		{
			base.enabled = false;
		}
	}

	public ParticleSystem target;

	public ParticleSystem subEmitter;

	public int subEmitterIndex;

	public UnityEvent onSubEmit;

	public float intervalScale = 1f;

	public float interval;

	[NonSerialized]
	private bool _canListen;

	[NonSerialized]
	private bool _listening;

	[NonSerialized]
	private bool _listenOnce;

	[NonSerialized]
	private TimeSince _sinceLastEmit;
}

using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class Breakable : MonoBehaviour
{
	private void Awake()
	{
		this._breakSignal.OnSignal += this.BreakRPC;
		if (this._rigidbody.IsNotNull())
		{
			this.m_useGravity = this._rigidbody.useGravity;
		}
	}

	private void BreakRPC(int owner, PhotonSignalInfo info)
	{
		VRRig vrrig = base.GetComponent<OwnerRig>();
		if (vrrig == null)
		{
			return;
		}
		if (vrrig.OwningNetPlayer.ActorNumber != owner)
		{
			return;
		}
		if (!this.m_spamChecker.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		this.OnBreak(true, false);
	}

	private void Setup()
	{
		if (this._collider == null)
		{
			SphereCollider sphereCollider;
			this.GetOrAddComponent(out sphereCollider);
			this._collider = sphereCollider;
		}
		this._collider.enabled = true;
		if (this._rigidbody == null)
		{
			this.GetOrAddComponent(out this._rigidbody);
		}
		this._rigidbody.isKinematic = false;
		this._rigidbody.useGravity = false;
		this._rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		this.UpdatePhysMasks();
		if (this.rendererRoot == null)
		{
			this._renderers = base.GetComponentsInChildren<Renderer>();
			return;
		}
		this._renderers = this.rendererRoot.GetComponentsInChildren<Renderer>();
	}

	private void OnCollisionEnter(Collision col)
	{
		this.OnBreak(true, true);
	}

	private void OnCollisionStay(Collision col)
	{
		this.OnBreak(true, true);
	}

	private void OnTriggerEnter(Collider col)
	{
		this.OnBreak(true, true);
	}

	private void OnTriggerStay(Collider col)
	{
		this.OnBreak(true, true);
	}

	private void OnEnable()
	{
		this._breakSignal.Enable();
		this._broken = false;
		this.OnSpawn(true);
	}

	private void OnDisable()
	{
		this._breakSignal.Disable();
		this._broken = false;
		this.OnReset(false);
		this.ShowRenderers(false);
	}

	public void Break()
	{
		this.OnBreak(true, true);
	}

	public void Reset()
	{
		this.OnReset(true);
	}

	protected virtual void ShowRenderers(bool visible)
	{
		if (this._renderers.IsNullOrEmpty<Renderer>())
		{
			return;
		}
		for (int i = 0; i < this._renderers.Length; i++)
		{
			Renderer renderer = this._renderers[i];
			if (renderer)
			{
				renderer.forceRenderingOff = !visible;
			}
		}
	}

	protected virtual void OnReset(bool callback = true)
	{
		if (this._breakEffect && this._breakEffect.isPlaying)
		{
			this._breakEffect.Stop();
		}
		this.ShowRenderers(true);
		this._broken = false;
		if (callback)
		{
			UnityEvent<Breakable> unityEvent = this.onReset;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(this);
		}
	}

	protected virtual void OnSpawn(bool callback = true)
	{
		this.startTime = Time.time;
		this.endTime = this.startTime + this.canBreakDelay;
		this.ShowRenderers(true);
		if (this._rigidbody.IsNotNull())
		{
			this._rigidbody.detectCollisions = true;
			this._rigidbody.useGravity = this.m_useGravity;
		}
		if (callback)
		{
			UnityEvent<Breakable> unityEvent = this.onSpawn;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(this);
		}
	}

	protected virtual void OnBreak(bool callback = true, bool signal = true)
	{
		if (this._broken)
		{
			return;
		}
		if (Time.time < this.endTime)
		{
			return;
		}
		if (this._breakEffect)
		{
			if (this._breakEffect.isPlaying)
			{
				this._breakEffect.Stop();
			}
			this._breakEffect.Play();
		}
		if (signal && PhotonNetwork.InRoom)
		{
			VRRig vrrig = base.GetComponent<OwnerRig>();
			if (vrrig != null)
			{
				this._breakSignal.Raise(vrrig.OwningNetPlayer.ActorNumber);
			}
		}
		this.ShowRenderers(false);
		if (this._rigidbody.IsNotNull())
		{
			this._rigidbody.detectCollisions = false;
			this._rigidbody.useGravity = false;
		}
		this._broken = true;
		if (callback)
		{
			UnityEvent<Breakable> unityEvent = this.onBreak;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(this);
		}
	}

	private void UpdatePhysMasks()
	{
		int physicsMask = (int)this._physicsMask;
		if (this._collider)
		{
			this._collider.includeLayers = physicsMask;
			this._collider.excludeLayers = ~physicsMask;
		}
		if (this._rigidbody)
		{
			this._rigidbody.includeLayers = physicsMask;
			this._rigidbody.excludeLayers = ~physicsMask;
		}
	}

	[SerializeField]
	private Collider _collider;

	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private GameObject rendererRoot;

	[SerializeField]
	private Renderer[] _renderers = new Renderer[0];

	[Space]
	[SerializeField]
	private ParticleSystem _breakEffect;

	[SerializeField]
	private UnityLayerMask _physicsMask = UnityLayerMask.GorillaHand;

	public UnityEvent<Breakable> onSpawn;

	public UnityEvent<Breakable> onBreak;

	public UnityEvent<Breakable> onReset;

	public float canBreakDelay = 1f;

	[SerializeField]
	private PhotonSignal<int> _breakSignal = "_breakSignal";

	[SerializeField]
	private CallLimiter m_spamChecker = new CallLimiter(2, 1f, 0.5f);

	[Space]
	[NonSerialized]
	private bool _broken;

	private bool m_useGravity = true;

	private float startTime;

	private float endTime;
}

using System;
using GorillaExtensions;
using TagEffects;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HandEffectsTester : MonoBehaviour, IHandEffectsTrigger
{
	public bool Static
	{
		get
		{
			return this.isStatic;
		}
	}

	Transform IHandEffectsTrigger.Transform
	{
		get
		{
			return base.transform;
		}
	}

	VRRig IHandEffectsTrigger.Rig
	{
		get
		{
			return null;
		}
	}

	IHandEffectsTrigger.Mode IHandEffectsTrigger.EffectMode
	{
		get
		{
			return this.mode;
		}
	}

	bool IHandEffectsTrigger.FingersDown
	{
		get
		{
			return this.mode == IHandEffectsTrigger.Mode.FistBump || this.mode == IHandEffectsTrigger.Mode.HighFive_And_FistBump;
		}
	}

	bool IHandEffectsTrigger.FingersUp
	{
		get
		{
			return this.mode == IHandEffectsTrigger.Mode.HighFive || this.mode == IHandEffectsTrigger.Mode.HighFive_And_FistBump;
		}
	}

	public bool RightHand { get; }

	private void Awake()
	{
		this.triggerZone = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		if (!HandEffectsTriggerRegistry.HasInstance)
		{
			HandEffectsTriggerRegistry.FindInstance();
		}
		HandEffectsTriggerRegistry.Instance.Register(this);
	}

	private void OnDisable()
	{
		HandEffectsTriggerRegistry.Instance.Unregister(this);
	}

	Vector3 IHandEffectsTrigger.Velocity
	{
		get
		{
			if (this.mode == IHandEffectsTrigger.Mode.HighFive)
			{
				return Vector3.zero;
			}
			IHandEffectsTrigger.Mode mode = this.mode;
			return Vector3.zero;
		}
	}

	TagEffectPack IHandEffectsTrigger.CosmeticEffectPack
	{
		get
		{
			return this.cosmeticEffectPack;
		}
	}

	public void OnTriggerEntered(IHandEffectsTrigger other)
	{
	}

	public bool InTriggerZone(IHandEffectsTrigger t)
	{
		if (!(base.transform.position - t.Transform.position).IsShorterThan(this.triggerZone.bounds.size))
		{
			return false;
		}
		RaycastHit raycastHit;
		switch (this.mode)
		{
		case IHandEffectsTrigger.Mode.HighFive:
			return t.FingersUp && this.triggerZone.Raycast(new Ray(t.Transform.position, t.Transform.right), out raycastHit, this.triggerRadius);
		case IHandEffectsTrigger.Mode.FistBump:
			return t.FingersDown && this.triggerZone.Raycast(new Ray(t.Transform.position, t.Transform.up), out raycastHit, this.triggerRadius);
		case IHandEffectsTrigger.Mode.HighFive_And_FistBump:
			return (t.FingersUp && this.triggerZone.Raycast(new Ray(t.Transform.position, t.Transform.right), out raycastHit, this.triggerRadius)) || (t.FingersDown && this.triggerZone.Raycast(new Ray(t.Transform.position, t.Transform.up), out raycastHit, this.triggerRadius));
		}
		return this.triggerZone.Raycast(new Ray(t.Transform.position, this.triggerZone.bounds.center - t.Transform.position), out raycastHit, this.triggerRadius);
	}

	[SerializeField]
	private TagEffectPack cosmeticEffectPack;

	private Collider triggerZone;

	public IHandEffectsTrigger.Mode mode;

	[SerializeField]
	private float triggerRadius = 0.07f;

	[SerializeField]
	private bool isStatic = true;
}

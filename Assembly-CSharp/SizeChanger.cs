using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SizeChanger : GorillaTriggerBox
{
	public int SizeLayerMask
	{
		get
		{
			int num = 0;
			if (this.affectLayerA)
			{
				num |= 1;
			}
			if (this.affectLayerB)
			{
				num |= 2;
			}
			if (this.affectLayerC)
			{
				num |= 4;
			}
			if (this.affectLayerD)
			{
				num |= 8;
			}
			return num;
		}
	}

	public SizeChanger.ChangerType MyType
	{
		get
		{
			return this.myType;
		}
	}

	public float MaxScale
	{
		get
		{
			return this.maxScale;
		}
	}

	public float MinScale
	{
		get
		{
			return this.minScale;
		}
	}

	public Transform StartPos
	{
		get
		{
			return this.startPos;
		}
	}

	public Transform EndPos
	{
		get
		{
			return this.endPos;
		}
	}

	public float StaticEasing
	{
		get
		{
			return this.staticEasing;
		}
	}

	private void Awake()
	{
		this.minScale = Mathf.Max(this.minScale, 0.01f);
		this.myCollider = base.GetComponent<Collider>();
	}

	public void OnEnable()
	{
		if (this.enterTrigger)
		{
			this.enterTrigger.OnEnter += this.OnTriggerEnter;
		}
		if (this.exitTrigger)
		{
			this.exitTrigger.OnExit += this.OnTriggerExit;
		}
		if (this.exitOnEnterTrigger)
		{
			this.exitOnEnterTrigger.OnEnter += this.OnTriggerExit;
		}
	}

	public void OnDisable()
	{
		if (this.enterTrigger)
		{
			this.enterTrigger.OnEnter -= this.OnTriggerEnter;
		}
		if (this.exitTrigger)
		{
			this.exitTrigger.OnExit -= this.OnTriggerExit;
		}
		if (this.exitOnEnterTrigger)
		{
			this.exitOnEnterTrigger.OnEnter -= this.OnTriggerExit;
		}
	}

	public void AddEnterTrigger(SizeChangerTrigger trigger)
	{
		if (trigger)
		{
			trigger.OnEnter += this.OnTriggerEnter;
		}
	}

	public void RemoveEnterTrigger(SizeChangerTrigger trigger)
	{
		if (trigger)
		{
			trigger.OnEnter -= this.OnTriggerEnter;
		}
	}

	public void AddExitOnEnterTrigger(SizeChangerTrigger trigger)
	{
		if (trigger)
		{
			trigger.OnEnter += this.OnTriggerExit;
		}
	}

	public void RemoveExitOnEnterTrigger(SizeChangerTrigger trigger)
	{
		if (trigger)
		{
			trigger.OnEnter -= this.OnTriggerExit;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!other.GetComponent<SphereCollider>())
		{
			return;
		}
		VRRig component = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
		if (component == null)
		{
			return;
		}
		this.acceptRig(component);
	}

	public void acceptRig(VRRig rig)
	{
		if (!rig.sizeManager.touchingChangers.Contains(this))
		{
			rig.sizeManager.touchingChangers.Add(this);
		}
		UnityAction onEnter = this.OnEnter;
		if (onEnter == null)
		{
			return;
		}
		onEnter();
	}

	public void OnTriggerExit(Collider other)
	{
		if (!other.GetComponent<SphereCollider>())
		{
			return;
		}
		VRRig component = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
		if (component == null)
		{
			return;
		}
		this.unacceptRig(component);
	}

	public void unacceptRig(VRRig rig)
	{
		rig.sizeManager.touchingChangers.Remove(this);
		UnityAction onExit = this.OnExit;
		if (onExit == null)
		{
			return;
		}
		onExit();
	}

	public Vector3 ClosestPoint(Vector3 position)
	{
		if (this.enterTrigger && this.exitTrigger)
		{
			Vector3 vector = this.enterTrigger.ClosestPoint(position);
			Vector3 vector2 = this.exitTrigger.ClosestPoint(position);
			if (Vector3.Distance(position, vector) >= Vector3.Distance(position, vector2))
			{
				return vector2;
			}
			return vector;
		}
		else
		{
			if (this.myCollider)
			{
				return this.myCollider.ClosestPoint(position);
			}
			return position;
		}
	}

	public void SetScaleCenterPoint(Transform centerPoint)
	{
		this.scaleAwayFromPoint = centerPoint;
	}

	public bool TryGetScaleCenterPoint(out Vector3 centerPoint)
	{
		if (this.scaleAwayFromPoint != null)
		{
			centerPoint = this.scaleAwayFromPoint.position;
			return true;
		}
		centerPoint = Vector3.zero;
		return false;
	}

	[SerializeField]
	private SizeChanger.ChangerType myType;

	[SerializeField]
	private float staticEasing;

	[SerializeField]
	private float maxScale;

	[SerializeField]
	private float minScale;

	private Collider myCollider;

	[SerializeField]
	private Transform startPos;

	[SerializeField]
	private Transform endPos;

	[SerializeField]
	private SizeChangerTrigger enterTrigger;

	[SerializeField]
	private SizeChangerTrigger exitTrigger;

	[SerializeField]
	private Transform scaleAwayFromPoint;

	[SerializeField]
	private SizeChangerTrigger exitOnEnterTrigger;

	public bool alwaysControlWhenEntered;

	public int priority;

	public bool aprilFoolsEnabled;

	public float startRadius;

	public float endRadius;

	public bool affectLayerA = true;

	public bool affectLayerB = true;

	public bool affectLayerC = true;

	public bool affectLayerD = true;

	public UnityAction OnExit;

	public UnityAction OnEnter;

	private HashSet<VRRig> unregisteredPresentRigs;

	public enum ChangerType
	{
		Static,
		Continuous,
		Radius
	}
}

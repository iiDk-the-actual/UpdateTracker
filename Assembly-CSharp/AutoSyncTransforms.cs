using System;
using GorillaExtensions;
using UnityEngine;

public class AutoSyncTransforms : MonoBehaviour
{
	public Transform TargetTransform
	{
		get
		{
			return this.m_transform;
		}
	}

	public Rigidbody TargetRigidbody
	{
		get
		{
			return this.m_rigidbody;
		}
	}

	private void Awake()
	{
		if (this.m_transform.IsNull())
		{
			this.m_transform = base.transform;
		}
		if (this.m_rigidbody.IsNull())
		{
			this.m_rigidbody = base.GetComponent<Rigidbody>();
		}
		if (this.m_transform.IsNull() || this.m_rigidbody.IsNull())
		{
			base.enabled = false;
			Debug.LogError("AutoSyncTransforms: Rigidbody or Transform is null, disabling!! Please add the missing reference or component", this);
			return;
		}
		this.clean = true;
	}

	private void OnEnable()
	{
		if (this.clean)
		{
			PostVRRigPhysicsSynch.AddSyncTarget(this);
		}
	}

	private void OnDisable()
	{
		if (this.clean)
		{
			PostVRRigPhysicsSynch.RemoveSyncTarget(this);
		}
	}

	[SerializeField]
	private Transform m_transform;

	[SerializeField]
	private Rigidbody m_rigidbody;

	private bool clean;
}

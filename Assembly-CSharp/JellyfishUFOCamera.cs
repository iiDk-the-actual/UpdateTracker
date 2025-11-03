using System;
using BoingKit;
using UnityEngine;

public class JellyfishUFOCamera : MonoBehaviour
{
	private void Start()
	{
		if (this.Target == null)
		{
			return;
		}
		this.m_spring.Reset(this.Target.transform.position);
	}

	private void FixedUpdate()
	{
		if (this.Target == null)
		{
			return;
		}
		this.m_spring.TrackExponential(this.Target.transform.position, 0.5f, Time.fixedDeltaTime);
		Vector3 normalized = (this.m_spring.Value - base.transform.position).normalized;
		base.transform.rotation = Quaternion.LookRotation(normalized);
	}

	public Transform Target;

	private Vector3Spring m_spring;
}

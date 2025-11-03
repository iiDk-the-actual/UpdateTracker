using System;
using BoingKit;
using UnityEngine;

public class ColliderSpinner : MonoBehaviour
{
	private void Start()
	{
		this.m_targetOffset = ((this.Target != null) ? (base.transform.position - this.Target.position) : Vector3.zero);
		this.m_spring.Reset(base.transform.position);
	}

	private void FixedUpdate()
	{
		Vector3 vector = this.Target.position + this.m_targetOffset;
		base.transform.position = this.m_spring.TrackExponential(vector, 0.02f, Time.fixedDeltaTime);
	}

	public Transform Target;

	private Vector3 m_targetOffset;

	private Vector3Spring m_spring;
}

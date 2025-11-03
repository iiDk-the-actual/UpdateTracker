using System;
using BoingKit;
using UnityEngine;

public class ScaleSpring : MonoBehaviour
{
	public void Tick()
	{
		this.m_targetScale = ((this.m_targetScale == ScaleSpring.kSmallScale) ? ScaleSpring.kLargeScale : ScaleSpring.kSmallScale);
		this.m_lastTickTime = Time.time;
		base.GetComponent<BoingEffector>().MoveDistance = ScaleSpring.kMoveDistance * ((this.m_targetScale == ScaleSpring.kSmallScale) ? (-1f) : 1f);
	}

	public void Start()
	{
		this.Tick();
		this.m_spring.Reset(this.m_targetScale * Vector3.one);
	}

	public void FixedUpdate()
	{
		if (Time.time - this.m_lastTickTime > ScaleSpring.kInterval)
		{
			this.Tick();
		}
		this.m_spring.TrackHalfLife(this.m_targetScale * Vector3.one, 6f, 0.05f, Time.fixedDeltaTime);
		base.transform.localScale = this.m_spring.Value;
		base.GetComponent<BoingEffector>().MoveDistance *= Mathf.Min(0.99f, 35f * Time.fixedDeltaTime);
	}

	private static readonly float kInterval = 2f;

	private static readonly float kSmallScale = 0.6f;

	private static readonly float kLargeScale = 2f;

	private static readonly float kMoveDistance = 30f;

	private Vector3Spring m_spring;

	private float m_targetScale;

	private float m_lastTickTime;
}

using System;
using BoingKit;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
	public void Start()
	{
	}

	public void Update()
	{
		this.m_phase += OrbitCamera.kOrbitSpeed * MathUtil.TwoPi * Time.deltaTime;
		base.transform.position = new Vector3(-4f * Mathf.Cos(this.m_phase), 6f, 4f * Mathf.Sin(this.m_phase));
		base.transform.rotation = Quaternion.LookRotation((new Vector3(0f, 3f, 0f) - base.transform.position).normalized);
	}

	private static readonly float kOrbitSpeed = 0.01f;

	private float m_phase;
}

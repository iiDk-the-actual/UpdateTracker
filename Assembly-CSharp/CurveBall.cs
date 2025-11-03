using System;
using BoingKit;
using UnityEngine;

public class CurveBall : MonoBehaviour
{
	public void Reset()
	{
		float num = Random.Range(0f, MathUtil.TwoPi);
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		this.m_speedX = 40f * num2;
		this.m_speedZ = 40f * num3;
		this.m_timer = 0f;
		Vector3 position = base.transform.position;
		position.x = -10f * num2;
		position.z = -10f * num3;
		base.transform.position = position;
	}

	public void Start()
	{
		this.Reset();
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		if (this.m_timer > this.Interval)
		{
			this.Reset();
		}
		Vector3 position = base.transform.position;
		position.x += this.m_speedX * deltaTime;
		position.z += this.m_speedZ * deltaTime;
		base.transform.position = position;
		this.m_timer += deltaTime;
	}

	public float Interval = 2f;

	private float m_speedX;

	private float m_speedZ;

	private float m_timer;
}

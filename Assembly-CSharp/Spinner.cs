using System;
using UnityEngine;

public class Spinner : MonoBehaviour
{
	public void OnEnable()
	{
		this.m_angle = Random.Range(0f, 360f);
	}

	public void Update()
	{
		this.m_angle += this.Speed * 360f * Time.deltaTime;
		base.transform.rotation = Quaternion.Euler(0f, -this.m_angle, 0f);
	}

	public float Speed;

	private float m_angle;
}

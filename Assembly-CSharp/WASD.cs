using System;
using UnityEngine;

public class WASD : MonoBehaviour
{
	public Vector3 Velocity
	{
		get
		{
			return this.m_velocity;
		}
	}

	public void Update()
	{
		Vector3 zero = Vector3.zero;
		float num = 0f;
		if (Input.GetKey(KeyCode.W))
		{
			zero.z += 1f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			zero.x -= 1f;
		}
		if (Input.GetKey(KeyCode.S))
		{
			zero.z -= 1f;
		}
		if (Input.GetKey(KeyCode.D))
		{
			zero.x += 1f;
		}
		Vector3 vector = ((zero.sqrMagnitude > 0f) ? (zero.normalized * this.Speed * Time.deltaTime) : Vector3.zero);
		Quaternion quaternion = Quaternion.AngleAxis(num * this.Omega * 57.29578f * Time.deltaTime, Vector3.up);
		this.m_velocity = vector / Time.deltaTime;
		base.transform.position += vector;
		base.transform.rotation = quaternion * base.transform.rotation;
	}

	public float Speed = 1f;

	public float Omega = 1f;

	public Vector3 m_velocity;
}

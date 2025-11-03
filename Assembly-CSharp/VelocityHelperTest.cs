using System;
using UnityEngine;

public class VelocityHelperTest : MonoBehaviour
{
	private void Setup()
	{
		this.lastPosition = base.transform.position;
		this.lastVelocity = Vector3.zero;
		this.velocity = Vector3.zero;
		this.speed = 0f;
	}

	private void Start()
	{
		this.Setup();
	}

	private void FixedUpdate()
	{
		float deltaTime = Time.deltaTime;
		Vector3 position = base.transform.position;
		Vector3 vector = (position - this.lastPosition) / deltaTime;
		this.velocity = Vector3.Lerp(this.lastVelocity, vector, deltaTime);
		this.speed = this.velocity.magnitude;
		this.lastPosition = position;
		this.lastVelocity = vector;
	}

	private void Update()
	{
	}

	public Vector3 velocity;

	public float speed;

	[Space]
	public Vector3 lastVelocity;

	public Vector3 lastPosition;

	[Space]
	[SerializeField]
	private float[] _deltaTimes = new float[5];
}

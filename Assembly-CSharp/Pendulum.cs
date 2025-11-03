using System;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
	private void Start()
	{
		this.pendulum = (this.ClockPendulum = base.gameObject.GetComponent<Transform>());
	}

	private void Update()
	{
		if (this.pendulum)
		{
			float num = this.MaxAngleDeflection * Mathf.Sin(Time.time * this.SpeedOfPendulum);
			this.pendulum.localRotation = Quaternion.Euler(0f, 0f, num);
			return;
		}
	}

	public float MaxAngleDeflection = 10f;

	public float SpeedOfPendulum = 1f;

	public Transform ClockPendulum;

	private Transform pendulum;
}

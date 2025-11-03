using System;
using UnityEngine;
using UnityEngine.Events;

public class Decelerate : MonoBehaviour
{
	public void Restart()
	{
		base.enabled = true;
	}

	private void Update()
	{
		if (!this._rigidbody)
		{
			return;
		}
		Vector3 vector = this._rigidbody.linearVelocity;
		vector *= this._friction;
		if (vector.Approx0(0.001f))
		{
			this._rigidbody.linearVelocity = Vector3.zero;
			UnityEvent unityEvent = this.onStop;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			base.enabled = false;
		}
		else
		{
			this._rigidbody.linearVelocity = vector;
		}
		if (this._resetOrientationOnRelease && !this._rigidbody.rotation.Approx(Quaternion.identity, 1E-06f))
		{
			this._rigidbody.rotation = Quaternion.identity;
		}
	}

	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private float _friction = 0.875f;

	[SerializeField]
	private bool _resetOrientationOnRelease;

	public UnityEvent onStop;
}

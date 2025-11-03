using System;
using UnityEngine;

public class UIMatchRotation : MonoBehaviour
{
	private void Start()
	{
		this.referenceTransform = Camera.main.transform;
		base.transform.forward = this.x0z(this.referenceTransform.forward);
	}

	private void Update()
	{
		Vector3 vector = this.x0z(base.transform.forward);
		Vector3 vector2 = this.x0z(this.referenceTransform.forward);
		float num = Vector3.Dot(vector, vector2);
		UIMatchRotation.State state = this.state;
		if (state != UIMatchRotation.State.Ready)
		{
			if (state != UIMatchRotation.State.Rotating)
			{
				return;
			}
			base.transform.forward = Vector3.Lerp(base.transform.forward, vector2, Time.deltaTime * this.lerpSpeed);
			if (Vector3.Dot(base.transform.forward, vector2) > 0.995f)
			{
				this.state = UIMatchRotation.State.Ready;
			}
		}
		else if (num < 1f - this.threshold)
		{
			this.state = UIMatchRotation.State.Rotating;
			return;
		}
	}

	private Vector3 x0z(Vector3 vector)
	{
		vector.y = 0f;
		return vector.normalized;
	}

	[SerializeField]
	private Transform referenceTransform;

	[SerializeField]
	private float threshold = 0.35f;

	[SerializeField]
	private float lerpSpeed = 5f;

	private UIMatchRotation.State state;

	private enum State
	{
		Ready,
		Rotating
	}
}

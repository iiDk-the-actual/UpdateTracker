using System;
using UnityEngine;

public class PredicatableRandomRotation : MonoBehaviour
{
	private void Start()
	{
		if (this.source == null)
		{
			this.source = base.transform;
		}
	}

	private void Update()
	{
		float num = (this.source.position.x * this.source.position.x + this.source.position.y * this.source.position.y + this.source.position.z * this.source.position.z) % 1f;
		base.transform.Rotate(this.rot * num);
	}

	[SerializeField]
	private Vector3 rot = Vector3.zero;

	[SerializeField]
	private Transform source;
}

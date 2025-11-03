using System;
using UnityEngine;

[RequireComponent(typeof(GorillaVelocityEstimator))]
public class VelocityBasedActivator : MonoBehaviour
{
	private void Start()
	{
		this.velocityEstimator = base.GetComponent<GorillaVelocityEstimator>();
	}

	private void Update()
	{
		this.k += this.velocityEstimator.linearVelocity.sqrMagnitude;
		this.k = Mathf.Max(this.k - Time.deltaTime * this.decay, 0f);
		if (!this.active && this.k > this.threshold)
		{
			this.activate(true);
		}
		if (this.active && this.k < this.threshold)
		{
			this.activate(false);
		}
	}

	private void activate(bool v)
	{
		this.active = v;
		for (int i = 0; i < this.activationTargets.Length; i++)
		{
			this.activationTargets[i].SetActive(v);
		}
	}

	private void OnDisable()
	{
		if (this.active)
		{
			this.activate(false);
		}
	}

	[SerializeField]
	private GameObject[] activationTargets;

	private GorillaVelocityEstimator velocityEstimator;

	private float k;

	private bool active;

	[SerializeField]
	private float decay = 1f;

	[SerializeField]
	private float threshold = 1f;
}

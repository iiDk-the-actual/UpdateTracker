using System;
using UnityEngine;

public class SpeedDrivenAnim : MonoBehaviour
{
	private void Start()
	{
		this.velocityEstimator = base.GetComponent<GorillaVelocityEstimator>();
		this.animator = base.GetComponent<Animator>();
		this.keyHash = Animator.StringToHash(this.animKey);
	}

	private void Update()
	{
		float num = Mathf.InverseLerp(this.speed0, this.speed1, this.velocityEstimator.linearVelocity.magnitude);
		this.currentBlend = Mathf.MoveTowards(this.currentBlend, num, this.maxChangePerSecond * Time.deltaTime);
		this.animator.SetFloat(this.keyHash, this.currentBlend);
	}

	[SerializeField]
	private float speed0;

	[SerializeField]
	private float speed1 = 1f;

	[SerializeField]
	private float maxChangePerSecond = 1f;

	[SerializeField]
	private string animKey = "speed";

	private GorillaVelocityEstimator velocityEstimator;

	private Animator animator;

	private int keyHash;

	private float currentBlend;
}

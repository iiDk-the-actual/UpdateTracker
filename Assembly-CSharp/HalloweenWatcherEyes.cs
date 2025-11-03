using System;
using System.Collections;
using GorillaLocomotion;
using UnityEngine;

public class HalloweenWatcherEyes : MonoBehaviour
{
	private void Start()
	{
		this.playersViewCenterCosAngle = Mathf.Cos(this.playersViewCenterAngle * 0.017453292f);
		this.watchMinCosAngle = Mathf.Cos(this.watchMaxAngle * 0.017453292f);
		base.StartCoroutine(this.CheckIfNearPlayer(Random.Range(0f, this.timeBetweenUpdates)));
		base.enabled = false;
	}

	private IEnumerator CheckIfNearPlayer(float initialSleep)
	{
		yield return new WaitForSeconds(initialSleep);
		for (;;)
		{
			base.enabled = (base.transform.position - GTPlayer.Instance.transform.position).sqrMagnitude < this.watchRange * this.watchRange;
			if (!base.enabled)
			{
				this.LookNormal();
			}
			yield return new WaitForSeconds(this.timeBetweenUpdates);
		}
		yield break;
	}

	private void Update()
	{
		Vector3 normalized = (GTPlayer.Instance.headCollider.transform.position - base.transform.position).normalized;
		if (Vector3.Dot(GTPlayer.Instance.headCollider.transform.forward, -normalized) > this.playersViewCenterCosAngle)
		{
			this.LookNormal();
			this.pretendingToBeNormalUntilTimestamp = Time.time + this.durationToBeNormalWhenPlayerLooks;
		}
		if (this.pretendingToBeNormalUntilTimestamp > Time.time)
		{
			return;
		}
		if (Vector3.Dot(base.transform.forward, normalized) < this.watchMinCosAngle)
		{
			this.LookNormal();
			return;
		}
		Quaternion quaternion = Quaternion.LookRotation(normalized, base.transform.up);
		Quaternion quaternion2 = Quaternion.Lerp(base.transform.rotation, quaternion, this.lerpValue);
		this.leftEye.transform.rotation = quaternion2;
		this.rightEye.transform.rotation = quaternion2;
		if (this.lerpDuration > 0f)
		{
			this.lerpValue = Mathf.MoveTowards(this.lerpValue, 1f, Time.deltaTime / this.lerpDuration);
			return;
		}
		this.lerpValue = 1f;
	}

	private void LookNormal()
	{
		this.leftEye.transform.localRotation = Quaternion.identity;
		this.rightEye.transform.localRotation = Quaternion.identity;
		this.lerpValue = 0f;
	}

	public float timeBetweenUpdates = 5f;

	public float watchRange;

	public float watchMaxAngle;

	public float lerpDuration = 1f;

	public float playersViewCenterAngle = 30f;

	public float durationToBeNormalWhenPlayerLooks = 3f;

	public GameObject leftEye;

	public GameObject rightEye;

	private float playersViewCenterCosAngle;

	private float watchMinCosAngle;

	private float pretendingToBeNormalUntilTimestamp;

	private float lerpValue;
}

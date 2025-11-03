using System;
using UnityEngine;

public class CrankableToyCarDeployed : MonoBehaviour
{
	public void Deploy(CrankableToyCarHoldable holdable, Vector3 launchPos, Quaternion launchRot, Vector3 releaseVel, float lifetime, bool isRemote = false)
	{
		this.holdable = holdable;
		holdable.OnCarDeployed();
		base.transform.position = launchPos;
		base.transform.rotation = launchRot;
		base.transform.localScale = holdable.transform.lossyScale;
		this.rb.linearVelocity = releaseVel;
		this.startedAtTimestamp = Time.time;
		this.expiresAtTimestamp = Time.time + lifetime;
		this.isRemote = isRemote;
	}

	private void Update()
	{
		if (!this.isRemote && Time.time > this.expiresAtTimestamp)
		{
			if (this.holdable != null)
			{
				this.holdable.OnCarReturned();
			}
			return;
		}
		if (!this.wheelDriver.hasCollision)
		{
			this.expiresAtTimestamp -= Time.deltaTime;
			if (!this.offGroundDrivingAudio.isPlaying)
			{
				this.offGroundDrivingAudio.GTPlay();
				this.drivingAudio.Stop();
			}
		}
		else if (!this.drivingAudio.isPlaying)
		{
			this.drivingAudio.GTPlay();
			this.offGroundDrivingAudio.Stop();
		}
		float num = Mathf.InverseLerp(this.startedAtTimestamp, this.expiresAtTimestamp, Time.time);
		float num2 = this.thrustCurve.Evaluate(num);
		this.wheelDriver.SetThrust(this.maxThrust * num2);
	}

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private FakeWheelDriver wheelDriver;

	[SerializeField]
	private Vector3 maxThrust;

	[SerializeField]
	private AnimationCurve thrustCurve;

	private float startedAtTimestamp;

	private float expiresAtTimestamp;

	private CrankableToyCarHoldable holdable;

	[SerializeField]
	private AudioSource drivingAudio;

	[SerializeField]
	private AudioSource offGroundDrivingAudio;

	private bool isRemote;
}

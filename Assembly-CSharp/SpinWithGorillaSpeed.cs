using System;
using GorillaExtensions;
using UnityEngine;

public class SpinWithGorillaSpeed : MonoBehaviour
{
	private void Awake()
	{
		this.rig = base.GetComponentInParent<VRRig>();
		this.initialRotation = base.transform.localRotation;
		this.spinAxis = this.initialRotation * this.axisOfRotation * Vector3.forward;
	}

	private void Update()
	{
		Vector3 vector = ((this.optionalVelocityEstimator != null) ? this.optionalVelocityEstimator.linearVelocity : this.rig.LatestVelocity());
		vector.y *= this.verticalSpeedInfluence;
		float num = vector.magnitude / this.maxSpeed;
		float num2 = Time.deltaTime * this.degreesPerSecondAtSpeed.Evaluate(num) * (this.clockwise ? (-1f) : 1f);
		this.currentAngle = Mathf.Repeat(this.currentAngle + num2, 360f);
		Quaternion quaternion = this.initialRotation * Quaternion.AngleAxis(this.currentAngle, this.spinAxis);
		base.transform.SetLocalPositionAndRotation(quaternion * this.centerOfRotation, quaternion);
		if (this.tickSound != null && this.tickClips.Length != 0)
		{
			this.tickAngle += num2;
			if (this.tickAngle >= this.tickSoundDegrees)
			{
				this.tickSound.pitch = this.tickPitchAtSpeed.Evaluate(num);
				this.tickSound.volume = this.tickVolumeAtSpeed.Evaluate(num);
				this.tickSound.clip = this.tickClips.GetRandomItem<AudioClip>();
				this.tickSound.GTPlay();
				this.tickAngle = Mathf.Repeat(this.tickAngle, this.tickSoundDegrees);
			}
		}
	}

	private void OnDisable()
	{
		this.currentAngle = 0f;
		this.tickAngle = 0f;
	}

	[Tooltip("Get the velocity from this component when determining the spin speed. If this is unset, it will use the unsmoothed velocity of the parent VRRig component.")]
	[SerializeField]
	private GorillaVelocityEstimator optionalVelocityEstimator;

	[SerializeField]
	private Quaternion axisOfRotation = Quaternion.identity;

	[SerializeField]
	private Vector3 centerOfRotation = Vector3.zero;

	[Tooltip("The reported speed will be divided by this value before being used to sample AnimationCurves, to allow them to be in the range 0-1.")]
	[SerializeField]
	private float maxSpeed;

	[SerializeField]
	private AnimationCurve degreesPerSecondAtSpeed;

	[SerializeField]
	private bool clockwise;

	[Tooltip("The Y component of the reported speed will be multiplied by this value. At 0, falling will have no effect on the rotation speed.")]
	[SerializeField]
	private float verticalSpeedInfluence = 1f;

	[Header("Ticking sound")]
	[Tooltip("After this many degrees of rotation, a \"tick\" sound will play.")]
	[SerializeField]
	private float tickSoundDegrees = 360f;

	[SerializeField]
	private AnimationCurve tickVolumeAtSpeed;

	[SerializeField]
	private AnimationCurve tickPitchAtSpeed;

	[SerializeField]
	private AudioSource tickSound;

	[SerializeField]
	private AudioClip[] tickClips;

	private VRRig rig;

	private Quaternion initialRotation;

	private Vector3 spinAxis;

	private float currentAngle;

	private float tickAngle;
}

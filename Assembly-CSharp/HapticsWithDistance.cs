using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class HapticsWithDistance : MonoBehaviour, ITickSystemTick
{
	private bool OnWrongLayer()
	{
		return base.gameObject.layer != 18;
	}

	public void SetVibrationMult(float mult)
	{
		this.vibrationMult = mult;
	}

	public void FingerFlexVibrationMult(bool dummy, float mult)
	{
		this.SetVibrationMult(mult);
	}

	private void Awake()
	{
		this.inverseColliderRadius = 1f / base.GetComponent<SphereCollider>().radius;
	}

	private void OnTriggerEnter(Collider other)
	{
		GorillaGrabber gorillaGrabber;
		if (other.TryGetComponent<GorillaGrabber>(out gorillaGrabber) && gorillaGrabber.enabled)
		{
			if (gorillaGrabber.IsLeftHand)
			{
				this.leftOfflineHand = gorillaGrabber.transform;
				TickSystem<object>.AddTickCallback(this);
				return;
			}
			if (gorillaGrabber.IsRightHand)
			{
				this.rightOfflineHand = gorillaGrabber.transform;
				TickSystem<object>.AddTickCallback(this);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (this.leftOfflineHand == other.transform)
		{
			this.leftOfflineHand = null;
			if (!this.rightOfflineHand)
			{
				TickSystem<object>.RemoveTickCallback(this);
				return;
			}
		}
		else if (this.rightOfflineHand == other.transform)
		{
			this.rightOfflineHand = null;
			if (!this.leftOfflineHand)
			{
				TickSystem<object>.RemoveTickCallback(this);
			}
		}
	}

	private void OnDisable()
	{
		this.leftOfflineHand = null;
		this.rightOfflineHand = null;
		TickSystem<object>.RemoveTickCallback(this);
	}

	public bool TickRunning { get; set; }

	public void Tick()
	{
		Vector3 position = base.transform.position;
		if (this.leftOfflineHand)
		{
			GorillaTagger.Instance.StartVibration(true, this.vibrationMult * this.vibrationIntensityByDistance.Evaluate(Vector3.Distance(this.leftOfflineHand.position, position) * this.inverseColliderRadius), Time.deltaTime);
		}
		if (this.rightOfflineHand)
		{
			GorillaTagger.Instance.StartVibration(false, this.vibrationMult * this.vibrationIntensityByDistance.Evaluate(Vector3.Distance(this.rightOfflineHand.position, position) * this.inverseColliderRadius), Time.deltaTime);
		}
	}

	[SerializeField]
	[Tooltip("X is the normalized distance and should start at 0 and end at 1. Y is the vibration amplitude and can be anywhere from 0-1.")]
	private AnimationCurve vibrationIntensityByDistance;

	private float inverseColliderRadius;

	private float vibrationMult = 1f;

	private Transform leftOfflineHand;

	private Transform rightOfflineHand;
}

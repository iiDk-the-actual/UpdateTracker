using System;
using GorillaLocomotion.Climbing;
using Unity.Mathematics;
using UnityEngine;

namespace GorillaLocomotion.Gameplay
{
	public class GorillaZipline : MonoBehaviour
	{
		public float currentSpeed { get; private set; }

		protected void FindTFromDistance(ref float t, float distance, int steps = 1000)
		{
			float num = distance / (float)steps;
			Vector3 vector = this.spline.GetPointLocal(t);
			float num2 = 0f;
			for (int i = 0; i < 1000; i++)
			{
				t += num;
				if (t >= 1f || t <= 0f)
				{
					break;
				}
				Vector3 pointLocal = this.spline.GetPointLocal(t);
				num2 += Vector3.Distance(pointLocal, vector);
				if (num2 >= Mathf.Abs(distance))
				{
					break;
				}
				vector = pointLocal;
			}
		}

		private float FindSlideHelperSpot(Vector3 grabPoint)
		{
			int i = 0;
			int num = 200;
			float num2 = 0.001f;
			float num3 = 1f / (float)num;
			float3 @float = base.transform.InverseTransformPoint(grabPoint);
			float num4 = 0f;
			float num5 = float.PositiveInfinity;
			while (i < num)
			{
				float num6 = math.distancesq(this.spline.GetPointLocal(num2), @float);
				if (num6 < num5)
				{
					num5 = num6;
					num4 = num2;
				}
				num2 += num3;
				i++;
			}
			return num4;
		}

		protected virtual void Start()
		{
			this.spline = base.GetComponent<BezierSpline>();
			GorillaClimbable gorillaClimbable = this.slideHelper;
			gorillaClimbable.onBeforeClimb = (Action<GorillaHandClimber, GorillaClimbableRef>)Delegate.Combine(gorillaClimbable.onBeforeClimb, new Action<GorillaHandClimber, GorillaClimbableRef>(this.OnBeforeClimb));
		}

		private void OnDestroy()
		{
			GorillaClimbable gorillaClimbable = this.slideHelper;
			gorillaClimbable.onBeforeClimb = (Action<GorillaHandClimber, GorillaClimbableRef>)Delegate.Remove(gorillaClimbable.onBeforeClimb, new Action<GorillaHandClimber, GorillaClimbableRef>(this.OnBeforeClimb));
		}

		public Vector3 GetCurrentDirection()
		{
			return this.spline.GetDirection(this.currentT);
		}

		protected void OnBeforeClimb(GorillaHandClimber hand, GorillaClimbableRef climbRef)
		{
			bool flag = this.currentClimber == null;
			this.currentClimber = hand;
			if (climbRef)
			{
				this.climbOffsetHelper.SetParent(climbRef.transform);
				this.climbOffsetHelper.position = hand.transform.position;
				this.climbOffsetHelper.localPosition = new Vector3(0f, 0f, this.climbOffsetHelper.localPosition.z);
			}
			this.currentT = this.FindSlideHelperSpot(this.climbOffsetHelper.position);
			this.slideHelper.transform.localPosition = this.spline.GetPointLocal(this.currentT);
			if (flag)
			{
				Vector3 averagedVelocity = GTPlayer.Instance.AveragedVelocity;
				float num = Vector3.Dot(averagedVelocity.normalized, this.spline.GetDirection(this.currentT));
				this.currentSpeed = averagedVelocity.magnitude * num * this.currentInheritVelocityMulti;
			}
		}

		private void Update()
		{
			if (this.currentClimber)
			{
				Vector3 direction = this.spline.GetDirection(this.currentT);
				float num = Physics.gravity.y * direction.y * this.settings.gravityMulti;
				this.currentSpeed = Mathf.MoveTowards(this.currentSpeed, this.settings.maxSpeed, num * Time.deltaTime);
				float num2 = MathUtils.Linear(this.currentSpeed, 0f, this.settings.maxFrictionSpeed, this.settings.friction, this.settings.maxFriction);
				this.currentSpeed = Mathf.MoveTowards(this.currentSpeed, 0f, num2 * Time.deltaTime);
				this.currentSpeed = Mathf.Min(this.currentSpeed, this.settings.maxSpeed);
				this.currentSpeed = Mathf.Max(this.currentSpeed, -this.settings.maxSpeed);
				float num3 = Mathf.Abs(this.currentSpeed);
				this.FindTFromDistance(ref this.currentT, this.currentSpeed * Time.deltaTime, 1000);
				this.slideHelper.transform.localPosition = this.spline.GetPointLocal(this.currentT);
				if (!this.audioSlide.gameObject.activeSelf)
				{
					this.audioSlide.gameObject.SetActive(true);
				}
				this.audioSlide.volume = MathUtils.Linear(num3, 0f, this.settings.maxSpeed, this.settings.minSlideVolume, this.settings.maxSlideVolume);
				this.audioSlide.pitch = MathUtils.Linear(num3, 0f, this.settings.maxSpeed, this.settings.minSlidePitch, this.settings.maxSlidePitch);
				if (!this.audioSlide.isPlaying)
				{
					this.audioSlide.GTPlay();
				}
				float num4 = MathUtils.Linear(num3, 0f, this.settings.maxSpeed, -0.1f, 0.75f);
				if (num4 > 0f)
				{
					GorillaTagger.Instance.DoVibration(this.currentClimber.xrNode, num4, Time.deltaTime);
				}
				if (!this.spline.Loop)
				{
					if (this.currentT >= 1f || this.currentT <= 0f)
					{
						this.currentClimber.ForceStopClimbing(false, true);
					}
				}
				else if (this.currentT >= 1f)
				{
					this.currentT = 0f;
				}
				else if (this.currentT <= 0f)
				{
					this.currentT = 1f;
				}
				if (!this.slideHelper.isBeingClimbed)
				{
					this.Stop();
				}
			}
			if (this.currentInheritVelocityMulti < 1f)
			{
				this.currentInheritVelocityMulti += Time.deltaTime * 0.2f;
				this.currentInheritVelocityMulti = Mathf.Min(this.currentInheritVelocityMulti, 1f);
			}
		}

		private void Stop()
		{
			this.currentClimber = null;
			this.audioSlide.GTStop();
			this.audioSlide.gameObject.SetActive(false);
			this.currentInheritVelocityMulti = 0.55f;
			this.currentSpeed = 0f;
		}

		[SerializeField]
		protected Transform segmentsRoot;

		[SerializeField]
		protected GameObject segmentPrefab;

		[SerializeField]
		protected GorillaClimbable slideHelper;

		[SerializeField]
		private AudioSource audioSlide;

		protected BezierSpline spline;

		[SerializeField]
		private Transform climbOffsetHelper;

		[SerializeField]
		private GorillaZiplineSettings settings;

		[SerializeField]
		protected float ziplineDistance = 15f;

		[SerializeField]
		protected float segmentDistance = 0.9f;

		private GorillaHandClimber currentClimber;

		private float currentT;

		private const float inheritVelocityRechargeRate = 0.2f;

		private const float inheritVelocityValueOnRelease = 0.55f;

		private float currentInheritVelocityMulti = 1f;
	}
}

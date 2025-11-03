using System;
using Photon.Pun;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace GorillaLocomotion.Gameplay
{
	public class NoncontrollableBroomstick : MonoBehaviour, IGorillaGrabable
	{
		private void Start()
		{
			this.smoothRotationTrackingRateExp = Mathf.Exp(this.smoothRotationTrackingRate);
			this.progressPerFixedUpdate = Time.fixedDeltaTime / this.duration;
			this.progress = this.SplineProgressOffet;
			this.secondsToCycles = 1.0 / (double)this.duration;
			if (this.unitySpline != null)
			{
				this.nativeSpline = new NativeSpline(this.unitySpline.Spline, this.unitySpline.transform.localToWorldMatrix, Allocator.Persistent);
			}
		}

		protected virtual void FixedUpdate()
		{
			if (PhotonNetwork.InRoom)
			{
				double num = PhotonNetwork.Time * this.secondsToCycles + (double)this.SplineProgressOffet;
				this.progress = (float)(num % 1.0);
			}
			else
			{
				this.progress = (this.progress + this.progressPerFixedUpdate) % 1f;
			}
			Quaternion quaternion = Quaternion.identity;
			if (this.unitySpline != null)
			{
				float3 @float;
				float3 float2;
				float3 float3;
				this.nativeSpline.Evaluate(this.progress, out @float, out float2, out float3);
				base.transform.position = @float;
				if (this.lookForward)
				{
					quaternion = Quaternion.LookRotation(new Vector3(float2.x, float2.y, float2.z));
				}
			}
			else if (this.spline != null)
			{
				Vector3 point = this.spline.GetPoint(this.progress, this.constantVelocity);
				base.transform.position = point;
				if (this.lookForward)
				{
					quaternion = Quaternion.LookRotation(this.spline.GetDirection(this.progress, this.constantVelocity));
				}
			}
			if (this.lookForward)
			{
				base.transform.rotation = Quaternion.Slerp(quaternion, base.transform.rotation, Mathf.Exp(-this.smoothRotationTrackingRateExp * Time.deltaTime));
			}
		}

		bool IGorillaGrabable.CanBeGrabbed(GorillaGrabber grabber)
		{
			return true;
		}

		void IGorillaGrabable.OnGrabbed(GorillaGrabber g, out Transform grabbedObject, out Vector3 grabbedLocalPosition)
		{
			grabbedObject = base.transform;
			grabbedLocalPosition = base.transform.InverseTransformPoint(g.transform.position);
		}

		void IGorillaGrabable.OnGrabReleased(GorillaGrabber g)
		{
		}

		private void OnDestroy()
		{
			this.nativeSpline.Dispose();
		}

		public bool MomentaryGrabOnly()
		{
			return this.momentaryGrabOnly;
		}

		string IGorillaGrabable.get_name()
		{
			return base.name;
		}

		public SplineContainer unitySpline;

		public BezierSpline spline;

		public float duration = 30f;

		public float smoothRotationTrackingRate = 0.5f;

		public bool lookForward = true;

		[SerializeField]
		private float SplineProgressOffet;

		private float progress;

		private float smoothRotationTrackingRateExp;

		[SerializeField]
		private bool constantVelocity;

		private float progressPerFixedUpdate;

		private double secondsToCycles;

		private NativeSpline nativeSpline;

		[SerializeField]
		private bool momentaryGrabOnly = true;
	}
}

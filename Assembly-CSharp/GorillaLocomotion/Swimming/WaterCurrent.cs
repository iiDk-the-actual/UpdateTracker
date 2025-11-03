using System;
using System.Collections.Generic;
using AA;
using CjLib;
using UnityEngine;

namespace GorillaLocomotion.Swimming
{
	public class WaterCurrent : MonoBehaviour
	{
		public float Speed
		{
			get
			{
				return this.currentSpeed;
			}
		}

		public float Accel
		{
			get
			{
				return this.currentAccel;
			}
		}

		public float InwardSpeed
		{
			get
			{
				return this.inwardCurrentSpeed;
			}
		}

		public float InwardAccel
		{
			get
			{
				return this.inwardCurrentAccel;
			}
		}

		public bool GetCurrentAtPoint(Vector3 worldPoint, Vector3 startingVelocity, float dt, out Vector3 currentVelocity, out Vector3 velocityChange)
		{
			float num = (this.fullEffectDistance + this.fadeDistance) * (this.fullEffectDistance + this.fadeDistance);
			bool flag = false;
			velocityChange = Vector3.zero;
			currentVelocity = Vector3.zero;
			float num2 = 0.0001f;
			float magnitude = startingVelocity.magnitude;
			if (magnitude > num2)
			{
				Vector3 vector = startingVelocity / magnitude;
				float num3 = Spring.DamperDecayExact(magnitude, this.dampingHalfLife, dt, 1E-05f);
				Vector3 vector2 = vector * num3;
				velocityChange += vector2 - startingVelocity;
			}
			for (int i = 0; i < this.splines.Count; i++)
			{
				CatmullRomSpline catmullRomSpline = this.splines[i];
				Vector3 vector3;
				float closestEvaluationOnSpline = catmullRomSpline.GetClosestEvaluationOnSpline(worldPoint, out vector3);
				Vector3 vector4 = catmullRomSpline.Evaluate(closestEvaluationOnSpline);
				Vector3 vector5 = vector4 - worldPoint;
				if (vector5.sqrMagnitude < num)
				{
					flag = true;
					float magnitude2 = vector5.magnitude;
					float num4 = ((magnitude2 > this.fullEffectDistance) ? (1f - Mathf.Clamp01((magnitude2 - this.fullEffectDistance) / this.fadeDistance)) : 1f);
					float num5 = Mathf.Clamp01(closestEvaluationOnSpline + this.velocityAnticipationAdjustment);
					Vector3 forwardTangent = catmullRomSpline.GetForwardTangent(num5, 0.01f);
					if (this.currentSpeed > num2 && Vector3.Dot(startingVelocity, forwardTangent) < num4 * this.currentSpeed)
					{
						velocityChange += forwardTangent * (this.currentAccel * dt);
					}
					else if (this.currentSpeed < num2 && Vector3.Dot(startingVelocity, forwardTangent) > num4 * this.currentSpeed)
					{
						velocityChange -= forwardTangent * (this.currentAccel * dt);
					}
					currentVelocity += forwardTangent * num4 * this.currentSpeed;
					float num6 = Mathf.InverseLerp(this.inwardCurrentNoEffectRadius, this.inwardCurrentFullEffectRadius, magnitude2);
					if (num6 > num2)
					{
						vector3 = Vector3.ProjectOnPlane(vector5, forwardTangent);
						Vector3 normalized = vector3.normalized;
						if (this.inwardCurrentSpeed > num2 && Vector3.Dot(startingVelocity, normalized) < num6 * this.inwardCurrentSpeed)
						{
							velocityChange += normalized * (this.InwardAccel * dt);
						}
						else if (this.inwardCurrentSpeed < num2 && Vector3.Dot(startingVelocity, normalized) > num6 * this.inwardCurrentSpeed)
						{
							velocityChange -= normalized * (this.InwardAccel * dt);
						}
					}
					this.debugSplinePoint = vector4;
				}
			}
			this.debugCurrentVelocity = velocityChange.normalized;
			return flag;
		}

		private void Update()
		{
			if (this.debugDrawCurrentQueries)
			{
				DebugUtil.DrawSphere(this.debugSplinePoint, 0.15f, 12, 12, Color.green, false, DebugUtil.Style.Wireframe);
				DebugUtil.DrawArrow(this.debugSplinePoint, this.debugSplinePoint + this.debugCurrentVelocity, 0.1f, 0.1f, 12, 0.1f, Color.yellow, false, DebugUtil.Style.Wireframe);
			}
		}

		private void OnDrawGizmosSelected()
		{
			int num = 16;
			for (int i = 0; i < this.splines.Count; i++)
			{
				CatmullRomSpline catmullRomSpline = this.splines[i];
				Vector3 vector = catmullRomSpline.Evaluate(0f);
				for (int j = 1; j <= num; j++)
				{
					float num2 = (float)j / (float)num;
					Vector3 vector2 = catmullRomSpline.Evaluate(num2);
					vector2 - vector;
					Quaternion quaternion = Quaternion.LookRotation(catmullRomSpline.GetForwardTangent(num2, 0.01f), Vector3.up);
					Gizmos.color = new Color(0f, 0.5f, 0.75f);
					this.DrawGizmoCircle(vector2, quaternion, this.fullEffectDistance);
					Gizmos.color = new Color(0f, 0.25f, 0.5f);
					this.DrawGizmoCircle(vector2, quaternion, this.fullEffectDistance + this.fadeDistance);
				}
			}
		}

		private void DrawGizmoCircle(Vector3 center, Quaternion rotation, float radius)
		{
			Vector3 vector = Vector3.right * radius;
			int num = 16;
			for (int i = 1; i <= num; i++)
			{
				float num2 = (float)i / (float)num * 2f * 3.1415927f;
				Vector3 vector2 = new Vector3(Mathf.Cos(num2), Mathf.Sin(num2), 0f) * radius;
				Gizmos.DrawLine(center + rotation * vector, center + rotation * vector2);
				vector = vector2;
			}
		}

		[SerializeField]
		private List<CatmullRomSpline> splines = new List<CatmullRomSpline>();

		[SerializeField]
		private float fullEffectDistance = 1f;

		[SerializeField]
		private float fadeDistance = 0.5f;

		[SerializeField]
		private float currentSpeed = 1f;

		[SerializeField]
		private float currentAccel = 10f;

		[SerializeField]
		private float velocityAnticipationAdjustment = 0.05f;

		[SerializeField]
		private float inwardCurrentFullEffectRadius = 1f;

		[SerializeField]
		private float inwardCurrentNoEffectRadius = 0.25f;

		[SerializeField]
		private float inwardCurrentSpeed = 1f;

		[SerializeField]
		private float inwardCurrentAccel = 10f;

		[SerializeField]
		private float dampingHalfLife = 0.25f;

		[SerializeField]
		private bool debugDrawCurrentQueries;

		private Vector3 debugCurrentVelocity = Vector3.zero;

		private Vector3 debugSplinePoint = Vector3.zero;
	}
}

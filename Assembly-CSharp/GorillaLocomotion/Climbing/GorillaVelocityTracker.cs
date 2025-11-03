using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaLocomotion.Climbing
{
	public class GorillaVelocityTracker : MonoBehaviour, ITickSystemTick
	{
		public bool TickRunning { get; set; }

		public void ResetState()
		{
			this.trans = base.transform;
			this.localSpaceData = new GorillaVelocityTracker.VelocityDataPoint[this.maxDataPoints];
			this.<ResetState>g__PopulateArray|20_0(this.localSpaceData);
			this.worldSpaceData = new GorillaVelocityTracker.VelocityDataPoint[this.maxDataPoints];
			this.<ResetState>g__PopulateArray|20_0(this.worldSpaceData);
			this.isRelativeTo = this.relativeTo != null;
			this.lastLocalSpacePos = this.GetPosition(false);
			this.lastWorldSpacePos = this.GetPosition(true);
			this.wasAboveThreshold = false;
		}

		private void Awake()
		{
			this.ResetState();
		}

		private void OnEnable()
		{
			TickSystem<object>.AddTickCallback(this);
		}

		private void OnDisable()
		{
			this.ResetState();
			TickSystem<object>.RemoveTickCallback(this);
		}

		public void SetRelativeTo(Transform tf)
		{
			this.relativeTo = tf;
			this.isRelativeTo = tf != null;
		}

		private Vector3 GetPosition(bool worldSpace)
		{
			if (worldSpace)
			{
				return this.trans.position;
			}
			if (this.isRelativeTo)
			{
				return this.relativeTo.InverseTransformPoint(this.trans.position);
			}
			return this.trans.localPosition;
		}

		public void Tick()
		{
			if (Time.frameCount <= this.lastTickedFrame)
			{
				return;
			}
			Vector3 position = this.GetPosition(false);
			Vector3 position2 = this.GetPosition(true);
			GorillaVelocityTracker.VelocityDataPoint velocityDataPoint = this.localSpaceData[this.currentDataPointIndex];
			velocityDataPoint.delta = (position - this.lastLocalSpacePos) / Time.deltaTime;
			velocityDataPoint.time = Time.time;
			this.localSpaceData[this.currentDataPointIndex] = velocityDataPoint;
			GorillaVelocityTracker.VelocityDataPoint velocityDataPoint2 = this.worldSpaceData[this.currentDataPointIndex];
			velocityDataPoint2.delta = (position2 - this.lastWorldSpacePos) / Time.deltaTime;
			velocityDataPoint2.time = Time.time;
			this.worldSpaceData[this.currentDataPointIndex] = velocityDataPoint2;
			this.lastLocalSpacePos = position;
			this.lastWorldSpacePos = position2;
			this.currentDataPointIndex++;
			if (this.currentDataPointIndex >= this.maxDataPoints)
			{
				this.currentDataPointIndex = 0;
			}
			if (this.useVelocityEvents)
			{
				this.GetLatestVelocity(this.useWorldSpaceForEvents);
			}
			this.lastTickedFrame = Time.frameCount;
		}

		private void AddToQueue(ref List<GorillaVelocityTracker.VelocityDataPoint> dataPoints, GorillaVelocityTracker.VelocityDataPoint newData)
		{
			dataPoints.Add(newData);
			if (dataPoints.Count >= this.maxDataPoints)
			{
				dataPoints.RemoveAt(0);
			}
		}

		public Vector3 GetAverageVelocity(bool worldSpace = false, float maxTimeFromPast = 0.15f, bool doMagnitudeCheck = false)
		{
			float num = maxTimeFromPast / 2f;
			GorillaVelocityTracker.VelocityDataPoint[] array;
			if (worldSpace)
			{
				array = this.worldSpaceData;
			}
			else
			{
				array = this.localSpaceData;
			}
			if (array.Length <= 1)
			{
				return Vector3.zero;
			}
			GorillaVelocityTracker.<>c__DisplayClass28_0 CS$<>8__locals1;
			CS$<>8__locals1.total = Vector3.zero;
			CS$<>8__locals1.totalMag = 0f;
			CS$<>8__locals1.added = 0;
			float num2 = Time.time - maxTimeFromPast;
			float num3 = Time.time - num;
			int i = 0;
			int num4 = this.currentDataPointIndex;
			while (i < this.maxDataPoints)
			{
				GorillaVelocityTracker.VelocityDataPoint velocityDataPoint = array[num4];
				if (doMagnitudeCheck && CS$<>8__locals1.added > 1 && velocityDataPoint.time >= num3)
				{
					if (velocityDataPoint.delta.magnitude >= CS$<>8__locals1.totalMag / (float)CS$<>8__locals1.added)
					{
						GorillaVelocityTracker.<GetAverageVelocity>g__AddPoint|28_0(velocityDataPoint, ref CS$<>8__locals1);
					}
				}
				else if (velocityDataPoint.time >= num2)
				{
					GorillaVelocityTracker.<GetAverageVelocity>g__AddPoint|28_0(velocityDataPoint, ref CS$<>8__locals1);
				}
				num4++;
				if (num4 >= this.maxDataPoints)
				{
					num4 = 0;
				}
				i++;
			}
			if (CS$<>8__locals1.added > 0)
			{
				return CS$<>8__locals1.total / (float)CS$<>8__locals1.added;
			}
			return Vector3.zero;
		}

		public Vector3 GetLatestVelocity(bool worldSpace = false)
		{
			GorillaVelocityTracker.VelocityDataPoint[] array;
			if (worldSpace)
			{
				array = this.worldSpaceData;
			}
			else
			{
				array = this.localSpaceData;
			}
			if (array[this.currentDataPointIndex].delta.magnitude >= this.latestVelocityThreshold && !this.wasAboveThreshold)
			{
				UnityEvent onLatestAboveThreshold = this.OnLatestAboveThreshold;
				if (onLatestAboveThreshold != null)
				{
					onLatestAboveThreshold.Invoke();
				}
				this.wasAboveThreshold = true;
			}
			else if (array[this.currentDataPointIndex].delta.magnitude < this.latestVelocityThreshold && this.wasAboveThreshold)
			{
				UnityEvent onLatestBelowThreshold = this.OnLatestBelowThreshold;
				if (onLatestBelowThreshold != null)
				{
					onLatestBelowThreshold.Invoke();
				}
				this.wasAboveThreshold = false;
			}
			return array[this.currentDataPointIndex].delta;
		}

		public float GetAverageSpeedChangeMagnitudeInDirection(Vector3 dir, bool worldSpace = false, float maxTimeFromPast = 0.05f)
		{
			GorillaVelocityTracker.VelocityDataPoint[] array;
			if (worldSpace)
			{
				array = this.worldSpaceData;
			}
			else
			{
				array = this.localSpaceData;
			}
			if (array.Length <= 1)
			{
				return 0f;
			}
			float num = 0f;
			int num2 = 0;
			float num3 = Time.time - maxTimeFromPast;
			bool flag = false;
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].time >= num3)
				{
					if (!flag)
					{
						vector = array[i].delta;
						flag = true;
					}
					else
					{
						num += Mathf.Abs(Vector3.Dot(array[i].delta - vector, dir));
						num2++;
					}
				}
			}
			if (num2 <= 0)
			{
				return 0f;
			}
			return num / (float)num2;
		}

		[CompilerGenerated]
		private void <ResetState>g__PopulateArray|20_0(GorillaVelocityTracker.VelocityDataPoint[] array)
		{
			for (int i = 0; i < this.maxDataPoints; i++)
			{
				array[i] = new GorillaVelocityTracker.VelocityDataPoint();
			}
		}

		[CompilerGenerated]
		internal static void <GetAverageVelocity>g__AddPoint|28_0(GorillaVelocityTracker.VelocityDataPoint point, ref GorillaVelocityTracker.<>c__DisplayClass28_0 A_1)
		{
			A_1.total += point.delta;
			A_1.totalMag += point.delta.magnitude;
			int added = A_1.added;
			A_1.added = added + 1;
		}

		[SerializeField]
		private int maxDataPoints = 20;

		[SerializeField]
		private Transform relativeTo;

		[Tooltip("Use in Editor to trigger events when above or higher than a desired latest velocity.")]
		[SerializeField]
		private bool useVelocityEvents;

		[SerializeField]
		private float latestVelocityThreshold;

		public UnityEvent OnLatestBelowThreshold;

		public UnityEvent OnLatestAboveThreshold;

		[SerializeField]
		private bool useWorldSpaceForEvents;

		private bool wasAboveThreshold;

		private int currentDataPointIndex;

		private GorillaVelocityTracker.VelocityDataPoint[] localSpaceData;

		private GorillaVelocityTracker.VelocityDataPoint[] worldSpaceData;

		private Transform trans;

		private Vector3 lastWorldSpacePos;

		private Vector3 lastLocalSpacePos;

		private bool isRelativeTo;

		private int lastTickedFrame = -1;

		public class VelocityDataPoint
		{
			public Vector3 delta;

			public float time = -1f;
		}
	}
}

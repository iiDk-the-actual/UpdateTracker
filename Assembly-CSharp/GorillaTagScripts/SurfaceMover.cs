using System;
using GorillaTagScripts.Builder;
using GT_CustomMapSupportRuntime;
using Photon.Pun;
using UnityEngine;

namespace GorillaTagScripts
{
	public class SurfaceMover : MonoBehaviour
	{
		private void Start()
		{
			MovingSurfaceManager.instance == null;
			MovingSurfaceManager.instance.RegisterSurfaceMover(this);
		}

		private void OnDestroy()
		{
			if (MovingSurfaceManager.instance != null)
			{
				MovingSurfaceManager.instance.UnregisterSurfaceMover(this);
			}
		}

		public void InitMovingSurface()
		{
			if (this.moveType == BuilderMovingPart.BuilderMovingPartType.Translation)
			{
				this.distance = Vector3.Distance(this.endXf.position, this.startXf.position);
				float num = this.distance / this.velocity;
				this.cycleDuration = num + this.cycleDelay;
			}
			else
			{
				if (this.rotationRelativeToStarting)
				{
					this.startingRotation = base.transform.localRotation.eulerAngles;
				}
				this.cycleDuration = this.rotationAmount / 360f / this.velocity;
				this.cycleDuration += this.cycleDelay;
			}
			float num2 = this.cycleDelay / this.cycleDuration;
			Vector2 vector = new Vector2(num2 / 2f, 0f);
			Vector2 vector2 = new Vector2(1f - num2 / 2f, 1f);
			float num3 = (vector2.y - vector.y) / (vector2.x - vector.x);
			this.lerpAlpha = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(num2 / 2f, 0f, 0f, num3),
				new Keyframe(1f - num2 / 2f, 1f, num3, 0f)
			});
			this.currT = this.startPercentage;
			uint num4 = (uint)(this.cycleDuration * 1000f);
			if (num4 == 0U)
			{
				num4 = 1U;
			}
			uint num5 = 2147483648U % num4;
			uint num6 = (uint)(this.startPercentage * num4);
			if (num6 >= num5)
			{
				this.startPercentageCycleOffset = num6 - num5;
				return;
			}
			this.startPercentageCycleOffset = num6 + num4 + num4 - num5;
		}

		private long NetworkTimeMs()
		{
			if (PhotonNetwork.InRoom)
			{
				return (long)((ulong)(PhotonNetwork.ServerTimestamp + (int)this.startPercentageCycleOffset + int.MinValue));
			}
			return (long)(Time.time * 1000f);
		}

		private long CycleLengthMs()
		{
			return (long)(this.cycleDuration * 1000f);
		}

		public double PlatformTime()
		{
			long num = this.NetworkTimeMs();
			long num2 = this.CycleLengthMs();
			return (double)(num - num / num2 * num2) / 1000.0;
		}

		public int CycleCount()
		{
			return (int)(this.NetworkTimeMs() / this.CycleLengthMs());
		}

		public float CycleCompletionPercent()
		{
			return Mathf.Clamp((float)(this.PlatformTime() / (double)this.cycleDuration), 0f, 1f);
		}

		public bool IsEvenCycle()
		{
			return this.CycleCount() % 2 == 0;
		}

		public void Move()
		{
			this.Progress();
			BuilderMovingPart.BuilderMovingPartType builderMovingPartType = this.moveType;
			if (builderMovingPartType == BuilderMovingPart.BuilderMovingPartType.Translation)
			{
				base.transform.localPosition = this.UpdatePointToPoint(this.percent);
				return;
			}
			if (builderMovingPartType != BuilderMovingPart.BuilderMovingPartType.Rotation)
			{
				return;
			}
			this.UpdateRotation(this.percent);
		}

		private Vector3 UpdatePointToPoint(float perc)
		{
			float num = this.lerpAlpha.Evaluate(perc);
			return Vector3.Lerp(this.startXf.localPosition, this.endXf.localPosition, num);
		}

		private void UpdateRotation(float perc)
		{
			float num = this.lerpAlpha.Evaluate(perc) * this.rotationAmount;
			if (this.rotationRelativeToStarting)
			{
				Vector3 vector = this.startingRotation;
				switch (this.rotationAxis)
				{
				case RotationAxis.X:
					vector.x += num;
					break;
				case RotationAxis.Y:
					vector.y += num;
					break;
				case RotationAxis.Z:
					vector.z += num;
					break;
				}
				base.transform.localRotation = Quaternion.Euler(vector);
				return;
			}
			switch (this.rotationAxis)
			{
			case RotationAxis.X:
				base.transform.localRotation = Quaternion.AngleAxis(num, Vector3.right);
				return;
			case RotationAxis.Y:
				base.transform.localRotation = Quaternion.AngleAxis(num, Vector3.up);
				return;
			case RotationAxis.Z:
				base.transform.localRotation = Quaternion.AngleAxis(num, Vector3.forward);
				return;
			default:
				return;
			}
		}

		private void Progress()
		{
			this.currT = this.CycleCompletionPercent();
			this.currForward = this.IsEvenCycle();
			this.percent = this.currT;
			if (this.reverseDirOnCycle)
			{
				this.percent = (this.currForward ? this.currT : (1f - this.currT));
			}
			if (this.reverseDir)
			{
				this.percent = 1f - this.percent;
			}
		}

		public void CopySettings(SurfaceMoverSettings settings)
		{
			this.moveType = (BuilderMovingPart.BuilderMovingPartType)settings.moveType;
			this.startPercentage = 0f;
			this.velocity = Math.Clamp(settings.velocity, 0.001f, Math.Abs(settings.velocity));
			this.reverseDirOnCycle = settings.reverseDirOnCycle;
			this.reverseDir = settings.reverseDir;
			this.cycleDelay = Math.Clamp(settings.cycleDelay, 0f, Math.Abs(settings.cycleDelay));
			this.startXf = settings.start;
			this.endXf = settings.end;
			this.rotationAxis = (RotationAxis)settings.rotationAxis;
			this.rotationAmount = Math.Clamp(settings.rotationAmount, 0.001f, Math.Abs(settings.rotationAmount));
			this.rotationRelativeToStarting = settings.rotationRelativeToStarting;
		}

		[SerializeField]
		private BuilderMovingPart.BuilderMovingPartType moveType;

		[SerializeField]
		private float startPercentage = 0.5f;

		[SerializeField]
		private float velocity;

		[SerializeField]
		private bool reverseDirOnCycle = true;

		[SerializeField]
		private bool reverseDir;

		[SerializeField]
		private float cycleDelay = 0.25f;

		[SerializeField]
		protected Transform startXf;

		[SerializeField]
		protected Transform endXf;

		[SerializeField]
		public RotationAxis rotationAxis = RotationAxis.Y;

		[SerializeField]
		public float rotationAmount = 360f;

		[SerializeField]
		public bool rotationRelativeToStarting;

		private AnimationCurve lerpAlpha;

		private float cycleDuration;

		private float distance;

		private Vector3 startingRotation;

		private float currT;

		private float percent;

		private bool currForward;

		private float dtSinceServerUpdate;

		private int lastServerTimeStamp;

		private float rotateStartAmt;

		private float rotateAmt;

		private uint startPercentageCycleOffset;
	}
}

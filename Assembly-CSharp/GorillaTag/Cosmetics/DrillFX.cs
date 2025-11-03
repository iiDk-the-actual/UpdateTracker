using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class DrillFX : MonoBehaviour
	{
		protected void Awake()
		{
			if (!DrillFX.appIsQuittingHandlerIsSubscribed)
			{
				DrillFX.appIsQuittingHandlerIsSubscribed = true;
				Application.quitting += DrillFX.HandleApplicationQuitting;
			}
			this.hasFX = this.fx != null;
			if (this.hasFX)
			{
				this.fxEmissionModule = this.fx.emission;
				this.fxEmissionMaxRate = this.fxEmissionModule.rateOverTimeMultiplier;
				this.fxShapeModule = this.fx.shape;
				this.fxShapeMaxRadius = this.fxShapeModule.radius;
			}
			this.hasAudio = this.loopAudio != null;
			if (this.hasAudio)
			{
				this.audioMaxVolume = this.loopAudio.volume;
				this.loopAudio.volume = 0f;
				this.loopAudio.loop = true;
				this.loopAudio.GTPlay();
			}
		}

		protected void OnEnable()
		{
			if (DrillFX.appIsQuitting)
			{
				return;
			}
			if (this.hasFX)
			{
				this.fxEmissionModule.rateOverTimeMultiplier = 0f;
			}
			if (this.hasAudio)
			{
				this.loopAudio.volume = 0f;
				this.loopAudio.loop = true;
				this.loopAudio.GTPlay();
			}
			this.ValidateLineCastPositions();
		}

		protected void OnDisable()
		{
			if (DrillFX.appIsQuitting)
			{
				return;
			}
			if (this.hasFX)
			{
				this.fxEmissionModule.rateOverTimeMultiplier = 0f;
			}
			if (this.hasAudio)
			{
				this.loopAudio.volume = 0f;
				this.loopAudio.GTStop();
			}
		}

		protected void LateUpdate()
		{
			if (DrillFX.appIsQuitting)
			{
				return;
			}
			Transform transform = base.transform;
			RaycastHit raycastHit;
			Vector3 vector = (Physics.Linecast(transform.TransformPoint(this.lineCastStart), transform.TransformPoint(this.lineCastEnd), out raycastHit, this.lineCastLayerMask, QueryTriggerInteraction.Ignore) ? raycastHit.point : this.lineCastEnd);
			Vector3 vector2 = transform.InverseTransformPoint(vector);
			float num = Mathf.Clamp01(Vector3.Distance(this.lineCastStart, vector2) / this.maxDepth);
			if (this.hasFX)
			{
				this.fxEmissionModule.rateOverTimeMultiplier = this.fxEmissionMaxRate * this.fxEmissionCurve.Evaluate(num);
				this.fxShapeModule.position = vector2;
				this.fxShapeModule.radius = Mathf.Lerp(this.fxShapeMaxRadius, this.fxMinRadiusScale * this.fxShapeMaxRadius, num);
			}
			if (this.hasAudio)
			{
				this.loopAudio.volume = Mathf.MoveTowards(this.loopAudio.volume, this.audioMaxVolume * this.loopAudioVolumeCurve.Evaluate(num), this.loopAudioVolumeTransitionSpeed * Time.deltaTime);
			}
		}

		private static void HandleApplicationQuitting()
		{
			DrillFX.appIsQuitting = true;
		}

		private bool ValidateLineCastPositions()
		{
			this.maxDepth = Vector3.Distance(this.lineCastStart, this.lineCastEnd);
			if (this.maxDepth > 1E-45f)
			{
				return true;
			}
			if (Application.isPlaying)
			{
				Debug.Log("DrillFX: lineCastStart and End are too close together. Disabling component.", this);
				base.enabled = false;
			}
			return false;
		}

		[SerializeField]
		private ParticleSystem fx;

		[SerializeField]
		private AnimationCurve fxEmissionCurve;

		[SerializeField]
		private float fxMinRadiusScale = 0.01f;

		[Tooltip("Right click menu has custom menu items. Anything starting with \"- \" is custom.")]
		[SerializeField]
		private AudioSource loopAudio;

		[SerializeField]
		private AnimationCurve loopAudioVolumeCurve;

		[Tooltip("Higher value makes it reach the target volume faster.")]
		[SerializeField]
		private float loopAudioVolumeTransitionSpeed = 3f;

		[FormerlySerializedAs("layerMask")]
		[Tooltip("The collision layers the line cast should intersect with")]
		[SerializeField]
		private LayerMask lineCastLayerMask;

		[Tooltip("The position in local space that the line cast starts.")]
		[SerializeField]
		private Vector3 lineCastStart = Vector3.zero;

		[Tooltip("The position in local space that the line cast ends.")]
		[SerializeField]
		private Vector3 lineCastEnd = Vector3.forward;

		private static bool appIsQuitting;

		private static bool appIsQuittingHandlerIsSubscribed;

		private float maxDepth;

		private bool hasFX;

		private ParticleSystem.EmissionModule fxEmissionModule;

		private float fxEmissionMaxRate;

		private ParticleSystem.ShapeModule fxShapeModule;

		private float fxShapeMaxRadius;

		private bool hasAudio;

		private float audioMaxVolume;
	}
}

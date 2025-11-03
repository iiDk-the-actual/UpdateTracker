using System;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace GorillaTagScripts.Builder
{
	public class BuilderSmallHandTrigger : MonoBehaviour
	{
		public bool TriggeredThisFrame
		{
			get
			{
				return this.lastTriggeredFrame == Time.frameCount;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!base.enabled)
			{
				return;
			}
			GorillaTriggerColliderHandIndicator componentInParent = other.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
			if (componentInParent == null)
			{
				return;
			}
			if (!this.hasCheckedZone)
			{
				BuilderTable builderTable;
				if (BuilderTable.TryGetBuilderTableForZone(VRRigCache.Instance.localRig.Rig.zoneEntity.currentZone, out builderTable))
				{
					this.ignoreScale = !builderTable.isTableMutable;
				}
				this.hasCheckedZone = true;
			}
			if (this.onlySmallHands && !this.ignoreScale && (double)VRRigCache.Instance.localRig.Rig.scaleFactor > 0.99)
			{
				return;
			}
			if (this.requireMinimumVelocity)
			{
				float num = this.minimumVelocityMagnitude * GorillaTagger.Instance.offlineVRRig.scaleFactor;
				if (GTPlayer.Instance.GetHandVelocityTracker(componentInParent.isLeftHand).GetAverageVelocity(true, 0.1f, false).sqrMagnitude < num * num)
				{
					return;
				}
			}
			GorillaTagger.Instance.StartVibration(componentInParent.isLeftHand, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration * 1.5f);
			this.lastTriggeredFrame = Time.frameCount;
			UnityEvent triggeredEvent = this.TriggeredEvent;
			if (triggeredEvent != null)
			{
				triggeredEvent.Invoke();
			}
			if (this.timeline != null && (this.timeline.time == 0.0 || this.timeline.time >= this.timeline.duration))
			{
				this.timeline.Play();
			}
			if (this.animation != null && this.animation.clip != null)
			{
				this.animation.Play();
			}
		}

		[Tooltip("Optional timeline to play to animate the thing getting activated, play sound, particles, etc...")]
		public PlayableDirector timeline;

		[Tooltip("Optional animation to play")]
		public Animation animation;

		private int lastTriggeredFrame = -1;

		public bool onlySmallHands;

		[SerializeField]
		protected bool requireMinimumVelocity;

		[SerializeField]
		protected float minimumVelocityMagnitude = 0.1f;

		private bool hasCheckedZone;

		private bool ignoreScale;

		internal UnityEvent TriggeredEvent = new UnityEvent();

		[SerializeField]
		private BuilderPiece myPiece;
	}
}

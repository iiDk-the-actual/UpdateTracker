using System;
using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class KnockbackTrigger : MonoBehaviour
	{
		public bool TriggeredThisFrame
		{
			get
			{
				return this.lastTriggeredFrame == Time.frameCount;
			}
		}

		private void CheckZone()
		{
			if (!this.hasCheckedZone)
			{
				BuilderTable builderTable;
				if (BuilderTable.TryGetBuilderTableForZone(VRRigCache.Instance.localRig.Rig.zoneEntity.currentZone, out builderTable))
				{
					this.ignoreScale = !builderTable.isTableMutable;
				}
				this.hasCheckedZone = true;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.gameObject.IsOnLayer(UnityLayer.GorillaBodyCollider) && !other.gameObject.IsOnLayer(UnityLayer.GorillaHead) && !other.gameObject.IsOnLayer(UnityLayer.GorillaHand))
			{
				return;
			}
			this.CheckZone();
			if (!this.ignoreScale && this.onlySmallMonke && (double)VRRigCache.Instance.localRig.Rig.scaleFactor > 0.99)
			{
				return;
			}
			this.collidersEntered.Add(other);
			if (this.collidersEntered.Count > 1)
			{
				return;
			}
			Vector3 vector = this.triggerVolume.ClosestPoint(GorillaTagger.Instance.headCollider.transform.position);
			Vector3 vector2 = vector - base.transform.TransformPoint(this.triggerVolume.center);
			vector2 -= Vector3.Project(vector2, base.transform.TransformDirection(this.localAxis));
			float magnitude = vector2.magnitude;
			Vector3 vector3 = Vector3.up;
			if (magnitude >= 0.01f)
			{
				vector3 = vector2 / magnitude;
			}
			GTPlayer.Instance.SetMaximumSlipThisFrame();
			GTPlayer.Instance.ApplyKnockback(vector3, this.knockbackVelocity * VRRigCache.Instance.localRig.Rig.scaleFactor, false);
			if (this.impactFX != null)
			{
				ObjectPools.instance.Instantiate(this.impactFX, vector, true);
			}
			GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength / 2f, Time.fixedDeltaTime);
			GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 2f, Time.fixedDeltaTime);
			this.lastTriggeredFrame = Time.frameCount;
		}

		private void OnTriggerExit(Collider other)
		{
			if (!other.gameObject.IsOnLayer(UnityLayer.GorillaBodyCollider) && !other.gameObject.IsOnLayer(UnityLayer.GorillaHead) && !other.gameObject.IsOnLayer(UnityLayer.GorillaHand))
			{
				return;
			}
			this.collidersEntered.Remove(other);
		}

		private void OnDisable()
		{
			this.collidersEntered.Clear();
		}

		[SerializeField]
		private BoxCollider triggerVolume;

		[SerializeField]
		private float knockbackVelocity;

		[SerializeField]
		private Vector3 localAxis;

		[SerializeField]
		private GameObject impactFX;

		[SerializeField]
		private bool onlySmallMonke;

		private bool hasCheckedZone;

		private bool ignoreScale;

		private int lastTriggeredFrame = -1;

		private List<Collider> collidersEntered = new List<Collider>(4);
	}
}

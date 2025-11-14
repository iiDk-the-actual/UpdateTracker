using System;
using UnityEngine;
using UnityEngine.Events;

namespace Cosmetics
{
	public class CosmeticsLocalHandReactor : MonoBehaviour
	{
		protected void Awake()
		{
			this.ownerRig = base.GetComponentInParent<VRRig>();
			if (this.ownerRig == null)
			{
				GorillaTagger componentInParent = base.GetComponentInParent<GorillaTagger>();
				if (componentInParent != null)
				{
					this.ownerRig = componentInParent.offlineVRRig;
					this.ownerIsLocal = this.ownerRig != null;
				}
			}
			if (this.ownerRig == null)
			{
				Debug.LogError("TriggerToggler: Disabling cannot find VRRig.");
				base.enabled = false;
				return;
			}
		}

		protected void LateUpdate()
		{
			if (this.ownerIsLocal)
			{
				if (Time.time < this.lastTriggerTime + this.cooldownTime)
				{
					return;
				}
				Transform transform = base.transform;
				if (Physics.OverlapSphereNonAlloc(base.transform.position, this.proximityThreshold * transform.lossyScale.x, this.colliders, this.handLayer) > 0)
				{
					GorillaTriggerColliderHandIndicator component = this.colliders[0].GetComponent<GorillaTriggerColliderHandIndicator>();
					if (component != null)
					{
						GorillaTagger.Instance.StartVibration(component.isLeftHand, this.hapticStrength, this.hapticDuration);
						UnityEvent<bool> unityEvent = this.onTrigger;
						if (unityEvent != null)
						{
							unityEvent.Invoke(component.isLeftHand);
						}
						this.lastTriggerTime = Time.time;
					}
				}
			}
		}

		[SerializeField]
		private float hapticStrength = 0.2f;

		[SerializeField]
		private float hapticDuration = 0.2f;

		[Tooltip("The distance threshold (in meters) for triggering the interaction.\nIf the hand enters this range, onTrigger is fired.")]
		public float proximityThreshold = 0.15f;

		[Tooltip("Minimum time (in seconds) between consecutive triggers.\n")]
		[SerializeField]
		private float cooldownTime = 0.5f;

		public UnityEvent<bool> onTrigger;

		private VRRig ownerRig;

		private bool ownerIsLocal;

		private float lastTriggerTime = float.MinValue;

		private readonly Collider[] colliders = new Collider[1];

		private LayerMask handLayer = 1024;
	}
}

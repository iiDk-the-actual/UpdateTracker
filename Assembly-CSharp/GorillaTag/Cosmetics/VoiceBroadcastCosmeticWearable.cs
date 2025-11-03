using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class VoiceBroadcastCosmeticWearable : MonoBehaviour, IGorillaSliceableSimple
	{
		private void Start()
		{
			VoiceBroadcastCosmetic[] componentsInChildren = base.GetComponentInParent<VRRig>().GetComponentsInChildren<VoiceBroadcastCosmetic>(true);
			this.voiceBroadcasters = new List<VoiceBroadcastCosmetic>();
			foreach (VoiceBroadcastCosmetic voiceBroadcastCosmetic in componentsInChildren)
			{
				if (voiceBroadcastCosmetic.talkingCosmeticType == this.talkingCosmeticType)
				{
					this.voiceBroadcasters.Add(voiceBroadcastCosmetic);
					voiceBroadcastCosmetic.SetWearable(this);
				}
			}
		}

		public void OnEnable()
		{
			if (this.playerHeadCollider == null)
			{
				VRRig componentInParent = base.GetComponentInParent<VRRig>();
				this.playerHeadCollider = ((componentInParent != null) ? componentInParent.rigContainer.HeadCollider : null);
			}
			if (this.headDistanceActivation && this.playerHeadCollider != null)
			{
				GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
			}
		}

		public void OnDisable()
		{
			GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
		}

		public void SliceUpdate()
		{
			if (Time.time - this.lastToggleTime >= this.toggleCooldown)
			{
				bool flag = (base.transform.position - this.playerHeadCollider.transform.position).sqrMagnitude <= this.headDistance * this.headDistance;
				if (flag != this.toggleState)
				{
					this.toggleState = flag;
					this.lastToggleTime = Time.time;
					if (flag)
					{
						UnityEvent unityEvent = this.onStartListening;
						if (unityEvent != null)
						{
							unityEvent.Invoke();
						}
					}
					else
					{
						UnityEvent unityEvent2 = this.onStopListening;
						if (unityEvent2 != null)
						{
							unityEvent2.Invoke();
						}
					}
					for (int i = 0; i < this.voiceBroadcasters.Count; i++)
					{
						this.voiceBroadcasters[i].SetListenState(flag);
					}
				}
			}
		}

		public void OnCosmeticStartListening()
		{
			if (this.headDistanceActivation)
			{
				return;
			}
			UnityEvent unityEvent = this.onStartListening;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		public void OnCosmeticStopListening()
		{
			if (this.headDistanceActivation)
			{
				return;
			}
			UnityEvent unityEvent = this.onStopListening;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		public TalkingCosmeticType talkingCosmeticType;

		[SerializeField]
		private bool headDistanceActivation = true;

		[SerializeField]
		private float headDistance = 0.4f;

		[SerializeField]
		private float toggleCooldown = 0.5f;

		private bool toggleState;

		private float lastToggleTime;

		[SerializeField]
		private UnityEvent onStartListening;

		[SerializeField]
		private UnityEvent onStopListening;

		private List<VoiceBroadcastCosmetic> voiceBroadcasters;

		private Collider playerHeadCollider;
	}
}

using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	[RequireComponent(typeof(Collider))]
	public class OnTriggerEventsCosmetic : MonoBehaviour
	{
		private bool IsMyItem()
		{
			return this.rig != null && this.rig.isOfflineVRRig;
		}

		private void Awake()
		{
			Collider[] components = base.GetComponents<Collider>();
			if (components == null || components.Length == 0)
			{
				Debug.LogError("OnTriggerEventsCosmetic requires at least one Collider on the same GameObject.");
				base.enabled = false;
				return;
			}
			bool flag = false;
			foreach (Collider collider in components)
			{
				if (collider != null && (collider.isTrigger || collider.attachedRigidbody != null))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Debug.LogWarning("OnTriggerEventsCosmetic: Collider is not set to Trigger. OnTrigger will not fire. Path=" + base.transform.GetPathQ(), base.transform);
			}
			this.rig = base.GetComponentInParent<VRRig>();
			if (this.rig == null && base.gameObject.GetComponentInParent<GTPlayer>() != null)
			{
				this.rig = GorillaTagger.Instance.offlineVRRig;
			}
			this.parentTransferable = base.GetComponentInParent<TransferrableObject>();
			List<OnTriggerEventsCosmetic.Listener> list = new List<OnTriggerEventsCosmetic.Listener>();
			List<OnTriggerEventsCosmetic.Listener> list2 = new List<OnTriggerEventsCosmetic.Listener>();
			List<OnTriggerEventsCosmetic.Listener> list3 = new List<OnTriggerEventsCosmetic.Listener>();
			if (this.eventListeners != null)
			{
				for (int j = 0; j < this.eventListeners.Length; j++)
				{
					OnTriggerEventsCosmetic.Listener listener = this.eventListeners[j];
					if (listener.tagSet == null)
					{
						if (listener.triggerTagsList != null && listener.triggerTagsList.Count > 0)
						{
							listener.tagSet = new HashSet<string>(listener.triggerTagsList);
						}
						else
						{
							listener.tagSet = new HashSet<string>();
						}
					}
					if (listener.eventType == OnTriggerEventsCosmetic.EventType.TriggerEnter)
					{
						list.Add(listener);
					}
					else if (listener.eventType == OnTriggerEventsCosmetic.EventType.TriggerStay)
					{
						list2.Add(listener);
					}
					else if (listener.eventType == OnTriggerEventsCosmetic.EventType.TriggerExit)
					{
						list3.Add(listener);
					}
				}
			}
			this.enterListeners = ((list.Count > 0) ? list.ToArray() : Array.Empty<OnTriggerEventsCosmetic.Listener>());
			this.stayListeners = ((list2.Count > 0) ? list2.ToArray() : Array.Empty<OnTriggerEventsCosmetic.Listener>());
			this.exitListeners = ((list3.Count > 0) ? list3.ToArray() : Array.Empty<OnTriggerEventsCosmetic.Listener>());
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!OnTriggerEventsCosmetic.IsOtherUsable(other))
			{
				return;
			}
			this.Dispatch(this.enterListeners, other);
		}

		private void OnTriggerStay(Collider other)
		{
			if (!OnTriggerEventsCosmetic.IsOtherUsable(other))
			{
				return;
			}
			this.Dispatch(this.stayListeners, other);
		}

		private void OnTriggerExit(Collider other)
		{
			if (!OnTriggerEventsCosmetic.IsOtherUsable(other))
			{
				return;
			}
			this.Dispatch(this.exitListeners, other);
		}

		private static bool IsOtherUsable(Collider other)
		{
			if (other == null)
			{
				return false;
			}
			GameObject gameObject = other.gameObject;
			return !(gameObject == null) && gameObject.activeInHierarchy;
		}

		private void Dispatch(OnTriggerEventsCosmetic.Listener[] listeners, Collider other)
		{
			if (listeners == null || listeners.Length == 0)
			{
				return;
			}
			int layer = other.gameObject.layer;
			bool flag = this.parentTransferable && this.parentTransferable.InLeftHand();
			Vector3 vector = ((this.myCollider != null) ? this.myCollider.bounds.center : base.transform.position);
			foreach (OnTriggerEventsCosmetic.Listener listener in listeners)
			{
				if ((listener.syncForEveryoneInRoom || this.IsMyItem()) && (!listener.fireOnlyWhileHeld || !this.parentTransferable || this.parentTransferable.InHand()) && (listener.tagSet == null || listener.tagSet.Count <= 0 || OnTriggerEventsCosmetic.CompareTagAny(other.gameObject, listener.tagSet)) && ((1 << layer) & listener.triggerLayerMask.value) != 0)
				{
					UnityEvent<bool, Collider> listenerComponent = listener.listenerComponent;
					if (listenerComponent != null)
					{
						listenerComponent.Invoke(flag, other);
					}
					Vector3 vector2 = other.ClosestPoint(vector);
					UnityEvent<Vector3> listenerComponentContactPoint = listener.listenerComponentContactPoint;
					if (listenerComponentContactPoint != null)
					{
						listenerComponentContactPoint.Invoke(vector2);
					}
					VRRig componentInParent = other.GetComponentInParent<VRRig>();
					if (componentInParent != null)
					{
						UnityEvent<VRRig> onTriggeredVRRig = listener.onTriggeredVRRig;
						if (onTriggeredVRRig != null)
						{
							onTriggeredVRRig.Invoke(componentInParent);
						}
					}
				}
			}
		}

		private static bool CompareTagAny(GameObject go, HashSet<string> tagSet)
		{
			if (tagSet == null || tagSet.Count == 0)
			{
				return true;
			}
			foreach (string text in tagSet)
			{
				if (!string.IsNullOrEmpty(text) && go.CompareTag(text))
				{
					return true;
				}
			}
			return false;
		}

		private bool IsTagValid(GameObject obj, OnTriggerEventsCosmetic.Listener listener)
		{
			return listener == null || (listener.tagSet == null || listener.tagSet.Count == 0) || OnTriggerEventsCosmetic.CompareTagAny(obj, listener.tagSet);
		}

		[Tooltip("List of per-condition listeners. Each entry specifies when (Enter/Stay/Exit), what to trigger with (layers/tags), and which UnityEvents to fire.")]
		public OnTriggerEventsCosmetic.Listener[] eventListeners = new OnTriggerEventsCosmetic.Listener[0];

		private OnTriggerEventsCosmetic.Listener[] enterListeners = Array.Empty<OnTriggerEventsCosmetic.Listener>();

		private OnTriggerEventsCosmetic.Listener[] stayListeners = Array.Empty<OnTriggerEventsCosmetic.Listener>();

		private OnTriggerEventsCosmetic.Listener[] exitListeners = Array.Empty<OnTriggerEventsCosmetic.Listener>();

		private Collider myCollider;

		private VRRig rig;

		private TransferrableObject parentTransferable;

		[Serializable]
		public class Listener
		{
			[Tooltip("Only trigger interactions with objects on these layers.")]
			public LayerMask triggerLayerMask;

			[Tooltip("Optional tag whitelist. If non-empty, triggers must match at least one of these tags.")]
			public List<string> triggerTagsList = new List<string>();

			[Tooltip("Choose which trigger phase invokes this listener: Enter, Stay, or Exit.")]
			public OnTriggerEventsCosmetic.EventType eventType;

			public UnityEvent<bool, Collider> listenerComponent;

			public UnityEvent<Vector3> listenerComponentContactPoint;

			public UnityEvent<VRRig> onTriggeredVRRig;

			[Tooltip("If true, fire for everyone in the room. If false, only fire when this item is owned locally (offline rig).")]
			public bool syncForEveryoneInRoom = true;

			[Tooltip("If true, only fire while this item is held. Requires a TransferrableObject on this object or a parent.")]
			public bool fireOnlyWhileHeld = true;

			[NonSerialized]
			public HashSet<string> tagSet;
		}

		public enum EventType
		{
			TriggerEnter,
			TriggerStay,
			TriggerExit
		}
	}
}

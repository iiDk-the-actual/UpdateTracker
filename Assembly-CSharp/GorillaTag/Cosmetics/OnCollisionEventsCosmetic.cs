using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	[RequireComponent(typeof(Collider))]
	public class OnCollisionEventsCosmetic : MonoBehaviour
	{
		private bool IsMyItem()
		{
			return this.rig != null && this.rig.isOfflineVRRig;
		}

		private void Awake()
		{
			this.myCollider = base.GetComponent<Collider>();
			if (this.myCollider == null)
			{
				Debug.LogError("OnCollisionEventsCosmetic requires a Collider on the same GameObject.");
				base.enabled = false;
				return;
			}
			if (this.myCollider.isTrigger)
			{
				Debug.LogWarning("OnCollisionEventsCosmetic: Collider is set to Trigger. OnCollision will not fire. Set it to non-trigger for collisions.");
			}
			this.rig = base.GetComponentInParent<VRRig>();
			this.parentTransferable = base.GetComponentInParent<TransferrableObject>();
			List<OnCollisionEventsCosmetic.Listener> list = new List<OnCollisionEventsCosmetic.Listener>();
			List<OnCollisionEventsCosmetic.Listener> list2 = new List<OnCollisionEventsCosmetic.Listener>();
			List<OnCollisionEventsCosmetic.Listener> list3 = new List<OnCollisionEventsCosmetic.Listener>();
			if (this.eventListeners != null)
			{
				for (int i = 0; i < this.eventListeners.Length; i++)
				{
					OnCollisionEventsCosmetic.Listener listener = this.eventListeners[i];
					if (listener.tagSet == null)
					{
						if (listener.collisionTagsList != null && listener.collisionTagsList.Count > 0)
						{
							listener.tagSet = new HashSet<string>(listener.collisionTagsList);
						}
						else
						{
							listener.tagSet = new HashSet<string>();
						}
					}
					if (listener.eventType == OnCollisionEventsCosmetic.EventType.CollisionEnter)
					{
						list.Add(listener);
					}
					else if (listener.eventType == OnCollisionEventsCosmetic.EventType.CollisionStay)
					{
						list2.Add(listener);
					}
					else if (listener.eventType == OnCollisionEventsCosmetic.EventType.CollisionExit)
					{
						list3.Add(listener);
					}
				}
			}
			this.enterListeners = ((list.Count > 0) ? list.ToArray() : Array.Empty<OnCollisionEventsCosmetic.Listener>());
			this.stayListeners = ((list2.Count > 0) ? list2.ToArray() : Array.Empty<OnCollisionEventsCosmetic.Listener>());
			this.exitListeners = ((list3.Count > 0) ? list3.ToArray() : Array.Empty<OnCollisionEventsCosmetic.Listener>());
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!OnCollisionEventsCosmetic.IsCollisionUsable(collision))
			{
				return;
			}
			this.Dispatch(this.enterListeners, collision);
		}

		private void OnCollisionStay(Collision collision)
		{
			if (!OnCollisionEventsCosmetic.IsCollisionUsable(collision))
			{
				return;
			}
			this.Dispatch(this.stayListeners, collision);
		}

		private void OnCollisionExit(Collision collision)
		{
			if (!OnCollisionEventsCosmetic.IsCollisionUsable(collision))
			{
				return;
			}
			this.Dispatch(this.exitListeners, collision);
		}

		private static bool IsCollisionUsable(Collision collision)
		{
			if (collision == null)
			{
				return false;
			}
			Collider collider = collision.collider;
			if (collider == null)
			{
				return false;
			}
			GameObject gameObject = collider.gameObject;
			return !(gameObject == null) && gameObject.activeInHierarchy;
		}

		private void Dispatch(OnCollisionEventsCosmetic.Listener[] listeners, Collision collision)
		{
			if (listeners == null || listeners.Length == 0)
			{
				return;
			}
			Collider collider = collision.collider;
			GameObject gameObject = ((collider != null) ? collider.gameObject : null);
			if (gameObject == null)
			{
				return;
			}
			int layer = gameObject.layer;
			bool flag = this.parentTransferable && this.parentTransferable.InLeftHand();
			Vector3 vector = ((this.myCollider != null) ? this.myCollider.bounds.center : base.transform.position);
			Vector3 vector2;
			if (collision.contactCount > 0)
			{
				vector2 = collision.GetContact(0).point;
			}
			else
			{
				vector2 = collider.ClosestPoint(vector);
			}
			foreach (OnCollisionEventsCosmetic.Listener listener in listeners)
			{
				if ((listener.syncForEveryoneInRoom || this.IsMyItem()) && (!listener.fireOnlyWhileHeld || !this.parentTransferable || this.parentTransferable.InHand()) && (listener.tagSet == null || listener.tagSet.Count <= 0 || OnCollisionEventsCosmetic.CompareTagAny(gameObject, listener.tagSet)) && ((1 << layer) & listener.collisionLayerMask.value) != 0)
				{
					if (listener.listenerComponent != null)
					{
						listener.listenerComponent.Invoke(flag, collision);
					}
					if (listener.listenerComponentContactPoint != null)
					{
						listener.listenerComponentContactPoint.Invoke(vector2);
					}
					VRRig componentInParent = gameObject.GetComponentInParent<VRRig>();
					if (componentInParent != null && listener.onCollidedVRRig != null)
					{
						listener.onCollidedVRRig.Invoke(componentInParent);
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

		private bool IsTagValid(GameObject obj, OnCollisionEventsCosmetic.Listener listener)
		{
			return listener == null || (listener.tagSet == null || listener.tagSet.Count == 0) || OnCollisionEventsCosmetic.CompareTagAny(obj, listener.tagSet);
		}

		[Tooltip("List of per-condition listeners. Each entry specifies when (Enter/Stay/Exit), what to collide with (layers/tags), and which UnityEvents to fire.")]
		public OnCollisionEventsCosmetic.Listener[] eventListeners = new OnCollisionEventsCosmetic.Listener[0];

		private OnCollisionEventsCosmetic.Listener[] enterListeners = Array.Empty<OnCollisionEventsCosmetic.Listener>();

		private OnCollisionEventsCosmetic.Listener[] stayListeners = Array.Empty<OnCollisionEventsCosmetic.Listener>();

		private OnCollisionEventsCosmetic.Listener[] exitListeners = Array.Empty<OnCollisionEventsCosmetic.Listener>();

		private Collider myCollider;

		private VRRig rig;

		private TransferrableObject parentTransferable;

		[Serializable]
		public class Listener
		{
			[Tooltip("Only collisions with objects on these layers will be considered.")]
			public LayerMask collisionLayerMask;

			[Tooltip("Optional tag whitelist. If non-empty, collisions must match at least one of these tags.")]
			public List<string> collisionTagsList = new List<string>();

			[Tooltip("Choose which collision phase triggers this listener: Enter, Stay, or Exit.")]
			public OnCollisionEventsCosmetic.EventType eventType;

			public UnityEvent<bool, Collision> listenerComponent;

			public UnityEvent<Vector3> listenerComponentContactPoint;

			public UnityEvent<VRRig> onCollidedVRRig;

			[Tooltip("If true, fire for everyone in the room. If false, only fire when this item is owned locally (offline rig).")]
			public bool syncForEveryoneInRoom = true;

			[Tooltip("If true, only fire while this item is held. Requires a TransferrableObject on this object or a parent.")]
			public bool fireOnlyWhileHeld = true;

			[NonSerialized]
			public HashSet<string> tagSet;
		}

		public enum EventType
		{
			CollisionEnter,
			CollisionStay,
			CollisionExit
		}
	}
}

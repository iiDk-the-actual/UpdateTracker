using System;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class FingerFlexEvent : MonoBehaviourTick
	{
		private void Awake()
		{
			this._rig = base.GetComponentInParent<VRRig>();
			this.parentTransferable = base.GetComponentInParent<TransferrableObject>();
		}

		private bool IsMyItem()
		{
			return this._rig != null && this._rig.isOfflineVRRig;
		}

		public override void Tick()
		{
			for (int i = 0; i < this.eventListeners.Length; i++)
			{
				FingerFlexEvent.Listener listener = this.eventListeners[i];
				this.FireEvents(listener);
			}
		}

		private void FireEvents(FingerFlexEvent.Listener listener)
		{
			if (!listener.syncForEveryoneInRoom && !this.IsMyItem())
			{
				return;
			}
			if (!this.ignoreTransferable && listener.fireOnlyWhileHeld && this.parentTransferable && !this.parentTransferable.InHand() && listener.eventType == FingerFlexEvent.EventType.OnFingerReleased)
			{
				if (listener.fingerRightLastValue > listener.fingerReleaseValue)
				{
					UnityEvent<bool, float> listenerComponent = listener.listenerComponent;
					if (listenerComponent != null)
					{
						listenerComponent.Invoke(false, 0f);
					}
					listener.fingerRightLastValue = 0f;
				}
				if (listener.fingerLeftLastValue > listener.fingerReleaseValue)
				{
					UnityEvent<bool, float> listenerComponent2 = listener.listenerComponent;
					if (listenerComponent2 != null)
					{
						listenerComponent2.Invoke(true, 0f);
					}
					listener.fingerLeftLastValue = 0f;
				}
			}
			if (!this.ignoreTransferable && this.parentTransferable && listener.fireOnlyWhileHeld && !this.parentTransferable.InHand())
			{
				return;
			}
			switch (this.fingerType)
			{
			case FingerFlexEvent.FingerType.Thumb:
			{
				float calcT = this._rig.leftThumb.calcT;
				float calcT2 = this._rig.rightThumb.calcT;
				this.FireEvents(listener, calcT, calcT2);
				return;
			}
			case FingerFlexEvent.FingerType.Index:
			{
				float calcT3 = this._rig.leftIndex.calcT;
				float calcT4 = this._rig.rightIndex.calcT;
				this.FireEvents(listener, calcT3, calcT4);
				return;
			}
			case FingerFlexEvent.FingerType.Middle:
			{
				float calcT5 = this._rig.leftMiddle.calcT;
				float calcT6 = this._rig.rightMiddle.calcT;
				this.FireEvents(listener, calcT5, calcT6);
				return;
			}
			case FingerFlexEvent.FingerType.IndexAndMiddleMin:
			{
				float num = Mathf.Min(this._rig.leftIndex.calcT, this._rig.leftMiddle.calcT);
				float num2 = Mathf.Min(this._rig.rightIndex.calcT, this._rig.rightMiddle.calcT);
				this.FireEvents(listener, num, num2);
				return;
			}
			default:
				return;
			}
		}

		private void FireEvents(FingerFlexEvent.Listener listener, float leftFinger, float rightFinger)
		{
			if ((this.ignoreTransferable && listener.checkLeftHand) || (this.parentTransferable && this.FingerFlexValidation(true)))
			{
				this.CheckFingerValue(listener, leftFinger, true, ref listener.fingerLeftLastValue);
				return;
			}
			if ((this.ignoreTransferable && !listener.checkLeftHand) || (this.parentTransferable && this.FingerFlexValidation(false)))
			{
				this.CheckFingerValue(listener, rightFinger, false, ref listener.fingerRightLastValue);
				return;
			}
			this.CheckFingerValue(listener, leftFinger, true, ref listener.fingerLeftLastValue);
			this.CheckFingerValue(listener, rightFinger, false, ref listener.fingerRightLastValue);
		}

		private void CheckFingerValue(FingerFlexEvent.Listener listener, float fingerValue, bool isLeft, ref float lastValue)
		{
			if (fingerValue > listener.fingerFlexValue)
			{
				listener.frameCounter++;
			}
			switch (listener.eventType)
			{
			case FingerFlexEvent.EventType.OnFingerFlexed:
				if (fingerValue > listener.fingerFlexValue && lastValue < listener.fingerFlexValue)
				{
					UnityEvent<bool, float> listenerComponent = listener.listenerComponent;
					if (listenerComponent != null)
					{
						listenerComponent.Invoke(isLeft, fingerValue);
					}
				}
				break;
			case FingerFlexEvent.EventType.OnFingerReleased:
				if (fingerValue <= listener.fingerReleaseValue && lastValue > listener.fingerReleaseValue)
				{
					UnityEvent<bool, float> listenerComponent2 = listener.listenerComponent;
					if (listenerComponent2 != null)
					{
						listenerComponent2.Invoke(isLeft, fingerValue);
					}
					listener.frameCounter = 0;
				}
				break;
			case FingerFlexEvent.EventType.OnFingerFlexStayed:
				if (fingerValue > listener.fingerFlexValue && lastValue >= listener.fingerFlexValue && listener.frameCounter % listener.frameInterval == 0)
				{
					UnityEvent<bool, float> listenerComponent3 = listener.listenerComponent;
					if (listenerComponent3 != null)
					{
						listenerComponent3.Invoke(isLeft, fingerValue);
					}
					listener.frameCounter = 0;
				}
				break;
			}
			lastValue = fingerValue;
		}

		private bool FingerFlexValidation(bool isLeftHand)
		{
			return (!this.parentTransferable.InLeftHand() || isLeftHand) && (this.parentTransferable.InLeftHand() || !isLeftHand);
		}

		[SerializeField]
		public bool ignoreTransferable;

		[SerializeField]
		private FingerFlexEvent.FingerType fingerType = FingerFlexEvent.FingerType.Index;

		public FingerFlexEvent.Listener[] eventListeners = new FingerFlexEvent.Listener[0];

		private VRRig _rig;

		private TransferrableObject parentTransferable;

		[Serializable]
		public class Listener
		{
			public FingerFlexEvent.EventType eventType;

			public UnityEvent<bool, float> listenerComponent;

			public float fingerFlexValue = 0.75f;

			public float fingerReleaseValue = 0.01f;

			[Tooltip("How many frames should pass to fire a finger flex stayed event")]
			public int frameInterval = 20;

			[Tooltip("This event will be fired for everyone in the room (synced) by default unless you uncheck this box so that it will be fired only for the local player.")]
			public bool syncForEveryoneInRoom = true;

			[Tooltip("Fire these events only when the item is held in hand, only works if there is a transferable component somewhere on the object or its parent.")]
			public bool fireOnlyWhileHeld = true;

			[Tooltip("Whether to check the left hand or the right hand, only works if \"ignoreTransferable\" is true.")]
			public bool checkLeftHand;

			internal int frameCounter;

			internal float fingerRightLastValue;

			internal float fingerLeftLastValue;
		}

		public enum EventType
		{
			OnFingerFlexed,
			OnFingerReleased,
			OnFingerFlexStayed
		}

		private enum FingerType
		{
			Thumb,
			Index,
			Middle,
			IndexAndMiddleMin
		}
	}
}

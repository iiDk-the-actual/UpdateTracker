using System;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class BackpackGrabbableCosmetic : HoldableObject
	{
		private void Awake()
		{
			this.currentItemsCount = this.startItemsCount;
			this.canGrab = true;
		}

		public override void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
		{
		}

		public override void DropItemCleanup()
		{
		}

		public void Update()
		{
			if (!this.canGrab && Time.time - this.lastGrabTime >= this.coolDownTimer)
			{
				this.canGrab = true;
			}
		}

		public override void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
		{
			if (this.IsEmpty())
			{
				Debug.LogWarning("Can't remove item, Backpack is empty, need to refill.");
				return;
			}
			if (!this.canGrab)
			{
				return;
			}
			this.lastGrabTime = Time.time;
			this.canGrab = false;
			SnowballThrowable snowballThrowable;
			((grabbingHand == EquipmentInteractor.instance.leftHand) ? SnowballMaker.leftHandInstance : SnowballMaker.rightHandInstance).TryCreateSnowball(this.materialIndex, out snowballThrowable);
			this.RemoveItem();
		}

		public void AddItem()
		{
			if (!this.useCapacity)
			{
				return;
			}
			if (this.maxCapacity <= this.currentItemsCount)
			{
				Debug.LogWarning("Can't add item, backpack is at full capacity.");
				return;
			}
			this.currentItemsCount++;
			this.UpdateState();
		}

		public void RemoveItem()
		{
			if (!this.useCapacity)
			{
				return;
			}
			if (this.currentItemsCount < 0)
			{
				Debug.LogWarning("Can't remove item, Backpack is empty.");
				return;
			}
			this.currentItemsCount--;
			this.UpdateState();
		}

		public void RefillBackpack()
		{
			if (!this.useCapacity)
			{
				return;
			}
			if (this.currentItemsCount == this.startItemsCount)
			{
				return;
			}
			this.currentItemsCount = this.startItemsCount;
			this.UpdateState();
		}

		public void EmptyBackpack()
		{
			if (!this.useCapacity)
			{
				return;
			}
			if (this.currentItemsCount == 0)
			{
				return;
			}
			this.currentItemsCount = 0;
			this.UpdateState();
		}

		public bool IsFull()
		{
			return !this.useCapacity || this.maxCapacity == this.currentItemsCount;
		}

		public bool IsEmpty()
		{
			return this.useCapacity && this.currentItemsCount == 0;
		}

		private void UpdateState()
		{
			if (!this.useCapacity)
			{
				return;
			}
			if (this.currentItemsCount == this.maxCapacity)
			{
				UnityEvent onReachedMaxCapacity = this.OnReachedMaxCapacity;
				if (onReachedMaxCapacity == null)
				{
					return;
				}
				onReachedMaxCapacity.Invoke();
				return;
			}
			else
			{
				if (this.currentItemsCount != 0)
				{
					if (this.currentItemsCount == this.startItemsCount)
					{
						UnityEvent onRefilled = this.OnRefilled;
						if (onRefilled == null)
						{
							return;
						}
						onRefilled.Invoke();
					}
					return;
				}
				UnityEvent onFullyEmptied = this.OnFullyEmptied;
				if (onFullyEmptied == null)
				{
					return;
				}
				onFullyEmptied.Invoke();
				return;
			}
		}

		[GorillaSoundLookup]
		public int materialIndex;

		[SerializeField]
		private bool useCapacity = true;

		[SerializeField]
		private float coolDownTimer = 2f;

		[SerializeField]
		private int maxCapacity;

		[SerializeField]
		private int startItemsCount;

		[Space]
		public UnityEvent OnReachedMaxCapacity;

		public UnityEvent OnFullyEmptied;

		public UnityEvent OnRefilled;

		private int currentItemsCount;

		private bool canGrab;

		private float lastGrabTime;
	}
}

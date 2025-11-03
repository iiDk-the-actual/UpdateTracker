using System;
using GorillaNetworking;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class NetworkedWearable : MonoBehaviour, ISpawnable, ITickSystemTick
	{
		private void Awake()
		{
			if (this.assignedSlot != CosmeticsController.CosmeticCategory.Paw)
			{
				this.isTwoHanded = false;
			}
			this.wearableSlot = this.CosmeticCategoryToWearableSlot(this.assignedSlot, true);
			this.leftSlot = this.CosmeticCategoryToWearableSlot(this.assignedSlot, true);
			this.rightSlot = this.CosmeticCategoryToWearableSlot(this.assignedSlot, false);
		}

		private void OnEnable()
		{
			if (!this.IsSpawned)
			{
				return;
			}
			if (this.isLocal && !this.listenForChangesLocal)
			{
				this.SetWearableStateBool(this.startTrue);
				return;
			}
			if (!this.TickRunning)
			{
				TickSystem<object>.AddTickCallback(this);
			}
		}

		public void ToggleWearableStateBool()
		{
			if (!this.isLocal || !this.IsSpawned)
			{
				return;
			}
			if (!NetworkedWearable.IsCategoryValid(this.assignedSlot))
			{
				return;
			}
			if (this.myRig == null)
			{
				return;
			}
			if (this.listenForChangesLocal)
			{
				GTDev.LogError<string>("NetworkedWearable with listenForChangesLocal calling ToggleWearableStateBool on object " + base.gameObject.name + ".You should not change state from a listener", null);
				return;
			}
			if (this.assignedSlot == CosmeticsController.CosmeticCategory.Paw && this.isTwoHanded)
			{
				GTDev.LogWarning<string>("NetworkedWearable calling ToggleWearableStateBool on two handed object " + base.gameObject.name + ". please use ToggleLeftWearableStateBool or ToggleRightWearableStateBool instead", null);
				this.ToggleLeftWearableStateBool();
				return;
			}
			this.value = !this.value;
			this.myRig.WearablePackedStates = GTBitOps.WriteBit(this.myRig.WearablePackedStates, (int)this.wearableSlot, this.value);
			this.OnWearableStateChanged();
		}

		public void SetWearableStateBool(bool newState)
		{
			if (!this.isLocal || !this.IsSpawned)
			{
				return;
			}
			if (!NetworkedWearable.IsCategoryValid(this.assignedSlot))
			{
				return;
			}
			if (this.myRig == null)
			{
				return;
			}
			if (this.listenForChangesLocal)
			{
				GTDev.LogError<string>("NetworkedWearable with listenForChangesLocal calling SetWearableStateBool on object " + base.gameObject.name + ".You should not change state from a listener", null);
				return;
			}
			if (this.assignedSlot == CosmeticsController.CosmeticCategory.Paw && this.isTwoHanded)
			{
				GTDev.LogWarning<string>("NetworkedWearable calling SetWearableStateBool on two handed object " + base.gameObject.name + ". please use SetLeftWearableStateBool or SetRightWearableStateBool instead", null);
				this.SetLeftWearableStateBool(newState);
				return;
			}
			if (this.value != newState)
			{
				this.value = newState;
				this.myRig.WearablePackedStates = GTBitOps.WriteBit(this.myRig.WearablePackedStates, (int)this.wearableSlot, this.value);
				this.OnWearableStateChanged();
			}
		}

		public void ToggleLeftWearableStateBool()
		{
			if (!this.isLocal || !this.IsSpawned)
			{
				return;
			}
			if (!NetworkedWearable.IsCategoryValid(this.assignedSlot))
			{
				return;
			}
			if (this.myRig == null)
			{
				return;
			}
			if (this.listenForChangesLocal)
			{
				GTDev.LogError<string>("NetworkedWearable with listenForChangesLocal calling ToggleLeftWearableStateBool on object " + base.gameObject.name + ".You should not change state from a listener", null);
				return;
			}
			if (this.assignedSlot != CosmeticsController.CosmeticCategory.Paw || !this.isTwoHanded)
			{
				GTDev.LogWarning<string>("NetworkedWearable calling ToggleLeftWearableStateBool on one handed object " + base.gameObject.name + ". Please use ToggleWearableStateBool instead", null);
				this.ToggleWearableStateBool();
				return;
			}
			this.leftHandValue = !this.leftHandValue;
			this.myRig.WearablePackedStates = GTBitOps.WriteBit(this.myRig.WearablePackedStates, (int)this.leftSlot, this.leftHandValue);
			this.OnLeftStateChanged();
		}

		public void ToggleRightWearableStateBool()
		{
			if (!this.isLocal || !this.IsSpawned)
			{
				return;
			}
			if (!NetworkedWearable.IsCategoryValid(this.assignedSlot))
			{
				return;
			}
			if (this.myRig == null)
			{
				return;
			}
			if (this.listenForChangesLocal)
			{
				GTDev.LogError<string>("NetworkedWearable with listenForChangesLocal calling ToggleRightWearableStateBool on object " + base.gameObject.name + ".You should not change state from a listener", null);
				return;
			}
			if (this.assignedSlot != CosmeticsController.CosmeticCategory.Paw || !this.isTwoHanded)
			{
				GTDev.LogWarning<string>("NetworkedWearable calling ToggleRightWearableStateBool on one handed object " + base.gameObject.name + ". Please use ToggleWearableStateBool instead", null);
				this.ToggleWearableStateBool();
				return;
			}
			this.rightHandValue = !this.rightHandValue;
			this.myRig.WearablePackedStates = GTBitOps.WriteBit(this.myRig.WearablePackedStates, (int)this.rightSlot, this.rightHandValue);
			this.OnRightStateChanged();
		}

		public void SetLeftWearableStateBool(bool newState)
		{
			if (!this.isLocal || !this.IsSpawned)
			{
				return;
			}
			if (!NetworkedWearable.IsCategoryValid(this.assignedSlot))
			{
				return;
			}
			if (this.myRig == null)
			{
				return;
			}
			if (this.listenForChangesLocal)
			{
				GTDev.LogError<string>("NetworkedWearable with listenForChangesLocal calling SetLeftWearableStateBool on object " + base.gameObject.name + ".You should not change state from a listener", null);
				return;
			}
			if (this.assignedSlot != CosmeticsController.CosmeticCategory.Paw || !this.isTwoHanded)
			{
				GTDev.LogWarning<string>("NetworkedWearable calling SetLeftWearableStateBool on one handed object " + base.gameObject.name + ". Please use SetWearableStateBool instead", null);
				this.SetWearableStateBool(newState);
				return;
			}
			if (this.leftHandValue != newState)
			{
				this.leftHandValue = newState;
				this.myRig.WearablePackedStates = GTBitOps.WriteBit(this.myRig.WearablePackedStates, (int)this.leftSlot, this.leftHandValue);
				this.OnLeftStateChanged();
			}
		}

		public void SetRightWearableStateBool(bool newState)
		{
			if (!this.isLocal || !this.IsSpawned)
			{
				return;
			}
			if (!NetworkedWearable.IsCategoryValid(this.assignedSlot))
			{
				return;
			}
			if (this.myRig == null)
			{
				return;
			}
			if (this.listenForChangesLocal)
			{
				GTDev.LogError<string>("NetworkedWearable with listenForChangesLocal calling SetRightWearableStateBool on object " + base.gameObject.name + ".You should not change state from a listener", null);
				return;
			}
			if (this.assignedSlot != CosmeticsController.CosmeticCategory.Paw || !this.isTwoHanded)
			{
				GTDev.LogWarning<string>("NetworkedWearable calling SetRightWearableStateBool on one handed object " + base.gameObject.name + ". Please use SetWearableStateBool instead", null);
				this.SetWearableStateBool(newState);
				return;
			}
			if (this.rightHandValue != newState)
			{
				this.rightHandValue = newState;
				this.myRig.WearablePackedStates = GTBitOps.WriteBit(this.myRig.WearablePackedStates, (int)this.rightSlot, this.rightHandValue);
				this.OnRightStateChanged();
			}
		}

		public void OnDisable()
		{
			if (this.isLocal && !this.listenForChangesLocal)
			{
				this.SetWearableStateBool(false);
				return;
			}
			if (this.TickRunning)
			{
				TickSystem<object>.RemoveTickCallback(this);
			}
		}

		private void OnWearableStateChanged()
		{
			if (this.value)
			{
				UnityEvent onWearableStateTrue = this.OnWearableStateTrue;
				if (onWearableStateTrue == null)
				{
					return;
				}
				onWearableStateTrue.Invoke();
				return;
			}
			else
			{
				UnityEvent onWearableStateFalse = this.OnWearableStateFalse;
				if (onWearableStateFalse == null)
				{
					return;
				}
				onWearableStateFalse.Invoke();
				return;
			}
		}

		private void OnLeftStateChanged()
		{
			if (this.leftHandValue)
			{
				UnityEvent onLeftWearableStateTrue = this.OnLeftWearableStateTrue;
				if (onLeftWearableStateTrue == null)
				{
					return;
				}
				onLeftWearableStateTrue.Invoke();
				return;
			}
			else
			{
				UnityEvent onLeftWearableStateFalse = this.OnLeftWearableStateFalse;
				if (onLeftWearableStateFalse == null)
				{
					return;
				}
				onLeftWearableStateFalse.Invoke();
				return;
			}
		}

		private void OnRightStateChanged()
		{
			if (this.rightHandValue)
			{
				UnityEvent onRightWearableStateTrue = this.OnRightWearableStateTrue;
				if (onRightWearableStateTrue == null)
				{
					return;
				}
				onRightWearableStateTrue.Invoke();
				return;
			}
			else
			{
				UnityEvent onRightWearableStateFalse = this.OnRightWearableStateFalse;
				if (onRightWearableStateFalse == null)
				{
					return;
				}
				onRightWearableStateFalse.Invoke();
				return;
			}
		}

		public bool IsSpawned { get; set; }

		public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

		public void OnSpawn(VRRig rig)
		{
			if (this.assignedSlot == CosmeticsController.CosmeticCategory.Paw && this.CosmeticSelectedSide == ECosmeticSelectSide.Both)
			{
				GTDev.LogWarning<string>(string.Format("NetworkedWearable: Cosmetic {0} with category {1} has select side Both, assuming left side!", base.gameObject.name, this.assignedSlot), null);
			}
			if (!NetworkedWearable.IsCategoryValid(this.assignedSlot))
			{
				GTDev.LogError<string>(string.Format("NetworkedWearable: Cosmetic {0} spawned with invalid category {1}!", base.gameObject.name, this.assignedSlot), null);
			}
			this.myRig = rig;
			this.isLocal = rig.isLocal;
			this.wearableSlot = this.CosmeticCategoryToWearableSlot(this.assignedSlot, this.CosmeticSelectedSide != ECosmeticSelectSide.Right);
			Debug.Log(string.Format("Networked Wearable {0} Select Side {1} slot {2}", base.gameObject.name, this.CosmeticSelectedSide, this.wearableSlot));
		}

		public void OnDespawn()
		{
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			if ((!this.isLocal || this.listenForChangesLocal) && this.IsSpawned)
			{
				if (this.assignedSlot == CosmeticsController.CosmeticCategory.Paw && this.isTwoHanded)
				{
					bool flag = GTBitOps.ReadBit(this.myRig.WearablePackedStates, (int)this.leftSlot);
					if (this.leftHandValue != flag)
					{
						this.leftHandValue = flag;
						this.OnLeftStateChanged();
					}
					flag = GTBitOps.ReadBit(this.myRig.WearablePackedStates, (int)this.rightSlot);
					if (this.rightHandValue != flag)
					{
						this.rightHandValue = flag;
						this.OnRightStateChanged();
						return;
					}
				}
				else
				{
					bool flag2 = GTBitOps.ReadBit(this.myRig.WearablePackedStates, (int)this.wearableSlot);
					if (this.value != flag2)
					{
						this.value = flag2;
						this.OnWearableStateChanged();
					}
				}
			}
		}

		public static bool IsCategoryValid(CosmeticsController.CosmeticCategory category)
		{
			switch (category)
			{
			case CosmeticsController.CosmeticCategory.Hat:
			case CosmeticsController.CosmeticCategory.Badge:
			case CosmeticsController.CosmeticCategory.Face:
			case CosmeticsController.CosmeticCategory.Paw:
			case CosmeticsController.CosmeticCategory.Fur:
			case CosmeticsController.CosmeticCategory.Shirt:
			case CosmeticsController.CosmeticCategory.Pants:
				return true;
			}
			return false;
		}

		private VRRig.WearablePackedStateSlots CosmeticCategoryToWearableSlot(CosmeticsController.CosmeticCategory category, bool isLeft)
		{
			switch (category)
			{
			case CosmeticsController.CosmeticCategory.Hat:
				return VRRig.WearablePackedStateSlots.Hat;
			case CosmeticsController.CosmeticCategory.Badge:
				return VRRig.WearablePackedStateSlots.Badge;
			case CosmeticsController.CosmeticCategory.Face:
				return VRRig.WearablePackedStateSlots.Face;
			case CosmeticsController.CosmeticCategory.Paw:
				if (!isLeft)
				{
					return VRRig.WearablePackedStateSlots.RightHand;
				}
				return VRRig.WearablePackedStateSlots.LeftHand;
			case CosmeticsController.CosmeticCategory.Fur:
				return VRRig.WearablePackedStateSlots.Fur;
			case CosmeticsController.CosmeticCategory.Shirt:
				return VRRig.WearablePackedStateSlots.Shirt;
			case CosmeticsController.CosmeticCategory.Pants:
				return VRRig.WearablePackedStateSlots.Pants1;
			}
			GTDev.LogWarning<string>(string.Format("NetworkedWearable: {0} item cannot set wearable state", category), null);
			return VRRig.WearablePackedStateSlots.Hat;
		}

		[Tooltip("Whether the wearable state is toggled on by default.")]
		[SerializeField]
		private bool startTrue;

		[Tooltip("This is to determine what bit to change in VRRig.WearablesPackedStates.")]
		[SerializeField]
		private CosmeticsController.CosmeticCategory assignedSlot;

		[FormerlySerializedAs("IsTwoHanded")]
		[SerializeField]
		private bool isTwoHanded;

		private const string listenInfo = "listenForChangesLocal should be false in most cases";

		private const string listenDetails = "listenForChangesLocal should be false in most cases\nIf you have a first person part and a local rig part that both need to react to a state change\ncall the Toggle/Set functions to change the state from one prefab and check \nlistenForChangesLocal on the other prefab ";

		[SerializeField]
		private bool listenForChangesLocal;

		private VRRig.WearablePackedStateSlots wearableSlot;

		private VRRig.WearablePackedStateSlots leftSlot = VRRig.WearablePackedStateSlots.LeftHand;

		private VRRig.WearablePackedStateSlots rightSlot = VRRig.WearablePackedStateSlots.RightHand;

		private VRRig myRig;

		private bool isLocal;

		private bool value;

		private bool leftHandValue;

		private bool rightHandValue;

		[SerializeField]
		protected UnityEvent OnWearableStateTrue;

		[SerializeField]
		protected UnityEvent OnWearableStateFalse;

		[SerializeField]
		protected UnityEvent OnLeftWearableStateTrue;

		[SerializeField]
		protected UnityEvent OnLeftWearableStateFalse;

		[SerializeField]
		protected UnityEvent OnRightWearableStateTrue;

		[SerializeField]
		protected UnityEvent OnRightWearableStateFalse;
	}
}

using System;
using System.Collections.Generic;
using GorillaGameModes;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class CosmeticSwapper : MonoBehaviour
	{
		private void Awake()
		{
			this.controller = CosmeticsController.instance;
		}

		private void OnEnable()
		{
			PlayerCosmeticsSystem.UnlockTemporaryCosmeticsGlobal(this.cosmeticIDs);
		}

		private void OnDisable()
		{
			PlayerCosmeticsSystem.LockTemporaryCosmeticsGlobal(this.cosmeticIDs);
		}

		public void SwapInCosmetic(VRRig vrRig)
		{
			this.TriggerSwap(vrRig);
		}

		public CosmeticSwapper.SwapMode GetCurrentMode()
		{
			return this.swapMode;
		}

		public bool ShouldHoldFinalStep()
		{
			return this.holdFinalStep;
		}

		public int GetCurrentStepIndex(VRRig rig)
		{
			if (rig == null)
			{
				return 0;
			}
			return rig.CosmeticStepIndex;
		}

		public int GetNumberOfSteps()
		{
			return this.cosmeticIDs.Count;
		}

		private void TriggerSwap(VRRig rig)
		{
			if (GorillaGameManager.instance != null && this.gameModeExclusion.Contains(GorillaGameManager.instance.GameType()))
			{
				return;
			}
			if (rig == null || this.controller == null || this.cosmeticIDs.Count == 0)
			{
				return;
			}
			if (rig != GorillaTagger.Instance.offlineVRRig)
			{
				return;
			}
			rig.SetCosmeticSwapper(this, this.stepTimeout);
			if (this.swapMode == CosmeticSwapper.SwapMode.AllAtOnce)
			{
				foreach (string text in this.cosmeticIDs)
				{
					CosmeticSwapper.CosmeticState? cosmeticState = this.SwapInCosmeticWithReturn(text, rig);
					if (cosmeticState != null)
					{
						rig.AddNewSwappedCosmetic(cosmeticState.Value);
					}
				}
				return;
			}
			int cosmeticStepIndex = rig.CosmeticStepIndex;
			if (cosmeticStepIndex < 0 || cosmeticStepIndex >= this.cosmeticIDs.Count)
			{
				return;
			}
			string text2 = this.cosmeticIDs[cosmeticStepIndex];
			CosmeticSwapper.CosmeticState? cosmeticState2 = this.SwapInCosmeticWithReturn(text2, rig);
			if (cosmeticState2 != null)
			{
				rig.AddNewSwappedCosmetic(cosmeticState2.Value);
				if (cosmeticStepIndex == this.cosmeticIDs.Count - 1)
				{
					if (this.holdFinalStep)
					{
						rig.MarkFinalCosmeticStep();
					}
					if (this.OnSwappingSequenceCompleted != null)
					{
						this.OnSwappingSequenceCompleted.Invoke(rig);
						return;
					}
				}
				else
				{
					rig.UnmarkFinalCosmeticStep();
				}
			}
		}

		private CosmeticSwapper.CosmeticState? SwapInCosmeticWithReturn(string nameOrId, VRRig rig)
		{
			if (this.controller == null)
			{
				return null;
			}
			CosmeticsController.CosmeticItem cosmeticItem = this.FindItem(nameOrId);
			if (cosmeticItem.isNullItem)
			{
				Debug.LogWarning("Cosmetic not found: " + nameOrId);
				return null;
			}
			bool flag;
			CosmeticsController.CosmeticSlots cosmeticSlot = this.GetCosmeticSlot(cosmeticItem, out flag);
			if (cosmeticSlot == CosmeticsController.CosmeticSlots.Count)
			{
				Debug.LogWarning("Could not determine slot for: " + cosmeticItem.displayName);
				return null;
			}
			CosmeticsController.CosmeticItem cosmeticItem2 = this.controller.currentWornSet.items[(int)cosmeticSlot];
			this.controller.ApplyCosmeticItemToSet(this.controller.tempUnlockedSet, cosmeticItem, flag, false);
			this.controller.UpdateWornCosmetics(true);
			return new CosmeticSwapper.CosmeticState?(new CosmeticSwapper.CosmeticState
			{
				cosmeticId = nameOrId,
				replacedItem = cosmeticItem2,
				slot = cosmeticSlot,
				isLeftHand = flag
			});
		}

		public void RestorePreviousCosmetic(CosmeticSwapper.CosmeticState state, VRRig rig)
		{
			if (this.controller == null)
			{
				return;
			}
			CosmeticsController.CosmeticItem cosmeticItem = this.FindItem(state.cosmeticId);
			if (cosmeticItem.isNullItem)
			{
				return;
			}
			this.controller.RemoveCosmeticItemFromSet(this.controller.tempUnlockedSet, cosmeticItem.displayName, false);
			if (!state.replacedItem.isNullItem)
			{
				this.controller.ApplyCosmeticItemToSet(this.controller.tempUnlockedSet, state.replacedItem, state.isLeftHand, false);
			}
			this.controller.UpdateWornCosmetics(true);
		}

		private CosmeticsController.CosmeticItem FindItem(string nameOrId)
		{
			CosmeticsController.CosmeticItem cosmeticItem;
			if (this.controller.allCosmeticsDict.TryGetValue(nameOrId, out cosmeticItem))
			{
				return cosmeticItem;
			}
			string text;
			if (this.controller.allCosmeticsItemIDsfromDisplayNamesDict.TryGetValue(nameOrId, out text))
			{
				return this.controller.GetItemFromDict(text);
			}
			return this.controller.nullItem;
		}

		private CosmeticsController.CosmeticSlots GetCosmeticSlot(CosmeticsController.CosmeticItem item, out bool isLeftHand)
		{
			isLeftHand = false;
			if (!item.isHoldable)
			{
				return CosmeticsController.CategoryToNonTransferrableSlot(item.itemCategory);
			}
			CosmeticsController.CosmeticSet currentWornSet = this.controller.currentWornSet;
			CosmeticsController.CosmeticItem cosmeticItem = currentWornSet.items[7];
			CosmeticsController.CosmeticItem cosmeticItem2 = currentWornSet.items[8];
			if (cosmeticItem.isNullItem || (!cosmeticItem2.isNullItem && item.itemName == cosmeticItem.itemName))
			{
				isLeftHand = true;
			}
			if (!isLeftHand)
			{
				return CosmeticsController.CosmeticSlots.HandRight;
			}
			return CosmeticsController.CosmeticSlots.HandLeft;
		}

		[SerializeField]
		private List<string> cosmeticIDs = new List<string>();

		[SerializeField]
		private CosmeticSwapper.SwapMode swapMode = CosmeticSwapper.SwapMode.StepByStep;

		[SerializeField]
		private float stepTimeout = 10f;

		[Tooltip("Hold final step as long as the swapper is being called within the timeframe")]
		[SerializeField]
		private bool holdFinalStep = true;

		[SerializeField]
		private UnityEvent<VRRig> OnSwappingSequenceCompleted;

		[SerializeField]
		private List<GameModeType> gameModeExclusion = new List<GameModeType>();

		private CosmeticsController controller;

		public enum SwapMode
		{
			AllAtOnce,
			StepByStep
		}

		public struct CosmeticState
		{
			public string cosmeticId;

			public CosmeticsController.CosmeticItem replacedItem;

			public CosmeticsController.CosmeticSlots slot;

			public bool isLeftHand;
		}
	}
}

using System;
using UnityEngine;

namespace GorillaTag
{
	[RequireComponent(typeof(VRRigCollection))]
	public class CosmeticTryOnNotifier : MonoBehaviour
	{
		private void Awake()
		{
			if (!base.TryGetComponent<VRRigCollection>(out this.m_vrrigCollection))
			{
				this.m_vrrigCollection = this.AddComponent<VRRigCollection>();
			}
			VRRigCollection vrrigCollection = this.m_vrrigCollection;
			vrrigCollection.playerEnteredCollection = (Action<RigContainer>)Delegate.Combine(vrrigCollection.playerEnteredCollection, new Action<RigContainer>(this.PlayerEnteredTryOnSpace));
			VRRigCollection vrrigCollection2 = this.m_vrrigCollection;
			vrrigCollection2.playerLeftCollection = (Action<RigContainer>)Delegate.Combine(vrrigCollection2.playerLeftCollection, new Action<RigContainer>(this.PlayerLeftTryOnSpace));
		}

		private void PlayerEnteredTryOnSpace(RigContainer playerRig)
		{
			CosmeticTryOnNotifier.Mode mode = this.mode;
			if (mode == CosmeticTryOnNotifier.Mode.TRY_ON)
			{
				PlayerCosmeticsSystem.SetRigTryOn(true, playerRig);
				return;
			}
			if (mode != CosmeticTryOnNotifier.Mode.ENABLE_LIST)
			{
				return;
			}
			PlayerCosmeticsSystem.UnlockTemporaryCosmeticsForPlayer(playerRig, this.unlockList.Strings);
		}

		private void PlayerLeftTryOnSpace(RigContainer playerRig)
		{
			CosmeticTryOnNotifier.Mode mode = this.mode;
			if (mode == CosmeticTryOnNotifier.Mode.TRY_ON)
			{
				PlayerCosmeticsSystem.SetRigTryOn(false, playerRig);
				return;
			}
			if (mode != CosmeticTryOnNotifier.Mode.ENABLE_LIST)
			{
				return;
			}
			PlayerCosmeticsSystem.LockTemporaryCosmeticsForPlayer(playerRig, this.unlockList.Strings);
		}

		private VRRigCollection m_vrrigCollection;

		[SerializeField]
		private CosmeticTryOnNotifier.Mode mode;

		[SerializeField]
		private StringList unlockList;

		private enum Mode
		{
			TRY_ON,
			ENABLE_LIST,
			ENABLE_LIST_TITLEDATA
		}
	}
}

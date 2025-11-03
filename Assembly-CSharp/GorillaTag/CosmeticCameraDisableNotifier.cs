using System;
using UnityEngine;

namespace GorillaTag
{
	[RequireComponent(typeof(VRRigCollection))]
	public class CosmeticCameraDisableNotifier : MonoBehaviour
	{
		private void Awake()
		{
			if (!base.TryGetComponent<VRRigCollection>(out this._vrrigCollection))
			{
				this._vrrigCollection = this.AddComponent<VRRigCollection>();
			}
			VRRigCollection vrrigCollection = this._vrrigCollection;
			vrrigCollection.playerEnteredCollection = (Action<RigContainer>)Delegate.Combine(vrrigCollection.playerEnteredCollection, new Action<RigContainer>(this.PlayerEnteredTryOnSpace));
			VRRigCollection vrrigCollection2 = this._vrrigCollection;
			vrrigCollection2.playerLeftCollection = (Action<RigContainer>)Delegate.Combine(vrrigCollection2.playerLeftCollection, new Action<RigContainer>(this.PlayerLeftTryOnSpace));
		}

		private void PlayerEnteredTryOnSpace(RigContainer playerRig)
		{
			if (playerRig.Rig.isLocal)
			{
				this._cosmeticCamera.enabled = false;
			}
		}

		private void PlayerLeftTryOnSpace(RigContainer playerRig)
		{
			if (playerRig.Rig.isLocal)
			{
				this._cosmeticCamera.enabled = true;
			}
		}

		private VRRigCollection _vrrigCollection;

		[SerializeField]
		private Camera _cosmeticCamera;
	}
}

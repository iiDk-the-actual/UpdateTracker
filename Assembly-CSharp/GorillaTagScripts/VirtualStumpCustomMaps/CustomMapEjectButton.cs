using System;
using System.Collections;
using GT_CustomMapSupportRuntime;
using UnityEngine;

namespace GorillaTagScripts.VirtualStumpCustomMaps
{
	public class CustomMapEjectButton : GorillaPressableButton
	{
		public override void ButtonActivation()
		{
			base.ButtonActivation();
			base.StartCoroutine(this.ButtonPressed_Local());
			if (!this.processing)
			{
				this.HandleTeleport();
			}
		}

		private IEnumerator ButtonPressed_Local()
		{
			this.isOn = true;
			this.UpdateColor();
			yield return new WaitForSeconds(this.debounceTime);
			this.isOn = false;
			this.UpdateColor();
			yield break;
		}

		private void HandleTeleport()
		{
			if (this.processing)
			{
				return;
			}
			this.processing = true;
			CustomMapEjectButton.EjectType ejectType = this.ejectType;
			if (ejectType != CustomMapEjectButton.EjectType.EjectFromVirtualStump)
			{
				if (ejectType == CustomMapEjectButton.EjectType.ReturnToVirtualStump)
				{
					CustomMapManager.ReturnToVirtualStump();
					this.processing = false;
					return;
				}
			}
			else
			{
				CustomMapManager.ExitVirtualStump(new Action<bool>(this.FinishTeleport));
			}
		}

		private void FinishTeleport(bool success = true)
		{
			if (!this.processing)
			{
				return;
			}
			this.processing = false;
		}

		public void CopySettings(CustomMapEjectButtonSettings customMapEjectButtonSettings)
		{
			this.ejectType = (CustomMapEjectButton.EjectType)customMapEjectButtonSettings.ejectType;
		}

		[SerializeField]
		private CustomMapEjectButton.EjectType ejectType;

		private bool processing;

		public enum EjectType
		{
			EjectFromVirtualStump,
			ReturnToVirtualStump
		}
	}
}

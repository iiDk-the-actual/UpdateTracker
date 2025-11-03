using System;
using System.Collections;
using UnityEngine;

namespace GameObjectScheduling.DeepLinks
{
	public class DeepLinkButton : GorillaPressableButton
	{
		public override void ButtonActivation()
		{
			base.ButtonActivation();
			this.sendingDeepLink = DeepLinkSender.SendDeepLink(this.deepLinkAppID, this.deepLinkPayload, new Action<string>(this.OnDeepLinkSent));
			base.StartCoroutine(this.ButtonPressed_Local());
		}

		private void OnDeepLinkSent(string message)
		{
			this.sendingDeepLink = false;
			if (!this.isOn)
			{
				this.UpdateColor();
			}
		}

		private IEnumerator ButtonPressed_Local()
		{
			this.isOn = true;
			this.UpdateColor();
			yield return new WaitForSeconds(this.pressedTime);
			this.isOn = false;
			if (!this.sendingDeepLink)
			{
				this.UpdateColor();
			}
			yield break;
		}

		[SerializeField]
		private ulong deepLinkAppID;

		[SerializeField]
		private string deepLinkPayload = "";

		[SerializeField]
		private float pressedTime = 0.2f;

		private bool sendingDeepLink;
	}
}

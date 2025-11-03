using System;
using System.Collections;
using UnityEngine;

namespace GorillaNetworking.Store
{
	public class BundlePurchaseButton : GorillaPressableButton, IGorillaSliceableSimple
	{
		public void OnEnable()
		{
			GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		}

		public void OnDisable()
		{
			GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		}

		public void SliceUpdate()
		{
			if (NetworkSystem.Instance != null && NetworkSystem.Instance.WrongVersion && !this.bError)
			{
				base.enabled = false;
				base.GetComponent<BoxCollider>().enabled = false;
				this.buttonRenderer.material = this.pressedMaterial;
				this.myText.text = this.UnavailableText;
			}
		}

		public override void ButtonActivation()
		{
			if (this.bError)
			{
				return;
			}
			base.ButtonActivation();
			BundleManager.instance.BundlePurchaseButtonPressed(this.playfabID);
			base.StartCoroutine(this.ButtonColorUpdate());
		}

		public void AlreadyOwn()
		{
			if (this.bError)
			{
				return;
			}
			base.enabled = false;
			base.GetComponent<BoxCollider>().enabled = false;
			this.buttonRenderer.material = this.pressedMaterial;
			this.onText = this.AlreadyOwnText;
			this.myText.text = this.AlreadyOwnText;
			this.isOn = true;
		}

		public void ResetButton()
		{
			if (this.bError)
			{
				return;
			}
			base.enabled = true;
			base.GetComponent<BoxCollider>().enabled = true;
			this.buttonRenderer.material = this.unpressedMaterial;
			this.SetOffText(true, false, false);
			this.isOn = false;
		}

		private IEnumerator ButtonColorUpdate()
		{
			this.buttonRenderer.material = this.pressedMaterial;
			yield return new WaitForSeconds(this.debounceTime);
			this.buttonRenderer.material = (this.isOn ? this.pressedMaterial : this.unpressedMaterial);
			yield break;
		}

		public void ErrorHappened()
		{
			this.bError = true;
			this.myText.text = this.ErrorText;
			this.buttonRenderer.material = this.unpressedMaterial;
			base.enabled = false;
			this.offText = this.ErrorText;
			this.onText = this.ErrorText;
			this.isOn = false;
		}

		public void InitializeData()
		{
			if (this.bError)
			{
				return;
			}
			this.SetOffText(true, false, false);
			this.buttonRenderer.material = this.unpressedMaterial;
			base.enabled = true;
			this.isOn = false;
		}

		public void UpdatePurchaseButtonText(string purchaseText)
		{
			if (!this.bError)
			{
				this.offText = purchaseText;
				this.UpdateColor();
			}
		}

		private const string MONKE_BLOCKS_BUNDLE_ALREADY_OWN_KEY = "MONKE_BLOCKS_BUNDLE_ALREADY_OWN";

		private const string MONKE_BLOCKS_BUNDLE_UNAVAILABLE_KEY = "MONKE_BLOCKS_BUNDLE_UNAVAILABLE";

		private const string MONKE_BLOCKS_BUNDLE_ERROR_KEY = "MONKE_BLOCKS_BUNDLE_ERROR";

		public bool bError;

		public string ErrorText = "ERROR COMPLETING PURCHASE! PLEASE RESTART THE GAME";

		public string AlreadyOwnText = "YOU OWN THE BUNDLE ALREADY! THANK YOU!";

		public string UnavailableText = "UNAVAILABLE";

		public string playfabID = "";
	}
}

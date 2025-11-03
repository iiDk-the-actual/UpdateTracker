using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GorillaPlayerLineButton : MonoBehaviour
{
	private void OnEnable()
	{
		if (Application.isEditor)
		{
			base.StartCoroutine(this.TestPressCheck());
		}
	}

	private void OnDisable()
	{
		if (Application.isEditor)
		{
			base.StopAllCoroutines();
		}
	}

	private IEnumerator TestPressCheck()
	{
		for (;;)
		{
			if (this.testPress)
			{
				this.testPress = false;
				if (this.buttonType == GorillaPlayerLineButton.ButtonType.Mute)
				{
					this.isOn = !this.isOn;
				}
				this.parentLine.PressButton(this.isOn, this.buttonType);
			}
			yield return new WaitForSeconds(1f);
		}
		yield break;
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (base.enabled && this.touchTime + this.debounceTime < Time.time && collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null)
		{
			this.touchTime = Time.time;
			GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
			if (this.buttonType == GorillaPlayerLineButton.ButtonType.Mute)
			{
				if (this.isAutoOn)
				{
					this.isOn = false;
				}
				else
				{
					this.isOn = !this.isOn;
				}
			}
			if (this.buttonType == GorillaPlayerLineButton.ButtonType.Mute || this.buttonType == GorillaPlayerLineButton.ButtonType.HateSpeech || this.buttonType == GorillaPlayerLineButton.ButtonType.Cheating || this.buttonType == GorillaPlayerLineButton.ButtonType.Cancel || this.parentLine.canPressNextReportButton)
			{
				this.parentLine.PressButton(this.isOn, this.buttonType);
				if (component != null)
				{
					GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
					GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, component.isLeftHand, 0.05f);
					if (PhotonNetwork.InRoom && GorillaTagger.Instance.myVRRig != null)
					{
						GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, new object[] { 67, component.isLeftHand, 0.05f });
					}
				}
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (this.buttonType != GorillaPlayerLineButton.ButtonType.Mute && other.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null)
		{
			this.parentLine.canPressNextReportButton = true;
		}
	}

	public void UpdateColor()
	{
		if (this.isOn)
		{
			base.GetComponent<MeshRenderer>().material = this.onMaterial;
			this.myText.text = this.onText;
			return;
		}
		if (this.isAutoOn)
		{
			base.GetComponent<MeshRenderer>().material = this.autoOnMaterial;
			this.myText.text = this.autoOnText;
			return;
		}
		base.GetComponent<MeshRenderer>().material = this.offMaterial;
		this.myText.text = this.offText;
	}

	public GorillaPlayerScoreboardLine parentLine;

	public GorillaPlayerLineButton.ButtonType buttonType;

	public bool isOn;

	public bool isAutoOn;

	public Material offMaterial;

	public Material onMaterial;

	public Material autoOnMaterial;

	public string offText;

	public string onText;

	public string autoOnText;

	public Text myText;

	public float debounceTime = 0.25f;

	public float touchTime;

	public bool testPress;

	public enum ButtonType
	{
		HateSpeech,
		Cheating,
		Toxicity,
		Mute,
		Report,
		Cancel
	}
}

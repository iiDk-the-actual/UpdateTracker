using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GorillaReportButton : MonoBehaviour
{
	public void AssignParentLine(GorillaPlayerScoreboardLine parent)
	{
		this.parentLine = parent;
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (base.enabled && this.touchTime + this.debounceTime < Time.time)
		{
			this.isOn = !this.isOn;
			this.UpdateColor();
			this.selected = !this.selected;
			this.touchTime = Time.time;
			GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
			GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, false, 0.05f);
			if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
			{
				GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, new object[] { 67, false, 0.05f });
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (this.metaReportType != GorillaReportButton.MetaReportReason.Cancel)
		{
			other.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null;
		}
	}

	public void UpdateColor()
	{
		if (this.isOn)
		{
			base.GetComponent<MeshRenderer>().material = this.onMaterial;
			return;
		}
		base.GetComponent<MeshRenderer>().material = this.offMaterial;
	}

	public GorillaReportButton.MetaReportReason metaReportType;

	public GorillaPlayerLineButton.ButtonType buttonType;

	public GorillaPlayerScoreboardLine parentLine;

	public bool isOn;

	public Material offMaterial;

	public Material onMaterial;

	public string offText;

	public string onText;

	public Text myText;

	public float debounceTime = 0.25f;

	public float touchTime;

	public bool testPress;

	public bool selected;

	[SerializeField]
	public enum MetaReportReason
	{
		HateSpeech,
		Cheating,
		Toxicity,
		Bullying,
		Doxing,
		Impersonation,
		Submit,
		Cancel
	}
}

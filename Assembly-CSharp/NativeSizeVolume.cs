using System;
using GorillaLocomotion;
using UnityEngine;

public class NativeSizeVolume : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		GTPlayer componentInParent = other.GetComponentInParent<GTPlayer>();
		if (componentInParent == null)
		{
			return;
		}
		NativeSizeVolume.NativeSizeVolumeAction onEnterAction = this.OnEnterAction;
		if (onEnterAction == NativeSizeVolume.NativeSizeVolumeAction.ApplySettings)
		{
			this.settings.WorldPosition = base.transform.position;
			componentInParent.SetNativeScale(this.settings);
			return;
		}
		if (onEnterAction != NativeSizeVolume.NativeSizeVolumeAction.ResetSize)
		{
			return;
		}
		componentInParent.SetNativeScale(null);
	}

	private void OnTriggerExit(Collider other)
	{
		GTPlayer componentInParent = other.GetComponentInParent<GTPlayer>();
		if (componentInParent == null)
		{
			return;
		}
		NativeSizeVolume.NativeSizeVolumeAction onExitAction = this.OnExitAction;
		if (onExitAction == NativeSizeVolume.NativeSizeVolumeAction.ApplySettings)
		{
			this.settings.WorldPosition = base.transform.position;
			componentInParent.SetNativeScale(this.settings);
			return;
		}
		if (onExitAction != NativeSizeVolume.NativeSizeVolumeAction.ResetSize)
		{
			return;
		}
		componentInParent.SetNativeScale(null);
	}

	[SerializeField]
	private Collider triggerVolume;

	[SerializeField]
	private NativeSizeChangerSettings settings;

	[SerializeField]
	private NativeSizeVolume.NativeSizeVolumeAction OnEnterAction;

	[SerializeField]
	private NativeSizeVolume.NativeSizeVolumeAction OnExitAction;

	[Serializable]
	private enum NativeSizeVolumeAction
	{
		None,
		ApplySettings,
		ResetSize
	}
}

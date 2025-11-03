using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class CrittersVoiceNoise : MonoBehaviour, IGorillaSliceableSimple
{
	private void Start()
	{
		this.speaker = base.GetComponent<GorillaSpeakerLoudness>();
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void SliceUpdate()
	{
		float num = 0f;
		if (this.speaker.IsSpeaking)
		{
			num = this.speaker.Loudness;
		}
		if (num > this.minTriggerThreshold && CrittersManager.instance.IsNotNull())
		{
			CrittersLoudNoise crittersLoudNoise = (CrittersLoudNoise)CrittersManager.instance.rigSetupByRig[this.rig].rigActors[4].actorSet;
			if (crittersLoudNoise.IsNotNull() && !crittersLoudNoise.soundEnabled)
			{
				float num2 = Mathf.Lerp(this.noiseVolumeMin, this.noisVolumeMax, Mathf.Clamp01((num - this.minTriggerThreshold) / this.maxTriggerThreshold));
				crittersLoudNoise.PlayVoiceSpeechLocal(PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time), 0.016666668f, num2);
			}
		}
	}

	[SerializeField]
	private GorillaSpeakerLoudness speaker;

	[SerializeField]
	private VRRig rig;

	[SerializeField]
	private float minTriggerThreshold = 0.01f;

	[SerializeField]
	private float maxTriggerThreshold = 0.3f;

	[SerializeField]
	private float noiseVolumeMin = 1f;

	[SerializeField]
	private float noisVolumeMax = 9f;
}

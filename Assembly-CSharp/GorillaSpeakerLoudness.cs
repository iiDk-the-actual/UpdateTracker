using System;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.Audio;
using Oculus.VoiceSDK.Utilities;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;

public class GorillaSpeakerLoudness : MonoBehaviour, IGorillaSliceableSimple, IDynamicFloat
{
	public bool IsSpeaking
	{
		get
		{
			return this.isSpeaking;
		}
	}

	public float Loudness
	{
		get
		{
			return this.loudness;
		}
	}

	public float LoudnessNormalized
	{
		get
		{
			return Mathf.Min(this.loudness / this.normalizedMax, 1f);
		}
	}

	public float floatValue
	{
		get
		{
			return this.LoudnessNormalized;
		}
	}

	public bool IsMicEnabled
	{
		get
		{
			return this.isMicEnabled;
		}
	}

	public float SmoothedLoudness
	{
		get
		{
			return this.smoothedLoudness;
		}
	}

	private void Start()
	{
		this.rigContainer = base.GetComponent<RigContainer>();
		this.timeLastUpdated = Time.time;
		this.deltaTime = Time.deltaTime;
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
		this.deltaTime = Time.time - this.timeLastUpdated;
		this.timeLastUpdated = Time.time;
		this.UpdateMicEnabled();
		this.UpdateLoudness();
		this.UpdateSmoothedLoudness();
	}

	private void UpdateMicEnabled()
	{
		if (this.rigContainer == null)
		{
			return;
		}
		VRRig rig = this.rigContainer.Rig;
		if (rig.isOfflineVRRig)
		{
			this.permission = this.permission || MicPermissionsManager.HasMicPermission();
			if (this.permission && !this.micConnected && Microphone.devices != null)
			{
				this.micConnected = Microphone.devices.Length != 0;
			}
			this.isMicEnabled = this.permission && this.micConnected;
			rig.IsMicEnabled = this.isMicEnabled;
			return;
		}
		this.isMicEnabled = rig.IsMicEnabled;
	}

	private void UpdateLoudness()
	{
		if (this.rigContainer == null)
		{
			return;
		}
		PhotonVoiceView voice = this.rigContainer.Voice;
		if (voice != null && this.speaker == null)
		{
			this.speaker = voice.SpeakerInUse;
		}
		if (this.recorder == null)
		{
			this.recorder = ((voice != null) ? voice.RecorderInUse : null);
		}
		if (this.recorder != null && this.offlineMic != null)
		{
			Microphone.End(UnityMicrophone.devices[0]);
			Object.Destroy(this.offlineMic);
			this.offlineMic = null;
			this.recorder.RestartRecording(true);
		}
		VRRig rig = this.rigContainer.Rig;
		if (rig.isOfflineVRRig && this.recorder == null && this.isMicEnabled && !Microphone.IsRecording(UnityMicrophone.devices[0]))
		{
			this.offlineMic = Microphone.Start(UnityMicrophone.devices[0], true, 1, 16000);
		}
		if ((rig.remoteUseReplacementVoice || rig.localUseReplacementVoice || GorillaComputer.instance.voiceChatOn == "FALSE") && rig.SpeakingLoudness > 0f && !this.rigContainer.ForceMute && !this.rigContainer.Muted)
		{
			this.isSpeaking = true;
			this.loudness = rig.SpeakingLoudness;
			return;
		}
		if (voice != null && voice.IsSpeaking)
		{
			this.isSpeaking = true;
			if (!(this.speaker != null))
			{
				this.loudness = 0f;
				return;
			}
			if (this.speakerVoiceToLoudness == null)
			{
				this.speakerVoiceToLoudness = this.speaker.GetComponent<SpeakerVoiceToLoudness>();
			}
			if (this.speakerVoiceToLoudness != null)
			{
				this.loudness = this.speakerVoiceToLoudness.loudness;
				return;
			}
		}
		else if (voice != null && this.recorder != null && NetworkSystem.Instance.IsObjectLocallyOwned(voice.gameObject) && this.recorder.IsCurrentlyTransmitting)
		{
			if (this.voiceToLoudness == null)
			{
				this.voiceToLoudness = this.recorder.GetComponent<VoiceToLoudness>();
			}
			this.isSpeaking = true;
			if (this.voiceToLoudness != null)
			{
				this.loudness = this.voiceToLoudness.loudness;
				return;
			}
			this.loudness = 0f;
			return;
		}
		else if (this.offlineMic != null && this.recorder == null && this.isMicEnabled && Microphone.IsRecording(UnityMicrophone.devices[0]))
		{
			this.isSpeaking = true;
			int num = Mathf.Min(Mathf.CeilToInt(this.deltaTime * 16000f), 16000);
			if (num > this.voiceSampleBuffer.Length)
			{
				Array.Resize<float>(ref this.voiceSampleBuffer, num);
			}
			if (this.offlineMic.samples >= num && this.offlineMic.GetData(this.voiceSampleBuffer, this.offlineMic.samples - num))
			{
				float num2 = 0f;
				for (int i = 0; i < this.voiceSampleBuffer.Length; i++)
				{
					num2 += Mathf.Abs(this.voiceSampleBuffer[i]);
				}
				this.loudness = num2 / (float)this.voiceSampleBuffer.Length;
				return;
			}
		}
		else
		{
			this.isSpeaking = false;
			this.loudness = 0f;
		}
	}

	private void UpdateSmoothedLoudness()
	{
		if (!this.isSpeaking)
		{
			this.smoothedLoudness = 0f;
			return;
		}
		if (!Mathf.Approximately(this.loudness, this.lastLoudness))
		{
			this.timeSinceLoudnessChange = 0f;
			this.smoothedLoudness = Mathf.Lerp(this.smoothedLoudness, this.loudness, Mathf.Clamp01(this.loudnessBlendStrength * this.deltaTime));
			this.lastLoudness = this.loudness;
			return;
		}
		if (this.timeSinceLoudnessChange > this.loudnessUpdateCheckRate)
		{
			this.smoothedLoudness = 0.001f;
			return;
		}
		this.smoothedLoudness = Mathf.Lerp(this.smoothedLoudness, this.loudness, Mathf.Clamp01(this.loudnessBlendStrength * this.deltaTime));
		this.timeSinceLoudnessChange += this.deltaTime;
	}

	private bool isSpeaking;

	private float loudness;

	[SerializeField]
	private float normalizedMax = 0.175f;

	private bool isMicEnabled;

	private RigContainer rigContainer;

	private Speaker speaker;

	private SpeakerVoiceToLoudness speakerVoiceToLoudness;

	private Recorder recorder;

	private VoiceToLoudness voiceToLoudness;

	private float smoothedLoudness;

	private float lastLoudness;

	private float timeSinceLoudnessChange;

	private float loudnessUpdateCheckRate = 0.2f;

	private float loudnessBlendStrength = 2f;

	private bool permission;

	private bool micConnected;

	private float timeLastUpdated;

	private float deltaTime;

	private AudioClip offlineMic;

	private float[] voiceSampleBuffer = new float[128];
}

using System;
using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;

[RequireComponent(typeof(Speaker))]
public class SpeakerVoiceToLoudness : MonoBehaviour
{
	private void Awake()
	{
		Speaker component = base.GetComponent<Speaker>();
		component.CustomAudioOutFactory = this.GetVolumeTracking(component);
	}

	private Func<IAudioOut<float>> GetVolumeTracking(Speaker speaker)
	{
		AudioOutDelayControl.PlayDelayConfig pdc = new AudioOutDelayControl.PlayDelayConfig
		{
			Low = this.playbackDelaySettings.MinDelaySoft,
			High = this.playbackDelaySettings.MaxDelaySoft,
			Max = this.playbackDelaySettings.MaxDelayHard
		};
		return () => new SpeakerVoiceLoudnessAudioOut(this, speaker.GetComponent<AudioSource>(), pdc, speaker.Logger, string.Empty, speaker.Logger.IsDebugEnabled);
	}

	[SerializeField]
	private PlaybackDelaySettings playbackDelaySettings = new PlaybackDelaySettings
	{
		MinDelaySoft = 200,
		MaxDelaySoft = 400,
		MaxDelayHard = 1000
	};

	public float loudness;
}

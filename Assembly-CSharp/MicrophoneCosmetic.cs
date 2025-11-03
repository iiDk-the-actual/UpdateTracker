using System;
using UnityEngine;

public class MicrophoneCosmetic : MonoBehaviour
{
	private void Awake()
	{
		this.audioSource = base.GetComponent<AudioSource>();
		if (!Application.isEditor && Application.platform == RuntimePlatform.Android && Microphone.devices.Length != 0)
		{
			this.audioSource.clip = Microphone.Start(Microphone.devices[0], true, 10, 16000);
		}
		else
		{
			int sampleRate = AudioSettings.GetConfiguration().sampleRate;
			this.audioSource.clip = Microphone.Start(null, true, 10, sampleRate);
		}
		this.audioSource.loop = true;
	}

	private void OnEnable()
	{
		int num = ((Application.platform == RuntimePlatform.Android && Microphone.devices.Length != 0) ? Microphone.GetPosition(Microphone.devices[0]) : Microphone.GetPosition(null));
		num -= 10;
		if ((float)num < 0f)
		{
			num = this.audioSource.clip.samples + num - 1;
		}
		this.audioSource.GTPlay();
		this.audioSource.timeSamples = num;
	}

	private void OnDisable()
	{
		this.audioSource.GTStop();
	}

	private void Update()
	{
		Vector3 vector = this.mouthTransform.position - base.transform.position;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = 0f;
		if (sqrMagnitude < this.mouthProximityRampRange.x * this.mouthProximityRampRange.x)
		{
			float magnitude = vector.magnitude;
			num = Mathf.InverseLerp(this.mouthProximityRampRange.x, this.mouthProximityRampRange.y, magnitude);
		}
		if (num != this.audioSource.volume)
		{
			this.audioSource.volume = num;
		}
		int num2 = (this.audioSource.timeSamples -= 10);
		if ((float)num2 < 0f)
		{
			num2 = this.audioSource.clip.samples + num2 - 1;
		}
		this.audioSource.clip.SetData(this.zero, num2);
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
	}

	[SerializeField]
	private Transform mouthTransform;

	[SerializeField]
	private Vector2 mouthProximityRampRange = new Vector2(0.6f, 0.3f);

	private AudioSource audioSource;

	private float[] zero = new float[1];
}

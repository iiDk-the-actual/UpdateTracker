using System;
using GorillaTag.Cosmetics;
using UnityEngine;

public class VoiceLoudnessReactor2 : MonoBehaviour, ITickSystemTick
{
	private float Loudness
	{
		get
		{
			return this.gsl.Loudness * this.sensitivity;
		}
	}

	private void OnEnable()
	{
		if (this.continuousProperties.Count == 0)
		{
			return;
		}
		if (this.gsl == null)
		{
			this.gsl = base.GetComponentInParent<GorillaSpeakerLoudness>(true);
			if (this.gsl == null)
			{
				GorillaTagger componentInParent = base.GetComponentInParent<GorillaTagger>();
				if (componentInParent != null)
				{
					this.gsl = componentInParent.offlineVRRig.GetComponent<GorillaSpeakerLoudness>();
					if (this.gsl == null)
					{
						return;
					}
				}
			}
		}
		this.smoothedLoudness = this.Loudness;
		TickSystem<object>.AddTickCallback(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	public bool TickRunning { get; set; }

	public void Tick()
	{
		float num = 1f - Mathf.Exp(-this.responsiveness * Time.deltaTime);
		this.smoothedLoudness = Mathf.Lerp(this.smoothedLoudness, this.Loudness, num);
		this.continuousProperties.ApplyAll(this.smoothedLoudness);
	}

	[Tooltip("How quickly the internal loudness approaches the real loudness. A low value will take a long time to match the true volume but will be more resistant to fluctuations. Note: If the value is too high, you may notice some jerkiness in the output because the underlying GorillaSpeakerLoudness doesn't update every frame.")]
	public float responsiveness = 5f;

	[Tooltip("Multiply the microphone input by this value. A good default is 15.")]
	public float sensitivity = 15f;

	public ContinuousPropertyArray continuousProperties;

	private GorillaSpeakerLoudness gsl;

	private float smoothedLoudness;
}

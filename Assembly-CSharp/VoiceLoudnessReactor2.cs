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
		TickSystem<object>.AddTickCallback(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	public bool TickRunning { get; set; }

	public void Tick()
	{
		this.continuousProperties.ApplyAll(this.Loudness);
	}

	[Tooltip("Multiply the microphone input by this value. A good default is 15.")]
	public float sensitivity = 15f;

	public ContinuousPropertyArray continuousProperties;

	private GorillaSpeakerLoudness gsl;
}

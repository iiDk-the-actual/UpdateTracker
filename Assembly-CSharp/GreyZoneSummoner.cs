using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GreyZoneSummoner : MonoBehaviour
{
	public Vector3 SummoningFocusPoint
	{
		get
		{
			return this.summoningFocusPoint.position;
		}
	}

	public float SummonerMaxDistance
	{
		get
		{
			return this.areaTriggerCollider.radius + 1f;
		}
	}

	private void OnEnable()
	{
		this.greyZoneManager = GreyZoneManager.Instance;
		if (this.greyZoneManager == null)
		{
			return;
		}
		this.greyZoneManager.RegisterSummoner(this);
		this.areaTriggerNotifier.TriggerEnterEvent += this.ColliderEnteredArea;
		this.areaTriggerNotifier.TriggerExitEvent += this.ColliderExitedArea;
	}

	private void OnDisable()
	{
		if (GreyZoneManager.Instance != null)
		{
			GreyZoneManager.Instance.DeregisterSummoner(this);
		}
		this.areaTriggerNotifier.TriggerEnterEvent -= this.ColliderEnteredArea;
		this.areaTriggerNotifier.TriggerExitEvent -= this.ColliderExitedArea;
	}

	public void UpdateProgressFeedback(bool greyZoneAvailable)
	{
		if (this.greyZoneManager == null)
		{
			return;
		}
		if (greyZoneAvailable && !this.candlesParent.gameObject.activeSelf)
		{
			this.candlesParent.gameObject.SetActive(true);
		}
		this.candlesTimeline.time = (double)Mathf.Clamp01(this.greyZoneManager.SummoningProgress) * this.candlesTimeline.duration;
		this.candlesTimeline.Evaluate();
		if (!this.greyZoneManager.GreyZoneActive)
		{
			float num = (float)this.summoningTones.Count * this.greyZoneManager.SummoningProgress;
			for (int i = 0; i < this.summoningTones.Count; i++)
			{
				float num2 = Mathf.InverseLerp((float)i, (float)i + 1f + this.summoningTonesFadeOverlap, num);
				this.summoningTones[i].volume = num2 * this.summoningTonesMaxVolume;
			}
		}
		this.greyZoneActivationButton.isOn = this.greyZoneManager.GreyZoneActive;
		this.greyZoneActivationButton.UpdateColor();
		for (int j = 0; j < this.greyZoneGravityFactorButtons.Count; j++)
		{
			this.greyZoneGravityFactorButtons[j].isOn = this.greyZoneManager.GravityFactorSelection == j;
			this.greyZoneGravityFactorButtons[j].UpdateColor();
		}
	}

	public void OnGreyZoneActivated()
	{
		base.StopAllCoroutines();
		base.StartCoroutine(this.FadeOutSummoningTones());
	}

	private IEnumerator FadeOutSummoningTones()
	{
		float fadeStartTime = Time.time;
		float fadeRate = 1f / this.summoningTonesFadeTime;
		while (Time.time < fadeStartTime + this.summoningTonesFadeTime)
		{
			for (int i = 0; i < this.summoningTones.Count; i++)
			{
				this.summoningTones[i].volume = Mathf.MoveTowards(this.summoningTones[i].volume, 0f, this.summoningTonesMaxVolume * fadeRate * Time.deltaTime);
			}
			yield return null;
		}
		for (int j = 0; j < this.summoningTones.Count; j++)
		{
			this.summoningTones[j].volume = 0f;
		}
		yield break;
	}

	public void ColliderEnteredArea(TriggerEventNotifier notifier, Collider other)
	{
		ZoneEntity component = other.GetComponent<ZoneEntity>();
		VRRig vrrig = ((component != null) ? component.entityRig : null);
		if (vrrig != null && this.greyZoneManager != null)
		{
			this.greyZoneManager.VRRigEnteredSummonerProximity(vrrig, this);
		}
	}

	public void ColliderExitedArea(TriggerEventNotifier notifier, Collider other)
	{
		ZoneEntity component = other.GetComponent<ZoneEntity>();
		VRRig vrrig = ((component != null) ? component.entityRig : null);
		if (vrrig != null && this.greyZoneManager != null)
		{
			this.greyZoneManager.VRRigExitedSummonerProximity(vrrig, this);
		}
	}

	[SerializeField]
	private Transform summoningFocusPoint;

	[SerializeField]
	private Transform candlesParent;

	[SerializeField]
	private PlayableDirector candlesTimeline;

	[SerializeField]
	private TriggerEventNotifier areaTriggerNotifier;

	[SerializeField]
	private SphereCollider areaTriggerCollider;

	[SerializeField]
	private GorillaPressableButton greyZoneActivationButton;

	[SerializeField]
	private List<AudioSource> summoningTones = new List<AudioSource>();

	[SerializeField]
	private float summoningTonesMaxVolume = 1f;

	[SerializeField]
	private float summoningTonesFadeOverlap = 0.5f;

	[SerializeField]
	private float summoningTonesFadeTime = 4f;

	[SerializeField]
	private List<GorillaPressableButton> greyZoneGravityFactorButtons = new List<GorillaPressableButton>();

	private GreyZoneManager greyZoneManager;
}

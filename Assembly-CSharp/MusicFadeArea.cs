using System;
using System.Collections;
using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;

public class MusicFadeArea : MonoBehaviour
{
	private void Awake()
	{
		for (int i = 0; i < this.sourcesToFadeIn.Count; i++)
		{
			this.sourcesToFadeIn[i].audioSource.Stop();
			this.sourcesToFadeIn[i].audioSource.volume = 0f;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other == GTPlayer.Instance.headCollider)
		{
			MusicManager.Instance.FadeOutMusic(this.fadeDuration);
			if (this.fadeCoroutine != null)
			{
				base.StopCoroutine(this.fadeCoroutine);
			}
			if (this.sourcesToFadeIn.Count > 0)
			{
				this.fadeCoroutine = base.StartCoroutine(this.FadeInSources());
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other == GTPlayer.Instance.headCollider)
		{
			MusicManager.Instance.FadeInMusic(this.fadeDuration);
			if (this.fadeCoroutine != null)
			{
				base.StopCoroutine(this.fadeCoroutine);
			}
			if (this.sourcesToFadeIn.Count > 0)
			{
				this.fadeCoroutine = base.StartCoroutine(this.FadeOutSources());
			}
		}
	}

	private IEnumerator FadeInSources()
	{
		for (int i = 0; i < this.sourcesToFadeIn.Count; i++)
		{
			this.sourcesToFadeIn[i].audioSource.Play();
			this.sourcesToFadeIn[i].audioSource.volume = this.sourcesToFadeIn[i].maxVolume * this.fadeProgress;
		}
		while (this.fadeProgress < 1f)
		{
			for (int j = 0; j < this.sourcesToFadeIn.Count; j++)
			{
				this.sourcesToFadeIn[j].audioSource.volume = this.sourcesToFadeIn[j].maxVolume * this.fadeProgress;
			}
			yield return null;
			this.fadeProgress = Mathf.MoveTowards(this.fadeProgress, 1f, Time.deltaTime / this.fadeDuration);
		}
		for (int k = 0; k < this.sourcesToFadeIn.Count; k++)
		{
			this.sourcesToFadeIn[k].audioSource.volume = this.sourcesToFadeIn[k].maxVolume;
		}
		yield break;
	}

	private IEnumerator FadeOutSources()
	{
		for (int i = 0; i < this.sourcesToFadeIn.Count; i++)
		{
			this.sourcesToFadeIn[i].audioSource.volume = this.sourcesToFadeIn[i].maxVolume * this.fadeProgress;
		}
		while (this.fadeProgress > 0f)
		{
			for (int j = 0; j < this.sourcesToFadeIn.Count; j++)
			{
				this.sourcesToFadeIn[j].audioSource.volume = this.sourcesToFadeIn[j].maxVolume * this.fadeProgress;
			}
			yield return null;
			this.fadeProgress = Mathf.MoveTowards(this.fadeProgress, 0f, Time.deltaTime / this.fadeDuration);
		}
		for (int k = 0; k < this.sourcesToFadeIn.Count; k++)
		{
			this.sourcesToFadeIn[k].audioSource.Stop();
			this.sourcesToFadeIn[k].audioSource.volume = 0f;
		}
		yield break;
	}

	[SerializeField]
	private List<MusicFadeArea.AudioSourceEntry> sourcesToFadeIn = new List<MusicFadeArea.AudioSourceEntry>();

	[SerializeField]
	private float fadeDuration = 3f;

	private float fadeProgress;

	private Coroutine fadeCoroutine;

	[Serializable]
	public struct AudioSourceEntry
	{
		public AudioSource audioSource;

		public float maxVolume;
	}
}

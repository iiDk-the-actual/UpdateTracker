using System;
using System.Collections;
using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour
{
	private void Reset()
	{
		this._source = base.GetComponent<AudioSource>();
		if (this._source)
		{
			this._source.playOnAwake = false;
		}
	}

	private void OnEnable()
	{
		this.Play();
	}

	private void OnDisable()
	{
		this.Stop();
	}

	public void Play()
	{
		if (this._loop && this._clips.Length == 1 && this._loopDelay == Vector2.zero)
		{
			this._source.clip = this._clips[0];
			this._source.loop = true;
			this._source.GTPlay();
			return;
		}
		this._source.loop = false;
		if (this._loop)
		{
			base.StartCoroutine(this.DoLoop());
			return;
		}
		this._source.clip = this._clips[Random.Range(0, this._clips.Length)];
		this._source.GTPlay();
	}

	private IEnumerator DoLoop()
	{
		while (base.enabled)
		{
			this._source.clip = this._clips[Random.Range(0, this._clips.Length)];
			this._source.GTPlay();
			while (this._source.isPlaying)
			{
				yield return null;
			}
			float num = Random.Range(this._loopDelay.x, this._loopDelay.y);
			if (num > 0f)
			{
				float waitEndTime = Time.time + num;
				while (Time.time < waitEndTime)
				{
					yield return null;
				}
			}
		}
		yield break;
	}

	public void Stop()
	{
		this._source.GTStop();
		this._source.loop = false;
	}

	[SerializeField]
	private AudioSource _source;

	[SerializeField]
	private AudioClip[] _clips;

	[SerializeField]
	private bool _loop;

	[SerializeField]
	private Vector2 _loopDelay;
}

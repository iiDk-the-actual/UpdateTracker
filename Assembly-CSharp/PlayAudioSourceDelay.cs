using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayAudioSourceDelay : MonoBehaviour
{
	public IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(this._delay);
		base.GetComponent<AudioSource>().GTPlay();
		yield break;
	}

	[SerializeField]
	private float _delay;
}

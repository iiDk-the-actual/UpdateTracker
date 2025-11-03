using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class GTAudioSourceExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTPlayOneShot(this AudioSource audioSource, IList<AudioClip> clips, float volumeScale = 1f)
	{
		audioSource.PlayOneShot(clips[Random.Range(0, clips.Count)], volumeScale);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTPlayOneShot(this AudioSource audioSource, AudioClip clip, float volumeScale = 1f)
	{
		audioSource.PlayOneShot(clip, volumeScale);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTPlay(this AudioSource audioSource)
	{
		audioSource.Play();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTPlay(this AudioSource audioSource, ulong delay)
	{
		audioSource.Play(delay);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTPause(this AudioSource audioSource)
	{
		audioSource.Pause();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTUnPause(this AudioSource audioSource)
	{
		audioSource.UnPause();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTStop(this AudioSource audioSource)
	{
		audioSource.Stop();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTPlayDelayed(this AudioSource audioSource, float delay)
	{
		audioSource.PlayDelayed(delay);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTPlayScheduled(this AudioSource audioSource, double time)
	{
		audioSource.PlayScheduled(time);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTPlayClipAtPoint(AudioClip clip, Vector3 position)
	{
		AudioSource.PlayClipAtPoint(clip, position);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GTPlayClipAtPoint(AudioClip clip, Vector3 position, float volume)
	{
		AudioSource.PlayClipAtPoint(clip, position, volume);
	}

	[Conditional("BETA")]
	[Conditional("UNITY_EDITOR")]
	private static void _BetaLogIfAudioSourceIsNotActiveAndEnabled(AudioSource audioSource)
	{
	}
}

using System;
using GorillaTag;
using UnityEngine;

[Serializable]
public class HandTapOverrides
{
	private const string PREFAB_TOOLTIP = "Must be in the global object pool and have a tag.\n\nPrefabs can have an FXModifier component to be adjusted after creation.";

	public bool overrideSurfacePrefab;

	[Tooltip("Must be in the global object pool and have a tag.\n\nPrefabs can have an FXModifier component to be adjusted after creation.")]
	public HashWrapper surfaceTapPrefab;

	public bool overrideGamemodePrefab;

	[Tooltip("Must be in the global object pool and have a tag.\n\nPrefabs can have an FXModifier component to be adjusted after creation.")]
	public HashWrapper gamemodeTapPrefab;

	public bool overrideSound;

	public AudioClip tapSound;
}

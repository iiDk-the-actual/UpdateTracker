using System;
using UnityEngine;

public class ColliderEnabledManager : MonoBehaviour
{
	private void Start()
	{
		this.floorEnabled = true;
		this.floorCollidersEnabled = true;
		ColliderEnabledManager.instance = this;
	}

	private void OnDestroy()
	{
		ColliderEnabledManager.instance = null;
	}

	public void DisableFloorForFrame()
	{
		this.floorEnabled = false;
	}

	private void LateUpdate()
	{
		if (!this.floorEnabled && this.floorCollidersEnabled)
		{
			this.DisableFloor();
		}
		if (!this.floorCollidersEnabled && Time.time > this.timeDisabled + this.disableLength)
		{
			this.floorCollidersEnabled = true;
		}
		Collider[] array = this.floorCollider;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = this.floorCollidersEnabled;
		}
		if (this.floorCollidersEnabled)
		{
			GorillaSurfaceOverride[] array2 = this.walls;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].overrideIndex = this.wallsBeforeMaterial;
			}
		}
		else
		{
			GorillaSurfaceOverride[] array2 = this.walls;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].overrideIndex = this.wallsAfterMaterial;
			}
		}
		this.floorEnabled = true;
	}

	private void DisableFloor()
	{
		this.floorCollidersEnabled = false;
		this.timeDisabled = Time.time;
	}

	public static ColliderEnabledManager instance;

	public Collider[] floorCollider;

	public bool floorEnabled;

	public bool wasFloorEnabled;

	public bool floorCollidersEnabled;

	[GorillaSoundLookup]
	public int wallsBeforeMaterial;

	[GorillaSoundLookup]
	public int wallsAfterMaterial;

	public GorillaSurfaceOverride[] walls;

	public float timeDisabled;

	public float disableLength;
}

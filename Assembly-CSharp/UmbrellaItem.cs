using System;
using GorillaTag;
using UnityEngine;

public class UmbrellaItem : TransferrableObject
{
	protected override void Start()
	{
		base.Start();
		this.itemState = TransferrableObject.ItemStates.State1;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		float num = GorillaTagger.Instance.tapHapticStrength / 4f;
		float fixedDeltaTime = Time.fixedDeltaTime;
		float num2 = 0.08f;
		int num3;
		if (this.itemState == TransferrableObject.ItemStates.State1)
		{
			num3 = this.SoundIdOpen;
			this.itemState = TransferrableObject.ItemStates.State0;
			BetterDayNightManager.instance.collidersToAddToWeatherSystems.Add(this.umbrellaRainDestroyTrigger);
		}
		else
		{
			num3 = this.SoundIdClose;
			this.itemState = TransferrableObject.ItemStates.State1;
			BetterDayNightManager.instance.collidersToAddToWeatherSystems.Remove(this.umbrellaRainDestroyTrigger);
		}
		base.ActivateItemFX(num, fixedDeltaTime, num3, num2);
		this.OnUmbrellaStateChanged();
	}

	internal override void OnEnable()
	{
		base.OnEnable();
		this.OnUmbrellaStateChanged();
	}

	internal override void OnDisable()
	{
		base.OnDisable();
		BetterDayNightManager.instance.collidersToAddToWeatherSystems.Remove(this.umbrellaRainDestroyTrigger);
	}

	public override void ResetToDefaultState()
	{
		base.ResetToDefaultState();
		BetterDayNightManager.instance.collidersToAddToWeatherSystems.Remove(this.umbrellaRainDestroyTrigger);
		this.itemState = TransferrableObject.ItemStates.State1;
		this.OnUmbrellaStateChanged();
	}

	public override bool OnRelease(DropZone zoneReleased, GameObject releasingHand)
	{
		if (!base.OnRelease(zoneReleased, releasingHand))
		{
			return false;
		}
		if (base.InHand())
		{
			return false;
		}
		if (this.itemState == TransferrableObject.ItemStates.State0)
		{
			this.OnActivate();
		}
		return true;
	}

	protected override void LateUpdateShared()
	{
		base.LateUpdateShared();
		UmbrellaItem.UmbrellaStates itemState = (UmbrellaItem.UmbrellaStates)this.itemState;
		if (itemState != this.previousUmbrellaState)
		{
			this.OnUmbrellaStateChanged();
		}
		this.UpdateAngles((itemState == UmbrellaItem.UmbrellaStates.UmbrellaOpen) ? this.startingAngles : this.endingAngles, this.lerpValue);
		this.previousUmbrellaState = itemState;
	}

	protected virtual void OnUmbrellaStateChanged()
	{
		bool flag = this.itemState == TransferrableObject.ItemStates.State0;
		GameObject[] array = this.gameObjectsActivatedOnOpen;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(flag);
		}
		ParticleSystem[] array2;
		if (flag)
		{
			array2 = this.particlesEmitOnOpen;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Play();
			}
			return;
		}
		array2 = this.particlesEmitOnOpen;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Stop();
		}
	}

	protected virtual void UpdateAngles(Quaternion[] toAngles, float t)
	{
		for (int i = 0; i < this.umbrellaBones.Length; i++)
		{
			this.umbrellaBones[i].localRotation = Quaternion.Lerp(this.umbrellaBones[i].localRotation, toAngles[i], t);
		}
	}

	protected void GenerateAngles()
	{
		this.startingAngles = new Quaternion[this.umbrellaBones.Length];
		for (int i = 0; i < this.endingAngles.Length; i++)
		{
			this.startingAngles[i] = this.umbrellaToCopy.startingAngles[i];
		}
		this.endingAngles = new Quaternion[this.umbrellaBones.Length];
		for (int j = 0; j < this.endingAngles.Length; j++)
		{
			this.endingAngles[j] = this.umbrellaToCopy.endingAngles[j];
		}
	}

	public override bool CanActivate()
	{
		return true;
	}

	public override bool CanDeactivate()
	{
		return true;
	}

	[AssignInCorePrefab]
	public Transform[] umbrellaBones;

	[AssignInCorePrefab]
	public Quaternion[] startingAngles;

	[AssignInCorePrefab]
	public Quaternion[] endingAngles;

	[AssignInCorePrefab]
	[Tooltip("Assign to use the 'Generate Angles' button")]
	private UmbrellaItem umbrellaToCopy;

	[AssignInCorePrefab]
	public float lerpValue = 0.25f;

	[AssignInCorePrefab]
	public Collider umbrellaRainDestroyTrigger;

	[AssignInCorePrefab]
	public GameObject[] gameObjectsActivatedOnOpen;

	[AssignInCorePrefab]
	public ParticleSystem[] particlesEmitOnOpen;

	[GorillaSoundLookup]
	public int SoundIdOpen = 64;

	[GorillaSoundLookup]
	public int SoundIdClose = 65;

	private UmbrellaItem.UmbrellaStates previousUmbrellaState = UmbrellaItem.UmbrellaStates.UmbrellaOpen;

	private enum UmbrellaStates
	{
		UmbrellaOpen = 1,
		UmbrellaClosed
	}
}

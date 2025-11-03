using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShadeRevealer : TransferrableObject
{
	protected override void Awake()
	{
		base.Awake();
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		for (int i = 0; i < this.enableWhenScanning.Length; i++)
		{
			hashSet.Add(this.enableWhenScanning[i]);
		}
		for (int j = 0; j < this.enableWhenTracking.Length; j++)
		{
			hashSet.Add(this.enableWhenTracking[j]);
		}
		for (int k = 0; k < this.enableWhenLocked.Length; k++)
		{
			hashSet.Add(this.enableWhenLocked[k]);
		}
		for (int l = 0; l < this.enableWhenPrimed.Length; l++)
		{
			hashSet.Add(this.enableWhenPrimed[l]);
		}
		this.objectsToDisableWhenOff = new GameObject[hashSet.Count];
		hashSet.CopyTo(this.objectsToDisableWhenOff);
	}

	private float GetDistanceToBeamRay(Vector3 toPosition)
	{
		return Vector3.Cross(this.beamForward.forward, toPosition).magnitude;
	}

	public ShadeRevealer.State GetBeamStateForPosition(Vector3 toPosition, float tolerance)
	{
		if (toPosition.magnitude <= this.beamLength + tolerance && Vector3.Dot(toPosition.normalized, this.beamForward.forward) > 0f)
		{
			float num = this.GetDistanceToBeamRay(toPosition) - tolerance;
			if (num <= this.lockThreshold)
			{
				return ShadeRevealer.State.LOCKED;
			}
			if (num <= this.trackThreshold)
			{
				return ShadeRevealer.State.TRACKING;
			}
		}
		return ShadeRevealer.State.SCANNING;
	}

	public ShadeRevealer.State GetBeamStateForCritter(CosmeticCritter critter, float tolerance)
	{
		return this.GetBeamStateForPosition(critter.transform.position - this.beamForward.position, tolerance);
	}

	public bool CritterWithinBeamThreshold(CosmeticCritter critter, ShadeRevealer.State criteria, float tolerance)
	{
		return this.GetBeamStateForCritter(critter, tolerance) >= criteria;
	}

	public void SetBestBeamState(ShadeRevealer.State state)
	{
		if (state > this.pendingBeamState)
		{
			this.pendingBeamState = state;
		}
	}

	private void SetObjectsEnabledFromState(ShadeRevealer.State state)
	{
		for (int i = 0; i < this.objectsToDisableWhenOff.Length; i++)
		{
			this.objectsToDisableWhenOff[i].SetActive(false);
		}
		GameObject[] array;
		switch (state)
		{
		case ShadeRevealer.State.SCANNING:
			array = this.enableWhenScanning;
			break;
		case ShadeRevealer.State.TRACKING:
			array = this.enableWhenTracking;
			break;
		case ShadeRevealer.State.LOCKED:
			array = this.enableWhenLocked;
			break;
		case ShadeRevealer.State.PRIMED:
			array = this.enableWhenPrimed;
			break;
		default:
			return;
		}
		for (int j = 0; j < array.Length; j++)
		{
			array[j].SetActive(true);
		}
	}

	protected override void LateUpdateShared()
	{
		base.LateUpdateShared();
		if (this.currentBeamState != this.pendingBeamState)
		{
			this.currentBeamState = this.pendingBeamState;
			this.SetObjectsEnabledFromState(this.currentBeamState);
		}
		this.beamSFX.pitch = 1f + this.shadeCatcher.GetActionTimeFrac() * 2f;
		if (this.isScanning)
		{
			this.pendingBeamState = ShadeRevealer.State.SCANNING;
		}
	}

	public void StartScanning()
	{
		this.shadeCatcher.enabled = true;
		this.initialActivationSFX.GTPlay();
		this.beamSFX.GTPlay();
		this.isScanning = true;
		this.currentBeamState = ShadeRevealer.State.OFF;
		this.pendingBeamState = ShadeRevealer.State.SCANNING;
	}

	public void StopScanning()
	{
		if (this.currentBeamState == ShadeRevealer.State.PRIMED)
		{
			UnityEvent unityEvent = this.onShadeLaunched;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
		}
		this.shadeCatcher.enabled = false;
		this.initialActivationSFX.GTStop();
		this.beamSFX.GTStop();
		this.isScanning = false;
		this.currentBeamState = ShadeRevealer.State.OFF;
		this.pendingBeamState = ShadeRevealer.State.OFF;
		this.SetObjectsEnabledFromState(ShadeRevealer.State.OFF);
	}

	public void ShadeCaught()
	{
		this.shadeCatcher.enabled = false;
		this.beamSFX.GTStop();
		this.catchSFX.GTPlay();
		this.catchFX.Play();
		this.isScanning = false;
		this.currentBeamState = ShadeRevealer.State.OFF;
		this.pendingBeamState = ShadeRevealer.State.PRIMED;
	}

	[SerializeField]
	private AudioSource initialActivationSFX;

	[SerializeField]
	private AudioSource beamSFX;

	[SerializeField]
	private AudioSource catchSFX;

	[SerializeField]
	private ParticleSystem catchFX;

	[Space]
	[SerializeField]
	private CosmeticCritterCatcherShade shadeCatcher;

	[Space]
	[Tooltip("The transform that represents the origin of the revealer beam.")]
	[SerializeField]
	private Transform beamForward;

	[Tooltip("The maximum length of the beam.")]
	[SerializeField]
	private float beamLength;

	[Tooltip("If the Shade is this close to the beam, set it to flee and have all Revealers enter Tracking mode.")]
	[SerializeField]
	private float trackThreshold;

	[Tooltip("If the Shade is this close to the beam, slow it down.")]
	[SerializeField]
	private float lockThreshold;

	[Tooltip("Editor-only object to help test the thresholds.")]
	[SerializeField]
	private Transform thresholdTester;

	[Tooltip("Whether to draw the tester or not.")]
	[SerializeField]
	private bool drawThresholdTesterInEditor = true;

	[Space]
	[Tooltip("Enable these objects while the beam is in Scanning mode.")]
	[SerializeField]
	private GameObject[] enableWhenScanning;

	[Tooltip("Enable these objects while the beam is in Tracking mode.")]
	[SerializeField]
	private GameObject[] enableWhenTracking;

	[Tooltip("Enable these objects while the beam is in Locked mode.")]
	[SerializeField]
	private GameObject[] enableWhenLocked;

	[Tooltip("Enable these objects while ready to fire.")]
	[SerializeField]
	private GameObject[] enableWhenPrimed;

	[Space]
	[SerializeField]
	private UnityEvent onShadeLaunched;

	private bool isScanning;

	private ShadeRevealer.State currentBeamState;

	private ShadeRevealer.State pendingBeamState;

	private GameObject[] objectsToDisableWhenOff;

	public enum State
	{
		OFF,
		SCANNING,
		TRACKING,
		LOCKED,
		PRIMED
	}
}

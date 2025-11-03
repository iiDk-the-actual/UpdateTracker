using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GorillaSlicerSimpleManager : MonoBehaviour
{
	protected void Awake()
	{
		if (GorillaSlicerSimpleManager.hasInstance && GorillaSlicerSimpleManager.instance != this)
		{
			Object.Destroy(this);
			return;
		}
		GorillaSlicerSimpleManager.SetInstance(this);
	}

	public static void CreateManager()
	{
		GorillaSlicerSimpleManager gorillaSlicerSimpleManager = new GameObject("GorillaSlicerSimpleManager").AddComponent<GorillaSlicerSimpleManager>();
		gorillaSlicerSimpleManager.fixedUpdateSlice = new List<IGorillaSliceableSimple>();
		gorillaSlicerSimpleManager.updateSlice = new List<IGorillaSliceableSimple>();
		gorillaSlicerSimpleManager.lateUpdateSlice = new List<IGorillaSliceableSimple>();
		gorillaSlicerSimpleManager.sW = new Stopwatch();
		GorillaSlicerSimpleManager.SetInstance(gorillaSlicerSimpleManager);
	}

	private static void SetInstance(GorillaSlicerSimpleManager manager)
	{
		GorillaSlicerSimpleManager.instance = manager;
		GorillaSlicerSimpleManager.hasInstance = true;
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad(manager);
		}
	}

	public static void RegisterSliceable(IGorillaSliceableSimple gSS, GorillaSlicerSimpleManager.UpdateStep step)
	{
		if (!GorillaSlicerSimpleManager.hasInstance)
		{
			GorillaSlicerSimpleManager.CreateManager();
		}
		switch (step)
		{
		case GorillaSlicerSimpleManager.UpdateStep.FixedUpdate:
			if (!GorillaSlicerSimpleManager.instance.fixedUpdateSlice.Contains(gSS))
			{
				GorillaSlicerSimpleManager.instance.fixedUpdateSlice.Add(gSS);
				return;
			}
			break;
		case GorillaSlicerSimpleManager.UpdateStep.Update:
			if (!GorillaSlicerSimpleManager.instance.updateSlice.Contains(gSS))
			{
				GorillaSlicerSimpleManager.instance.updateSlice.Add(gSS);
				return;
			}
			break;
		case GorillaSlicerSimpleManager.UpdateStep.LateUpdate:
			if (!GorillaSlicerSimpleManager.instance.lateUpdateSlice.Contains(gSS))
			{
				GorillaSlicerSimpleManager.instance.lateUpdateSlice.Add(gSS);
			}
			break;
		default:
			return;
		}
	}

	public static void UnregisterSliceable(IGorillaSliceableSimple gSS, GorillaSlicerSimpleManager.UpdateStep step)
	{
		if (!GorillaSlicerSimpleManager.hasInstance)
		{
			GorillaSlicerSimpleManager.CreateManager();
		}
		switch (step)
		{
		case GorillaSlicerSimpleManager.UpdateStep.FixedUpdate:
			if (GorillaSlicerSimpleManager.instance.fixedUpdateSlice.Contains(gSS))
			{
				GorillaSlicerSimpleManager.instance.fixedUpdateSlice.Remove(gSS);
				return;
			}
			break;
		case GorillaSlicerSimpleManager.UpdateStep.Update:
			if (GorillaSlicerSimpleManager.instance.updateSlice.Contains(gSS))
			{
				GorillaSlicerSimpleManager.instance.updateSlice.Remove(gSS);
				return;
			}
			break;
		case GorillaSlicerSimpleManager.UpdateStep.LateUpdate:
			if (GorillaSlicerSimpleManager.instance.lateUpdateSlice.Contains(gSS))
			{
				GorillaSlicerSimpleManager.instance.lateUpdateSlice.Remove(gSS);
			}
			break;
		default:
			return;
		}
	}

	public void FixedUpdate()
	{
		if (this.updateIndex < 0 || this.updateIndex >= this.fixedUpdateSlice.Count + this.updateSlice.Count + this.lateUpdateSlice.Count)
		{
			this.updateIndex = 0;
		}
		this.sW.Restart();
		while (this.ticksThisFrame + this.sW.ElapsedTicks < this.ticksPerFrame && this.updateIndex < this.fixedUpdateSlice.Count)
		{
			IGorillaSliceableSimple gorillaSliceableSimple = this.fixedUpdateSlice[this.updateIndex];
			if (0 <= this.updateIndex && this.updateIndex < this.fixedUpdateSlice.Count)
			{
				MonoBehaviour monoBehaviour = gorillaSliceableSimple as MonoBehaviour;
				if (monoBehaviour == null || monoBehaviour.isActiveAndEnabled)
				{
					gorillaSliceableSimple.SliceUpdate();
				}
			}
			this.updateIndex++;
		}
		this.ticksThisFrame += this.sW.ElapsedTicks;
		this.sW.Stop();
	}

	public void Update()
	{
		int count = this.fixedUpdateSlice.Count;
		int count2 = this.updateSlice.Count;
		int num = count + count2;
		this.sW.Restart();
		while (this.ticksThisFrame + this.sW.ElapsedTicks < this.ticksPerFrame && count <= this.updateIndex && this.updateIndex < num)
		{
			IGorillaSliceableSimple gorillaSliceableSimple = this.updateSlice[this.updateIndex - count];
			if (0 <= this.updateIndex - count && this.updateIndex - count < this.updateSlice.Count)
			{
				MonoBehaviour monoBehaviour = gorillaSliceableSimple as MonoBehaviour;
				if (monoBehaviour == null || monoBehaviour.isActiveAndEnabled)
				{
					gorillaSliceableSimple.SliceUpdate();
				}
			}
			this.updateIndex++;
		}
		this.ticksThisFrame += this.sW.ElapsedTicks;
		this.sW.Stop();
	}

	public void LateUpdate()
	{
		int count = this.fixedUpdateSlice.Count;
		int count2 = this.updateSlice.Count;
		int count3 = this.lateUpdateSlice.Count;
		int num = count + count2;
		int num2 = num + count3;
		this.sW.Restart();
		while (this.ticksThisFrame + this.sW.ElapsedTicks < this.ticksPerFrame && num <= this.updateIndex && this.updateIndex < num2)
		{
			IGorillaSliceableSimple gorillaSliceableSimple = this.lateUpdateSlice[this.updateIndex - num];
			if (0 <= this.updateIndex - num && this.updateIndex - num < this.lateUpdateSlice.Count)
			{
				MonoBehaviour monoBehaviour = gorillaSliceableSimple as MonoBehaviour;
				if (monoBehaviour == null || monoBehaviour.isActiveAndEnabled)
				{
					gorillaSliceableSimple.SliceUpdate();
				}
			}
			this.updateIndex++;
		}
		this.sW.Stop();
		if (this.updateIndex >= num2)
		{
			this.updateIndex = -1;
		}
		this.ticksThisFrame = 0L;
	}

	public static GorillaSlicerSimpleManager instance;

	public static bool hasInstance;

	public List<IGorillaSliceableSimple> fixedUpdateSlice;

	public List<IGorillaSliceableSimple> updateSlice;

	public List<IGorillaSliceableSimple> lateUpdateSlice;

	public long ticksPerFrame = 1000L;

	public long ticksThisFrame;

	public int updateIndex = -1;

	public Stopwatch sW;

	public enum UpdateStep
	{
		FixedUpdate,
		Update,
		LateUpdate
	}
}

using System;
using System.Collections.Generic;
using GorillaTag;
using UnityEngine;

public class GorillaVelocityEstimatorManager : MonoBehaviour
{
	protected void Awake()
	{
		if (GorillaVelocityEstimatorManager.hasInstance && GorillaVelocityEstimatorManager.instance != this)
		{
			Object.Destroy(this);
			return;
		}
		GorillaVelocityEstimatorManager.SetInstance(this);
	}

	protected void OnDestroy()
	{
		if (GorillaVelocityEstimatorManager.instance == this)
		{
			GorillaVelocityEstimatorManager.hasInstance = false;
			GorillaVelocityEstimatorManager.instance = null;
		}
	}

	protected void LateUpdate()
	{
		if (GTAppState.isQuitting)
		{
			return;
		}
		for (int i = 0; i < GorillaVelocityEstimatorManager.estimators.Count; i++)
		{
			if (GorillaVelocityEstimatorManager.estimators[i] != null)
			{
				GorillaVelocityEstimatorManager.estimators[i].TriggeredLateUpdate();
			}
		}
	}

	public static void CreateManager()
	{
		if (GTAppState.isQuitting)
		{
			return;
		}
		GorillaVelocityEstimatorManager.SetInstance(new GameObject("GorillaVelocityEstimatorManager").AddComponent<GorillaVelocityEstimatorManager>());
	}

	private static void SetInstance(GorillaVelocityEstimatorManager manager)
	{
		if (GTAppState.isQuitting)
		{
			return;
		}
		GorillaVelocityEstimatorManager.instance = manager;
		GorillaVelocityEstimatorManager.hasInstance = true;
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad(manager);
		}
	}

	public static void Register(GorillaVelocityEstimator velEstimator)
	{
		if (GTAppState.isQuitting)
		{
			return;
		}
		if (!GorillaVelocityEstimatorManager.hasInstance)
		{
			GorillaVelocityEstimatorManager.CreateManager();
		}
		if (!GorillaVelocityEstimatorManager.estimators.Contains(velEstimator))
		{
			GorillaVelocityEstimatorManager.estimators.Add(velEstimator);
		}
	}

	public static void Unregister(GorillaVelocityEstimator velEstimator)
	{
		if (GTAppState.isQuitting)
		{
			return;
		}
		if (!GorillaVelocityEstimatorManager.hasInstance)
		{
			GorillaVelocityEstimatorManager.CreateManager();
		}
		if (GorillaVelocityEstimatorManager.estimators.Contains(velEstimator))
		{
			GorillaVelocityEstimatorManager.estimators.Remove(velEstimator);
		}
	}

	public static GorillaVelocityEstimatorManager instance;

	public static bool hasInstance = false;

	public static readonly List<GorillaVelocityEstimator> estimators = new List<GorillaVelocityEstimator>(1024);
}

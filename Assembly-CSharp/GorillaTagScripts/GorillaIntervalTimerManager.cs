using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTagScripts
{
	public class GorillaIntervalTimerManager : MonoBehaviour
	{
		protected void Awake()
		{
			if (GorillaIntervalTimerManager.hasInstance && GorillaIntervalTimerManager.instance != null && GorillaIntervalTimerManager.instance != this)
			{
				Object.Destroy(this);
				return;
			}
			GorillaIntervalTimerManager.SetInstance(this);
		}

		private static void CreateManager()
		{
			GorillaIntervalTimerManager.SetInstance(new GameObject("GorillaIntervalTimerManager").AddComponent<GorillaIntervalTimerManager>());
		}

		private static void SetInstance(GorillaIntervalTimerManager manager)
		{
			GorillaIntervalTimerManager.instance = manager;
			GorillaIntervalTimerManager.hasInstance = true;
			if (Application.isPlaying)
			{
				Object.DontDestroyOnLoad(manager);
			}
		}

		public static void RegisterGorillaTimer(GorillaIntervalTimer gTimer)
		{
			if (!GorillaIntervalTimerManager.hasInstance)
			{
				GorillaIntervalTimerManager.CreateManager();
			}
			if (!GorillaIntervalTimerManager.allTimers.Contains(gTimer))
			{
				GorillaIntervalTimerManager.allTimers.Add(gTimer);
			}
		}

		public static void UnregisterGorillaTimer(GorillaIntervalTimer gTimer)
		{
			if (!GorillaIntervalTimerManager.hasInstance)
			{
				GorillaIntervalTimerManager.CreateManager();
			}
			if (GorillaIntervalTimerManager.allTimers.Contains(gTimer))
			{
				GorillaIntervalTimerManager.allTimers.Remove(gTimer);
			}
		}

		private void Update()
		{
			for (int i = 0; i < GorillaIntervalTimerManager.allTimers.Count; i++)
			{
				GorillaIntervalTimerManager.allTimers[i].InvokeUpdate();
			}
		}

		private static GorillaIntervalTimerManager instance;

		private static bool hasInstance = false;

		private static List<GorillaIntervalTimer> allTimers = new List<GorillaIntervalTimer>();
	}
}

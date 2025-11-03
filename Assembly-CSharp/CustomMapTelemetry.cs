using System;
using System.Collections;
using Unity.Profiling;
using UnityEngine;

public class CustomMapTelemetry : MonoBehaviour
{
	public static bool IsActive
	{
		get
		{
			return CustomMapTelemetry.metricsCaptureStarted || CustomMapTelemetry.perfCaptureStarted;
		}
	}

	private void Awake()
	{
		if (CustomMapTelemetry.instance == null)
		{
			CustomMapTelemetry.instance = this;
			return;
		}
		if (CustomMapTelemetry.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private static void OnPlayerJoinedRoom(NetPlayer obj)
	{
		CustomMapTelemetry.runningPlayerCount++;
		CustomMapTelemetry.maxPlayersInMap = Math.Max(CustomMapTelemetry.runningPlayerCount, CustomMapTelemetry.maxPlayersInMap);
	}

	private static void OnPlayerLeftRoom(NetPlayer obj)
	{
		CustomMapTelemetry.runningPlayerCount--;
		CustomMapTelemetry.minPlayersInMap = Math.Min(CustomMapTelemetry.runningPlayerCount, CustomMapTelemetry.minPlayersInMap);
	}

	public static void StartMapTracking()
	{
		if (CustomMapTelemetry.metricsCaptureStarted || CustomMapTelemetry.perfCaptureStarted)
		{
			return;
		}
		CustomMapTelemetry.mapEnterTime = Time.unscaledTime;
		float value = Random.value;
		if (value <= 0.01f)
		{
			CustomMapTelemetry.StartMetricsCapture();
		}
		else if (value >= 0.99f)
		{
			CustomMapTelemetry.StartPerfCapture();
		}
		if (!CustomMapTelemetry.metricsCaptureStarted)
		{
			bool flag = CustomMapTelemetry.perfCaptureStarted;
		}
	}

	public static void EndMapTracking()
	{
		CustomMapTelemetry.EndMetricsCapture();
		CustomMapTelemetry.EndPerfCapture();
		CustomMapTelemetry.mapName = "NULL";
		CustomMapTelemetry.mapCreatorUsername = "NULL";
		CustomMapTelemetry.mapEnterTime = -1f;
		CustomMapTelemetry.mapModId = 0L;
	}

	private static void StartMetricsCapture()
	{
		if (CustomMapTelemetry.metricsCaptureStarted)
		{
			return;
		}
		CustomMapTelemetry.metricsCaptureStarted = true;
		NetworkSystem.Instance.OnPlayerJoined -= CustomMapTelemetry.OnPlayerJoinedRoom;
		NetworkSystem.Instance.OnPlayerJoined += CustomMapTelemetry.OnPlayerJoinedRoom;
		NetworkSystem.Instance.OnPlayerLeft -= CustomMapTelemetry.OnPlayerLeftRoom;
		NetworkSystem.Instance.OnPlayerLeft += CustomMapTelemetry.OnPlayerLeftRoom;
		CustomMapTelemetry.runningPlayerCount = NetworkSystem.Instance.RoomPlayerCount;
		CustomMapTelemetry.minPlayersInMap = CustomMapTelemetry.runningPlayerCount;
		CustomMapTelemetry.maxPlayersInMap = CustomMapTelemetry.runningPlayerCount;
	}

	private static void EndMetricsCapture()
	{
		if (!CustomMapTelemetry.metricsCaptureStarted)
		{
			return;
		}
		CustomMapTelemetry.metricsCaptureStarted = false;
		NetworkSystem.Instance.OnPlayerJoined -= CustomMapTelemetry.OnPlayerJoinedRoom;
		NetworkSystem.Instance.OnPlayerLeft -= CustomMapTelemetry.OnPlayerLeftRoom;
		CustomMapTelemetry.inPrivateRoom = NetworkSystem.Instance.InRoom && NetworkSystem.Instance.SessionIsPrivate;
		int num = Mathf.RoundToInt(Time.unscaledTime - CustomMapTelemetry.mapEnterTime);
		if (num < 30)
		{
			return;
		}
		if (CustomMapTelemetry.mapName.Equals("NULL") || CustomMapTelemetry.mapModId == 0L)
		{
			Debug.LogError("[CustomMapTelemetry::EndMetricsCapture] mapName or mapModID is invalid, throwing out this capture data...");
			return;
		}
		GorillaTelemetry.PostCustomMapTracking(CustomMapTelemetry.mapName, CustomMapTelemetry.mapModId, CustomMapTelemetry.mapCreatorUsername, CustomMapTelemetry.minPlayersInMap, CustomMapTelemetry.maxPlayersInMap, num, CustomMapTelemetry.inPrivateRoom);
	}

	private static void StartPerfCapture()
	{
		if (CustomMapTelemetry.perfCaptureStarted)
		{
			return;
		}
		CustomMapTelemetry.perfCaptureStarted = true;
		if (CustomMapTelemetry.instance.perfCaptureCoroutine != null)
		{
			CustomMapTelemetry.EndPerfCapture();
		}
		CustomMapTelemetry.drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 1, ProfilerRecorderOptions.Default);
		CustomMapTelemetry.LowestFPS = int.MaxValue;
		CustomMapTelemetry.HighestFPS = int.MinValue;
		CustomMapTelemetry.totalFPS = 0;
		CustomMapTelemetry.totalDrawCalls = 0;
		CustomMapTelemetry.totalPlayerCount = 0;
		CustomMapTelemetry.frameCounter = 0;
		CustomMapTelemetry.instance.perfCaptureCoroutine = CustomMapTelemetry.instance.StartCoroutine(CustomMapTelemetry.instance.CaptureMapPerformance());
	}

	private static void EndPerfCapture()
	{
		if (!CustomMapTelemetry.perfCaptureStarted)
		{
			return;
		}
		CustomMapTelemetry.perfCaptureStarted = false;
		if (CustomMapTelemetry.instance.perfCaptureCoroutine != null)
		{
			CustomMapTelemetry.instance.StopAllCoroutines();
			CustomMapTelemetry.instance.perfCaptureCoroutine = null;
		}
		CustomMapTelemetry.drawCallsRecorder.Dispose();
		if (CustomMapTelemetry.frameCounter == 0)
		{
			return;
		}
		int num = Mathf.RoundToInt(Time.unscaledTime - CustomMapTelemetry.mapEnterTime);
		CustomMapTelemetry.AverageFPS = CustomMapTelemetry.totalFPS / CustomMapTelemetry.frameCounter;
		CustomMapTelemetry.AverageDrawCalls = CustomMapTelemetry.totalDrawCalls / CustomMapTelemetry.frameCounter;
		CustomMapTelemetry.AveragePlayerCount = CustomMapTelemetry.totalPlayerCount / CustomMapTelemetry.frameCounter;
		if (num < 30)
		{
			return;
		}
		if (CustomMapTelemetry.mapName.Equals("NULL") || CustomMapTelemetry.mapModId == 0L)
		{
			Debug.LogError("[CustomMapTelemetry::EndPerfCapture] mapName or mapModID is invalid, throwing out this capture data...");
			return;
		}
		GorillaTelemetry.PostCustomMapPerformance(CustomMapTelemetry.mapName, CustomMapTelemetry.mapModId, CustomMapTelemetry.LowestFPS, CustomMapTelemetry.LowestFPSDrawCalls, CustomMapTelemetry.LowestFPSPlayerCount, CustomMapTelemetry.AverageFPS, CustomMapTelemetry.AverageDrawCalls, CustomMapTelemetry.AveragePlayerCount, CustomMapTelemetry.HighestFPS, CustomMapTelemetry.HighestFPSDrawCalls, CustomMapTelemetry.HighestFPSPlayerCount, num);
	}

	private IEnumerator CaptureMapPerformance()
	{
		for (;;)
		{
			int num = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
			int num2 = Mathf.RoundToInt((float)CustomMapTelemetry.drawCallsRecorder.LastValue);
			int roomPlayerCount = NetworkSystem.Instance.RoomPlayerCount;
			CustomMapTelemetry.totalFPS += num;
			CustomMapTelemetry.totalDrawCalls += num2;
			CustomMapTelemetry.totalPlayerCount += roomPlayerCount;
			if (num > CustomMapTelemetry.HighestFPS)
			{
				CustomMapTelemetry.HighestFPS = num;
				CustomMapTelemetry.HighestFPSDrawCalls = num2;
				CustomMapTelemetry.HighestFPSPlayerCount = roomPlayerCount;
			}
			if (num < CustomMapTelemetry.LowestFPS)
			{
				CustomMapTelemetry.LowestFPS = num;
				CustomMapTelemetry.LowestFPSDrawCalls = num2;
				CustomMapTelemetry.LowestFPSPlayerCount = roomPlayerCount;
			}
			CustomMapTelemetry.frameCounter++;
			yield return null;
		}
		yield break;
	}

	private void OnDestroy()
	{
		if (this.perfCaptureCoroutine != null)
		{
			CustomMapTelemetry.EndMapTracking();
		}
	}

	[OnEnterPlay_SetNull]
	private static volatile CustomMapTelemetry instance;

	private static string mapName;

	private static long mapModId;

	private static string mapCreatorUsername;

	private static bool metricsCaptureStarted;

	private static float mapEnterTime;

	private static int runningPlayerCount;

	private static int minPlayersInMap;

	private static int maxPlayersInMap;

	private static bool inPrivateRoom;

	private const int minimumPlaytimeForTracking = 30;

	private static int LowestFPS = int.MaxValue;

	private static int LowestFPSDrawCalls;

	private static int LowestFPSPlayerCount;

	private static int AverageFPS;

	private static int AverageDrawCalls;

	private static int AveragePlayerCount;

	private static int HighestFPS = int.MinValue;

	private static int HighestFPSDrawCalls;

	private static int HighestFPSPlayerCount;

	private static int totalFPS;

	private static int totalDrawCalls;

	private static int totalPlayerCount;

	private static int frameCounter;

	private Coroutine perfCaptureCoroutine;

	private static ProfilerRecorder drawCallsRecorder;

	private static bool perfCaptureStarted;
}

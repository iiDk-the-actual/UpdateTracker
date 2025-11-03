using System;
using UnityEngine;

public class GTDelayedExec : ITickSystemTick
{
	public static GTDelayedExec instance { get; private set; }

	public static int listenerCount { get; private set; }

	[OnEnterPlay_Run]
	private static void EdReInit()
	{
		GTDelayedExec._listenerDelays = new float[1024];
		GTDelayedExec._listeners = new GTDelayedExec.Listener[1024];
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void InitializeAfterAssemblies()
	{
		GTDelayedExec.listenerCount = 0;
		GTDelayedExec.instance = new GTDelayedExec();
		TickSystem<object>.AddTickCallback(GTDelayedExec.instance);
	}

	internal static void Add(IDelayedExecListener listener, float delay, int contextId)
	{
		if (GTDelayedExec.listenerCount >= GTDelayedExec.maxListenersCount)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"ERROR!!!  GTDelayedExec: Recovering from default maximum number of delayed listeners ",
				1024.ToString(),
				" reached. Please set the k_defaultMaxListenersCount value to ",
				(GTDelayedExec.maxListenersCount * 2).ToString(),
				"."
			}));
			GTDelayedExec.maxListenersCount *= 2;
			Array.Resize<float>(ref GTDelayedExec._listenerDelays, GTDelayedExec.maxListenersCount);
			Array.Resize<GTDelayedExec.Listener>(ref GTDelayedExec._listeners, GTDelayedExec.maxListenersCount);
		}
		GTDelayedExec._listenerDelays[GTDelayedExec.listenerCount] = Time.unscaledTime + delay;
		GTDelayedExec._listeners[GTDelayedExec.listenerCount] = new GTDelayedExec.Listener(listener, contextId);
		GTDelayedExec.listenerCount++;
	}

	bool ITickSystemTick.TickRunning { get; set; }

	void ITickSystemTick.Tick()
	{
		for (int i = 0; i < GTDelayedExec.listenerCount; i++)
		{
			if (Time.unscaledTime >= GTDelayedExec._listenerDelays[i])
			{
				try
				{
					GTDelayedExec._listeners[i].listener.OnDelayedAction(GTDelayedExec._listeners[i].contextId);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				GTDelayedExec.listenerCount--;
				GTDelayedExec._listenerDelays[i] = GTDelayedExec._listenerDelays[GTDelayedExec.listenerCount];
				GTDelayedExec._listeners[i] = GTDelayedExec._listeners[GTDelayedExec.listenerCount];
				i--;
			}
		}
	}

	public const int k_defaultMaxListenersCount = 1024;

	public static int maxListenersCount = 1024;

	private static float[] _listenerDelays = new float[1024];

	private static GTDelayedExec.Listener[] _listeners = new GTDelayedExec.Listener[1024];

	private struct Listener
	{
		public Listener(IDelayedExecListener listener, int contextId)
		{
			this.listener = listener;
			this.contextId = contextId;
		}

		public readonly IDelayedExecListener listener;

		public readonly int contextId;
	}
}

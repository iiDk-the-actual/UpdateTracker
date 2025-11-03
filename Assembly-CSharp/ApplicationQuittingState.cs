using System;
using UnityEngine;

public static class ApplicationQuittingState
{
	public static bool IsQuitting { get; private set; }

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		Application.quitting += ApplicationQuittingState.HandleApplicationQuitting;
	}

	private static void HandleApplicationQuitting()
	{
		ApplicationQuittingState.IsQuitting = true;
	}
}

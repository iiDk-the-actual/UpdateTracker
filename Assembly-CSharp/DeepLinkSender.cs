using System;
using UnityEngine;

public static class DeepLinkSender
{
	public static bool SendDeepLink(ulong deepLinkAppID, string deepLinkMessage, Action<string> onSent)
	{
		Debug.LogError("[DeepLinkSender::SendDeepLink] Called on non-oculus platform!");
		return false;
	}

	private static Action<string> currentDeepLinkSentCallback;
}

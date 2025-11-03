using System;
using UnityEngine;

public static class EchoUtils
{
	[HideInCallstack]
	public static T Echo<T>(this T message)
	{
		Debug.Log(message);
		return message;
	}

	[HideInCallstack]
	public static T Echo<T>(this T message, Object context)
	{
		Debug.Log(message, context);
		return message;
	}
}

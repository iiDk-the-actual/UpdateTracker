using System;
using System.Diagnostics;
using Cysharp.Text;
using Drawing;
using UnityEngine;

public static class GTDev
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void InitializeOnLoad()
	{
		GTDev.FetchDevID();
	}

	[HideInCallstack]
	public static void Log<T>(T msg, string channel = null)
	{
	}

	[HideInCallstack]
	public static void Log<T>(T msg, Object context, string channel = null)
	{
	}

	[HideInCallstack]
	public static void LogError<T>(T msg, string channel = null)
	{
	}

	[HideInCallstack]
	public static void LogError<T>(T msg, Object context, string channel = null)
	{
	}

	[HideInCallstack]
	public static void LogWarning<T>(T msg, string channel = null)
	{
	}

	[HideInCallstack]
	public static void LogWarning<T>(T msg, Object context, string channel = null)
	{
	}

	[HideInCallstack]
	public static void LogSilent<T>(T msg, string channel = null)
	{
	}

	[HideInCallstack]
	public static void LogSilent<T>(T msg, Object context, string channel = null)
	{
	}

	[HideInCallstack]
	[Conditional("UNITY_EDITOR")]
	public static void LogEditorOnly<T>(T msg, string channel = null)
	{
	}

	[HideInCallstack]
	[Conditional("UNITY_EDITOR")]
	public static void LogEditorOnly<T>(T msg, Object context, string channel = null)
	{
	}

	[HideInCallstack]
	[Conditional("BETA")]
	[Conditional("UNITY_EDITOR")]
	public static void LogBetaOnly<T>(T msg, string channel = null)
	{
	}

	[HideInCallstack]
	[Conditional("BETA")]
	[Conditional("UNITY_EDITOR")]
	public static void LogBetaOnly<T>(T msg, Object context, string channel = null)
	{
	}

	[HideInCallstack]
	[Conditional("UNITY_EDITOR")]
	public static void LogErrorEd<T>(T msg, string channel = null)
	{
	}

	[HideInCallstack]
	[Conditional("UNITY_EDITOR")]
	public static void LogErrorEd<T>(T msg, Object context, string channel = null)
	{
	}

	[HideInCallstack]
	[Conditional("BETA")]
	[Conditional("UNITY_EDITOR")]
	public static void LogErrorBeta<T>(T msg, string channel = null)
	{
	}

	[HideInCallstack]
	[Conditional("BETA")]
	[Conditional("UNITY_EDITOR")]
	public static void LogErrorBeta<T>(T msg, Object context, string channel = null)
	{
	}

	[HideInCallstack]
	[Conditional("UNITY_EDITOR")]
	public static void CallEditorOnly(Action call)
	{
	}

	public static int DevID
	{
		get
		{
			return GTDev.FetchDevID();
		}
	}

	private static int FetchDevID()
	{
		if (GTDev.gHasDevID)
		{
			return GTDev.gDevID;
		}
		int num = StaticHash.Compute(SystemInfo.deviceUniqueIdentifier);
		int num2 = StaticHash.Compute(Environment.UserDomainName);
		int num3 = StaticHash.Compute(Environment.UserName);
		int num4 = StaticHash.Compute(Application.unityVersion);
		GTDev.gDevID = StaticHash.Compute(num, num2, num3, num4);
		GTDev.gHasDevID = true;
		return GTDev.gDevID;
	}

	[HideInCallstack]
	[Conditional("_GTDEV_ON_")]
	private static void _Log<T>(Action<object, Object> log, Action<object> logNoCtx, T msg, Object ctx, string channel)
	{
	}

	private static Mesh SphereMesh()
	{
		if (!GTDev.gSphereMesh)
		{
			GTDev.gSphereMesh = Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx");
		}
		return GTDev.gSphereMesh;
	}

	[Conditional("_GTDEV_ON_")]
	public unsafe static void Ping3D(this Collider col, Color color = default(Color), float duration = 8f)
	{
		if (color == default(Color))
		{
			color = GTDev.gDefaultColor;
		}
		if (color.a.Approx0(1E-06f))
		{
			return;
		}
		Matrix4x4 localToWorldMatrix = col.transform.localToWorldMatrix;
		SRand srand = new SRand(localToWorldMatrix.QuantizedId128().GetHashCode());
		color.r = srand.NextFloat();
		color.g = srand.NextFloat();
		color.b = srand.NextFloat();
		CommandBuilder commandBuilder = *Draw.ingame;
		using (commandBuilder.WithDuration(duration))
		{
			commandBuilder.PushMatrix(localToWorldMatrix);
			commandBuilder.PushLineWidth(2f, true);
			commandBuilder.PushColor(color);
			BoxCollider boxCollider = col as BoxCollider;
			if (boxCollider == null)
			{
				SphereCollider sphereCollider = col as SphereCollider;
				if (sphereCollider == null)
				{
					CapsuleCollider capsuleCollider = col as CapsuleCollider;
					if (capsuleCollider != null)
					{
						commandBuilder.WireCapsule(capsuleCollider.center, Vector3.up, capsuleCollider.height, capsuleCollider.radius, color);
					}
				}
				else
				{
					commandBuilder.WireSphere(sphereCollider.center, sphereCollider.radius, color);
				}
			}
			else
			{
				commandBuilder.WireBox(boxCollider.center, boxCollider.size);
			}
			commandBuilder.Label2D(Vector3.zero, col.name, 16f, LabelAlignment.Center);
			commandBuilder.PopColor();
			commandBuilder.PopLineWidth();
			commandBuilder.PopMatrix();
		}
	}

	[Conditional("_GTDEV_ON_")]
	public unsafe static void Ping3D(this Vector3 vec, Color color = default(Color), float duration = 8f)
	{
		if (color == default(Color))
		{
			color = GTDev.gDefaultColor;
		}
		else
		{
			color.a = GTDev.gDefaultColor.a;
		}
		string text = ZString.Format<float, float, float>("{{ X: {0:##0.0000}, Y: {1:##0.0000}, Z: {2:##0.0000} }}", vec.x, vec.y, vec.z);
		CommandBuilder commandBuilder = *Draw.ingame;
		using (commandBuilder.WithDuration(duration))
		{
			using (commandBuilder.WithLineWidth(2f, true))
			{
				commandBuilder.Cross(vec, 0.64f, color);
			}
			commandBuilder.Label2D(vec + Vector3.down * 0.64f, text, 16f, LabelAlignment.Center, color);
		}
	}

	[Conditional("_GTDEV_ON_")]
	public unsafe static void Ping3D<T>(this T value, Vector3 position, Color color = default(Color), float duration = 8f)
	{
		if (color == default(Color))
		{
			color = GTDev.gDefaultColor;
		}
		string text = ZString.Concat<T>(value);
		CommandBuilder commandBuilder = *Draw.ingame;
		using (commandBuilder.WithDuration(duration))
		{
			commandBuilder.Label2D(position, text, 16f, LabelAlignment.Center, color);
		}
	}

	[OnEnterPlay_Set(0)]
	private static int gDevID;

	[OnEnterPlay_Set(false)]
	private static bool gHasDevID;

	private static readonly Color gDefaultColor = new Color(0f, 1f, 1f, 0.32f);

	private const string kFormatF = "{{ X: {0:##0.0000}, Y: {1:##0.0000}, Z: {2:##0.0000} }}";

	private const float kDuration = 8f;

	private static Mesh gSphereMesh;
}

using System;
using UnityEngine;

public class GTContactManager : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeOnLoad()
	{
	}

	private static GTContactPoint[] InitContactPoints(int count)
	{
		GTContactPoint[] array = new GTContactPoint[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = new GTContactPoint();
		}
		return array;
	}

	public static void RaiseContact(Vector3 point, Vector3 normal)
	{
		if (GTContactManager.gNextFree == -1)
		{
			return;
		}
		float time = GTShaderGlobals.Time;
		GTContactPoint gtcontactPoint = GTContactManager._gContactPoints[GTContactManager.gNextFree];
		gtcontactPoint.contactPoint = point;
		gtcontactPoint.radius = 0.04f;
		gtcontactPoint.counterVelocity = normal;
		gtcontactPoint.timestamp = time;
		gtcontactPoint.lifetime = 2f;
		gtcontactPoint.color = GTContactManager.gRND.NextColor();
		gtcontactPoint.free = 0U;
	}

	public static void ProcessContacts()
	{
		Matrix4x4[] shaderData = GTContactManager.ShaderData;
		GTContactPoint[] gContactPoints = GTContactManager._gContactPoints;
		int frame = GTShaderGlobals.Frame;
		for (int i = 0; i < 32; i++)
		{
			GTContactManager.Transfer(ref gContactPoints[i].data, ref shaderData[i]);
		}
	}

	private static void Transfer(ref Matrix4x4 from, ref Matrix4x4 to)
	{
		to.m00 = from.m00;
		to.m01 = from.m01;
		to.m02 = from.m02;
		to.m03 = from.m03;
		to.m10 = from.m10;
		to.m11 = from.m11;
		to.m12 = from.m12;
		to.m13 = from.m13;
		to.m20 = from.m20;
		to.m21 = from.m21;
		to.m22 = from.m22;
		to.m23 = from.m23;
		to.m30 = from.m30;
		to.m31 = from.m31;
		to.m32 = from.m32;
		to.m33 = from.m33;
	}

	public const int MAX_CONTACTS = 32;

	public static Matrix4x4[] ShaderData = new Matrix4x4[32];

	private static GTContactPoint[] _gContactPoints = GTContactManager.InitContactPoints(32);

	private static int gNextFree = 0;

	private static SRand gRND = new SRand(DateTime.UtcNow);
}

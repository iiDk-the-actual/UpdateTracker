using System;
using UnityEngine;

public class GTShaderGlobals : MonoBehaviour, IGorillaSliceableSimple
{
	public static Vector3 WorldSpaceCameraPos
	{
		get
		{
			return GTShaderGlobals.gMainCameraWorldPos;
		}
	}

	public static float Time
	{
		get
		{
			return GTShaderGlobals.gTime;
		}
	}

	public static int Frame
	{
		get
		{
			return GTShaderGlobals.gIFrame;
		}
	}

	private void Awake()
	{
		GTShaderGlobals.gMainCamera = Camera.main;
		if (GTShaderGlobals.gMainCamera)
		{
			GTShaderGlobals.gMainCameraXform = GTShaderGlobals.gMainCamera.transform;
			GTShaderGlobals.gMainCameraWorldPos = GTShaderGlobals.gMainCameraXform.position;
		}
		this.SliceUpdate();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		GTShaderGlobals.InitBlueNoiseTex();
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void SliceUpdate()
	{
		GTShaderGlobals.UpdateTime();
		GTShaderGlobals.UpdateFrame();
		GTShaderGlobals.UpdateCamera();
	}

	private static void UpdateFrame()
	{
		GTShaderGlobals.gIFrame = global::UnityEngine.Time.frameCount;
		Shader.SetGlobalInteger(GTShaderGlobals._GT_iFrame, GTShaderGlobals.gIFrame);
	}

	private static void UpdateCamera()
	{
		if (!GTShaderGlobals.gMainCameraXform)
		{
			return;
		}
		GTShaderGlobals.gMainCameraWorldPos = GTShaderGlobals.gMainCameraXform.position;
		Shader.SetGlobalVector(GTShaderGlobals._GT_WorldSpaceCameraPos, GTShaderGlobals.gMainCameraWorldPos);
	}

	private static void UpdateTime()
	{
		GTShaderGlobals.gTime = (float)(DateTime.UtcNow - GTShaderGlobals.gStartTime).TotalSeconds;
		Shader.SetGlobalFloat(GTShaderGlobals._GT_Time, GTShaderGlobals.gTime);
	}

	private static void UpdatePawns()
	{
		GTShaderGlobals.gActivePawns = GorillaPawn.ActiveCount;
		GorillaPawn.SyncPawnData();
		Shader.SetGlobalMatrixArray(GTShaderGlobals._GT_PawnData, GTShaderGlobals.gPawnData);
		Shader.SetGlobalInteger(GTShaderGlobals._GT_PawnActiveCount, GTShaderGlobals.gActivePawns);
	}

	private static void InitBlueNoiseTex()
	{
		GTShaderGlobals.gBlueNoiseTex = Resources.Load<Texture2D>("Graphics/Textures/noise_blue_rgba_128");
		GTShaderGlobals.gBlueNoiseTexWH = GTShaderGlobals.gBlueNoiseTex.GetTexelSize();
		Shader.SetGlobalTexture(GTShaderGlobals._GT_BlueNoiseTex, GTShaderGlobals.gBlueNoiseTex);
		Shader.SetGlobalVector(GTShaderGlobals._GT_BlueNoiseTex_WH, GTShaderGlobals.gBlueNoiseTexWH);
	}

	private static Camera gMainCamera;

	private static Transform gMainCameraXform;

	private static Vector3 gMainCameraWorldPos;

	[Space]
	private static int gIFrame;

	private static float gTime;

	[Space]
	private static Texture2D gBlueNoiseTex;

	private static Vector4 gBlueNoiseTexWH;

	[Space]
	private static int gActivePawns;

	[Space]
	private static DateTime gStartTime = DateTime.Today.AddDays(-1.0).ToUniversalTime();

	private static Matrix4x4[] gPawnData = GorillaPawn.ShaderData;

	private static ShaderHashId _GT_WorldSpaceCameraPos = "_GT_WorldSpaceCameraPos";

	private static ShaderHashId _GT_BlueNoiseTex = "_GT_BlueNoiseTex";

	private static ShaderHashId _GT_BlueNoiseTex_WH = "_GT_BlueNoiseTex_WH";

	private static ShaderHashId _GT_iFrame = "_GT_iFrame";

	private static ShaderHashId _GT_Time = "_GT_Time";

	private static ShaderHashId _GT_PawnData = "_GT_PawnData";

	private static ShaderHashId _GT_PawnActiveCount = "_GT_PawnActiveCount";
}

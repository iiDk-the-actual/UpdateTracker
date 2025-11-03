using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public static class GTUberShaderUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetStencilComparison(this Material m, GTShaderStencilCompare cmp)
	{
		m.SetFloat(GTUberShaderUtils._StencilComparison, (float)cmp);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetStencilPassFrontOp(this Material m, GTShaderStencilOp op)
	{
		m.SetFloat(GTUberShaderUtils._StencilPassFront, (float)op);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetStencilReferenceValue(this Material m, int value)
	{
		m.SetFloat(GTUberShaderUtils._StencilReference, (float)value);
	}

	public static void SetVisibleToXRay(this Material m, bool visible, bool saveToDisk = false)
	{
		GTShaderStencilCompare gtshaderStencilCompare = (visible ? GTShaderStencilCompare.Equal : GTShaderStencilCompare.NotEqual);
		GTShaderStencilOp gtshaderStencilOp = (visible ? GTShaderStencilOp.Replace : GTShaderStencilOp.Keep);
		m.SetStencilComparison(gtshaderStencilCompare);
		m.SetStencilPassFrontOp(gtshaderStencilOp);
		m.SetStencilReferenceValue(7);
	}

	public static void SetRevealsXRay(this Material m, bool reveals, bool changeQueue = true, bool saveToDisk = false)
	{
		m.SetFloat(GTUberShaderUtils._ZWrite, (float)(reveals ? 0 : 1));
		m.SetFloat(GTUberShaderUtils._ColorMask_, (float)(reveals ? 0 : 14));
		m.SetStencilComparison(GTShaderStencilCompare.Disabled);
		m.SetStencilPassFrontOp(reveals ? GTShaderStencilOp.Replace : GTShaderStencilOp.Keep);
		m.SetStencilReferenceValue(reveals ? 7 : 0);
		if (changeQueue)
		{
			int renderQueue = m.renderQueue;
			m.renderQueue = renderQueue + (reveals ? (-1) : 1);
		}
	}

	public static int GetNearestRenderQueue(this Material m, out RenderQueue queue)
	{
		int renderQueue = m.renderQueue;
		int num = -1;
		int num2 = int.MaxValue;
		for (int i = 0; i < GTUberShaderUtils.kRenderQueueInts.Length; i++)
		{
			int num3 = GTUberShaderUtils.kRenderQueueInts[i];
			int num4 = Math.Abs(num3 - renderQueue);
			if (num2 > num4)
			{
				num = num3;
				num2 = num4;
			}
		}
		queue = (RenderQueue)num;
		return num;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitOnLoad()
	{
		GTUberShaderUtils.kUberShader = Shader.Find("GorillaTag/UberShader");
	}

	private static Shader kUberShader;

	private static readonly ShaderHashId _StencilComparison = "_StencilComparison";

	private static readonly ShaderHashId _StencilPassFront = "_StencilPassFront";

	private static readonly ShaderHashId _StencilReference = "_StencilReference";

	private static readonly ShaderHashId _ColorMask_ = "_ColorMask_";

	private static readonly ShaderHashId _ManualZWrite = "_ManualZWrite";

	private static readonly ShaderHashId _ZWrite = "_ZWrite";

	private static readonly int[] kRenderQueueInts = new int[] { 1000, 2000, 2450, 2500, 3000, 4000 };
}

using System;
using Drawing;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class Xform : MonoBehaviour
{
	public float3 localExtents
	{
		get
		{
			return this.localScale * 0.5f;
		}
	}

	public Matrix4x4 LocalTRS()
	{
		return Matrix4x4.TRS(this.localPosition, this.localRotation, this.localScale);
	}

	public Matrix4x4 TRS()
	{
		if (this.parent.AsNull<Transform>() == null)
		{
			return this.LocalTRS();
		}
		return this.parent.localToWorldMatrix * this.LocalTRS();
	}

	private unsafe void Update()
	{
		Matrix4x4 matrix4x = this.TRS();
		CommandBuilder commandBuilder = *Draw.ingame;
		using (commandBuilder.WithMatrix(matrix4x))
		{
			using (commandBuilder.WithLineWidth(2f, true))
			{
				commandBuilder.PlaneWithNormal(Xform.AXIS_XR_RT * 0.5f, Xform.AXIS_XR_RT, Xform.F2_ONE, Xform.CR);
				commandBuilder.PlaneWithNormal(Xform.AXIS_YG_UP * 0.5f, Xform.AXIS_YG_UP, Xform.F2_ONE, Xform.CG);
				commandBuilder.PlaneWithNormal(Xform.AXIS_ZB_FW * 0.5f, Xform.AXIS_ZB_FW, Xform.F2_ONE, Xform.CB);
				commandBuilder.WireBox(float3.zero, quaternion.identity, 1f, this.displayColor);
			}
		}
	}

	public Transform parent;

	[Space]
	public Color displayColor = SRand.New().NextColor();

	[Space]
	public float3 localPosition = float3.zero;

	public float3 localScale = Vector3.one;

	public Quaternion localRotation = quaternion.identity;

	private static readonly float3 F3_ONE = 1f;

	private static readonly float2 F2_ONE = 1f;

	private static readonly float3 AXIS_ZB_FW = new float3(0f, 0f, 1f);

	private static readonly float3 AXIS_YG_UP = new float3(0f, 1f, 0f);

	private static readonly float3 AXIS_XR_RT = new float3(1f, 0f, 0f);

	private static readonly Color CR = new Color(1f, 0f, 0f, 0.24f);

	private static readonly Color CG = new Color(0f, 1f, 0f, 0.24f);

	private static readonly Color CB = new Color(0f, 0f, 1f, 0.24f);
}

using System;
using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class GizmoRenderer : MonoBehaviour
{
	private void Update()
	{
		this.RenderGizmos();
	}

	private unsafe void RenderGizmos()
	{
		if (this.renderMode == GizmoRenderer.RenderMode.Never)
		{
			return;
		}
		if (this.gizmos == null)
		{
			return;
		}
		int num = this.gizmos.Length;
		if (num == 0)
		{
			return;
		}
		CommandBuilder commandBuilder = *Draw.ingame;
		Transform transform = base.transform;
		for (int i = 0; i < num; i++)
		{
			GizmoRenderer.GizmoInfo gizmoInfo = this.gizmos[i];
			if (gizmoInfo.render)
			{
				Transform transform2 = (gizmoInfo.target ? gizmoInfo.target : transform);
				using (commandBuilder.InLocalSpace(transform2))
				{
					using (commandBuilder.WithLineWidth(gizmoInfo.lineWidth, false))
					{
						GizmoRenderer.gRenderFuncs[(int)gizmoInfo.type](commandBuilder, gizmoInfo);
					}
				}
			}
		}
	}

	private static void RenderPlaneWire(CommandBuilder draw, GizmoRenderer.GizmoInfo gizmo)
	{
		draw.WirePlane(gizmo.center, gizmo.rotation, gizmo.size.xz, gizmo.color);
	}

	private static void RenderPlaneSolid(CommandBuilder draw, GizmoRenderer.GizmoInfo gizmo)
	{
		draw.SolidPlane(gizmo.center, gizmo.rotation, gizmo.size.xz, gizmo.color);
	}

	private static void RenderGridWire(CommandBuilder draw, GizmoRenderer.GizmoInfo gizmo)
	{
		draw.WireGrid(gizmo.center, gizmo.rotation, gizmo.gridCells, gizmo.size.xz, gizmo.color);
	}

	private static void RenderBoxWire(CommandBuilder draw, GizmoRenderer.GizmoInfo gizmo)
	{
		draw.WireBox(gizmo.center, gizmo.rotation, gizmo.size, gizmo.color);
	}

	private static void RenderBoxSolid(CommandBuilder draw, GizmoRenderer.GizmoInfo gizmo)
	{
		draw.SolidBox(gizmo.center, gizmo.rotation, gizmo.size, gizmo.color);
	}

	private static void RenderSphereWire(CommandBuilder draw, GizmoRenderer.GizmoInfo gizmo)
	{
		draw.WireSphere(gizmo.center, gizmo.radius * 0.5f, gizmo.color);
	}

	private static void RenderSphereSolid(CommandBuilder draw, GizmoRenderer.GizmoInfo gizmo)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(gizmo.center, quaternion.identity, new float3(gizmo.radius));
		using (draw.WithMatrix(matrix4x))
		{
			draw.SolidMesh(GizmoRenderer.gSphereMesh, gizmo.color);
		}
	}

	private static void RenderLabel3D(CommandBuilder draw, GizmoRenderer.GizmoInfo gizmo)
	{
		draw.Label3D(gizmo.center, gizmo.rotation, gizmo.text, gizmo.textSize * 0.1f, GizmoRenderer.gLabelAligns[(int)gizmo.textAlign], gizmo.color);
	}

	private static void RenderLabel2D(CommandBuilder draw, GizmoRenderer.GizmoInfo gizmo)
	{
		draw.Label2D(gizmo.center, gizmo.text, gizmo.textSize * gizmo.textPPU, GizmoRenderer.gLabelAligns[(int)gizmo.textAlign], gizmo.color);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeOnLoad()
	{
		GizmoRenderer.gSphereMesh = Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx");
	}

	private static Color GetRandomColor()
	{
		Color color = Color.HSVToRGB((float)(DateTime.UtcNow.Ticks % 65536L) / 65535f, 1f, 1f, true);
		color.a = 1f;
		return color;
	}

	public GizmoRenderer.RenderMode renderMode = GizmoRenderer.RenderMode.Always;

	public bool includeInBuild;

	public GizmoRenderer.GizmoInfo[] gizmos = new GizmoRenderer.GizmoInfo[0];

	private static readonly Action<CommandBuilder, GizmoRenderer.GizmoInfo>[] gRenderFuncs = new Action<CommandBuilder, GizmoRenderer.GizmoInfo>[]
	{
		new Action<CommandBuilder, GizmoRenderer.GizmoInfo>(GizmoRenderer.RenderBoxWire),
		new Action<CommandBuilder, GizmoRenderer.GizmoInfo>(GizmoRenderer.RenderBoxSolid),
		new Action<CommandBuilder, GizmoRenderer.GizmoInfo>(GizmoRenderer.RenderSphereWire),
		new Action<CommandBuilder, GizmoRenderer.GizmoInfo>(GizmoRenderer.RenderSphereSolid),
		new Action<CommandBuilder, GizmoRenderer.GizmoInfo>(GizmoRenderer.RenderLabel3D),
		new Action<CommandBuilder, GizmoRenderer.GizmoInfo>(GizmoRenderer.RenderLabel2D),
		new Action<CommandBuilder, GizmoRenderer.GizmoInfo>(GizmoRenderer.RenderGridWire),
		new Action<CommandBuilder, GizmoRenderer.GizmoInfo>(GizmoRenderer.RenderPlaneSolid),
		new Action<CommandBuilder, GizmoRenderer.GizmoInfo>(GizmoRenderer.RenderPlaneWire)
	};

	private static readonly LabelAlignment[] gLabelAligns = new LabelAlignment[]
	{
		LabelAlignment.Center,
		LabelAlignment.MiddleRight,
		LabelAlignment.MiddleLeft,
		LabelAlignment.BottomCenter,
		LabelAlignment.BottomRight,
		LabelAlignment.BottomLeft,
		LabelAlignment.TopRight,
		LabelAlignment.TopLeft,
		LabelAlignment.TopCenter
	};

	private static Mesh gSphereMesh;

	[Serializable]
	public class GizmoInfo
	{
		public bool render = true;

		public GizmoRenderer.GizmoType type;

		public Color color = GizmoRenderer.GetRandomColor();

		public uint lineWidth = 1U;

		[Space]
		public Transform target;

		[Space]
		public float3 center = float3.zero;

		public float3 size = Vector3.one;

		public float radius = 1f;

		public quaternion rotation = quaternion.identity;

		[Space]
		public string text = string.Empty;

		public float textSize = 4f;

		public GizmoRenderer.TextAlign textAlign;

		public uint textPPU = 24U;

		[Space]
		public int2 gridCells = new int2(4);
	}

	[Flags]
	public enum RenderMode : uint
	{
		Never = 0U,
		InEditor = 1U,
		InBuild = 2U,
		Always = 3U
	}

	public enum GizmoType : uint
	{
		BoxWire,
		BoxSolid,
		SphereWire,
		SphereSolid,
		Label3D,
		Label2D,
		GridWire,
		PlaneSolid,
		PlaneWire
	}

	public enum TextAlign : uint
	{
		Center,
		MiddleRight,
		MiddleLeft,
		BottomCenter,
		BottomRight,
		BottomLeft,
		TopRight,
		TopLeft,
		TopCenter
	}
}

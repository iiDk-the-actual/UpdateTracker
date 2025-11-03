using System;
using System.Collections.Generic;
using UnityEngine;

namespace CjLib
{
	public class DebugUtil
	{
		private static Material GetMaterial(DebugUtil.Style style, bool depthTest, bool capShiftScale)
		{
			int num = 0;
			if (style - DebugUtil.Style.FlatShaded <= 1)
			{
				num |= 1;
			}
			if (capShiftScale)
			{
				num |= 2;
			}
			if (depthTest)
			{
				num |= 4;
			}
			if (DebugUtil.s_materialPool == null)
			{
				DebugUtil.s_materialPool = new Dictionary<int, Material>();
			}
			Material material;
			if (!DebugUtil.s_materialPool.TryGetValue(num, out material) || material == null)
			{
				if (material == null)
				{
					DebugUtil.s_materialPool.Remove(num);
				}
				Shader shader = Shader.Find(depthTest ? "CjLib/Primitive" : "CjLib/PrimitiveNoZTest");
				if (shader == null)
				{
					return null;
				}
				material = new Material(shader);
				if ((num & 1) != 0)
				{
					material.EnableKeyword("NORMAL_ON");
				}
				if ((num & 2) != 0)
				{
					material.EnableKeyword("CAP_SHIFT_SCALE");
				}
				DebugUtil.s_materialPool.Add(num, material);
			}
			return material;
		}

		private static MaterialPropertyBlock GetMaterialPropertyBlock()
		{
			if (DebugUtil.s_materialProperties == null)
			{
				return DebugUtil.s_materialProperties = new MaterialPropertyBlock();
			}
			return DebugUtil.s_materialProperties;
		}

		public static void DrawLine(Vector3 v0, Vector3 v1, Color color, bool depthTest = true)
		{
			Mesh mesh = PrimitiveMeshFactory.Line(v0, v1);
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(DebugUtil.Style.Wireframe, depthTest, false);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(1f, 1f, 1f, 0f));
			materialPropertyBlock.SetFloat("_ZBias", DebugUtil.s_wireframeZBias);
			Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawLines(Vector3[] aVert, Color color, bool depthTest = true)
		{
			Mesh mesh = PrimitiveMeshFactory.Lines(aVert);
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(DebugUtil.Style.Wireframe, depthTest, false);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(1f, 1f, 1f, 0f));
			materialPropertyBlock.SetFloat("_ZBias", DebugUtil.s_wireframeZBias);
			Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawLineStrip(Vector3[] aVert, Color color, bool depthTest = true)
		{
			Mesh mesh = PrimitiveMeshFactory.LineStrip(aVert);
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(DebugUtil.Style.Wireframe, depthTest, false);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(1f, 1f, 1f, 0f));
			materialPropertyBlock.SetFloat("_ZBias", DebugUtil.s_wireframeZBias);
			Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawArc(Vector3 center, Vector3 from, Vector3 normal, float angle, float radius, int numSegments, Color color, bool depthTest = true)
		{
			if (numSegments <= 0)
			{
				return;
			}
			from.Normalize();
			from *= radius;
			Vector3[] array = new Vector3[numSegments + 1];
			array[0] = center + from;
			float num = 1f / (float)numSegments;
			Quaternion quaternion = QuaternionUtil.AxisAngle(normal, angle * num);
			Vector3 vector = quaternion * from;
			for (int i = 1; i <= numSegments; i++)
			{
				array[i] = center + vector;
				vector = quaternion * vector;
			}
			DebugUtil.DrawLineStrip(array, color, depthTest);
		}

		public static void DrawLocator(Vector3 position, Vector3 right, Vector3 up, Vector3 forward, Color rightColor, Color upColor, Color forwardColor, float size = 0.5f)
		{
			DebugUtil.DrawLine(position, position + right * size, rightColor, true);
			DebugUtil.DrawLine(position, position + up * size, upColor, true);
			DebugUtil.DrawLine(position, position + forward * size, forwardColor, true);
		}

		public static void DrawLocator(Vector3 position, Vector3 right, Vector3 up, Vector3 forward, float size = 0.5f)
		{
			DebugUtil.DrawLocator(position, right, up, forward, Color.red, Color.green, Color.blue, size);
		}

		public static void DrawLocator(Vector3 position, Quaternion rotation, Color rightColor, Color upColor, Color forwardColor, float size = 0.5f)
		{
			Vector3 vector = rotation * Vector3.right;
			Vector3 vector2 = rotation * Vector3.up;
			Vector3 vector3 = rotation * Vector3.forward;
			DebugUtil.DrawLocator(position, vector, vector2, vector3, rightColor, upColor, forwardColor, size);
		}

		public static void DrawLocator(Vector3 position, Quaternion rotation, float size = 0.5f)
		{
			DebugUtil.DrawLocator(position, rotation, Color.red, Color.green, Color.blue, size);
		}

		public static void DrawBox(Vector3 center, Quaternion rotation, Vector3 dimensions, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			if (dimensions.x < MathUtil.Epsilon || dimensions.y < MathUtil.Epsilon || dimensions.z < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case DebugUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.BoxWireframe();
				break;
			case DebugUtil.Style.SolidColor:
				mesh = PrimitiveMeshFactory.BoxSolidColor();
				break;
			case DebugUtil.Style.FlatShaded:
			case DebugUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.BoxFlatShaded();
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(style, depthTest, false);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(dimensions.x, dimensions.y, dimensions.z, 0f));
			materialPropertyBlock.SetFloat("_ZBias", (style == DebugUtil.Style.Wireframe) ? DebugUtil.s_wireframeZBias : 0f);
			Graphics.DrawMesh(mesh, center, rotation, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawRect(Vector3 center, Quaternion rotation, Vector2 dimensions, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			if (dimensions.x < MathUtil.Epsilon || dimensions.y < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case DebugUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.RectWireframe();
				break;
			case DebugUtil.Style.SolidColor:
				mesh = PrimitiveMeshFactory.RectSolidColor();
				break;
			case DebugUtil.Style.FlatShaded:
			case DebugUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.RectFlatShaded();
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(style, depthTest, false);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(dimensions.x, 1f, dimensions.y, 0f));
			materialPropertyBlock.SetFloat("_ZBias", (style == DebugUtil.Style.Wireframe) ? DebugUtil.s_wireframeZBias : 0f);
			Graphics.DrawMesh(mesh, center, rotation, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawRect2D(Vector3 center, float rotationDeg, Vector2 dimensions, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			Quaternion quaternion = Quaternion.AngleAxis(rotationDeg, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.right);
			DebugUtil.DrawRect(center, quaternion, dimensions, color, depthTest, style);
		}

		public static void DrawCircle(Vector3 center, Quaternion rotation, float radius, int numSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			if (radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case DebugUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.CircleWireframe(numSegments);
				break;
			case DebugUtil.Style.SolidColor:
				mesh = PrimitiveMeshFactory.CircleSolidColor(numSegments);
				break;
			case DebugUtil.Style.FlatShaded:
			case DebugUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.CircleFlatShaded(numSegments);
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(style, depthTest, false);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(radius, radius, radius, 0f));
			materialPropertyBlock.SetFloat("_ZBias", (style == DebugUtil.Style.Wireframe) ? DebugUtil.s_wireframeZBias : 0f);
			Graphics.DrawMesh(mesh, center, rotation, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawCircle(Vector3 center, Vector3 normal, float radius, int numSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(Vector3.Cross((Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.5f) ? Vector3.up : Vector3.forward, normal)), normal);
			DebugUtil.DrawCircle(center, quaternion, radius, numSegments, color, depthTest, style);
		}

		public static void DrawCircle2D(Vector3 center, float radius, int numSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			DebugUtil.DrawCircle(center, Vector3.forward, radius, numSegments, color, depthTest, style);
		}

		public static void DrawCylinder(Vector3 center, Quaternion rotation, float height, float radius, int numSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			if (height < MathUtil.Epsilon || radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case DebugUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.CylinderWireframe(numSegments);
				break;
			case DebugUtil.Style.SolidColor:
				mesh = PrimitiveMeshFactory.CylinderSolidColor(numSegments);
				break;
			case DebugUtil.Style.FlatShaded:
				mesh = PrimitiveMeshFactory.CylinderFlatShaded(numSegments);
				break;
			case DebugUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.CylinderSmoothShaded(numSegments);
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(style, depthTest, true);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(radius, radius, radius, height));
			materialPropertyBlock.SetFloat("_ZBias", (style == DebugUtil.Style.Wireframe) ? DebugUtil.s_wireframeZBias : 0f);
			Graphics.DrawMesh(mesh, center, rotation, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawCylinder(Vector3 point0, Vector3 point1, float radius, int numSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			Vector3 vector = point1 - point0;
			float magnitude = vector.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return;
			}
			vector.Normalize();
			Vector3 vector2 = 0.5f * (point0 + point1);
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(Vector3.Cross((Mathf.Abs(Vector3.Dot(vector.normalized, Vector3.up)) < 0.5f) ? Vector3.up : Vector3.forward, vector)), vector);
			DebugUtil.DrawCylinder(vector2, quaternion, magnitude, radius, numSegments, color, depthTest, style);
		}

		public static void DrawSphere(Vector3 center, Quaternion rotation, float radius, int latSegments, int longSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			if (radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case DebugUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.SphereWireframe(latSegments, longSegments);
				break;
			case DebugUtil.Style.SolidColor:
				mesh = PrimitiveMeshFactory.SphereSolidColor(latSegments, longSegments);
				break;
			case DebugUtil.Style.FlatShaded:
				mesh = PrimitiveMeshFactory.SphereFlatShaded(latSegments, longSegments);
				break;
			case DebugUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.SphereSmoothShaded(latSegments, longSegments);
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(style, depthTest, false);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(radius, radius, radius, 0f));
			materialPropertyBlock.SetFloat("_ZBias", (style == DebugUtil.Style.Wireframe) ? DebugUtil.s_wireframeZBias : 0f);
			Graphics.DrawMesh(mesh, center, rotation, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawSphere(Vector3 center, float radius, int latSegments, int longSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			DebugUtil.DrawSphere(center, Quaternion.identity, radius, latSegments, longSegments, color, depthTest, style);
		}

		public static void DrawSphereTripleCircles(Vector3 center, Quaternion rotation, float radius, int numSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			Vector3 vector = rotation * Vector3.right;
			Vector3 vector2 = rotation * Vector3.up;
			Vector3 vector3 = rotation * Vector3.forward;
			DebugUtil.DrawCircle(center, vector, radius, numSegments, color, depthTest, style);
			DebugUtil.DrawCircle(center, vector2, radius, numSegments, color, depthTest, style);
			DebugUtil.DrawCircle(center, vector3, radius, numSegments, color, depthTest, style);
		}

		public static void DrawSphereTripleCircles(Vector3 center, float radius, int numSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			DebugUtil.DrawSphereTripleCircles(center, Quaternion.identity, radius, numSegments, color, depthTest, style);
		}

		public static void DrawCapsule(Vector3 center, Quaternion rotation, float height, float radius, int latSegmentsPerCap, int longSegmentsPerCap, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			if (height < MathUtil.Epsilon || radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case DebugUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.CapsuleWireframe(latSegmentsPerCap, longSegmentsPerCap, true, true, false);
				break;
			case DebugUtil.Style.SolidColor:
				mesh = PrimitiveMeshFactory.CapsuleSolidColor(latSegmentsPerCap, longSegmentsPerCap, true, true, false);
				break;
			case DebugUtil.Style.FlatShaded:
				mesh = PrimitiveMeshFactory.CapsuleFlatShaded(latSegmentsPerCap, longSegmentsPerCap, true, true, false);
				break;
			case DebugUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.CapsuleSmoothShaded(latSegmentsPerCap, longSegmentsPerCap, true, true, false);
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(style, depthTest, true);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(radius, radius, radius, height));
			materialPropertyBlock.SetFloat("_ZBias", (style == DebugUtil.Style.Wireframe) ? DebugUtil.s_wireframeZBias : 0f);
			Graphics.DrawMesh(mesh, center, rotation, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawCapsule(Vector3 point0, Vector3 point1, float radius, int latSegmentsPerCap, int longSegmentsPerCap, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			Vector3 vector = point1 - point0;
			float magnitude = vector.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return;
			}
			vector.Normalize();
			Vector3 vector2 = 0.5f * (point0 + point1);
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(Vector3.Cross((Mathf.Abs(Vector3.Dot(vector.normalized, Vector3.up)) < 0.5f) ? Vector3.up : Vector3.forward, vector)), vector);
			DebugUtil.DrawCapsule(vector2, quaternion, magnitude, radius, latSegmentsPerCap, longSegmentsPerCap, color, depthTest, style);
		}

		public static void DrawCapsule2D(Vector3 center, float rotationDeg, float height, float radius, int capSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			if (height < MathUtil.Epsilon || radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case DebugUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.Capsule2DWireframe(capSegments);
				break;
			case DebugUtil.Style.SolidColor:
				mesh = PrimitiveMeshFactory.Capsule2DSolidColor(capSegments);
				break;
			case DebugUtil.Style.FlatShaded:
			case DebugUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.Capsule2DFlatShaded(capSegments);
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(style, depthTest, true);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(radius, radius, radius, height));
			materialPropertyBlock.SetFloat("_ZBias", (style == DebugUtil.Style.Wireframe) ? DebugUtil.s_wireframeZBias : 0f);
			Graphics.DrawMesh(mesh, center, Quaternion.AngleAxis(rotationDeg, Vector3.forward), material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawCone(Vector3 baseCenter, Quaternion rotation, float height, float radius, int numSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			if (height < MathUtil.Epsilon || radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case DebugUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.ConeWireframe(numSegments);
				break;
			case DebugUtil.Style.SolidColor:
				mesh = PrimitiveMeshFactory.ConeSolidColor(numSegments);
				break;
			case DebugUtil.Style.FlatShaded:
				mesh = PrimitiveMeshFactory.ConeFlatShaded(numSegments);
				break;
			case DebugUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.ConeSmoothShaded(numSegments);
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Material material = DebugUtil.GetMaterial(style, depthTest, false);
			MaterialPropertyBlock materialPropertyBlock = DebugUtil.GetMaterialPropertyBlock();
			materialPropertyBlock.SetColor("_Color", color);
			materialPropertyBlock.SetVector("_Dimensions", new Vector4(radius, height, radius, 0f));
			materialPropertyBlock.SetFloat("_ZBias", (style == DebugUtil.Style.Wireframe) ? DebugUtil.s_wireframeZBias : 0f);
			Graphics.DrawMesh(mesh, baseCenter, rotation, material, 0, null, 0, materialPropertyBlock, false, false, false);
		}

		public static void DrawCone(Vector3 baseCenter, Vector3 top, float radius, int numSegments, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			Vector3 vector = top - baseCenter;
			float magnitude = vector.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return;
			}
			vector.Normalize();
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(Vector3.Cross((Mathf.Abs(Vector3.Dot(vector, Vector3.up)) < 0.5f) ? Vector3.up : Vector3.forward, vector)), vector);
			DebugUtil.DrawCone(baseCenter, quaternion, magnitude, radius, numSegments, color, depthTest, style);
		}

		public static void DrawArrow(Vector3 from, Vector3 to, float coneRadius, float coneHeight, int numSegments, float stemThickness, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			Vector3 vector = to - from;
			float magnitude = vector.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return;
			}
			vector.Normalize();
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(Vector3.Cross((Mathf.Abs(Vector3.Dot(vector, Vector3.up)) < 0.5f) ? Vector3.up : Vector3.forward, vector)), vector);
			DebugUtil.DrawCone(to - coneHeight * vector, quaternion, coneHeight, coneRadius, numSegments, color, depthTest, style);
			if (stemThickness <= 0f)
			{
				if (style == DebugUtil.Style.Wireframe)
				{
					to -= coneHeight * vector;
				}
				DebugUtil.DrawLine(from, to, color, depthTest);
				return;
			}
			if (coneHeight < magnitude)
			{
				to -= coneHeight * vector;
				DebugUtil.DrawCylinder(from, to, 0.5f * stemThickness, numSegments, color, depthTest, style);
			}
		}

		public static void DrawArrow(Vector3 from, Vector3 to, float size, Color color, bool depthTest = true, DebugUtil.Style style = DebugUtil.Style.Wireframe)
		{
			DebugUtil.DrawArrow(from, to, 0.5f * size, size, 8, 0f, color, depthTest, style);
		}

		private static float s_wireframeZBias = 0.0001f;

		private const int kNormalFlag = 1;

		private const int kCapShiftScaleFlag = 2;

		private const int kDepthTestFlag = 4;

		private static Dictionary<int, Material> s_materialPool;

		private static MaterialPropertyBlock s_materialProperties;

		public enum Style
		{
			Wireframe,
			SolidColor,
			FlatShaded,
			SmoothShaded
		}
	}
}

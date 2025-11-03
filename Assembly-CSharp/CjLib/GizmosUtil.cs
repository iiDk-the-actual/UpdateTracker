using System;
using UnityEngine;

namespace CjLib
{
	public class GizmosUtil
	{
		public static void DrawLine(Vector3 v0, Vector3 v1, Color color)
		{
			Gizmos.color = color;
			Gizmos.DrawLine(v0, v1);
		}

		public static void DrawLines(Vector3[] aVert, Color color)
		{
			Gizmos.color = color;
			for (int i = 0; i < aVert.Length; i += 2)
			{
				Gizmos.DrawLine(aVert[i], aVert[i + 1]);
			}
		}

		public static void DrawLineStrip(Vector3[] aVert, Color color)
		{
			Gizmos.color = color;
			for (int i = 0; i < aVert.Length; i++)
			{
				Gizmos.DrawLine(aVert[i], aVert[i + 1]);
			}
		}

		public static void DrawBox(Vector3 center, Quaternion rotation, Vector3 dimensions, Color color, GizmosUtil.Style style = GizmosUtil.Style.FlatShaded)
		{
			if (dimensions.x < MathUtil.Epsilon || dimensions.y < MathUtil.Epsilon || dimensions.z < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			if (style != GizmosUtil.Style.Wireframe)
			{
				if (style - GizmosUtil.Style.FlatShaded <= 1)
				{
					mesh = PrimitiveMeshFactory.BoxFlatShaded();
				}
			}
			else
			{
				mesh = PrimitiveMeshFactory.BoxWireframe();
			}
			if (mesh == null)
			{
				return;
			}
			Gizmos.color = color;
			if (style == GizmosUtil.Style.Wireframe)
			{
				Gizmos.DrawWireMesh(mesh, center, rotation, dimensions);
				return;
			}
			Gizmos.DrawMesh(mesh, center, rotation, dimensions);
		}

		public static void DrawCylinder(Vector3 center, Quaternion rotation, float height, float radius, int numSegments, Color color, GizmosUtil.Style style = GizmosUtil.Style.SmoothShaded)
		{
			if (height < MathUtil.Epsilon || radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case GizmosUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.CylinderWireframe(numSegments);
				break;
			case GizmosUtil.Style.FlatShaded:
				mesh = PrimitiveMeshFactory.CylinderFlatShaded(numSegments);
				break;
			case GizmosUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.CylinderSmoothShaded(numSegments);
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Gizmos.color = color;
			if (style == GizmosUtil.Style.Wireframe)
			{
				Gizmos.DrawWireMesh(mesh, center, rotation, new Vector3(radius, height, radius));
				return;
			}
			Gizmos.DrawMesh(mesh, center, rotation, new Vector3(radius, height, radius));
		}

		public static void DrawCylinder(Vector3 point0, Vector3 point1, float radius, int numSegments, Color color, GizmosUtil.Style style = GizmosUtil.Style.SmoothShaded)
		{
			Vector3 vector = point1 - point0;
			float magnitude = vector.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return;
			}
			vector.Normalize();
			Vector3 vector2 = 0.5f * (point0 + point1);
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(Vector3.Cross((Vector3.Dot(vector.normalized, Vector3.up) < 0.5f) ? Vector3.up : Vector3.forward, vector)), vector);
			GizmosUtil.DrawCylinder(vector2, quaternion, magnitude, radius, numSegments, color, style);
		}

		public static void DrawSphere(Vector3 center, Quaternion rotation, float radius, int latSegments, int longSegments, Color color, GizmosUtil.Style style = GizmosUtil.Style.SmoothShaded)
		{
			if (radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case GizmosUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.SphereWireframe(latSegments, longSegments);
				break;
			case GizmosUtil.Style.FlatShaded:
				mesh = PrimitiveMeshFactory.SphereFlatShaded(latSegments, longSegments);
				break;
			case GizmosUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.SphereSmoothShaded(latSegments, longSegments);
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Gizmos.color = color;
			if (style == GizmosUtil.Style.Wireframe)
			{
				Gizmos.DrawWireMesh(mesh, center, rotation, new Vector3(radius, radius, radius));
				return;
			}
			Gizmos.DrawMesh(mesh, center, rotation, new Vector3(radius, radius, radius));
		}

		public static void DrawSphere(Vector3 center, float radius, int latSegments, int longSegments, Color color, GizmosUtil.Style style = GizmosUtil.Style.SmoothShaded)
		{
			GizmosUtil.DrawSphere(center, Quaternion.identity, radius, latSegments, longSegments, color, style);
		}

		public static void DrawCapsule(Vector3 center, Quaternion rotation, float height, float radius, int latSegmentsPerCap, int longSegmentsPerCap, Color color, GizmosUtil.Style style = GizmosUtil.Style.SmoothShaded)
		{
			if (height < MathUtil.Epsilon || radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			Mesh mesh2 = null;
			switch (style)
			{
			case GizmosUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.CapsuleWireframe(latSegmentsPerCap, longSegmentsPerCap, true, true, false);
				mesh2 = PrimitiveMeshFactory.CapsuleWireframe(latSegmentsPerCap, longSegmentsPerCap, false, false, true);
				break;
			case GizmosUtil.Style.FlatShaded:
				mesh = PrimitiveMeshFactory.CapsuleFlatShaded(latSegmentsPerCap, longSegmentsPerCap, true, true, false);
				mesh2 = PrimitiveMeshFactory.CapsuleFlatShaded(latSegmentsPerCap, longSegmentsPerCap, false, false, true);
				break;
			case GizmosUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.CapsuleSmoothShaded(latSegmentsPerCap, longSegmentsPerCap, true, true, false);
				mesh2 = PrimitiveMeshFactory.CapsuleSmoothShaded(latSegmentsPerCap, longSegmentsPerCap, false, false, true);
				break;
			}
			if (mesh == null || mesh2 == null)
			{
				return;
			}
			Vector3 vector = rotation * Vector3.up;
			Vector3 vector2 = 0.5f * (height - radius) * vector;
			Vector3 vector3 = center + vector2;
			Vector3 vector4 = center - vector2;
			Quaternion quaternion = Quaternion.AngleAxis(180f, vector) * rotation;
			Gizmos.color = color;
			if (style == GizmosUtil.Style.Wireframe)
			{
				Gizmos.DrawWireMesh(mesh, vector3, rotation, new Vector3(radius, radius, radius));
				Gizmos.DrawWireMesh(mesh, vector4, quaternion, new Vector3(-radius, -radius, radius));
				Gizmos.DrawWireMesh(mesh2, center, rotation, new Vector3(radius, height, radius));
				return;
			}
			Gizmos.DrawMesh(mesh, vector3, rotation, new Vector3(radius, radius, radius));
			Gizmos.DrawMesh(mesh, vector4, quaternion, new Vector3(-radius, -radius, radius));
			Gizmos.DrawMesh(mesh2, center, rotation, new Vector3(radius, height, radius));
		}

		public static void DrawCapsule(Vector3 point0, Vector3 point1, float radius, int latSegmentsPerCap, int longSegmentsPerCap, Color color, GizmosUtil.Style style = GizmosUtil.Style.SmoothShaded)
		{
			Vector3 vector = point1 - point0;
			float magnitude = vector.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return;
			}
			vector.Normalize();
			Vector3 vector2 = 0.5f * (point0 + point1);
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(Vector3.Cross((Vector3.Dot(vector.normalized, Vector3.up) < 0.5f) ? Vector3.up : Vector3.forward, vector)), vector);
			GizmosUtil.DrawCapsule(vector2, quaternion, magnitude, radius, latSegmentsPerCap, longSegmentsPerCap, color, style);
		}

		public static void DrawCone(Vector3 baseCenter, Quaternion rotation, float height, float radius, int numSegments, Color color, GizmosUtil.Style style = GizmosUtil.Style.FlatShaded)
		{
			if (height < MathUtil.Epsilon || radius < MathUtil.Epsilon)
			{
				return;
			}
			Mesh mesh = null;
			switch (style)
			{
			case GizmosUtil.Style.Wireframe:
				mesh = PrimitiveMeshFactory.ConeWireframe(numSegments);
				break;
			case GizmosUtil.Style.FlatShaded:
				mesh = PrimitiveMeshFactory.ConeFlatShaded(numSegments);
				break;
			case GizmosUtil.Style.SmoothShaded:
				mesh = PrimitiveMeshFactory.ConeSmoothShaded(numSegments);
				break;
			}
			if (mesh == null)
			{
				return;
			}
			Gizmos.color = color;
			if (style == GizmosUtil.Style.Wireframe)
			{
				Gizmos.DrawWireMesh(mesh, baseCenter, rotation, new Vector3(radius, height, radius));
				return;
			}
			Gizmos.DrawMesh(mesh, baseCenter, rotation, new Vector3(radius, height, radius));
		}

		public static void DrawCone(Vector3 baseCenter, Vector3 top, float radius, int numSegments, Color color, GizmosUtil.Style style = GizmosUtil.Style.FlatShaded)
		{
			Vector3 vector = top - baseCenter;
			float magnitude = vector.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return;
			}
			vector.Normalize();
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(Vector3.Cross((Vector3.Dot(vector, Vector3.up) < 0.5f) ? Vector3.up : Vector3.forward, vector)), vector);
			GizmosUtil.DrawCone(baseCenter, quaternion, magnitude, radius, numSegments, color, style);
		}

		public static void DrawArrow(Vector3 from, Vector3 to, float coneRadius, float coneHeight, int numSegments, float stemThickness, Color color, GizmosUtil.Style style = GizmosUtil.Style.FlatShaded)
		{
			Vector3 vector = to - from;
			float magnitude = vector.magnitude;
			if (magnitude < MathUtil.Epsilon)
			{
				return;
			}
			vector.Normalize();
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(Vector3.Cross((Vector3.Dot(vector, Vector3.up) < 0.5f) ? Vector3.up : Vector3.forward, vector)), vector);
			GizmosUtil.DrawCone(to - coneHeight * vector, quaternion, coneHeight, coneRadius, numSegments, color, style);
			if (stemThickness <= 0f)
			{
				if (style != GizmosUtil.Style.Wireframe)
				{
					to -= coneHeight * vector;
				}
				GizmosUtil.DrawLine(from, to, color);
				return;
			}
			if (coneHeight < magnitude)
			{
				to -= coneHeight * vector;
				GizmosUtil.DrawCylinder(from, to, 0.5f * stemThickness, numSegments, color, style);
			}
		}

		public static void DrawArrow(Vector3 from, Vector3 to, float size, Color color, GizmosUtil.Style style = GizmosUtil.Style.FlatShaded)
		{
			GizmosUtil.DrawArrow(from, to, 0.5f * size, size, 8, 0f, color, style);
		}

		public enum Style
		{
			Wireframe,
			FlatShaded,
			SmoothShaded
		}
	}
}

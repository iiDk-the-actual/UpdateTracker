using System;
using UnityEngine;

namespace CjLib
{
	[ExecuteInEditMode]
	public class DrawArrow : DrawBase
	{
		private void OnValidate()
		{
			this.ConeRadius = Mathf.Max(0f, this.ConeRadius);
			this.ConeHeight = Mathf.Max(0f, this.ConeHeight);
			this.StemThickness = Mathf.Max(0f, this.StemThickness);
			this.NumSegments = Mathf.Max(4, this.NumSegments);
		}

		protected override void Draw(Color color, DebugUtil.Style style, bool depthTest)
		{
			DebugUtil.DrawArrow(base.transform.position, base.transform.position + base.transform.TransformVector(this.LocalEndVector), this.ConeRadius, this.ConeHeight, this.NumSegments, this.StemThickness, color, depthTest, style);
		}

		public Vector3 LocalEndVector = Vector3.right;

		public float ConeRadius = 0.05f;

		public float ConeHeight = 0.1f;

		public float StemThickness = 0.05f;

		public int NumSegments = 8;
	}
}

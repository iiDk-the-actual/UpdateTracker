using System;
using UnityEngine;

namespace CjLib
{
	[ExecuteInEditMode]
	public class DrawArc : DrawBase
	{
		private void OnValidate()
		{
			this.Wireframe = true;
			this.Style = DebugUtil.Style.Wireframe;
			this.Radius = Mathf.Max(0f, this.Radius);
			this.NumSegments = Mathf.Max(0, this.NumSegments);
		}

		protected override void Draw(Color color, DebugUtil.Style style, bool depthTest)
		{
			Quaternion quaternion = QuaternionUtil.AxisAngle(Vector3.forward, this.StartAngle * MathUtil.Deg2Rad);
			DebugUtil.DrawArc(base.transform.position, base.transform.rotation * quaternion * Vector3.right, base.transform.rotation * Vector3.forward, this.ArcAngle * MathUtil.Deg2Rad, this.Radius, this.NumSegments, color, depthTest);
		}

		public float Radius = 1f;

		public int NumSegments = 64;

		public float StartAngle;

		public float ArcAngle = 60f;
	}
}

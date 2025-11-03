using System;
using UnityEngine;

namespace CjLib
{
	[ExecuteInEditMode]
	public class DrawCircle : DrawBase
	{
		private void OnValidate()
		{
			this.Radius = Mathf.Max(0f, this.Radius);
			this.NumSegments = Mathf.Max(0, this.NumSegments);
		}

		protected override void Draw(Color color, DebugUtil.Style style, bool depthTest)
		{
			DebugUtil.DrawCircle(base.transform.position, base.transform.rotation * Vector3.back, this.Radius, this.NumSegments, color, depthTest, style);
		}

		public float Radius = 1f;

		public int NumSegments = 64;
	}
}

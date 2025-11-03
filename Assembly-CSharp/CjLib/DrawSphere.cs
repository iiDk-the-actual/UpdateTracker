using System;
using UnityEngine;

namespace CjLib
{
	[ExecuteInEditMode]
	public class DrawSphere : DrawBase
	{
		private void OnValidate()
		{
			this.Radius = Mathf.Max(0f, this.Radius);
			this.LatSegments = Mathf.Max(0, this.LatSegments);
		}

		protected override void Draw(Color color, DebugUtil.Style style, bool depthTest)
		{
			DebugUtil.DrawSphere(base.transform.position, base.transform.rotation, this.Radius * base.transform.lossyScale.x, this.LatSegments, this.LongSegments, color, depthTest, style);
		}

		public float Radius = 1f;

		public int LatSegments = 12;

		public int LongSegments = 12;
	}
}

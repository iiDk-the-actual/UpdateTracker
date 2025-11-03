using System;
using UnityEngine;

namespace CjLib
{
	[ExecuteInEditMode]
	public class DrawLine : DrawBase
	{
		private void OnValidate()
		{
			this.Wireframe = true;
			this.Style = DebugUtil.Style.Wireframe;
		}

		protected override void Draw(Color color, DebugUtil.Style style, bool depthTest)
		{
			DebugUtil.DrawLine(base.transform.position, base.transform.position + base.transform.TransformVector(this.LocalEndVector), color, depthTest);
		}

		public Vector3 LocalEndVector = Vector3.right;
	}
}

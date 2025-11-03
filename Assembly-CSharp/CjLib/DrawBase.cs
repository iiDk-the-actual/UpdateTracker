using System;
using UnityEngine;

namespace CjLib
{
	public abstract class DrawBase : MonoBehaviour
	{
		private void Update()
		{
			if (this.Style != DebugUtil.Style.Wireframe)
			{
				this.Draw(this.ShadededColor, this.Style, this.DepthTest);
			}
			if (this.Style == DebugUtil.Style.Wireframe || this.Wireframe)
			{
				this.Draw(this.WireframeColor, DebugUtil.Style.Wireframe, this.DepthTest);
			}
		}

		protected abstract void Draw(Color color, DebugUtil.Style style, bool depthTest);

		public Color WireframeColor = Color.white;

		public Color ShadededColor = Color.gray;

		public bool Wireframe;

		public DebugUtil.Style Style = DebugUtil.Style.FlatShaded;

		public bool DepthTest = true;
	}
}

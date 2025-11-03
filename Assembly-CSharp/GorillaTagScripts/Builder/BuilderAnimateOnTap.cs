using System;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderAnimateOnTap : BuilderPieceTappable
	{
		public override void OnTapReplicated()
		{
			base.OnTapReplicated();
			this.anim.Rewind();
			this.anim.Play();
		}

		[SerializeField]
		private Animation anim;
	}
}

using System;
using System.Collections;
using UnityEngine;

namespace GorillaTag
{
	public class CustomMapTestingScript : GorillaPressableButton
	{
		public override void ButtonActivation()
		{
			base.ButtonActivation();
			base.StartCoroutine(this.ButtonPressed_Local());
		}

		private IEnumerator ButtonPressed_Local()
		{
			this.isOn = true;
			this.UpdateColor();
			yield return new WaitForSeconds(this.debounceTime);
			this.isOn = false;
			this.UpdateColor();
			yield break;
		}
	}
}

using System;
using UnityEngine;

public class NativeSizeChangerButton : GorillaPressableButton
{
	public override void ButtonActivation()
	{
		this.nativeSizeChanger.Activate(this.settings);
	}

	[SerializeField]
	private NativeSizeChanger nativeSizeChanger;

	[SerializeField]
	private NativeSizeChangerSettings settings;
}

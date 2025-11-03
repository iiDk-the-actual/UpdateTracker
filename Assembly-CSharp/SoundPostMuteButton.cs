using System;
using UnityEngine;

public class SoundPostMuteButton : GorillaPressableButton
{
	public override void ButtonActivation()
	{
		base.ButtonActivation();
		if (!this.IsDummyButton)
		{
			SynchedMusicController[] array = this.musicControllers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].MuteAudio(this);
			}
			return;
		}
		if (this._targetMuteButton != null)
		{
			this._targetMuteButton.ButtonActivation();
		}
	}

	public SynchedMusicController[] musicControllers;

	[Tooltip("If true, then this button will passthrough clicks to a connected SoundPostMuteButton.")]
	public bool IsDummyButton;

	[SerializeField]
	[Tooltip("The targetted SoundPostMuteButton if this is a dummy button.")]
	private SoundPostMuteButton _targetMuteButton;
}

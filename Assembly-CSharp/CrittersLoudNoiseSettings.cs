using System;

public class CrittersLoudNoiseSettings : CrittersActorSettings
{
	public override void UpdateActorSettings()
	{
		base.UpdateActorSettings();
		CrittersLoudNoise crittersLoudNoise = (CrittersLoudNoise)this.parentActor;
		crittersLoudNoise.soundVolume = this._soundVolume;
		crittersLoudNoise.soundDuration = this._soundDuration;
		crittersLoudNoise.soundEnabled = this._soundEnabled;
		crittersLoudNoise.disableWhenSoundDisabled = this._disableWhenSoundDisabled;
		crittersLoudNoise.volumeFearAttractionMultiplier = this._volumeFearAttractionMultiplier;
	}

	public float _soundVolume;

	public float _soundDuration;

	public bool _soundEnabled;

	public bool _disableWhenSoundDisabled;

	public float _volumeFearAttractionMultiplier = 1f;
}

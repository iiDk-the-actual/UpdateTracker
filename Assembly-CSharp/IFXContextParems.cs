using System;

public interface IFXContextParems<T> where T : FXSArgs
{
	FXSystemSettings settings { get; }

	void OnPlayFX(T parems);
}

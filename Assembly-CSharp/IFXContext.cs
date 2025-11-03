using System;

public interface IFXContext
{
	FXSystemSettings settings { get; }

	void OnPlayFX();
}

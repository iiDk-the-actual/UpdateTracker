using System;

public interface IFXEffectContext<T> where T : IFXEffectContextObject
{
	T effectContext { get; }

	FXSystemSettings settings { get; }
}

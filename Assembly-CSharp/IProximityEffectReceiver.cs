using System;

public interface IProximityEffectReceiver
{
	void OnProximityCalculated(float distance, float alignment, float parallel);
}

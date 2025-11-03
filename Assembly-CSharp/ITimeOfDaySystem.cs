using System;

public interface ITimeOfDaySystem
{
	double currentTimeInSeconds { get; }

	double totalTimeInSeconds { get; }
}

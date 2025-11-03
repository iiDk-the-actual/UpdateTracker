using System;

internal interface ITickSystemTick
{
	bool TickRunning { get; set; }

	void Tick();
}

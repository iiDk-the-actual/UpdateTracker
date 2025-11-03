using System;

internal interface ITickSystemPre
{
	bool PreTickRunning { get; set; }

	void PreTick();
}

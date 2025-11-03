using System;

internal interface ITickSystemPost
{
	bool PostTickRunning { get; set; }

	void PostTick();
}

using System;

internal interface IDelayedExecListener
{
	void OnDelayedAction(int contextId);
}

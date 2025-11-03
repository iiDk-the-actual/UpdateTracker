using System;
using System.Collections.Generic;

public interface IGameEntityDebugComponent
{
	void GetDebugTextLines(out List<string> strings);
}

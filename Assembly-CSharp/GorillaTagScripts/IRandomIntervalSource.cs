using System;

namespace GorillaTagScripts
{
	public interface IRandomIntervalSource
	{
		float GetNextIntervalSeconds();
	}
}

using System;

namespace GorillaTagScripts.GhostReactor.SoakTasks
{
	public interface IGhostReactorSoakTask
	{
		bool Complete { get; }

		bool Update();

		void Reset();
	}
}

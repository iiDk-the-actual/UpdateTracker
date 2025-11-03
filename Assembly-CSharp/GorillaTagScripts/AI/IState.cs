using System;

namespace GorillaTagScripts.AI
{
	public interface IState
	{
		void Tick();

		void OnEnter();

		void OnExit();
	}
}

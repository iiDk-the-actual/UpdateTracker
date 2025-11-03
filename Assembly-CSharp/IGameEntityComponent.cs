using System;

public interface IGameEntityComponent
{
	void OnEntityInit();

	void OnEntityDestroy();

	void OnEntityStateChange(long prevState, long newState);
}

using System;

public interface IBuilderPieceFunctional
{
	void OnStateChanged(byte newState, NetPlayer instigator, int timeStamp);

	void OnStateRequest(byte newState, NetPlayer instigator, int timeStamp);

	bool IsStateValid(byte state);

	void FunctionalPieceUpdate();

	void FunctionalPieceFixedUpdate()
	{
		throw new NotImplementedException();
	}

	float GetInteractionDistace()
	{
		return 2.5f;
	}
}

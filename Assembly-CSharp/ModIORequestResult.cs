using System;
using Modio;

public struct ModIORequestResult
{
	public static ModIORequestResult CreateFailureResult(string inMessage)
	{
		ModIORequestResult modIORequestResult;
		modIORequestResult.success = false;
		modIORequestResult.message = inMessage;
		return modIORequestResult;
	}

	public static ModIORequestResult CreateSuccessResult()
	{
		ModIORequestResult modIORequestResult;
		modIORequestResult.success = true;
		modIORequestResult.message = "";
		return modIORequestResult;
	}

	public static ModIORequestResult CreateFromError(Error error)
	{
		ModIORequestResult modIORequestResult;
		if (error)
		{
			modIORequestResult.success = false;
			modIORequestResult.message = error.GetMessage();
		}
		else
		{
			modIORequestResult.success = true;
			modIORequestResult.message = "";
		}
		return modIORequestResult;
	}

	public bool success;

	public string message;
}

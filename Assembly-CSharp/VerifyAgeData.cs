using System;

public class VerifyAgeData
{
	public VerifyAgeData(VerifyAgeResponse response, int? age)
	{
		if (response == null)
		{
			return;
		}
		this.Status = response.Status;
		if (response.Session == null && response.DefaultSession == null)
		{
			return;
		}
		this.Session = new TMPSession(response.Session, response.DefaultSession, age, this.Status);
	}

	public readonly SessionStatus Status;

	public readonly TMPSession Session;
}

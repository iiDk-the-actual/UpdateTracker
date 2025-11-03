using System;

[Serializable]
public class SendChallengeEmailRequest : KIDRequestData
{
	public string Email;

	public string Locale;
}

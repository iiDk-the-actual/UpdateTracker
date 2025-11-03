using System;
using System.Runtime.CompilerServices;
using KID.Model;

public class VerifyAgeResponse
{
	public SessionStatus Status { get; set; }

	[Nullable(2)]
	public Session Session
	{
		[NullableContext(2)]
		get;
		[NullableContext(2)]
		set;
	}

	public KIDDefaultSession DefaultSession { get; set; }
}

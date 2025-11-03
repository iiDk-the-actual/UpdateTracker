using System;

[Serializable]
public class ErrorContent
{
	public string Message { get; set; }

	public string Error { get; set; }

	public override string ToString()
	{
		return "Error: " + this.Error + ", Message: " + this.Message;
	}
}

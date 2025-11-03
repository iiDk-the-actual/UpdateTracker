using System;
using System.Threading.Tasks;

public class ProgressionUtil
{
	public static async Task WaitForMothershipSessionToken()
	{
		while (MothershipClientContext.Token.IsNullOrEmpty() || MothershipClientContext.MothershipId.IsNullOrEmpty())
		{
			await Task.Yield();
			await Task.Delay(1000);
		}
	}
}

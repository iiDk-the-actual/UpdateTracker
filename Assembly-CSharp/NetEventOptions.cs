using System;
using Photon.Realtime;

public class NetEventOptions
{
	public bool HasWebHooks
	{
		get
		{
			return this.Flags != WebFlags.Default;
		}
	}

	public NetEventOptions()
	{
	}

	public NetEventOptions(int reciever, int[] actors, byte flags)
	{
		this.Reciever = (NetEventOptions.RecieverTarget)reciever;
		this.TargetActors = actors;
		this.Flags = new WebFlags(flags);
	}

	public NetEventOptions.RecieverTarget Reciever;

	public int[] TargetActors;

	public WebFlags Flags = WebFlags.Default;

	public enum RecieverTarget
	{
		others,
		all,
		master
	}
}

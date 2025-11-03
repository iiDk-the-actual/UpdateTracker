using System;
using Photon.Pun;
using UnityEngine;

[Serializable]
public class CallLimiter
{
	public CallLimiter()
	{
	}

	public CallLimiter(int historyLength, float coolDown, float latencyMax = 0.5f)
	{
		this.callTimeHistory = new float[historyLength];
		this.callHistoryLength = historyLength;
		for (int i = 0; i < historyLength; i++)
		{
			this.callTimeHistory[i] = float.MinValue;
		}
		this.timeCooldown = coolDown;
		this.maxLatency = (double)latencyMax;
	}

	public bool CheckCallServerTime(double time)
	{
		double currentTime = PhotonNetwork.CurrentTime;
		double num = this.maxLatency;
		double num2 = 4294967.295 - this.maxLatency;
		double num3;
		if (currentTime > num || time < num)
		{
			if (time > currentTime + 0.05)
			{
				return false;
			}
			num3 = currentTime - time;
		}
		else
		{
			double num4 = num2 + currentTime;
			if (time > currentTime + 0.05 && time < num4)
			{
				return false;
			}
			num3 = currentTime + (4294967.295 - time);
		}
		if (num3 > this.maxLatency)
		{
			return false;
		}
		int num5 = ((this.oldTimeIndex > 0) ? (this.oldTimeIndex - 1) : (this.callHistoryLength - 1));
		double num6 = (double)this.callTimeHistory[num5];
		if (num6 > num2 && time < num6)
		{
			this.Reset();
		}
		else if (time < num6)
		{
			return false;
		}
		return this.CheckCallTime((float)time);
	}

	public virtual bool CheckCallTime(float time)
	{
		if (this.callTimeHistory[this.oldTimeIndex] > time)
		{
			this.blockCall = true;
			this.blockStartTime = time;
			return false;
		}
		this.callTimeHistory[this.oldTimeIndex] = time + this.timeCooldown;
		int num = this.oldTimeIndex + 1;
		this.oldTimeIndex = num;
		this.oldTimeIndex = num % this.callHistoryLength;
		this.blockCall = false;
		return true;
	}

	public virtual void Reset()
	{
		if (this.callTimeHistory == null)
		{
			return;
		}
		for (int i = 0; i < this.callHistoryLength; i++)
		{
			this.callTimeHistory[i] = float.MinValue;
		}
		this.oldTimeIndex = 0;
		this.blockStartTime = 0f;
		this.blockCall = false;
	}

	protected const double k_serverMaxTime = 4294967.295;

	[SerializeField]
	protected float[] callTimeHistory;

	[Space]
	[SerializeField]
	protected int callHistoryLength;

	[SerializeField]
	protected float timeCooldown;

	[SerializeField]
	protected double maxLatency;

	private int oldTimeIndex;

	protected bool blockCall;

	protected float blockStartTime;
}

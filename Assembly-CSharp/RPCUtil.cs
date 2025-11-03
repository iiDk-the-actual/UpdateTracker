using System;
using System.Collections.Generic;
using UnityEngine;

internal class RPCUtil
{
	public static bool NotSpam(string id, PhotonMessageInfoWrapped info, float delay)
	{
		RPCUtil.RPCCallID rpccallID = new RPCUtil.RPCCallID(id, info.senderID);
		if (!RPCUtil.RPCCallLog.ContainsKey(rpccallID))
		{
			RPCUtil.RPCCallLog.Add(rpccallID, Time.time);
			return true;
		}
		if (Time.time - RPCUtil.RPCCallLog[rpccallID] > delay)
		{
			RPCUtil.RPCCallLog[rpccallID] = Time.time;
			return true;
		}
		return false;
	}

	public static bool SafeValue(float v)
	{
		return !float.IsNaN(v) && float.IsFinite(v);
	}

	public static bool SafeValue(float v, float min, float max)
	{
		return RPCUtil.SafeValue(v) && v <= max && v >= min;
	}

	private static Dictionary<RPCUtil.RPCCallID, float> RPCCallLog = new Dictionary<RPCUtil.RPCCallID, float>();

	private struct RPCCallID : IEquatable<RPCUtil.RPCCallID>
	{
		public RPCCallID(string nameOfFunction, int senderId)
		{
			this._senderID = senderId;
			this._nameOfFunction = nameOfFunction;
		}

		public readonly int SenderID
		{
			get
			{
				return this._senderID;
			}
		}

		public readonly string NameOfFunction
		{
			get
			{
				return this._nameOfFunction;
			}
		}

		bool IEquatable<RPCUtil.RPCCallID>.Equals(RPCUtil.RPCCallID other)
		{
			return other.NameOfFunction.Equals(this.NameOfFunction) && other.SenderID.Equals(this.SenderID);
		}

		private int _senderID;

		private string _nameOfFunction;
	}
}

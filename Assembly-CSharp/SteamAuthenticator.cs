using System;
using System.Text;
using Steamworks;
using UnityEngine;

public class SteamAuthenticator : MonoBehaviour
{
	public HAuthTicket GetAuthTicket(Action<string> successCallback, Action<EResult> failureCallback)
	{
		HAuthTicket ticketHandle = HAuthTicket.Invalid;
		Callback<GetAuthSessionTicketResponse_t> ticketCallback = null;
		byte[] ticketBlob = new byte[1024];
		uint ticketSize = 0U;
		ticketCallback = Callback<GetAuthSessionTicketResponse_t>.Create(delegate(GetAuthSessionTicketResponse_t response)
		{
			if (response.m_hAuthTicket != ticketHandle)
			{
				return;
			}
			ticketCallback.Dispose();
			ticketCallback = null;
			if (response.m_eResult != EResult.k_EResultOK)
			{
				Action<EResult> failureCallback3 = failureCallback;
				if (failureCallback3 == null)
				{
					return;
				}
				failureCallback3(response.m_eResult);
				return;
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (uint num = 0U; num < ticketSize; num += 1U)
				{
					stringBuilder.AppendFormat("{0:x2}", ticketBlob[(int)num]);
				}
				Action<string> successCallback2 = successCallback;
				if (successCallback2 == null)
				{
					return;
				}
				successCallback2(stringBuilder.ToString());
				return;
			}
		});
		SteamNetworkingIdentity steamNetworkingIdentity = default(SteamNetworkingIdentity);
		ticketHandle = SteamUser.GetAuthSessionTicket(ticketBlob, ticketBlob.Length, out ticketSize, ref steamNetworkingIdentity);
		if (ticketHandle == HAuthTicket.Invalid)
		{
			Action<EResult> failureCallback2 = failureCallback;
			if (failureCallback2 != null)
			{
				failureCallback2(EResult.k_EResultFail);
			}
		}
		return ticketHandle;
	}

	public HAuthTicket GetAuthTicketForWebApi(string authenticatorId, Action<string> successCallback, Action<EResult> failureCallback)
	{
		HAuthTicket ticketHandle = HAuthTicket.Invalid;
		Callback<GetTicketForWebApiResponse_t> ticketCallback = null;
		ticketCallback = Callback<GetTicketForWebApiResponse_t>.Create(delegate(GetTicketForWebApiResponse_t response)
		{
			if (response.m_hAuthTicket != ticketHandle)
			{
				return;
			}
			ticketCallback.Dispose();
			ticketCallback = null;
			if (response.m_eResult != EResult.k_EResultOK)
			{
				Action<EResult> failureCallback3 = failureCallback;
				if (failureCallback3 == null)
				{
					return;
				}
				failureCallback3(response.m_eResult);
				return;
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < response.m_cubTicket; i++)
				{
					stringBuilder.AppendFormat("{0:x2}", response.m_rgubTicket[i]);
				}
				Action<string> successCallback2 = successCallback;
				if (successCallback2 == null)
				{
					return;
				}
				successCallback2(stringBuilder.ToString());
				return;
			}
		});
		ticketHandle = SteamUser.GetAuthTicketForWebApi(authenticatorId);
		if (ticketHandle == HAuthTicket.Invalid)
		{
			Action<EResult> failureCallback2 = failureCallback;
			if (failureCallback2 != null)
			{
				failureCallback2(EResult.k_EResultFail);
			}
		}
		return ticketHandle;
	}
}

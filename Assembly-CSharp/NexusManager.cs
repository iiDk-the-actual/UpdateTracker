using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using NexusSDK;
using UnityEngine;
using UnityEngine.Networking;

public class NexusManager : MonoBehaviour
{
	private void Awake()
	{
		if (NexusManager.instance == null)
		{
			NexusManager.instance = this;
			return;
		}
		Object.Destroy(this);
	}

	private void Start()
	{
		SDKInitializer.Init(this.publicApiKey, this.environment);
	}

	public static IEnumerator GetMembers(NexusManager.GetMembersRequest RequestParams, Action<AttributionAPI.GetMembers200Response> onSuccess, Action<string> onFailure)
	{
		string text = SDKInitializer.ApiBaseUrl + "/manage/members";
		List<string> list = new List<string>();
		if (RequestParams.page != 0)
		{
			list.Add("page=" + RequestParams.page.ToString());
		}
		if (RequestParams.pageSize != 0)
		{
			list.Add("pageSize=" + RequestParams.pageSize.ToString());
		}
		text += "?";
		text += string.Join("&", list);
		using (UnityWebRequest webRequest = UnityWebRequest.Get(text))
		{
			webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
			yield return webRequest.SendWebRequest();
			if (webRequest.responseCode == 200L)
			{
				AttributionAPI.GetMembers200Response getMembers200Response = JsonConvert.DeserializeObject<AttributionAPI.GetMembers200Response>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				if (onSuccess != null)
				{
					onSuccess(getMembers200Response);
				}
			}
			else if (onFailure != null)
			{
				onFailure(webRequest.error);
			}
		}
		UnityWebRequest webRequest = null;
		yield break;
		yield break;
	}

	public void VerifyCreatorCode(string code, Action<Member> onSuccess, Action onFailure)
	{
		NexusManager.GetMemberByCodeRequest getMemberByCodeRequest = new NexusManager.GetMemberByCodeRequest
		{
			memberCode = code
		};
		base.StartCoroutine(NexusManager.GetMemberByCode(getMemberByCodeRequest, onSuccess, onFailure));
	}

	public static IEnumerator GetMemberByCode(NexusManager.GetMemberByCodeRequest RequestParams, Action<Member> onSuccess, Action onFailure)
	{
		string text = SDKInitializer.ApiBaseUrl + "/manage/members/{memberCode}";
		text = text.Replace("{memberCode}", RequestParams.memberCode);
		List<string> list = new List<string>();
		text += "?";
		text += string.Join("&", list);
		using (UnityWebRequest webRequest = UnityWebRequest.Get(text))
		{
			webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
			yield return webRequest.SendWebRequest();
			if (webRequest.responseCode == 200L)
			{
				Member member = JsonConvert.DeserializeObject<Member>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				if (onSuccess != null)
				{
					onSuccess(member);
				}
			}
			else if (onFailure != null)
			{
				onFailure();
			}
		}
		UnityWebRequest webRequest = null;
		yield break;
		yield break;
	}

	private string publicApiKey = "nexus_pk_4c18dcb1531846c7abad4cb00c5242bb";

	private string environment = "production";

	public static NexusManager instance;

	private Member[] validatedMembers;

	[Serializable]
	public struct GetMemberByCodeRequest
	{
		public string memberCode { readonly get; set; }

		public string groupId { readonly get; set; }
	}

	[Serializable]
	public struct GetMembersRequest
	{
		public int page { readonly get; set; }

		public int pageSize { readonly get; set; }
	}
}

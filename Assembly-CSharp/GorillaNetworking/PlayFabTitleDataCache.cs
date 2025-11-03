using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using LitJson;
using PlayFab;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaNetworking
{
	public class PlayFabTitleDataCache : MonoBehaviour
	{
		public static PlayFabTitleDataCache Instance { get; private set; }

		private static string FilePath
		{
			get
			{
				return Path.Combine(Application.persistentDataPath, "TitleDataCache.json");
			}
		}

		public void GetTitleData(string name, Action<string> callback, Action<PlayFabError> errorCallback, bool ignoreCache = false)
		{
			Dictionary<string, string> dictionary;
			string text;
			if (!ignoreCache && !this.isFirstLoad && this.localizedTitleData.TryGetValue(LocalisationManager.CurrentLanguage.Identifier.Code, out dictionary) && dictionary.TryGetValue(name, out text))
			{
				callback.SafeInvoke(text);
				return;
			}
			PlayFabTitleDataCache.DataRequest dataRequest = new PlayFabTitleDataCache.DataRequest
			{
				Name = name,
				Callback = callback,
				ErrorCallback = errorCallback
			};
			this.requests.Add(dataRequest);
			this.TryUpdateData();
		}

		private void Awake()
		{
			if (PlayFabTitleDataCache.Instance != null)
			{
				Object.Destroy(this);
				return;
			}
			PlayFabTitleDataCache.Instance = this;
		}

		private void Start()
		{
			this.UpdateData();
			LocalisationManager.RegisterOnLanguageChanged(new Action(this.TryUpdateData));
		}

		private void OnDestroy()
		{
			LocalisationManager.UnregisterOnLanguageChanged(new Action(this.TryUpdateData));
		}

		private void TryUpdateData()
		{
			if (!this.isFirstLoad && this.updateDataCoroutine == null)
			{
				this.UpdateData();
			}
		}

		public CacheImport LoadDataFromFile()
		{
			CacheImport cacheImport;
			try
			{
				if (!File.Exists(PlayFabTitleDataCache.FilePath))
				{
					Debug.LogWarning("[PlayFabTitleDataCache::LoadDataFromFile] Title data file " + PlayFabTitleDataCache.FilePath + " does not exist!");
					cacheImport = null;
				}
				else
				{
					cacheImport = JsonMapper.ToObject<CacheImport>(File.ReadAllText(PlayFabTitleDataCache.FilePath)) ?? new CacheImport();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("[PlayFabTitleDataCache::LoadDataFromFile] Error reading PlayFab title data from file: {0}", ex));
				cacheImport = null;
			}
			return cacheImport;
		}

		private static void SaveDataToFile(string filepath, Dictionary<string, Dictionary<string, string>> titleData)
		{
			try
			{
				string text = JsonMapper.ToJson(new CacheImport
				{
					DeploymentId = MothershipClientApiUnity.DeploymentId,
					TitleData = titleData
				});
				File.WriteAllText(filepath, text);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("[PlayFabTitleDataCache::SaveDataToFile] Error writing PlayFab title data to file: {0}", ex));
			}
		}

		public void UpdateData()
		{
			this.updateDataCoroutine = base.StartCoroutine(this.UpdateDataCo());
		}

		private IEnumerator UpdateDataCo()
		{
			try
			{
				PlayFabTitleDataCache.<>c__DisplayClass23_0 CS$<>8__locals1 = new PlayFabTitleDataCache.<>c__DisplayClass23_0();
				CacheImport oldCache = this.LoadDataFromFile();
				string currentLocale = LocalisationManager.CurrentLanguage.Identifier.Code;
				Dictionary<string, string> titleData;
				if (!this.localizedTitleData.TryGetValue(currentLocale, out titleData))
				{
					this.localizedTitleData[currentLocale] = new Dictionary<string, string>();
					titleData = this.localizedTitleData[currentLocale];
				}
				Dictionary<string, string> oldLocalizedCache;
				if (oldCache == null || oldCache.TitleData == null || !oldCache.TitleData.TryGetValue(currentLocale, out oldLocalizedCache))
				{
					oldLocalizedCache = new Dictionary<string, string>();
				}
				yield return new WaitUntil(() => MothershipClientApiUnity.IsClientLoggedIn());
				bool wipeOldData = oldCache == null || oldCache.DeploymentId != MothershipClientApiUnity.DeploymentId;
				CS$<>8__locals1.newTitleData = null;
				CS$<>8__locals1.mothershipError = null;
				Stopwatch sw = Stopwatch.StartNew();
				Debug.Log("[PlayFabTitleDataCache::UpdateDataCo] Starting Mothership API call");
				StringVector stringVector = new StringVector();
				foreach (PlayFabTitleDataCache.DataRequest dataRequest in this.requests)
				{
					stringVector.Add(dataRequest.Name);
				}
				CS$<>8__locals1.finished = false;
				Debug.Log("[PlayFabTitleDataCache::UpdateDataCo] Keys to fetch: " + string.Join(", ", stringVector));
				Debug.Log(string.Format("[PlayFabTitleDataCache::UpdateDataCo] Calling MothershipClientApiUnity.ListMothershipTitleData with TitleId={0}, EnvironmentId={1}, DeploymentId={2}, keys count={3}", new object[]
				{
					MothershipClientApiUnity.TitleId,
					MothershipClientApiUnity.EnvironmentId,
					MothershipClientApiUnity.DeploymentId,
					stringVector.Count
				}));
				if (!MothershipClientApiUnity.ListMothershipTitleData(MothershipClientApiUnity.TitleId, MothershipClientApiUnity.EnvironmentId, MothershipClientApiUnity.DeploymentId, stringVector, delegate(ListClientMothershipTitleDataResponse response)
				{
					string text6 = "[PlayFabTitleDataCache::UpdateDataCo] Mothership API success callback - Response: {0}, Results: {1}";
					object obj = response != null;
					int? num;
					if (response == null)
					{
						num = null;
					}
					else
					{
						TitleDataShortVector results = response.Results;
						num = ((results != null) ? new int?(results.Count) : null);
					}
					int? num2 = num;
					Debug.Log(string.Format(text6, obj, num2.GetValueOrDefault()));
					if (response != null && response.Results != null)
					{
						CS$<>8__locals1.newTitleData = new Dictionary<string, string>();
						for (int j = 0; j < response.Results.Count; j++)
						{
							MothershipTitleDataShort mothershipTitleDataShort = response.Results[j];
							string text7 = "[PlayFabTitleDataCache::UpdateDataCo] Processing title data item {0}: key='{1}', data length={2}";
							object obj2 = j;
							object key = mothershipTitleDataShort.key;
							string data = mothershipTitleDataShort.data;
							Debug.Log(string.Format(text7, obj2, key, (data != null) ? data.Length : 0));
							if (!string.IsNullOrEmpty(mothershipTitleDataShort.key))
							{
								CS$<>8__locals1.newTitleData[mothershipTitleDataShort.key] = mothershipTitleDataShort.data;
							}
						}
						CS$<>8__locals1.mothershipError = null;
						Debug.Log(string.Format("[PlayFabTitleDataCache::UpdateDataCo] Successfully processed {0} title data items", CS$<>8__locals1.newTitleData.Count));
					}
					else
					{
						CS$<>8__locals1.mothershipError = "Failed to fetch title data - response or results were null";
						Debug.LogError("[PlayFabTitleDataCache::UpdateDataCo] " + CS$<>8__locals1.mothershipError);
					}
					CS$<>8__locals1.finished = true;
				}, delegate(MothershipError error, int statusCode)
				{
					CS$<>8__locals1.mothershipError = string.Format("Error fetching title data: {0} (Status: {1})", ((error != null) ? error.Message : null) ?? "Unknown error", statusCode);
					Debug.LogError("[PlayFabTitleDataCache::UpdateDataCo] Mothership API error callback - " + CS$<>8__locals1.mothershipError);
					CS$<>8__locals1.finished = true;
				}))
				{
					CS$<>8__locals1.mothershipError = "Mothership API call was not sent.";
					Debug.LogError("[PlayFabTitleDataCache::UpdateDataCo] " + CS$<>8__locals1.mothershipError);
				}
				Debug.Log("[PlayFabTitleDataCache::UpdateDataCo] Waiting for Mothership API response");
				yield return new WaitUntil(() => CS$<>8__locals1.finished);
				Debug.Log(string.Format("[PlayFabTitleDataCache::UpdateDataCo] {0:N5}s", sw.Elapsed.TotalSeconds));
				if (CS$<>8__locals1.newTitleData != null)
				{
					Debug.Log(string.Format("[PlayFabTitleDataCache::UpdateDataCo] Processing {0} new title data items", CS$<>8__locals1.newTitleData.Count));
					if (wipeOldData)
					{
						this.localizedTitleData.Clear();
						this.localizedTitleData[currentLocale] = new Dictionary<string, string>();
						titleData = this.localizedTitleData[currentLocale];
					}
					if (!this.localesUpdated.ContainsKey(currentLocale))
					{
						titleData.Clear();
					}
					foreach (KeyValuePair<string, string> keyValuePair in CS$<>8__locals1.newTitleData)
					{
						string text;
						string text2;
						keyValuePair.Deconstruct(out text, out text2);
						string text3 = text;
						string text4 = text2;
						Debug.Log("[PlayFabTitleDataCache::UpdateDataCo] Updating title data key: " + text3);
						titleData[text3] = text4;
						for (int i = this.requests.Count - 1; i >= 0; i--)
						{
							PlayFabTitleDataCache.DataRequest dataRequest2 = this.requests[i];
							if (dataRequest2.Name == text3)
							{
								Action<string> callback = dataRequest2.Callback;
								if (callback != null)
								{
									callback(text4);
								}
								this.requests.RemoveAt(i);
								break;
							}
						}
						string text5;
						if (oldLocalizedCache.TryGetValue(text3, out text5) && text5 != text4)
						{
							PlayFabTitleDataCache.DataUpdate onTitleDataUpdate = this.OnTitleDataUpdate;
							if (onTitleDataUpdate != null)
							{
								onTitleDataUpdate.Invoke(text3);
							}
						}
					}
					this.localesUpdated[currentLocale] = true;
					PlayFabTitleDataCache.SaveDataToFile(PlayFabTitleDataCache.FilePath, this.localizedTitleData);
				}
				CS$<>8__locals1 = null;
				oldCache = null;
				currentLocale = null;
				titleData = null;
				oldLocalizedCache = null;
				sw = null;
			}
			finally
			{
				this.ClearRequestWithError(null);
				this.isFirstLoad = false;
				this.updateDataCoroutine = null;
			}
			yield break;
			yield break;
		}

		private static string MD5(string value)
		{
			HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider();
			byte[] bytes = Encoding.Default.GetBytes(value);
			byte[] array = hashAlgorithm.ComputeHash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in array)
			{
				stringBuilder.Append(b.ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		private void ClearRequestWithError(PlayFabError e = null)
		{
			if (e == null)
			{
				e = new PlayFabError();
			}
			foreach (PlayFabTitleDataCache.DataRequest dataRequest in this.requests)
			{
				dataRequest.ErrorCallback.SafeInvoke(e);
			}
			this.requests.Clear();
		}

		public PlayFabTitleDataCache.DataUpdate OnTitleDataUpdate;

		private const string FileName = "TitleDataCache.json";

		private readonly List<PlayFabTitleDataCache.DataRequest> requests = new List<PlayFabTitleDataCache.DataRequest>();

		private Dictionary<string, Dictionary<string, string>> localizedTitleData = new Dictionary<string, Dictionary<string, string>>();

		private Dictionary<string, bool> localesUpdated = new Dictionary<string, bool>();

		private bool isFirstLoad = true;

		private Coroutine updateDataCoroutine;

		[Serializable]
		public sealed class DataUpdate : UnityEvent<string>
		{
		}

		private class DataRequest
		{
			public string Name { get; set; }

			public Action<string> Callback { get; set; }

			public Action<PlayFabError> ErrorCallback { get; set; }
		}
	}
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
	public IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(this._delay);
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(this._sceneName, LoadSceneMode.Single);
		while (asyncOperation.progress < 0.99f)
		{
			yield return null;
		}
		asyncOperation.allowSceneActivation = true;
		yield break;
	}

	[SerializeField]
	private float _delay;

	[SerializeField]
	private string _sceneName;
}

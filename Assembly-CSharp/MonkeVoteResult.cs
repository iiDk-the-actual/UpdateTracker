using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MonkeVoteResult : MonoBehaviour
{
	public string Text
	{
		get
		{
			return this._text;
		}
		set
		{
			TMP_Text optionText = this._optionText;
			this._text = value;
			optionText.text = value;
		}
	}

	public void ShowResult(string questionOption, int percentage, bool showVote, bool showPrediction, bool isWinner)
	{
		this._optionText.text = questionOption;
		this._optionIndicator.SetActive(true);
		this._scoreText.text = ((percentage >= 0) ? string.Format("{0}%", percentage) : "--");
		this._voteIndicator.SetActive(showVote);
		this._guessWinIndicator.SetActive(showPrediction && isWinner);
		this._guessLoseIndicator.SetActive(showPrediction && !isWinner);
		this._youWinIndicator.SetActive(isWinner && showPrediction);
		this._mostPopularIndicator.SetActive(isWinner);
		this.ShowRockPile(percentage);
	}

	public void HideResult()
	{
		this._optionIndicator.SetActive(false);
		this._voteIndicator.SetActive(false);
		this._guessWinIndicator.SetActive(false);
		this._guessLoseIndicator.SetActive(false);
		this._youWinIndicator.SetActive(false);
		this._mostPopularIndicator.SetActive(false);
		this.ShowRockPile(0);
	}

	private void ShowRockPile(int percentage)
	{
		this._rockPiles.Show(percentage);
	}

	public void SetDynamicMeshesVisible(bool visible)
	{
		this._mostPopularIndicator.SetActive(visible);
		this._voteIndicator.SetActive(visible);
		this._guessWinIndicator.SetActive(visible);
		this._guessLoseIndicator.SetActive(visible);
		this._rockPiles.Show(visible ? 100 : (-1));
	}

	[SerializeField]
	private GameObject _optionIndicator;

	[SerializeField]
	private TMP_Text _optionText;

	[FormerlySerializedAs("_scoreLabelPost")]
	[SerializeField]
	private GameObject _scoreIndicator;

	[SerializeField]
	private TMP_Text _scoreText;

	[SerializeField]
	private GameObject _voteIndicator;

	[SerializeField]
	private GameObject _guessWinIndicator;

	[SerializeField]
	private GameObject _guessLoseIndicator;

	[SerializeField]
	private GameObject _mostPopularIndicator;

	[SerializeField]
	private GameObject _youWinIndicator;

	[SerializeField]
	private RockPiles _rockPiles;

	private MonkeVoteMachine _machine;

	private string _text = string.Empty;

	private bool _canVote;

	private float _rockPileHeight;
}

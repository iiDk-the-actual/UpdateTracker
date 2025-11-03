using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GameObjectScheduling;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MonkeVoteMachine : MonoBehaviour
{
	private void Reset()
	{
		this.Configure();
	}

	private void Awake()
	{
		this._proximityTrigger.OnEnter += this.OnPlayerEnteredVoteProximity;
	}

	private void Start()
	{
		MonkeVoteController.instance.OnPollsUpdated += this.HandleOnPollsUpdated;
		MonkeVoteController.instance.OnVoteAccepted += this.HandleOnVoteAccepted;
		MonkeVoteController.instance.OnVoteFailed += this.HandleOnVoteFailed;
		MonkeVoteController.instance.OnCurrentPollEnded += this.HandleCurrentPollEnded;
		this.Init();
	}

	private void OnDestroy()
	{
		this._proximityTrigger.OnEnter -= this.OnPlayerEnteredVoteProximity;
		MonkeVoteController.instance.OnPollsUpdated -= this.HandleOnPollsUpdated;
		MonkeVoteController.instance.OnVoteAccepted -= this.HandleOnVoteAccepted;
		MonkeVoteController.instance.OnVoteFailed -= this.HandleOnVoteFailed;
		MonkeVoteController.instance.OnCurrentPollEnded -= this.HandleCurrentPollEnded;
	}

	public void Init()
	{
		this._isTestingPoll = false;
		this._previousPoll = (this._currentPoll = null);
		this._waitingOnVote = false;
		foreach (MonkeVoteOption monkeVoteOption in this._votingOptions)
		{
			monkeVoteOption.ResetState();
			monkeVoteOption.OnVote += this.OnVoteEntered;
		}
		this.UpdatePollDisplays();
	}

	private void OnPlayerEnteredVoteProximity()
	{
		MonkeVoteController.instance.RequestPolls();
	}

	private void HandleOnPollsUpdated()
	{
		this.UpdatePollDisplays();
	}

	private void UpdatePollDisplays()
	{
		if (MonkeVoteController.instance == null)
		{
			this.SetState(MonkeVoteMachine.VotingState.None, true);
			this.ShowResults(null);
			return;
		}
		MonkeVoteController.FetchPollsResponse lastPollData = MonkeVoteController.instance.GetLastPollData();
		if (lastPollData != null)
		{
			this._previousPoll = new MonkeVoteMachine.PollEntry(lastPollData);
			this.ShowResults(this._previousPoll);
		}
		else
		{
			this.ShowResults(null);
		}
		MonkeVoteController.FetchPollsResponse currentPollData = MonkeVoteController.instance.GetCurrentPollData();
		if (currentPollData == null)
		{
			this.SetState(MonkeVoteMachine.VotingState.None, true);
			return;
		}
		this._nextPollUpdate = MonkeVoteController.instance.GetCurrentPollCompletionTime();
		this._currentPoll = new MonkeVoteMachine.PollEntry(currentPollData);
		MonkeVoteMachine.PollEntry currentPoll = this._currentPoll;
		if (currentPoll != null && currentPoll.IsValid)
		{
			ValueTuple<int, int> vote = this.GetVote(this._currentPoll.PollId);
			int item = vote.Item1;
			int item2 = vote.Item2;
			MonkeVoteMachine.VotingState votingState = ((item < 0) ? MonkeVoteMachine.VotingState.Voting : ((item2 < 0) ? MonkeVoteMachine.VotingState.Predicting : MonkeVoteMachine.VotingState.Complete));
			this.SetState(votingState, true);
			return;
		}
		this.SetState(MonkeVoteMachine.VotingState.None, true);
	}

	private void HandleOnVoteAccepted()
	{
		int lastVotePollId = MonkeVoteController.instance.GetLastVotePollId();
		int lastVoteSelectedOption = MonkeVoteController.instance.GetLastVoteSelectedOption();
		bool lastVoteWasPrediction = MonkeVoteController.instance.GetLastVoteWasPrediction();
		this.OnVoteResponseReceived(lastVotePollId, lastVoteSelectedOption, lastVoteWasPrediction, true);
	}

	private void HandleOnVoteFailed()
	{
		this._waitingOnVote = false;
		int lastVotePollId = MonkeVoteController.instance.GetLastVotePollId();
		int lastVoteSelectedOption = MonkeVoteController.instance.GetLastVoteSelectedOption();
		bool lastVoteWasPrediction = MonkeVoteController.instance.GetLastVoteWasPrediction();
		this.OnVoteResponseReceived(lastVotePollId, lastVoteSelectedOption, lastVoteWasPrediction, false);
	}

	private void HandleCurrentPollEnded()
	{
		if (this._proximityTrigger.isPlayerNearby)
		{
			MonkeVoteController.instance.RequestPolls();
		}
	}

	[Tooltip("Hide dynamic child meshes to avoid them getting combined into the parent mesh on awake")]
	private void HideDynamicMeshes()
	{
		this.SetDynamicMeshesVisible(false);
	}

	[Tooltip("Show dynamic child meshes to allow easy visualization")]
	private void ShowDynamicMeshes()
	{
		this.SetDynamicMeshesVisible(true);
	}

	private void SetDynamicMeshesVisible(bool enabled)
	{
		MonkeVoteOption[] votingOptions = this._votingOptions;
		for (int i = 0; i < votingOptions.Length; i++)
		{
			votingOptions[i].SetDynamicMeshesVisible(enabled);
		}
		MonkeVoteResult[] results = this._results;
		for (int i = 0; i < results.Length; i++)
		{
			results[i].SetDynamicMeshesVisible(enabled);
		}
	}

	private void Configure()
	{
		this._audio = base.GetComponentInChildren<AudioSource>();
		this._audio.spatialBlend = 1f;
		this._votingOptions = base.GetComponentsInChildren<MonkeVoteOption>();
		this._results = base.GetComponentsInChildren<MonkeVoteResult>();
	}

	public void CreateNextDummyPoll()
	{
		this._isTestingPoll = true;
		if (this._currentPoll != null)
		{
			this._previousPoll = this._currentPoll;
		}
		else
		{
			this._previousPoll = null;
		}
		this.ShowResults(this._previousPoll);
		int num = 0;
		if (this._previousPoll != null)
		{
			num = this._previousPoll.PollId + 1;
		}
		string text = "Test Question Number: " + Random.Range(1, 101).ToString();
		string text2 = "Answer " + Random.Range(1, 101).ToString();
		string text3 = "Answer " + Random.Range(1, 101).ToString();
		string[] array = new string[] { text2, text3 };
		this._currentPoll = new MonkeVoteMachine.PollEntry(num, text, array);
		MonkeVoteMachine.PollEntry currentPoll = this._currentPoll;
		if (currentPoll != null && currentPoll.IsValid)
		{
			ValueTuple<int, int> vote = this.GetVote(this._currentPoll.PollId);
			int item = vote.Item1;
			int item2 = vote.Item2;
			MonkeVoteMachine.VotingState votingState = ((item < 0) ? MonkeVoteMachine.VotingState.Voting : ((item2 < 0) ? MonkeVoteMachine.VotingState.Predicting : MonkeVoteMachine.VotingState.Complete));
			this.SetState(votingState, true);
			return;
		}
		this.SetState(MonkeVoteMachine.VotingState.None, true);
	}

	private void VoteLeft()
	{
		this.OnVoteEntered(this._votingOptions[0], null);
	}

	private void VoteRight()
	{
		this.OnVoteEntered(this._votingOptions[1], null);
	}

	private void VoteWinner()
	{
		if (this._currentPoll != null)
		{
			if (this._currentPoll.VoteCount[0] > this._currentPoll.VoteCount[1])
			{
				this.OnVoteEntered(this._votingOptions[0], null);
				return;
			}
			this.OnVoteEntered(this._votingOptions[1], null);
		}
	}

	private void ClearLocalData()
	{
		this.ClearLocalVoteAndPredictionData();
		this.UpdatePollDisplays();
	}

	private void SetState(MonkeVoteMachine.VotingState newState, bool instant = true)
	{
		this._state = newState;
		MonkeVoteMachine.PollEntry currentPoll = this._currentPoll;
		bool flag = currentPoll != null && currentPoll.IsValid;
		if (this._state < MonkeVoteMachine.VotingState.None || this._state > MonkeVoteMachine.VotingState.Complete || (this._state != MonkeVoteMachine.VotingState.None && !flag))
		{
			this._state = MonkeVoteMachine.VotingState.None;
		}
		if (flag)
		{
			int item = this.GetVote(this._currentPoll.PollId).Item2;
			if (this._state < MonkeVoteMachine.VotingState.Predicting)
			{
				this.SaveVote(this._currentPoll.PollId, -1, item);
			}
			int item2 = this.GetVote(this._currentPoll.PollId).Item1;
			if (this._state < MonkeVoteMachine.VotingState.Complete)
			{
				this.SaveVote(this._currentPoll.PollId, item2, -1);
			}
		}
		bool flag2 = true;
		switch (this._state)
		{
		case MonkeVoteMachine.VotingState.None:
			this._timerText.SetFixedText(this._pollsClosedText);
			this._titleText.text = this._defaultTitle;
			this._questionText.text = this._defaultQuestion;
			flag2 = false;
			break;
		case MonkeVoteMachine.VotingState.Voting:
			this._timerText.SetCountdownTime(this._nextPollUpdate);
			this._titleText.text = this._voteTitle;
			this._questionText.text = this._currentPoll.Question;
			break;
		case MonkeVoteMachine.VotingState.Predicting:
			this._timerText.SetCountdownTime(this._nextPollUpdate);
			this._titleText.text = this._predictTitle;
			this._questionText.text = this._predictQuestion;
			break;
		case MonkeVoteMachine.VotingState.Complete:
			this._timerText.SetCountdownTime(this._nextPollUpdate);
			this._titleText.text = this._completeTitle;
			this._questionText.text = this._currentPoll.Question;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		int num;
		int num2;
		if (!flag)
		{
			num = -1;
			num2 = -1;
		}
		else
		{
			ValueTuple<int, int> vote = this.GetVote(this._currentPoll.PollId);
			num = vote.Item1;
			num2 = vote.Item2;
		}
		if (flag2)
		{
			for (int i = 0; i < this._votingOptions.Length; i++)
			{
				this._votingOptions[i].Text = this._currentPoll.VoteOptions[i];
				this._votingOptions[i].ShowIndicators(num == i, num2 == i, instant);
			}
			return;
		}
		foreach (MonkeVoteOption monkeVoteOption in this._votingOptions)
		{
			monkeVoteOption.Text = string.Empty;
			monkeVoteOption.ShowIndicators(false, false, true);
		}
	}

	private void ShowResults(MonkeVoteMachine.PollEntry entry)
	{
		if (entry != null && entry.IsValid)
		{
			ValueTuple<int, int> vote = this.GetVote(entry.PollId);
			int item = vote.Item1;
			int item2 = vote.Item2;
			GTDev.Log<string>(string.Format("Showing {0} V:{1} P:{2}", entry.Question, item, item2), null);
			List<int> list = this.ConvertToPercentages(entry.VoteCount);
			int num = 0;
			int num2 = -1;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] > num)
				{
					num = list[i];
					num2 = i;
				}
			}
			this._resultsTitleText.text = this._defaultResultsTitle;
			this._resultsQuestionText.text = entry.Question;
			for (int j = 0; j < entry.VoteOptions.Length; j++)
			{
				this._results[j].ShowResult(entry.VoteOptions[j], list[j], item == j, item2 == j, num2 == j);
			}
			int prePollStreak = this.GetPrePollStreak(entry.PollId);
			int postPollStreak = this.GetPostPollStreak(entry);
			this._resultsStreakText.text = ((postPollStreak >= prePollStreak) ? string.Format(this._streakBlurb, postPollStreak) : string.Format(this._streakLostBlurb, prePollStreak, postPollStreak));
			return;
		}
		this._resultsTitleText.text = this._defaultResultsTitle;
		this._resultsQuestionText.text = this._defaultQuestion;
		this._resultsStreakText.text = string.Empty;
		MonkeVoteResult[] results = this._results;
		for (int k = 0; k < results.Length; k++)
		{
			results[k].HideResult();
		}
	}

	private List<int> ConvertToPercentages(int[] votes)
	{
		List<int> list = new List<int>();
		List<float> list2 = new List<float>();
		if (votes == null || votes.Length == 0)
		{
			list.Add(-1);
			list.Add(-1);
			return list;
		}
		if (votes.Length == 1)
		{
			list.Add(100);
			list.Add(0);
			return list;
		}
		int num = MonkeVoteMachine.<ConvertToPercentages>g__Sum|64_0(votes);
		if (num == 0)
		{
			list.Add(-1);
			list.Add(-1);
			return list;
		}
		int num2 = -1;
		int num3 = 0;
		for (int i = 0; i < votes.Length; i++)
		{
			if (votes[i] > num2)
			{
				num2 = votes[i];
				num3 = i;
			}
			float num4 = (float)votes[i] / (float)num * 100f;
			list.Add((int)num4);
			list2.Add(num4 - (float)((int)num4));
		}
		int num5 = MonkeVoteMachine.<ConvertToPercentages>g__Sum|64_0(list);
		int num6 = 100 - num5;
		for (int j = 0; j < num6; j++)
		{
			int num7 = MonkeVoteMachine.<ConvertToPercentages>g__LargestFractionIndex|64_1(list2);
			List<int> list3 = list;
			int num8 = num7;
			int num9 = list3[num8];
			list3[num8] = num9 + 1;
			list2[num7] = 0f;
		}
		if (list.Count == 2 && list[num3] == 50)
		{
			List<int> list4 = list;
			int num9 = num3;
			list4[num9]++;
			list4 = list;
			num9 = 1 - num3;
			list4[num9]--;
		}
		return list;
	}

	private void OnVoteEntered(MonkeVoteOption option, Collider votingCollider)
	{
		if (this._waitingOnVote || (Time.time < this._voteCooldownEnd && !this._isTestingPoll))
		{
			this.PlayVoteFailEffects();
			return;
		}
		int num = Array.IndexOf<MonkeVoteOption>(this._votingOptions, option);
		if (num < 0)
		{
			return;
		}
		switch (this._state)
		{
		case MonkeVoteMachine.VotingState.Voting:
			this.Vote(this._currentPoll.PollId, num, false);
			return;
		case MonkeVoteMachine.VotingState.Predicting:
			this.Vote(this._currentPoll.PollId, num, true);
			return;
		}
		this.PlayVoteFailEffects();
	}

	private void Vote(int id, int option, bool isPrediction)
	{
		if (option < 0 || this._waitingOnVote)
		{
			return;
		}
		this._waitingOnVote = true;
		if (this._isTestingPoll)
		{
			this.OnVoteResponseReceived(id, option, isPrediction, true);
			return;
		}
		MonkeVoteController.instance.Vote(id, option, isPrediction);
	}

	private void OnVoteResponseReceived(int id, int option, bool isPrediction, bool success)
	{
		this._waitingOnVote = false;
		if (success)
		{
			this.PlayVoteSuccessEffects();
			this._voteCooldownEnd = Time.time + this._voteCooldown;
			ValueTuple<int, int> vote = this.GetVote(id);
			int num = vote.Item1;
			int num2 = vote.Item2;
			if (!isPrediction)
			{
				int num3 = num2;
				num = option;
				num2 = num3;
			}
			else
			{
				num = num;
				num2 = option;
			}
			this.SaveVote(id, num, num2);
			MonkeVoteMachine.VotingState state = this._state;
			if (state != MonkeVoteMachine.VotingState.Voting)
			{
				if (state == MonkeVoteMachine.VotingState.Predicting)
				{
					this.SetState(MonkeVoteMachine.VotingState.Complete, false);
				}
			}
			else
			{
				this.SetState(MonkeVoteMachine.VotingState.Predicting, false);
			}
			if (isPrediction && id == this._currentPoll.PollId)
			{
				this.SavePrePollStreak(id, this.GetPostPollStreak(this._previousPoll));
				return;
			}
		}
		else
		{
			this.PlayVoteFailEffects();
		}
	}

	private async void PlayVoteSuccessEffects()
	{
		this._audio.GTPlayOneShot(this._voteSuccessDing, this._audio.volume);
		await Task.Delay(500);
		this._voteTubeAudio.GTPlayOneShot(this._voteProcessingSound, this._voteTubeAudio.volume);
	}

	private void PlayVoteFailEffects()
	{
		this._audio.GTPlayOneShot(this._voteFailSound, this._audio.volume);
	}

	private void SaveVote(int id, int voteOption, int predictionOption)
	{
		int @int = PlayerPrefs.GetInt("Vote_Current_Id", -1);
		if (@int == -1 || @int == id)
		{
			PlayerPrefs.SetInt("Vote_Current_Id", id);
			PlayerPrefs.SetInt("Vote_Current_Option", voteOption);
			PlayerPrefs.SetInt("Vote_Current_Prediction", predictionOption);
		}
		else
		{
			PlayerPrefs.SetInt("Vote_Previous_Id", @int);
			PlayerPrefs.SetInt("Vote_Previous_Option", PlayerPrefs.GetInt("Vote_Current_Option"));
			PlayerPrefs.SetInt("Vote_Previous_Prediction", PlayerPrefs.GetInt("Vote_Current_Prediction"));
			PlayerPrefs.SetInt("Vote_Previous_Streak", PlayerPrefs.GetInt("Vote_Current_Streak"));
			PlayerPrefs.SetInt("Vote_Current_Id", id);
			PlayerPrefs.SetInt("Vote_Current_Option", voteOption);
			PlayerPrefs.SetInt("Vote_Current_Prediction", predictionOption);
			PlayerPrefs.SetInt("Vote_Current_Streak", 0);
		}
		PlayerPrefs.Save();
	}

	[return: TupleElementNames(new string[] { "voteOption", "predictionOption" })]
	private ValueTuple<int, int> GetVote(int voteId)
	{
		if (PlayerPrefs.GetInt("Vote_Current_Id", -1) == voteId)
		{
			int @int = PlayerPrefs.GetInt("Vote_Current_Option", -1);
			int int2 = PlayerPrefs.GetInt("Vote_Current_Prediction", -1);
			return new ValueTuple<int, int>(@int, int2);
		}
		if (PlayerPrefs.GetInt("Vote_Previous_Id", -1) == voteId)
		{
			int int3 = PlayerPrefs.GetInt("Vote_Previous_Option", -1);
			int int4 = PlayerPrefs.GetInt("Vote_Previous_Prediction", -1);
			return new ValueTuple<int, int>(int3, int4);
		}
		return new ValueTuple<int, int>(-1, -1);
	}

	private void SavePrePollStreak(int id, int streak)
	{
		if (id < 0)
		{
			return;
		}
		if (PlayerPrefs.GetInt("Vote_Current_Id", -1) == id)
		{
			PlayerPrefs.SetInt("Vote_Current_Streak", streak);
			return;
		}
		if (PlayerPrefs.GetInt("Vote_Previous_Id", -1) == id)
		{
			PlayerPrefs.SetInt("Vote_Previous_Streak", streak);
		}
	}

	private int GetPrePollStreak(int id)
	{
		if (id < 0)
		{
			return 0;
		}
		if (PlayerPrefs.GetInt("Vote_Current_Id", -1) == id)
		{
			return PlayerPrefs.GetInt("Vote_Current_Streak", 0);
		}
		if (PlayerPrefs.GetInt("Vote_Previous_Id", -1) == id)
		{
			return PlayerPrefs.GetInt("Vote_Previous_Streak", 0);
		}
		return 0;
	}

	private int GetPostPollStreak(MonkeVoteMachine.PollEntry entry)
	{
		if (entry == null || !entry.IsValid)
		{
			return 0;
		}
		int item = this.GetVote(entry.PollId).Item2;
		if (item < 0)
		{
			return 0;
		}
		int prePollStreak = this.GetPrePollStreak(entry.PollId);
		if (item != entry.GetWinner())
		{
			return 0;
		}
		return prePollStreak + 1;
	}

	private void ClearLocalVoteAndPredictionData()
	{
		PlayerPrefs.DeleteKey("Vote_Current_Id");
		PlayerPrefs.DeleteKey("Vote_Current_Option");
		PlayerPrefs.DeleteKey("Vote_Current_Prediction");
		PlayerPrefs.DeleteKey("Vote_Current_Streak");
		PlayerPrefs.DeleteKey("Vote_Previous_Id");
		PlayerPrefs.DeleteKey("Vote_Previous_Option");
		PlayerPrefs.DeleteKey("Vote_Previous_Prediction");
		PlayerPrefs.DeleteKey("Vote_Previous_Streak");
	}

	[CompilerGenerated]
	internal static int <ConvertToPercentages>g__Sum|64_0(IList<int> items)
	{
		int num = 0;
		foreach (int num2 in items)
		{
			num += num2;
		}
		return num;
	}

	[CompilerGenerated]
	internal static int <ConvertToPercentages>g__LargestFractionIndex|64_1(IList<float> fractions)
	{
		float num = float.NegativeInfinity;
		int num2 = -1;
		for (int i = 0; i < fractions.Count; i++)
		{
			if (fractions[i] > num)
			{
				num = fractions[i];
				num2 = i;
			}
		}
		return num2;
	}

	private const string kVoteCurrentIdKey = "Vote_Current_Id";

	private const string kVoteCurrentOptionKey = "Vote_Current_Option";

	private const string kVoteCurrentPredictionKey = "Vote_Current_Prediction";

	private const string kVoteCurrentStreak = "Vote_Current_Streak";

	private const string kVotePreviousIdKey = "Vote_Previous_Id";

	private const string kVotePreviousOptionKey = "Vote_Previous_Option";

	private const string kVotePreviousPredictionKey = "Vote_Previous_Prediction";

	private const string kVotePreviousStreak = "Vote_Previous_Streak";

	[SerializeField]
	private MonkeVoteProximityTrigger _proximityTrigger;

	[Header("VOTING")]
	[SerializeField]
	private string _pollsClosedText = "POLLS CLOSED";

	[SerializeField]
	private string _defaultTitle = "MONKE VOTE";

	[SerializeField]
	private string _voteTitle = "VOTE";

	[SerializeField]
	private string _predictTitle = "GUESS";

	[SerializeField]
	private string _completeTitle = "VOTING COMPLETE";

	[SerializeField]
	private string _defaultQuestion = "COME BACK LATER";

	[SerializeField]
	private string _predictQuestion = "WHICH WILL BE MORE POPULAR?";

	[Tooltip("Must be in the format \"STREAK: {0}\"")]
	[SerializeField]
	private string _streakBlurb = "PREDICTION STREAK: {0}";

	[Tooltip("Must be in the format \"LOST {0} PREDICTION STREAK! STREAK: {1}\"")]
	[SerializeField]
	private string _streakLostBlurb = "<color=red>{0} POLL STREAK LOST!</color>  STREAK: {1}";

	[SerializeField]
	private float _voteCooldown = 1f;

	[SerializeField]
	private MonkeVoteOption[] _votingOptions;

	[SerializeField]
	private CountdownText _timerText;

	[SerializeField]
	private TMP_Text _titleText;

	[SerializeField]
	private TMP_Text _questionText;

	[Header("RESULTS")]
	[SerializeField]
	private string _defaultResultsTitle = "PREVIOUS QUESTION";

	[SerializeField]
	private TMP_Text _resultsTitleText;

	[SerializeField]
	private TMP_Text _resultsQuestionText;

	[SerializeField]
	private TMP_Text _resultsStreakText;

	[SerializeField]
	private MonkeVoteResult[] _results;

	[FormerlySerializedAs("_sound")]
	[Header("FX")]
	[SerializeField]
	private AudioSource _audio;

	[FormerlySerializedAs("_voteProcessingAudio")]
	[SerializeField]
	private AudioSource _voteTubeAudio;

	[SerializeField]
	private AudioClip[] _voteFailSound;

	[SerializeField]
	private AudioClip[] _voteSuccessDing;

	[FormerlySerializedAs("_voteSuccessSound")]
	[SerializeField]
	private AudioClip[] _voteProcessingSound;

	private MonkeVoteMachine.VotingState _state;

	private float _voteCooldownEnd;

	private bool _waitingOnVote;

	private MonkeVoteMachine.PollEntry _currentPoll;

	private MonkeVoteMachine.PollEntry _previousPoll;

	private DateTime _nextPollUpdate;

	private bool _isTestingPoll;

	public enum VotingState
	{
		None,
		Voting,
		Predicting,
		Complete
	}

	public class PollEntry
	{
		public PollEntry(int pollId, string question, string[] voteOptions)
		{
			this.PollId = pollId;
			this.Question = question;
			this.VoteOptions = voteOptions;
			this.VoteCount = new int[2];
			this.VoteCount[0] = Random.Range(0, 50000);
			this.VoteCount[1] = Random.Range(0, 50000);
			this.PredictionCount = new int[2];
			this.PredictionCount[0] = Random.Range(0, 50000);
			this.PredictionCount[1] = Random.Range(0, 50000);
			this.StartTime = DateTime.Now;
			this.EndTime = DateTime.Now + TimeSpan.FromSeconds(20.0);
		}

		public PollEntry(MonkeVoteController.FetchPollsResponse poll)
		{
			this.PollId = poll.PollId;
			this.Question = poll.Question;
			this.VoteOptions = poll.VoteOptions.ToArray();
			this.VoteCount = poll.VoteCount.ToArray();
			this.PredictionCount = poll.PredictionCount.ToArray();
			this.StartTime = poll.StartTime;
			this.EndTime = poll.EndTime;
		}

		public int GetWinner()
		{
			if (this.VoteCount == null || this.VoteCount.Length == 0)
			{
				return -1;
			}
			int num = int.MinValue;
			int num2 = -1;
			for (int i = 0; i < this.VoteCount.Length; i++)
			{
				if (this.VoteCount[i] > num)
				{
					num = this.VoteCount[i];
					num2 = i;
				}
			}
			return num2;
		}

		public bool IsValid
		{
			get
			{
				string[] voteOptions = this.VoteOptions;
				return voteOptions != null && voteOptions.Length == 2;
			}
		}

		public int PollId;

		public string Question;

		public string[] VoteOptions;

		public int[] VoteCount;

		public int[] PredictionCount;

		public DateTime StartTime;

		public DateTime EndTime;
	}
}

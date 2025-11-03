using System;

public interface GorillaQuestManager
{
	void LoadQuestsFromJson(string jsonString);

	void LoadQuestProgress();

	void SaveQuestProgress();

	void SetupAllQuestEventListeners();

	void ClearAllQuestEventListeners();

	void HandleQuestProgressChanged(bool initialLoad);

	void HandleQuestCompleted(int questID);
}

using System;
using System.Collections.Generic;
using Cysharp.Text;
using GorillaExtensions;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class TextTyperAnimatorMono : MonoBehaviour, IGorillaSliceableSimple
{
	public void EdRestartAnimation()
	{
		this.m_textMesh.maxVisibleCharacters = 0;
	}

	protected void Awake()
	{
		this._has_typingSoundBank = this.m_typingSoundBank != null;
		this._has_beginEntrySoundBank = this.m_beginEntrySoundBank != null;
		this._waitTime = this._random.NextFloat(this.m_typingSpeedMinMax.x, this.m_typingSpeedMinMax.y);
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void SliceUpdate()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		int num = this.m_textMesh.maxVisibleCharacters;
		if (num < 0 || num >= this._charCount || this._timeOfLastTypedChar + this._waitTime > realtimeSinceStartup)
		{
			return;
		}
		num = (this.m_textMesh.maxVisibleCharacters = num + 1);
		this._timeOfLastTypedChar = realtimeSinceStartup;
		if (this._has_beginEntrySoundBank && num == 1)
		{
			this.m_beginEntrySoundBank.Play();
		}
		else if (this._has_typingSoundBank)
		{
			this.m_typingSoundBank.Play();
		}
		this._waitTime = this._random.NextFloat(this.m_typingSpeedMinMax.x, this.m_typingSpeedMinMax.y);
	}

	public void SetText(string text, IList<int> entryIndexes, int nonRichTextTagsCharCount)
	{
		this._charCount = nonRichTextTagsCharCount;
		this.m_textMesh.SetText(text);
		this.m_textMesh.maxVisibleCharacters = 0;
		this._SetEntryIndexes(entryIndexes);
	}

	public void SetText(string text, IList<int> entryIndexes)
	{
		this.SetText(text, entryIndexes, text.Length);
		this.m_textMesh.SetText(text);
		this.m_textMesh.maxVisibleCharacters = 0;
		this._SetEntryIndexes(entryIndexes);
	}

	public void SetText(string text)
	{
		this.SetText(text, Array.Empty<int>());
	}

	public void SetText(Utf16ValueStringBuilder zStringBuilder, IList<int> entryIndexes, int nonRichTextTagsCharCount)
	{
		this._charCount = nonRichTextTagsCharCount;
		this.m_textMesh.SetTextToZString(zStringBuilder);
		this.m_textMesh.maxVisibleCharacters = 0;
		this._SetEntryIndexes(entryIndexes);
	}

	public void SetText(Utf16ValueStringBuilder zStringBuilder)
	{
		this.SetText(zStringBuilder, Array.Empty<int>(), zStringBuilder.Length);
	}

	private void _SetEntryIndexes(IList<int> entryIndexes)
	{
		this._entryIndexes.Clear();
		this._entryIndexes.AddRange(entryIndexes);
	}

	public void UpdateText(Utf16ValueStringBuilder zStringBuilder, int nonRichTextTagsCharCount)
	{
		TMP_Text textMesh = this.m_textMesh;
		this._charCount = nonRichTextTagsCharCount;
		textMesh.maxVisibleCharacters = nonRichTextTagsCharCount;
		this.m_textMesh.SetTextToZString(zStringBuilder);
	}

	[FormerlySerializedAs("_textMesh")]
	[Tooltip("Text Mesh Pro component.")]
	[SerializeField]
	private TMP_Text m_textMesh;

	[Tooltip("Delay between characters in seconds")]
	[SerializeField]
	private Vector2 m_typingSpeedMinMax = new Vector2(0.05f, 0.1f);

	[Header("Audio")]
	[Tooltip("AudioClips to play while typing.")]
	[SerializeField]
	private SoundBankPlayer m_typingSoundBank;

	private bool _has_typingSoundBank;

	[Tooltip("AudioClips to play when a ")]
	[SerializeField]
	private SoundBankPlayer m_beginEntrySoundBank;

	private bool _has_beginEntrySoundBank;

	private int _charCount;

	private readonly List<int> _entryIndexes = new List<int>(16);

	private float _waitTime;

	private float _timeOfLastTypedChar = -1f;

	private Unity.Mathematics.Random _random = new Unity.Mathematics.Random(6746U);
}

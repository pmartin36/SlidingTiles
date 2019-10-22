using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SoundType {
	Music,
	VFX
}

public class AudioController : MonoBehaviour
{
	public SoundType SoundType;
	private Button muteButton;
	private Slider slider;

	[SerializeField]
	private Sprite mutedSprite;
	[SerializeField]
	private Sprite unmutedSprite;

	public void Start() {
		muteButton = GetComponentInChildren<Button>();
		slider = GetComponentInChildren<Slider>();
	}

	public void OnSliderValueChange() {
		if(slider.value > 0 && muteButton.image.sprite == mutedSprite) {
			muteButton.image.sprite = unmutedSprite;
		}
		else if(slider.value <= 0.001f && muteButton.image.sprite == unmutedSprite) {
			muteButton.image.sprite = mutedSprite;
		}

		// tell audio manager that value changed
		GameManager.Instance.AdjustAudio(SoundType, slider.value);
	}

	public void OnMuteButtonToggle() {
		bool isMuting = slider.value > 0;
		slider.value = isMuting ? 0 : 1; // maybe should animate
		muteButton.image.sprite = isMuting ? mutedSprite : unmutedSprite;

		// tell audio manager that value changed
		GameManager.Instance.AdjustAudio(SoundType, slider.value);
	}
}

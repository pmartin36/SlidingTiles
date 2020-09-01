using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdvertisementManager
{
	private static readonly float TimeBetweenAds = 5 * 60;
    public float TimeOfLastAd { get; set; } = 0;
	public bool AdsRemoved => GameManager.Instance.SaveData.AdsRemoved;

	private string gameId;
	private bool testMode;

	public AdvertisementManager() {
		switch (Application.platform) {
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.LinuxEditor:
				gameId = "3439423";
				testMode = true;
				break;
			case RuntimePlatform.IPhonePlayer:
				gameId = "3439422";
				testMode = false;
				break;
			case RuntimePlatform.Android:
				gameId = "3439423";
				testMode = false;
				break;
			default:
				throw new System.Exception("Invalid Platform");
		}
		Advertisement.Initialize(gameId, testMode);
	}

	public void TryShowAd() {
		float timeSinceLastAd = Time.time - TimeOfLastAd;
		if(!AdsRemoved && timeSinceLastAd > TimeBetweenAds) {
			Advertisement.Show();	
			TimeOfLastAd = Time.time;
		}
	}
}

using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSelect : MenuCopyComponent
{
	public static float CameraWipeTime = 0.5f;
	private const float tileWidth = 150;

	public MenuCopyManager CopyManager;
	public LevelSelectButton Back;
	public NumberedLevelSelectButton[] NumberedLevelButtons;
	
	public bool LevelSelectOpen => WorldSelected > 0;
	public int WorldSelected { get; set; }

	private Vector2 WorldSelectedTilePosition;
	private Vector2[] LevelPositions;
	private Vector2[] WorldPositions;

	private int highestUnlockedWorld;
	private int highestUnlockedLevel;
	private MenuManager menuManager;

	private LevelData[,] levelData;

	void Awake() {
		WorldSelectedTilePosition = new Vector2(0f, 3f * tileWidth - 1);
		WorldPositions = new[] {
			new Vector2(	-2f * tileWidth,	2f * tileWidth - 1),
			new Vector2(	0f * tileWidth,		2f * tileWidth - 1),
			new Vector2(	2f * tileWidth,		2f * tileWidth - 1),
			new Vector2(	-2f * tileWidth,	0f * tileWidth - 1),
			new Vector2(	0f * tileWidth,		0f * tileWidth - 1),
			new Vector2(	2f * tileWidth,		0f * tileWidth - 1),
			new Vector2(	-2f * tileWidth,	-2f * tileWidth - 1),
			new Vector2(	 0f * tileWidth,	-2f * tileWidth - 1),
			new Vector2(	2f * tileWidth,		-2f * tileWidth - 1),
			new Vector2(	-2f * tileWidth,	-4f * tileWidth - 1),
			new Vector2(	 0f * tileWidth,	-4f * tileWidth - 1),
			new Vector2(	 2f * tileWidth,	-4f * tileWidth - 1)
		};
		LevelPositions = new [] {
			new Vector2(    2f * tileWidth,     2f * tileWidth - 1),
			new Vector2(    -2f * tileWidth,    0f * tileWidth - 1),
			new Vector2(    0f * tileWidth,     0f * tileWidth - 1),
			new Vector2(    2f * tileWidth,     0f * tileWidth - 1),
			new Vector2(    -2f * tileWidth,    -2f * tileWidth - 1),
			new Vector2(     0f * tileWidth,    -2f * tileWidth - 1),
			new Vector2(    2f * tileWidth,     -2f * tileWidth - 1),
			new Vector2(    -2f * tileWidth,    -4f * tileWidth - 1),
			new Vector2(     0f * tileWidth,    -4f * tileWidth - 1),
			new Vector2(     2f * tileWidth,    -4f * tileWidth - 1),
			//garbage
			Vector2.zero,
			Vector2.zero
		};

		int highest = GameManager.Instance.HighestUnlockedLevel;
		SceneHelpers.GetWorldAndLevelFromBuildIndex(highest, out highestUnlockedWorld, out highestUnlockedLevel);

		levelData = GameManager.Instance.SaveData.LevelData;
		NumberedLevelButtons = GetComponentsInChildren<NumberedLevelSelectButton>().ToArray(); // 1 - 12
		for(int i = 0; i < NumberedLevelButtons.Length; i++) {
			NumberedLevelSelectButton b = NumberedLevelButtons[i];
			b.Init(i + 1);
			SetLevelSelectButton(b);
			b.TryEnableInteractable();
		}
		Back.Init();

		if(LevelSelectOpen) {
			NumberedLevelSelectButton b = NumberedLevelButtons[WorldSelected - 1];
			SetLevelSelectButton(b);
			if (SelectedWorldComplete) {
				b.Interactable = true;
				b.SetOnClick(SwitchToWorldCompleteScene);
			}
			else {
				b.Interactable = false;
			}
			Back.transform.localScale = Vector3.one;
		}
		else {
			Back.SetHidden(true, null, true);
		}

    }

	public void Init(int worldSelected, MenuManager manager) {
		WorldSelected = worldSelected;
		menuManager = manager;
	}

	public void SetLevelSelectButton(NumberedLevelSelectButton b) {
		if(LevelSelectOpen) {
			b.SetPaywalled(false);
			if (b.Number != WorldSelected) {
				bool worldFullyUnlocked = WorldSelected < highestUnlockedWorld;

				// what is usually #12 is used as a flex tile to fill the spot of the world selected
				if(b.Number == 12) {
					if (WorldSelected < 11) {
						int stars = levelData[WorldSelected - 1, WorldSelected - 1].MaxStarsCollected;
						b.SetStayHidden(false);
						b.SetButtonInfo(LevelSelectPosition(WorldSelected), WorldSelected, worldFullyUnlocked || WorldSelected <= highestUnlockedLevel, false, stars);
					}
					else {
						// only 10 levels, don't need to show if level 11 is selected
						b.SetStayHidden(true);
					}
				}
				else if(b.Number == 11) {
					// since 12 is flexing, 11 is never needed in level select
					b.SetStayHidden(true);
				}
				else {
					int stars = levelData[WorldSelected - 1, b.Number - 1].MaxStarsCollected;
					b.SetStayHidden(false);
					b.SetButtonInfo(LevelSelectPosition(b.Number), b.Number, worldFullyUnlocked || b.Number <= highestUnlockedLevel, false, stars);
  				}
			}	
			else {
				int len = levelData.GetLength(1);
				int minStars = 4;
				for (int i = 0; i < len; i++)
					minStars = Mathf.Min(minStars, levelData[b.Number - 1, i].MaxStarsCollected);
				b.SetButtonInfo(WorldSelectedTilePosition, WorldSelected, true, false, minStars);
			}
		}
		else {
			int len = levelData.GetLength(1);
			int minStars = 4;
			for(int i = 0; i < len; i++)
				minStars = Mathf.Min(minStars, levelData[b.Number-1, i].MaxStarsCollected);

			bool hidden = b.Number > GameManager.ShownWorlds;
			b.SetStayHidden(hidden);

			int highestUnlockedAndAvailable = Mathf.Min(highestUnlockedWorld, GameManager.AvailableWorlds);

			if (!hidden) {
				b.SetButtonInfo(
					position:	WorldSelectPosition(b.Number), 
					num:		b.Number, 
					unlocked:	b.Number <= highestUnlockedAndAvailable,
					paywalled:	false,
					stars: minStars
				);
			}
		}
	}

	public void ButtonSelected(NumberedLevelSelectButton button) {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		if (LevelSelectOpen) {
			int buildIndex = SceneHelpers.GetBuildIndexFromLevel(WorldSelected, button.TempNumber.HasValue ? button.TempNumber.Value : button.Number);
			MusicManager.Instance.SlideVolume(0.4f, 1f);
			GameManager.Instance.LoadScene(buildIndex, null);
		}
		else {
			if(button.Paywalled) {
				Debug.Log($"TAKE ME TO THE STORE TO BUY WORLD {button.Number}!");
			}
			else {
				levelData = GameManager.Instance.SaveData.LevelData;
				CopyManager.OnLevelChange(button.Number);

				(this.MirroredComponent as LevelSelect).OnLevelSelectNoAction(button.MirroredComponent as NumberedLevelSelectButton);
				OnLevelSelectNoAction(button);

				// tell camera to wipe
				// world one and no world share the same style
				if (WorldSelected > 1) {
					StartCoroutine(CameraWipe(0));
				}
			}
		}
	}

	public void BackSelected() {
		MMVibrationManager.Haptic(HapticTypes.Selection);
		// wipe
		// world one and no world share the same style
		if (WorldSelected > 1) {
			StartCoroutine(CameraWipe(1));
		}
		MusicManager.Instance.LoadMusicForWorldAndChangeTrack(1, 1f, 0.4f);
		(this.MirroredComponent as LevelSelect).BackAction();
		BackAction();	
	}

	public void BackAction() {
		int prevSelected = WorldSelected;
		WorldSelected = 0;

		// hide back button
		Back.SetHidden(true);

		levelData = GameManager.Instance.SaveData.LevelData;
		foreach (NumberedLevelSelectButton b in NumberedLevelButtons) {
			b.TempNumber = null;

			if (prevSelected == b.Number) {
				// move button back to original position
				b.SetSlidePosition(
					WorldSelectPosition(b.Number), 
					true,
					() => ButtonSelected(b)
				);
			}
			else {
				b.SetHidden(true, () => {
					SetLevelSelectButton(b);
					b.SetHidden(false, null);
				});
			}
		}

		
	}

	public void OnLevelSelectNoAction(NumberedLevelSelectButton button) {
		WorldSelected = button.Number;

		// move button to S position
		// if the world is complete, allow it to be clickable to display world complete info
		button.SetSlidePosition(
			WorldSelectedTilePosition,
			SelectedWorldComplete, 
			SwitchToWorldCompleteScene
		);

		// move all other buttons to their position
		foreach (NumberedLevelSelectButton b in NumberedLevelButtons) {
			if (b != button) {
				b.SetHidden(true, () => {
					SetLevelSelectButton(b);
					b.SetHidden(false, null);
					Back.SetHidden(false, null);
				});
			}
		}
	}

	public bool SelectedWorldComplete {
		get {
			int len = levelData.GetLength(1);
			bool worldComplete = true;
			for (int i = 0; i < len; i++) {
				LevelData ld = GameManager.Instance.SaveData.LevelData[WorldSelected - 1, i];
				if (ld.MaxStarsCollected < 0) {
					worldComplete = false;
					break;
				}
			}
			return worldComplete;
		}
	}

	public System.Action SwitchToWorldCompleteScene => 
		() => {
			GameManager.Instance.SaveData.LastPlayedWorld = WorldSelected;
			GameManager.Instance.LoadScene(
				SceneHelpers.WorldCompleteBuildIndex,
				null,
				() => {
					WorldCompleteManager wcm = (GameManager.Instance.ContextManager as WorldCompleteManager);
					wcm.HideContinue();
				});
		};

	/// <summary>
	/// Get the position on the template for the ith level on the level select screen
	/// </summary>
	public Vector2 LevelSelectPosition(int num) {
		return LevelPositions[num - 1];
	}

	/// <summary>
	/// Get the position on the template for the ith level on the world select screen
	/// </summary>
	public Vector2 WorldSelectPosition(int num) {
		return WorldPositions[num - 1];
	}

	private IEnumerator CameraWipe(float target) {
		float time = 0;
		float start = 1 - target;
		float animationTime = CameraWipeTime;
		//yield return new WaitForSeconds(0.25f);
		while (time < animationTime) {
			float v = Mathf.Lerp(start, target, time / animationTime);
			menuManager.LevelBlend = v;
			time += Time.deltaTime;
			yield return null;
		}
		menuManager.LevelBlend = target;
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSelect : MonoBehaviour
{
	public LevelSelectButton Back;
	public NumberedLevelSelectButton[] NumberedLevelButtons;

	public bool LevelSelectOpen => WorldSelected > 0;
	public int WorldSelected { get; set; }

	private Vector2 WorldSelectedTilePosition;
	private Vector2[] LevelPositions;
	private Vector2[] WorldPositions;

	private int highestUnlockedWorld;
	private int highestUnlockedLevel;

	void Awake() {
		WorldSelected = 0;

		float tileWidth = 150;
		WorldSelectedTilePosition = new Vector2(2.5f * tileWidth, 1.5f * tileWidth);
		WorldPositions = new[] {
			new Vector2(-3.5f * tileWidth,  0.5f * tileWidth),
			new Vector2(-1.5f * tileWidth,  0.5f * tileWidth),
			new Vector2( 0.5f * tileWidth,  0.5f * tileWidth),
			new Vector2( 2.5f * tileWidth,  0.5f * tileWidth),
			new Vector2(-2.5f * tileWidth, -0.5f * tileWidth),
			new Vector2(-0.5f * tileWidth, -0.5f * tileWidth),
			new Vector2( 1.5f * tileWidth, -0.5f * tileWidth),
			new Vector2( 3.5f * tileWidth, -0.5f * tileWidth),
			new Vector2(-3.5f * tileWidth, -1.5f * tileWidth),
			new Vector2(-1.5f * tileWidth, -1.5f * tileWidth),
			new Vector2( 0.5f * tileWidth, -1.5f * tileWidth),
			new Vector2( 2.5f * tileWidth, -1.5f * tileWidth)
		};
		LevelPositions = new [] {
			new Vector2(-0.5f * tileWidth,  0.5f * tileWidth),
			new Vector2( 1.5f * tileWidth,  0.5f * tileWidth),
			new Vector2( 3.5f * tileWidth,  0.5f * tileWidth),
			new Vector2(-1.5f * tileWidth, -0.5f * tileWidth),
			new Vector2( 0.5f * tileWidth, -0.5f * tileWidth),
			new Vector2( 2.5f * tileWidth, -0.5f * tileWidth),
			new Vector2(-2.5f * tileWidth, -1.5f * tileWidth),
			new Vector2(-0.5f * tileWidth, -1.5f * tileWidth),
			new Vector2( 1.5f * tileWidth, -1.5f * tileWidth),
			new Vector2( 3.5f * tileWidth, -1.5f * tileWidth),
			//garbage
			Vector2.zero,
			Vector2.zero
		};

		int highest = GameManager.Instance.HighestUnlockedLevel;
		highestUnlockedWorld = (highest - GameManager.TutorialLevelStart - 2) / 10 + 1;
		highestUnlockedLevel = (highest - GameManager.TutorialLevelStart - 2) % 10 + 1;

		NumberedLevelButtons = GetComponentsInChildren<NumberedLevelSelectButton>().OrderBy(g => g.name).ToArray(); // 1 - 12
		for(int i = 0; i < NumberedLevelButtons.Length; i++) {
			NumberedLevelSelectButton b = NumberedLevelButtons[i];
			b.Init(i + 1);
			SetLevelSelectButton(b);
			b.TryEnableInteractable();
		}

		Back.Init();
    }

	public void SetLevelSelectButton(NumberedLevelSelectButton b) {
		if(LevelSelectOpen) {
			b.SetPaywalled(false);
			if (b.Number != WorldSelected) {
				// what is usually #12 is used as a flex tile to fill the spot of the world selected
				if(b.Number == 12) {
					if (WorldSelected < 11) {
						b.SetStayHidden(false);
						b.SetButtonInfo(LevelSelectPosition(WorldSelected), WorldSelected, WorldSelected <= highestUnlockedLevel, false);
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
					b.SetStayHidden(false);
					b.SetButtonInfo(LevelSelectPosition(b.Number), b.Number, b.Number <= highestUnlockedLevel, false);
  				}
			}	
		}
		else {
			b.SetStayHidden(false); // worlds are never hidden
			b.SetButtonInfo(
				position:	WorldSelectPosition(b.Number), 
				num:		b.Number, 
				unlocked:	b.Number <= highestUnlockedWorld,
				paywalled:	b.Number > GameManager.Instance.HighestOwnedWorld
			);
		}
	}

	public void ButtonSelected(NumberedLevelSelectButton button) {
		if(LevelSelectOpen) {
			int buildIndex = GameManager.Instance.GetBuildIndexFromLevel(WorldSelected, button.TempNumber.HasValue ? button.TempNumber.Value : button.Number);
			GameManager.Instance.LoadScene(buildIndex, null);
		}
		else {
			WorldSelected = button.Number;

			if(button.Paywalled) {
				Debug.Log("TAKE ME TO THE STORE!");
			}
			else {
				// move button to S position
				button.SetSlidePosition(WorldSelectedTilePosition, false);

				// move all other buttons to their position
				foreach(NumberedLevelSelectButton b in NumberedLevelButtons) {
					if(b != button) {
						b.SetHidden(true, () => {
							SetLevelSelectButton(b);
							b.SetHidden(false, null);
							Back.SetHidden(false, null);
						});
					}
				}
			}
		}
	}

	public void BackSelected() {
		int prevSelected = WorldSelected;
		WorldSelected = 0;

		// hide back button
		Back.SetHidden(true, null);

		foreach (NumberedLevelSelectButton b in NumberedLevelButtons) {
			b.TempNumber = null;
			if(prevSelected == b.Number) {
				// move button back to original position
				b.SetSlidePosition(WorldSelectPosition(b.Number), true);
			}
			else {
				b.SetHidden(true, () => {
					SetLevelSelectButton(b);
					b.SetHidden(false, null);
				});
			}
		}
	}

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
}

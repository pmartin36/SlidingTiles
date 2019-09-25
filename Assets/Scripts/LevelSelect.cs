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
		};
		
		NumberedLevelButtons = GetComponentsInChildren<NumberedLevelSelectButton>().OrderBy(g => g.name).ToArray(); // 1 - 12
		for(int i = 0; i < NumberedLevelButtons.Length; i++) {
			NumberedLevelSelectButton b = NumberedLevelButtons[i];
			b.Init(i + 1);
			SetLevelSelectButton(b);
		}

		Back.Init();
    }

	public void SetLevelSelectButton(NumberedLevelSelectButton b) {
		if(LevelSelectOpen) {
			if(b.Number != WorldSelected) {
				if (b.Number == 11) {
					if(WorldSelected == 12) {
						b.SetStayHidden(true);
					}
					else {
						b.SetPositionAndNumber(LevelSelectPosition(WorldSelected), WorldSelected);
					}
				}
				else if(b.Number == 12) {
					if (WorldSelected < 11) {
						b.SetPositionAndNumber(LevelSelectPosition(WorldSelected), WorldSelected);
					}
					else {
						b.SetStayHidden(true);
					}
				}
				else {
					b.SetPosition(LevelSelectPosition(b.Number));
  				}
			}	
		}
		else {
			b.SetPositionAndNumber(WorldSelectPosition(b.Number), b.Number);
		}
	}

	public void ButtonSelected(NumberedLevelSelectButton button) {
		if(LevelSelectOpen) {
			int buildIndex = GameManager.Instance.GetBuildIndexFromLevel(WorldSelected, button.Number);
			GameManager.Instance.LoadScene(buildIndex, null);
		}
		else {
			WorldSelected = button.Number;

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

	public void BackSelected() {
		int prevSelected = WorldSelected;
		WorldSelected = 0;

		// hide back button
		Back.SetHidden(true, null);

		foreach (NumberedLevelSelectButton b in NumberedLevelButtons) {
			if(prevSelected == b.Number) {
				// move button back to original position
				b.SetSlidePosition(WorldSelectPosition(b.Number), true);
			}
			else {
				b.SetHidden(true, () => {
					SetLevelSelectButton(b);
					b.SetStayHidden(false);
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

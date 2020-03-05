using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class LevelManager : ContextManager {
	public int World { get; set; }
	public Grid Grid { get; set; }

	public Tile SelectedTile { get; private set; }
	protected LayerMask tileMask;
	protected Vector3 grabPoint;
	protected Vector3 grabReleasePoint;
	protected Vector3 grabMovingVelocityAverage;

	protected WinType winType;
	protected int collectedStars;
	protected GoalFlag goalFlag;

	protected TilePreview Preview;

	protected CancellationTokenSource cts;

	protected LevelInfoUI LevelInfo;
	protected bool TimerRunning;
	protected float ElapsedTime;
	protected TimeInfo timeInfo;

	public GameObject LevelObjectContainer;

	public RespawnManager RespawnManager { get; protected set; }
	public Player Player { get; set; }
	public bool Won { get; set; }

	public bool Paused { get; set; }

	public bool SnapAfterDeselected;

	// gif stuff, please comment me out on release
	[Header("Gif Stuff")]
	public bool ShowFingerprint;
	private Fingerprint Fingerprint; 

	public override void Awake() {
		base.Awake();
		bool parsed = Int32.TryParse(gameObject.scene.name.Split('-')[0], out int world);
		if(parsed) {
			World = Mathf.Max(1, world);
		}
		else {
			// tutorial level
			World = 1;
		}

		GameManager.Instance.SaveData.LastPlayedWorld = World;
	}

	public override void Start() {
		base.Start();
		Init();
		tileMask = 1 << LayerMask.NameToLayer("Tile");
	}

	public virtual void Init() {
		SnapAfterDeselected = true;

		Grid = FindObjectsOfType<Grid>().First(g => g.gameObject.scene == this.gameObject.scene);
		winType = FindObjectsOfType<WinType>().First(g => g.gameObject.scene == this.gameObject.scene);
		Preview = FindObjectsOfType<TilePreview>().First(g => g.gameObject.scene == this.gameObject.scene);

		Player = GameObject.FindObjectsOfType<Player>().First(g => g.gameObject.scene == this.gameObject.scene);
		Player.aliveChanged += PlayerAliveChange;

		LevelInfo = FindObjectsOfType<LevelInfoUI>().First(g => g.gameObject.scene == this.gameObject.scene);
		LevelInfo.SetTimer(ElapsedTime);

		StartCoroutine(RunTimer());
		CreateRespawnManager();
	}

	public virtual void CreateRespawnManager() {
		RespawnManager = new RespawnManager(gameObject.scene, Player);
	}

	public override void Update() {
		base.Update();
	}

	// called every frame from context manager
	public override void HandleInput(InputPackage p) {
		if(!Won) {
			if(p.Touchdown) { 
				if(p.TouchdownChange) {
					// clicked
					Tile t = Physics2D.OverlapCircleAll(p.MousePositionWorldSpace, 3f, tileMask) // roughly 1/3 of a tile of tap tolerance
										.Select(g => g.GetComponent<Tile>())
										.Where(g => g != null && g.Movable)
										.OrderBy(g => Vector2.Distance(p.MousePositionWorldSpace, g.transform.position))
										.FirstOrDefault();

					if (t != null) {
						SelectedTile = t;
						grabPoint = p.MousePositionWorldSpace;
						SelectedTile.Select(true);

						Vibration.VibratePop();

						grabMovingVelocityAverage = Vector2.zero;
					}

					#if UNITY_EDITOR
						if(ShowFingerprint) {
							Fingerprint = GetComponentInChildren<Fingerprint>(true);
							Fingerprint.gameObject.SetActive(true);
							Fingerprint.transform.position = p.MousePositionWorldSpace;
						}
					#endif
				}
				else if(SelectedTile != null) {
					grabMovingVelocityAverage = p.MousePositionWorldSpaceDelta * 0.1f + grabMovingVelocityAverage * 0.9f;

					float scale = SelectedTile.transform.lossyScale.x;
					Vector2 moveAmount = (p.MousePositionWorldSpace - grabPoint) / scale;
					Tilespace tileBeforeMove = SelectedTile.Space;
					bool changedTilespaces = SelectedTile.TryMoveToPosition(SelectedTile.GetPositionFromInput(moveAmount), p.MousePositionWorldSpaceDelta);

					if(changedTilespaces) {
						if(SelectedTile.Space.Sticky) {
							SelectedTile = null;
						}
						else {
							Vector3 move = ((Vector2)SelectedTile.transform.position - SelectedTile.PositionWhenSelected);
							grabPoint += move;
							if (Mathf.Abs(move.y) > Mathf.Abs(move.x) && Mathf.Abs(moveAmount.x) < 2 * Tile.BaseThreshold) {
								grabPoint.x = p.MousePositionWorldSpace.x;
							}
							else if(Mathf.Abs(moveAmount.y) < 2 * Tile.BaseThreshold) {
								grabPoint.y = p.MousePositionWorldSpace.y;
							}

							SelectedTile.Select(true);
						}
					}		

					if(!TimerRunning && !Won && !SelectedTile.Centered) {
						StartTimer();
					}
				}

				Vector3 position = p.MousePositionWorldSpace + Player.Direction * -7.5f;
				if (Vector2.Distance(Player.transform.position, p.MousePositionWorldSpace) < 5) {				
					Preview.Show(true, position);
					Preview.WatchedPosition = Player.transform.position;				
				}
				else {
					Preview.Show(false, position);
				}

				#if UNITY_EDITOR
					if(Fingerprint != null)
						Fingerprint.transform.position = p.MousePositionWorldSpace;
				#endif
			}
			else if (!p.Touchdown && p.TouchdownChange) {
				Preview.Show(false);
				if (SelectedTile != null) {
					Vector2 avgOnRelease = grabMovingVelocityAverage;
					if(!SelectedTile.Centered) {
						avgOnRelease *= SelectedTile.NormalizedPosition;
					}
					Debug.Log(avgOnRelease);

					SelectedTile.SetResidualVelocity(avgOnRelease);
					SelectedTile.Select(false);
					SelectedTile = null;
				}

				#if UNITY_EDITOR
				if (Fingerprint != null)
						Fingerprint.gameObject.SetActive(false);
				#endif
			}
		}
	}

	public void AddStar() {
		collectedStars++;
	}

	public virtual void PlayPauseButtonClicked() {
		if (Player.Alive) {
			SetPause(!Paused);
		}
		else {
			RespawnManager.RespawnPlayer();
			StartTimer();
		}
	}

	public virtual void PlayerAliveChange(object player, bool alive) {
		if (!alive) {
			collectedStars = 0;
		}
	}

	public virtual void Reset(bool fromRightSideButton) {
		if(!fromRightSideButton) {
			winType.Hide();
			StartCoroutine(winType.WhenTilesOffScreen(() => {
				Won = false;
				winType.Reset();
			}));
		}
		else if(Won) {
			return;
		}
		AcceptingInputs = true;
		collectedStars = 0;
		goalFlag?.Reset();
		Grid?.Reset();
		Player?.SetAlive(false);
		Preview.Show(false);
		
		TimerRunning = false;
		ElapsedTime = 0;
		LevelInfo.SetTimer(ElapsedTime);

		SetPause(false);
	}

	public void StartTimer() {
		TimerRunning = true;
		RespawnManager.ActionButtons.ResetButton.interactable = true;
	}

	public void SetPause(bool pause) {
		if(!pause && !Player.CanUnpause()) {
			return;
		}
		Paused = pause;
		RespawnManager.ActionButtons.Pause(!Player.Alive || pause);
		Player.SetPaused(pause);
	}

	public void HideLevel() {
		LevelObjectContainer.SetActive(false);
		RespawnManager.ActionButtons.gameObject.SetActive(false);
		LevelInfo.gameObject.SetActive(false);
	}

	public void PlayerWin(GoalFlag gf) {
		goalFlag = gf;
		Won = true;

		int bi = SceneHelpers.GetCurrentLevelBuildIndex();
		SceneHelpers.GetWorldAndLevelFromBuildIndex(bi, out int world, out int level);

		// don't save data for tutorial levels
		if(world > 0 && level > 0) {
			timeInfo = new TimeInfo();
			timeInfo.Time = ElapsedTime;

			LevelData ld = GameManager.Instance.SaveData.LevelData[world-1, level-1];
			if(collectedStars > ld.MaxStarsCollected) {
				ld.MaxStarsCollected = collectedStars;
			}
			if(collectedStars < 3) {
				if(ld.AnyStarCompletionTime < 0 || ElapsedTime < ld.AnyStarCompletionTime) {
					ld.AnyStarCompletionTime = ElapsedTime;
					timeInfo.Record = true;
				}
			}
			else {
				if (ld.ThreeStarCompletionTime < 0 || ElapsedTime < ld.ThreeStarCompletionTime) {
					ld.ThreeStarCompletionTime = ElapsedTime;
					timeInfo.Record = true;
				}
			}
		}

		GameManager.Instance.SaveLevelCompleteData(bi+1);	
	}

	public void PlayerWinAnimation() {
		if (SelectedTile != null) {
			Preview.Show(false);
			SelectedTile.Select(false);
			SelectedTile = null;
		}
		grabPoint = new Vector2(1000, 1000);
		winType.Run(timeInfo, collectedStars, RespawnManager.Stars.Length, ActionSelected);

		cts = new CancellationTokenSource();

		int current_bi = SceneHelpers.GetCurrentLevelBuildIndex();
		int current_bi_world = SceneHelpers.GetWorldFromBuildIndex(current_bi);
		int next_bi = SceneHelpers.GetNextLevelBuildIndex();
		int next_bi_world = SceneHelpers.GetWorldFromBuildIndex(next_bi);
		if (current_bi_world > 0 && current_bi_world < next_bi_world) {
			next_bi = SceneHelpers.WorldCompleteBuildIndex;
		}

		// TODO: this if/else can be removed once we've guaranteed that every world has 10 levels
		// and replaced with just the statement inside the IF
		if (next_bi < SceneHelpers.SceneCount) {
			GameManager.Instance.AsyncLoadScene(next_bi, StartCoroutine(WaitActionSelected()), cts, null, false);
		}
		else {
			GameManager.Instance.SaveData.LastPlayedWorld = 0;
		}
		
	}

	public void ActionSelected(WinTypeAction w) {
		AcceptingInputs = false;

		int currentScene = SceneHelpers.GetCurrentLevelBuildIndex();
		// if we're not going to the next scene, cancel the load of the next scene
		if (w != WinTypeAction.Next) {
			cts.Cancel();
		}

		switch (w) {
			case WinTypeAction.Menu:
				GameManager.Instance.LoadScene(SceneHelpers.MenuBuildIndex, StartCoroutine(winType.WhenTilesOffScreen()));
				break;
			case WinTypeAction.Reset:
				GameManager.Instance.ShowAd();
				Reset(false);
				break;
			case WinTypeAction.LevelSelect:
				GoToLevelSelect(false);
				break;
			case WinTypeAction.Next:
				// TODO: All this logic can be removed once we've guaranteed that every world has 10 levels 
				int current_bi = SceneHelpers.GetCurrentLevelBuildIndex();
				int current_bi_world = SceneHelpers.GetWorldFromBuildIndex(current_bi);
				int next_bi = SceneHelpers.GetNextLevelBuildIndex();
				int next_bi_world = SceneHelpers.GetWorldFromBuildIndex(next_bi);
				if (next_bi < SceneHelpers.SceneCount || (current_bi_world > 0 && current_bi_world < next_bi_world)) {
					GameManager.Instance.ShowAd();

					// hide objects in the current level so that as the wintype animation is playing, we see the next level
					HideLevel();

					// once the tiles are offscreen, we can finally unload the level
					StartCoroutine(winType.WhenTilesOffScreen(() => {
						GameManager.Instance.UnloadScene(currentScene, null);
					}));
				}
				else {
					GoToLevelSelect(false);
				}
				break;
		}
	}

	public void GoToLevelSelect(bool fromRightSideButton) {
		if(fromRightSideButton) {
			if(Won) {
				return;
			}
			else {
				GameManager.Instance.LoadScene(SceneHelpers.MenuBuildIndex);
			}
		}
		else {
			GameManager.Instance.LoadScene(
				SceneHelpers.MenuBuildIndex,
				StartCoroutine(winType.WhenTilesOffScreen()),
				() => GameManager.Instance.MenuManager.OpenLevelSelect(true)
			);
		}
	}

	public IEnumerator WaitActionSelected() {
		yield return new WaitUntil( () => winType.ActionSelected );
	}

	public IEnumerator RunTimer() {
		float deltaT = 0.049f;
		var t = new WaitForSecondsRealtime(deltaT);
		while(true) {
			if (TimerRunning && !Won) {
				ElapsedTime += deltaT;
				LevelInfo.SetTimer(ElapsedTime);
			}
			yield return t;
		}
	}

	private void OnDestroy() {
		StopCoroutine(RunTimer());
		if(Player != null)
			Player.aliveChanged -= PlayerAliveChange;
	}
}

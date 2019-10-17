using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class LevelManager : ContextManager {

	public Grid Grid { get; set; }

	protected LayerMask tileMask;
	protected Tile SelectedTile;
	protected Vector3 grabPoint;
	protected Vector3 grabReleasePoint;

	protected WinType winType;
	protected int collectedStars;
	protected GoalFlag goalFlag;

	protected CancellationTokenSource cts;

	public GameObject LevelObjectContainer;

	public RespawnManager RespawnManager { get; protected set; }
	public bool Won { get; set; }

	public override void Start() {
		base.Start();
		Init();
		tileMask = 1 << LayerMask.NameToLayer("Tile");
	}

	public virtual void Init() {
		Grid = FindObjectsOfType<Grid>().First(g => g.gameObject.scene == this.gameObject.scene);
		winType = FindObjectsOfType<WinType>().First(g => g.gameObject.scene == this.gameObject.scene);
		CreateRespawnManager();
	}

	public virtual void CreateRespawnManager() {
		RespawnManager = new RespawnManager(gameObject.scene);
	}

	// called every frame from context manager
	public override void HandleInput(InputPackage p) {
		if(!Won) {
			if(p.Touchdown) { 
				if(p.TouchdownChange) {
					// clicked
					SelectedTile = Physics2D.OverlapPoint(p.MousePositionWorldSpace, tileMask)?.GetComponent<Tile>();

					if (SelectedTile != null && SelectedTile.Movable) {
						grabPoint = p.MousePositionWorldSpace;
						SelectedTile.Select(true);
					}
				}
				else if(SelectedTile != null) {
					float scale = SelectedTile.transform.lossyScale.x;
					Vector2 moveAmount = (p.MousePositionWorldSpace - grabPoint) / scale;
					Tilespace tileBeforeMove = SelectedTile.Space;
					bool moved = SelectedTile.TryMove(moveAmount, p.MousePositionWorldSpaceDelta);

					if(moved) {
						Vector3 move = ((Vector2)SelectedTile.transform.position - SelectedTile.PositionWhenSelected);
						grabPoint += move;
						if(Mathf.Abs(move.y) > Mathf.Abs(move.x)) {
							grabPoint.x = p.MousePositionWorldSpace.x;
						}
						else {
							grabPoint.y = p.MousePositionWorldSpace.y;
						}
						SelectedTile.Select(true);
					}
				}
			}
			else if(SelectedTile != null && !p.Touchdown && p.TouchdownChange) {
				SelectedTile.Select(false);
				SelectedTile = null;
			}
		}
	}

	public void AddStar() {
		collectedStars++;
	}

	public virtual void Respawn() {
		RespawnManager.RespawnPlayer();
	}

	public virtual void Reset(bool fromButton) {
		if(!fromButton) {
			winType.Hide();
		}
		AcceptingInputs = true;
		collectedStars = 0;
		goalFlag?.Reset();
		Grid?.Reset();
		RespawnManager.Player?.SetAlive(false);
		StartCoroutine(winType.WhenTilesOffScreen(() => {
			Won = false;
			winType.Reset();
		}));	
	}

	public void HideLevel() {
		LevelObjectContainer.SetActive(false);
		RespawnManager.ActionButtons.gameObject.SetActive(false);
	}

	public void PlayerWin(GoalFlag gf) {
		goalFlag = gf;
		Won = true;
		if(SelectedTile != null) {
			SelectedTile.Select(false);
			SelectedTile = null;
		}
		grabPoint = new Vector2(1000,1000);
		winType.Run(collectedStars, RespawnManager.Stars.Length, ActionSelected);

		cts = new CancellationTokenSource();
		GameManager.Instance.AsyncLoadScene(GameManager.Instance.GetNextLevelBuildIndex(), StartCoroutine(WaitActionSelected()), cts, null, false);
	}

	public void ActionSelected(WinTypeAction w) {
		AcceptingInputs = false;

		int currentScene = GameManager.Instance.GetCurrentLevelBuildIndex();
		// if we're not going to the next scene, cancel the load of the next scene
		if (w != WinTypeAction.Next) {
			cts.Cancel();
		}

		switch (w) {
			case WinTypeAction.Menu:
				GameManager.Instance.LoadScene(GameManager.MenuBuildIndex, StartCoroutine(winType.WhenTilesOffScreen()));
				break;
			case WinTypeAction.Reset:
				Reset(false);
				break;
			case WinTypeAction.LevelSelect:
				GameManager.Instance.LoadScene(
					GameManager.MenuBuildIndex,
					StartCoroutine(winType.WhenTilesOffScreen()),
					() => GameManager.Instance.MenuManager.OpenLevelSelect(false)
				);
				break;
			case WinTypeAction.Next:
				// hide objects in the current level so that as the wintype animation is playing, we see the next level
				HideLevel();

				// once the tiles are offscreen, we can finally unload the level
				StartCoroutine(winType.WhenTilesOffScreen(() => {
					GameManager.Instance.UnloadScene(currentScene, null);
				}));
				break;
		}
	}

	public IEnumerator WaitActionSelected() {
		yield return new WaitUntil( () => winType.ActionSelected );
	}
}

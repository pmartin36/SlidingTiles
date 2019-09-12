using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class LevelManager : ContextManager {

	public static event EventHandler<Vector2> Move;

	private LayerMask tileMask;
	private Tile SelectedTile;
	private Vector3 grabPoint;
	private Vector3 grabReleasePoint;

	private WinType winType;
	private int collectedStars;

	private CancellationTokenSource cts;
	private Coroutine waitLoadNextScene;

	public RespawnManager RespawnManager { get; private set; }
	public bool Won { get; set; }

	public override void Awake() {
		base.Awake();
		GetComponent<InputManager>().ContextManager = this;
		tileMask = 1 << LayerMask.NameToLayer("Tile");
		RespawnManager = GetComponent<RespawnManager>();
	}

	public override void Start() {
		base.Start();
		winType = FindObjectOfType<WinType>();
		PlayerWin();
	}

	public override void HandleInput(InputPackage p) {
		if(Won) {
			if(!winType.IsAnimating) {
				WinTypeAction w = WinTypeAction.None;
				if(p.LeftMouse) {
					if(p.LeftMouseChange || grabPoint.sqrMagnitude > 1000f) {
						grabPoint = p.MousePositionWorldSpace;
						grabReleasePoint = Vector2.zero;
					}
					else {
						// moving
						w = winType.SetGrabPosition(p.MousePositionWorldSpace - grabPoint);
					}
				}
				else if (!p.LeftMouse) {
					if(p.LeftMouseChange) {
						grabReleasePoint = p.MousePositionWorldSpace;
					}
					w = winType.SetPositionNoGrab(grabReleasePoint);
				}

				if(w != WinTypeAction.None) {
					// if we're not going to the next scene, cancel the load of the next scene
					if(w != WinTypeAction.Next) {
						cts.Cancel();
					}
					StopCoroutine(waitLoadNextScene);

					switch (w) {
						case WinTypeAction.Menu:
							GameManager.Instance.LoadScene(GameManager.MenuBuildIndex, StartCoroutine(winType.WhenTilesOffScreen()));
							break;
						case WinTypeAction.Reset:
							Reset();
							break;
						case WinTypeAction.LevelSelect:
							GameManager.Instance.LoadScene(GameManager.LevelSelectBuildIndex, StartCoroutine(winType.WhenTilesOffScreen()));
							break;
						case WinTypeAction.Next:
							break;
					}				
				}		
			}
		}
		else {
			if(p.LeftMouse) { 
				if(p.LeftMouseChange) {
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
			else if(SelectedTile != null && !p.LeftMouse && p.LeftMouseChange) {
				SelectedTile.Select(false);
				SelectedTile = null;
			}
		}
	}

	public static void ClearMovingTiles() {
		Move = null;
	}

	public void AddStar() {
		collectedStars++;
	}

	private void Reset() {
		winType.Hide();
		StartCoroutine(winType.WhenTilesOffScreen(() => {
			Won = false;
			winType.Reset();
		}));	
	}

	public void PlayerWin() {
		Won = true;
		if(SelectedTile != null) {
			SelectedTile.Select(false);
			SelectedTile = null;
		}
		grabPoint = new Vector2(1000,1000);
		RespawnManager?.Player?.SetAlive(false);
		winType.Run(collectedStars);

		cts = new CancellationTokenSource();
		waitLoadNextScene = StartCoroutine(WaitActionSelected());
		GameManager.Instance.LoadSceneAsync(GameManager.Instance.GetNextLevelBuildIndex(), waitLoadNextScene, cts);
	}

	public IEnumerator WaitActionSelected() {
		yield return new WaitUntil( () => false );
	}
}

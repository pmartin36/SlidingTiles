using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class LevelManager : ContextManager {

	public Grid Grid { get; set; }

	private LayerMask tileMask;
	private Tile SelectedTile;
	private Vector3 grabPoint;
	private Vector3 grabReleasePoint;

	private WinType winType;
	private int collectedStars;
	private GoalFlag goalFlag;

	private CancellationTokenSource cts;

	public RespawnManager RespawnManager { get; private set; }
	public bool Won { get; set; }
	public override bool AcceptingInputs => winType == null || !winType.ActionSelected;

	public override void Awake() {
		if(GameManager.Instance.LevelManager == null) {
			// if we're not coming from another level, init now
			// otherwise, we'll init when the other level is removed
			Init();
		}
		base.Awake();
		GetComponent<InputManager>().ContextManager = this;
		tileMask = 1 << LayerMask.NameToLayer("Tile");
	}

	public virtual void Init() {
		winType = FindObjectOfType<WinType>();
		RespawnManager = new RespawnManager();
	}

	public override void HandleInput(InputPackage p) {
		if(Won) {
			if(!winType.IsAnimating) {
				WinTypeAction w = WinTypeAction.None;
				if(p.Touchdown) {
					if(p.TouchdownChange || grabPoint.sqrMagnitude > 1000f) {
						grabPoint = p.MousePositionWorldSpace;
						grabReleasePoint = Vector2.zero;
					}
					else {
						// moving
						w = winType.SetGrabPosition(p.MousePositionWorldSpace - grabPoint);
					}
				}
				else if (!p.Touchdown) {
					if(p.TouchdownChange) {
						grabReleasePoint = p.MousePositionWorldSpace;
					}
					w = winType.SetPositionNoGrab(grabReleasePoint);
				}

				if(w != WinTypeAction.None) {
					int currentScene = GameManager.Instance.GetCurrentLevelBuildIndex();
					// if we're not going to the next scene, cancel the load of the next scene
					if(w != WinTypeAction.Next) {
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
							winType.Hide();
							Destroy(Grid.gameObject);
							RespawnManager.Destroy();
							StartCoroutine(winType.WhenTilesOffScreen(() => {
								GameManager.Instance.UnloadScene(currentScene, (a) => {
									GameManager.Instance.LevelManager.Init(); // not this one
								});
							}));
							break;
					}				
				}		
			}
		}
		else {
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
		collectedStars = 0;
		goalFlag?.Reset();
		Grid?.Reset();
		RespawnManager.Player?.SetAlive(false);
		StartCoroutine(winType.WhenTilesOffScreen(() => {
			Won = false;
			winType.Reset();
		}));	
	}

	public void PlayerWin(GoalFlag gf) {
		goalFlag = gf;
		Won = true;
		if(SelectedTile != null) {
			SelectedTile.Select(false);
			SelectedTile = null;
		}
		grabPoint = new Vector2(1000,1000);
		winType.Run(collectedStars);

		cts = new CancellationTokenSource();
		GameManager.Instance.AsyncLoadScene(GameManager.Instance.GetNextLevelBuildIndex(), StartCoroutine(WaitActionSelected()), cts, null, false);
	}

	public IEnumerator WaitActionSelected() {
		yield return new WaitUntil( () => winType.ActionSelected );
	}
}

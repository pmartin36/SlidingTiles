using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Tile : MonoBehaviour
{
	public const float BaseThreshold = 0.15f;
	private const float BaseThresholdSquared = BaseThreshold * BaseThreshold;

	private const float SpeedCap = 4f;

	public Tilespace Space { get; set; }

	public bool Movable;

	public bool Selected { get; set; }
	public Vector2 PositionWhenSelected { get; set; }

	public bool Centered { get => transform.localPosition.sqrMagnitude < 0.000002f; } // (skinWidth / scale)^2 

	private LayerMask tileMask;
	protected LayerMask playerMask;
	private SpriteRenderer spriteRenderer;

	private PlatformController[] childPlatforms;

	private Vector3 lastFrameVelocity;
	private Vector3 lastFramePosition;

	private bool initialMovable;
	private Tilespace initialTilespace;

	private static int LoadedMaterialWorld = -1;
	private static Material ImmobileMaterial;
	private static Material SelectedMaterial;
	private static Material UnselectedMaterial;

	private static AsyncOperationHandle<Material> OnImmobileMaterialLoad;

	private void Awake() {
		playerMask = 1 << LayerMask.NameToLayer("Player");
	}

	private void Start() {
		tileMask = 1 << LayerMask.NameToLayer("Tile");
		spriteRenderer = GetComponent<SpriteRenderer>();

		if(Movable && UnselectedMaterial != spriteRenderer.material) {
			UnselectedMaterial = spriteRenderer.material;
		}
		if(GameManager.Instance.LastPlayedWorld != LoadedMaterialWorld) {
			Debug.Log($"{LoadedMaterialWorld}  {GameManager.Instance.LastPlayedWorld}");
			LoadedMaterialWorld = GameManager.Instance.LastPlayedWorld;
		
			Addressables.LoadAssetAsync<Material>($"Level_SelectedTile").Completed +=
				(obj) => SelectedMaterial = obj.Result;

			Addressables.LoadAssetAsync<Material>($"World{LoadedMaterialWorld}/Level_ImmobileTile").Completed +=
				(obj) => ImmobileMaterial = obj.Result;
		}

		//if (!Movable) {
		//	OnImmobileMaterialLoad.Completed += (obj) => spriteRenderer.sharedMaterial = obj.Result;
		//}

		childPlatforms = GetComponentsInChildren<PlatformController>();
		lastFramePosition = transform.position;
		lastFrameVelocity = Vector3.zero;	

	}

	public void Init(Tilespace t) {
		this.Space = t;
		this.initialTilespace = t;
		this.initialMovable = this.Movable;
	}

	public virtual void Update() {
		LevelManager lm = GameManager.Instance.LevelManager;
		if(lm != null && lm.SnapAfterDeselected && lm.SelectedTile == null && !Centered) {
			Vector3 position = new Vector3(Mathf.Round(transform.localPosition.x), Mathf.Round(transform.localPosition.y));
			Vector3 moveAmount = position - transform.localPosition;

			float distToMove = Time.deltaTime * SpeedCap;
			if (distToMove < moveAmount.magnitude) {
				moveAmount = moveAmount.normalized * distToMove;
			}

			HashSet<Tile> tilesToMove = new HashSet<Tile>() { this };
			Vector2 v2 = position;
			Direction direction = GetDirectionFromPosition(ref v2);
			if (CanMoveTo(ref moveAmount, tilesToMove, direction)) {
				foreach (Tile t in tilesToMove) {
					t.Move(moveAmount, direction);
				}
			}
			return;
		}
		// try centering selected tiles that are close to being centered
		else if (Selected && transform.localPosition.sqrMagnitude <= BaseThresholdSquared) {
			float distToMove = Time.deltaTime * SpeedCap;
			Vector3 moveAmount;
			if (distToMove > transform.localPosition.magnitude) {
				moveAmount = -transform.localPosition;
			}
			else {
				moveAmount = -transform.localPosition.normalized * distToMove;
			}

			var noDirection = Direction.None;
			HashSet<Tile> tilesToMove = new HashSet<Tile>() { this };
            if (CanMoveTo(ref moveAmount, tilesToMove, noDirection)) {
				foreach (Tile t in tilesToMove) {
					t.Move(moveAmount, noDirection);
				}
			}
		}
	}

	public virtual void Select(bool select) {
		Selected = select;
		if (Selected) {
			PositionWhenSelected = transform.position;
		}
		spriteRenderer.sharedMaterial = 
			Movable  
				? Selected 
					? SelectedMaterial 
					: UnselectedMaterial
				: ImmobileMaterial;
	}

	public void CompleteMove(Tilespace space) {
		this.Space = space;
		space.Tile = this;		
	}

	public virtual bool Move(Vector3 moveAmount, Direction d) {
		Vector2 globalMoveAmount = moveAmount * transform.lossyScale.x;

		foreach (PlatformController c in childPlatforms) {
			c.Premove(ref globalMoveAmount);
		}
		transform.localPosition += moveAmount;
		foreach (PlatformController c in childPlatforms) {
			c.Postmove(ref globalMoveAmount);
		}

		float mag = transform.localPosition.magnitude;
		if(mag > BaseThreshold) {
			if (mag > 1 - BaseThreshold) {
				// snap to next spot
				Tilespace next = Space.GetNeighborInDirection(d);		
				if(Space.Tile == this) {
					Space.Tile = null;
				}
			
				next.SetChildTile(this);
				return true;
			}	
		}
		return false;
	}

	public bool CanMoveTo(ref Vector3 moveAmount, HashSet<Tile> tilesToMove, Direction d) {
		bool goingTowardCenter = (transform.localPosition + moveAmount).sqrMagnitude < transform.localPosition.sqrMagnitude;// && Vector2.Dot(localPosition, transform.localPosition) < 0;
		bool validTilespace = d.Value.sqrMagnitude < 0.1f || Space.GetNeighborInDirection(d) != null;
		if(validTilespace || goingTowardCenter) {
			Vector3 DebugOriginalMoveAmount = new Vector2(moveAmount.x, moveAmount.y);
			Vector2 size = Mathf.Abs(moveAmount.x) > 0.0001f 
				? new Vector2(moveAmount.magnitude * 0.5f, transform.lossyScale.x * 0.90f)
				: new Vector2(transform.lossyScale.x * 0.90f, moveAmount.magnitude * 0.5f);
			Collider2D[] collisions = 
				Physics2D.OverlapBoxAll(
					this.transform.position + moveAmount.normalized * (transform.lossyScale.x + moveAmount.magnitude + 0.001f) * 0.5f,
					size, 
					0,
					tileMask)
					.Where(r => r.gameObject != this.gameObject).ToArray();

			Vector2 globalMoveAmount = moveAmount * transform.lossyScale.x;
			foreach (PlatformController p in childPlatforms) {
				p.CheckBlocking(ref globalMoveAmount, tilesToMove);
			}
			Vector2 newMove = globalMoveAmount / transform.lossyScale.x;
			if(newMove.sqrMagnitude - moveAmount.sqrMagnitude > 0.1f) {
				// TODO (Audio): Play *dink* metal noise

			}
			moveAmount = newMove;

			if (collisions.Length == 0) {
				return true;
			}
			else if (collisions.Length == 1) {
				Debug.DrawRay(this.transform.position, transform.lossyScale * 0.5f + moveAmount, Color.red);
			
				Tile collidedTile = collisions[0].GetComponent<Tile>();
				bool canMoveInDirection = collidedTile.Centered || Mathf.Abs(Vector2.Dot(moveAmount.normalized, collidedTile.transform.localPosition.normalized)) > 0.1f;
				if (collidedTile.Movable && !collidedTile.Selected && canMoveInDirection) {		
					// Debug.DrawLine(this.transform.position, hit.transform.position, Color.blue, 0.25f);
					tilesToMove.Add(collidedTile);
					return collidedTile.CanMoveTo(ref moveAmount, tilesToMove, d);
				}
			}
		}
		return false;
	}

	public bool TryMove(Vector2 mouseMoveSinceSelection, Vector2 delta) {
		if(Movable) {
			Vector2 position = mouseMoveSinceSelection + (this.PositionWhenSelected - (Vector2)Space.transform.position) / transform.lossyScale.x;
			bool centered = Centered;
			float deltaTimeSpeedCap = SpeedCap * Time.deltaTime;

			if(!centered) {
				Vector2 normalized = new Vector2(Mathf.Abs(transform.localPosition.x), Mathf.Abs(transform.localPosition.y)).normalized;
				var pNorm = position * normalized;
				float pNormMag = pNorm.magnitude;

				Vector2 orthogonalVector = (position - pNorm);
				float orthogonalVectorMag = orthogonalVector.magnitude;

				// if the direction orthogonal to the movement direction is larger, 
				// we'll assume player is trying to change direction and center the tile
				if (pNormMag < 1 
					&& orthogonalVectorMag > pNormMag
					&& orthogonalVectorMag > deltaTimeSpeedCap 
					&& orthogonalVectorMag > BaseThreshold * 2) {
					if((1 - pNormMag) < BaseThreshold * 2) {
						position = normalized;
					}
					else if(pNormMag < BaseThreshold * 2) {
						position = Vector2.zero;
					}
					else {
						position = pNorm;
					}
				}
				else {
					position = pNorm;
				}
			}

			Direction direction = GetDirectionFromPosition(ref position);
			
			if(!centered && transform.localPosition.magnitude < BaseThreshold) {
				position = transform.localPosition.normalized * Mathf.Min(transform.localPosition.magnitude, BaseThreshold - 0.001f);
			}
			else if(centered && position.magnitude < BaseThreshold) {
				return false;	
			}

			// limit movement
			Vector3 move = (position - (Vector2)transform.localPosition);
			if (centered && position.magnitude > BaseThreshold) {
				position = (BaseThreshold + 0.001f) * position.normalized;
			}
			else if (move.magnitude > deltaTimeSpeedCap) {
				position = transform.localPosition + move.normalized * deltaTimeSpeedCap;
			}

			float mag = position.magnitude;
			if(position.magnitude >= 1) {
				position = position.normalized;
			}

			bool moved = false;
			float scale = transform.lossyScale.x;
			Vector3 moveAmount = position - (Vector2)transform.localPosition;

			HashSet<Tile> tilesToMove = new HashSet<Tile>();
						
			if (CanMoveTo(ref moveAmount, tilesToMove, direction)) {
				moved = this.Move(moveAmount, direction);
				foreach (Tile t in tilesToMove) {
					var tMoveAmount = moveAmount;
					t.Move(tMoveAmount, direction);
				}
			}
			return moved;
		}
		return false;
	}

	public Direction GetDirectionFromPosition(ref Vector2 position) {
		float absX = Mathf.Abs(position.x);
		float absY = Mathf.Abs(position.y);
		if (absX > absY) {		
			position *= Vector2.right;
			return Mathf.Sign(position.x) > 0 ? Direction.Right : Direction.Left;
		}
		else {
			position *= Vector2.up;
			return Mathf.Sign(position.y) > 0 ? Direction.Up : Direction.Down;
		}
	}

	public void Reset() {
		transform.parent = initialTilespace.transform;
		this.Space = initialTilespace;
		this.Movable = initialMovable;
		transform.localPosition = Vector2.zero;
	}

	public void SetImmobile() {
		Movable = false;
		if(Selected) {
			Select(false);
		}
	}

	public bool IsPlayerOnTile() => Physics2D.OverlapBox(transform.position, transform.lossyScale, 0, playerMask) != null;
}

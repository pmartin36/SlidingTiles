using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MoreMountains.NiceVibrations;

public class Tile : MonoBehaviour, IRequireResources
{
	public bool Loaded { get; set; }
	public const float BaseThreshold = 0.15f;
	private const float BaseThresholdSquared = BaseThreshold * BaseThreshold;
	private bool movedThisFrame;

	private const float SpeedCap = 4f;

	public Tilespace Space { get; set; }

	public Movable Movable;

	public bool Selected { get; set; }
	public Vector2 PositionWhenSelected { get; set; }
	public Vector2 ResidualVelocity { get; set; }

	public bool Centered { get => transform.localPosition.sqrMagnitude < 0.000002f; } // (skinWidth / scale)^2 
	public Vector2 NormalizedPosition => new Vector2(Mathf.Abs(transform.localPosition.x), Mathf.Abs(transform.localPosition.y)).normalized;

	private LayerMask tileMask;
	protected LayerMask playerMask;

	protected List<SpriteRenderer> spriteRenderers;
	protected SpriteRenderer bottom;
	protected SpriteRenderer top;

	protected AudioSource audio;

	protected PlatformController[] childPlatforms;

	private Vector3 lastFramePosition;
	private float movingVelocityAverage;

	private bool initialMovable;
	private bool temporaryUnmovable;

	private static int LoadedMaterialWorld = -1;
	private static Material ImmobileMaterial;
	private static Material SelectedMaterial;
	private List<Material> AllMaterials;

	private static AudioClip StickySound;
	private static AudioClip MoveSound;

	private bool ResetThisFrame;

	// Rotation Info
	private TileRotator rotator;
	private static RotationInfo RotationInfo;
	public bool Rotating => rotator != null;

	public float Rotation0to90 => Rotation % 90;
	public float RotationPercent => Mathf.Abs(Rotation0to90) / 90;

	public float Rotation { get; set; }
	public int RotationDirection { get; set; }

	public SpriteRenderer Box;
	public SpriteRenderer BoxBottom;
	private SpriteRenderer BoxSide;

	protected void Awake() {
		playerMask = 1 << LayerMask.NameToLayer("Player");
	}

	protected virtual void Start() {
		tileMask = 1 << LayerMask.NameToLayer("Tile");
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
		bottom = spriteRenderers.FirstOrDefault(g => g.CompareTag("TileBottom"));
		top = spriteRenderers.FirstOrDefault(g => !g.CompareTag("TileBottom"));

		top.sortingOrder = -this.Space.Position.y * 3;
		if(bottom != null)
			bottom.sortingOrder = top.sortingOrder - 2;

		audio = GetComponent<AudioSource>();
		AllMaterials = new List<Material>() { top.sharedMaterial };

		if(GameManager.Instance.LastPlayedWorld != LoadedMaterialWorld && Movable) {
			LoadedMaterialWorld = GameManager.Instance.LastPlayedWorld;
			int loadedObjects = 0;
		
			Addressables.LoadAssetAsync<Material>($"Level_SelectedTile").Completed +=
				(obj) => {
					SelectedMaterial = obj.Result;
					Loaded = ++loadedObjects >= 2;
				};
			Addressables.LoadAssetAsync<AudioClip>("StickySound").Completed +=
				(obj) => {
					StickySound = obj.Result;
					Loaded = ++loadedObjects >= 2;
				};
			Addressables.LoadAssetAsync<RotationInfo>("RotationInfo").Completed +=
			(obj) => {
				RotationInfo = obj.Result;
			};
		}
		else {
			Loaded = true;
		}

		if(MoveSound == null || MoveSound.name != audio.clip.name) {
			MoveSound = audio.clip;
		}

		//if (!Movable) {
		//	OnImmobileMaterialLoad.Completed += (obj) => spriteRenderer.sharedMaterial = obj.Result;
		//}

		childPlatforms = GetComponentsInChildren<PlatformController>();
		lastFramePosition = transform.position;
		ResetThisFrame = true;
	}

	public void Init(Tilespace t) {
		this.Space = t;
		this.initialMovable = this.Movable;
	}

	public virtual void FixedUpdate() {
		LevelManager lm = GameManager.Instance.LevelManager;
		if(lm.Won) return;

		if(!movedThisFrame && ResidualVelocity.sqrMagnitude > 0.001f) {
			ResidualMove();


			//HashSet<Tile> tilesToMove = new HashSet<Tile>() { this };
			//Vector2 moveDirection = ResidualVelocity.normalized;
			//Direction direction = GetDirectionFromPosition(ref moveDirection);
			//if (Vector2.Dot(position, transform.localPosition) < 0) {
			//	moveAmount = -(Vector2)transform.localPosition;
			//}
			//if (CanMoveTo(ref moveAmount, tilesToMove, direction)) {
			//	foreach (Tile t in tilesToMove) {
			//		t.Move(moveAmount, direction);
			//	}
			//}
		}

		if (!movedThisFrame && lm != null && lm.SnapAfterDeselected && lm.SelectedTile == null && !Centered) {
			Vector3 position = new Vector3(Mathf.Round(transform.localPosition.x), Mathf.Round(transform.localPosition.y));
			Vector3 moveAmount = position - transform.localPosition;

			float distToMove = Time.fixedDeltaTime * SpeedCap;
			if (distToMove < moveAmount.magnitude) {
				moveAmount = moveAmount.normalized * distToMove;
			}

			HashSet<Tile> tilesToMove = new HashSet<Tile>() { this };
			Vector2 v2 = position;
			Direction direction = GetDirectionFromPosition(ref v2);
			if (CanMoveTo(ref moveAmount, tilesToMove, direction)) {
				foreach (Tile t in tilesToMove.Reverse()) {
					if(!t.Centered) {
						t.Move(moveAmount, direction);
					}
				}
			}
		}
		// try centering selected tiles that are close to being centered
		else if (Selected && transform.localPosition.sqrMagnitude <= BaseThresholdSquared) {
			float distToMove = Time.fixedDeltaTime * SpeedCap;
			Vector3 moveAmount = -transform.localPosition;
			if (distToMove < transform.localPosition.magnitude) {
				moveAmount = -transform.localPosition.normalized * distToMove;
			}

			var noDirection = Direction.None;
			HashSet<Tile> tilesToMove = new HashSet<Tile>() { this };
            if (CanMoveTo(ref moveAmount, tilesToMove, noDirection)) {
				foreach (Tile t in tilesToMove.Reverse()) {
					t.Move(moveAmount, noDirection);
				}
			}
		}

		movedThisFrame = false;
	}

	public virtual void Update() {
		if(Rotating) {
			UpdateMaterialProperties();
			UpdateRotations();
		}
	}

	public virtual void LateUpdate() {
		if(Movable && !temporaryUnmovable) {
			if(ResetThisFrame) {
				movingVelocityAverage = 0f;
			}
			else {
				Vector2 diff = (transform.position - lastFramePosition) / transform.localScale.x;
				movingVelocityAverage = 0.25f * diff.magnitude + movingVelocityAverage * 0.75f;
			}

			if(movingVelocityAverage < 0.001f) {
				if (audio.enabled) {
					audio.enabled = false;
				}
			}
			else {
				if (!audio.enabled) {
					audio.enabled = true;
					audio.time = audio.clip.length * Random.value;
				}

				// 0.9 is basically max speed
				// 0.01 is barely moving
				float v = Mathf.InverseLerp(-0.2f, 0.4f, movingVelocityAverage);
				audio.volume = Mathf.Lerp(0.0f, Selected ? 1f : 0.8f, v) * GameManager.Instance.SaveData.FxVolume;
				
				//float p = Mathf.InverseLerp(0f, 0.6f, movingVelocityAverage);
				//audio.pitch = Mathf.Lerp(0.8f, 1f, movingVelocityAverage);
			}
			lastFramePosition = transform.position;
		}
		ResetThisFrame = false;
	}

	public virtual void SetResidualVelocity(Vector2 avgVelocity) {
		if(avgVelocity.sqrMagnitude > 1.5f) {
			ResidualVelocity = SpeedCap * avgVelocity.normalized;
			ResidualMove();
		}
	}

	public void ResidualMove() {
		Vector3 moveAmount = ResidualVelocity * Time.fixedDeltaTime;
		Vector2 position = moveAmount + transform.localPosition;
		TryMoveToPosition(position, moveAmount, true);
	}

	public virtual void Select(bool select) {
		Selected = select;
		if (Selected) {
			PositionWhenSelected = transform.position;
			ResidualVelocity = Vector2.zero;

			if(GameManager.Instance.LevelManager.ShowSelectionMaterial && !AllMaterials.Contains(SelectedMaterial)) {
				AllMaterials.Add(SelectedMaterial);
				top.sharedMaterials = AllMaterials.ToArray();
			}
		}
		else if(GameManager.Instance.LevelManager.ShowSelectionMaterial) {
			AllMaterials.RemoveAll(m => m == SelectedMaterial);
			top.sharedMaterials = AllMaterials.ToArray();
		}
	}

	public void CompleteMove(Tilespace space) {
		this.Space = space;
		space.Tile = this;		
	}

	public virtual bool Move(Vector3 moveAmount, Direction d, bool markMoved = true) {
		// if we're going up to the border, make sure we don't overshoot
		if (!Centered && Space.GetNeighborInDirection(d) == null && Vector2.Dot(transform.localPosition, transform.localPosition + moveAmount) < 0.00001f) {
			moveAmount = -transform.localPosition;
		}

		Vector2 globalMoveAmount = moveAmount * transform.lossyScale.x;
		
		foreach (PlatformController c in childPlatforms) {
			c.Premove(ref globalMoveAmount);
		}
		transform.localPosition += moveAmount;
		foreach (PlatformController c in childPlatforms) {
			c.Postmove(ref globalMoveAmount);
		}
		//Debug.Log($"tile move: {moveAmount.y * transform.lossyScale.y}");

		bool changedTilespaces = false;

		float mag = transform.localPosition.magnitude;
		if (mag > 1 - BaseThreshold) {
			// snap to next spot
			Tilespace next = Space.GetNeighborInDirection(d);		
			if(Space.Tile == this) {
				Space.Tile = null;
			}

			next.SetChildTile(this, true);
			changedTilespaces = true;
			top.sortingOrder = -this.Space.Position.y * 3;
			if (bottom != null)
				bottom.sortingOrder = top.sortingOrder - 2;
		}

		if (Space.Sticky && transform.localPosition.magnitude < BaseThresholdSquared) {
			this.SetMovable(false);
			MMVibrationManager.Haptic(HapticTypes.LightImpact);
		}
		if(markMoved) {
			movedThisFrame = true;
		}
		return changedTilespaces;
	}

	public bool CanMoveTo(ref Vector3 moveAmount, HashSet<Tile> tilesToMove, Direction d) {
		bool goingTowardCenter = (transform.localPosition + moveAmount).sqrMagnitude < transform.localPosition.sqrMagnitude;// && Vector2.Dot(localPosition, transform.localPosition) < 0;
		bool validTilespace = d.Value.sqrMagnitude < 0.1f || Space.GetNeighborInDirection(d) != null;
		if(validTilespace || goingTowardCenter) {
			Vector3 DebugOriginalMoveAmount = new Vector2(moveAmount.x, moveAmount.y);
			Vector2 size = Mathf.Abs(moveAmount.x) > 0.0001f 
				? new Vector2(moveAmount.magnitude * 0.5f, 0.90f) * transform.lossyScale.x
				: new Vector2(0.90f, moveAmount.magnitude * 0.5f) * transform.lossyScale.x;
			RaycastHit2D[] collisions = 
				Physics2D.BoxCastAll(
					this.transform.position + moveAmount.normalized * ((1 + moveAmount.magnitude) * transform.lossyScale.x * 0.5f),
					size, 
					0,
					Vector2.zero,
					0,
					tileMask)
					.Where(r => r.collider.gameObject != this.gameObject).ToArray();
			Debug.DrawRay(this.transform.position, moveAmount.normalized * ((transform.lossyScale.x + 0.001f) * 0.5f + moveAmount.magnitude * transform.lossyScale.x), Color.white);
			Vector2 globalMoveAmount = moveAmount * transform.lossyScale.x;
			foreach (PlatformController p in childPlatforms) {
				p.CheckBlocking(ref globalMoveAmount, tilesToMove);
			}
			Vector2 newMove = globalMoveAmount / transform.lossyScale.x;
			//if(newMove.sqrMagnitude - moveAmount.sqrMagnitude > 0.1f) {
			//	// TODO (Audio): Play *dink* metal noise
			//}
			moveAmount = newMove;

			if (collisions.Length == 0) {
				return true;
			}
			else if (collisions.Length == 1) {
				Debug.DrawLine(this.transform.position, collisions[0].point, Color.red);
			
				Tile collidedTile = collisions[0].collider.GetComponent<Tile>();
				bool canMoveInDirection = collidedTile.Centered || Mathf.Abs(Vector2.Dot(moveAmount.normalized, collidedTile.transform.localPosition.normalized)) > 0.1f;
				if (collidedTile.Movable && !collidedTile.temporaryUnmovable && !collidedTile.Selected && canMoveInDirection) {		
					// Debug.DrawLine(this.transform.position, hit.transform.position, Color.blue, 0.25f);
					tilesToMove.Add(collidedTile);
					return collidedTile.CanMoveTo(ref moveAmount, tilesToMove, d);
				}
			}
		}
		return false;
	}

	public Vector2 GetPositionFromInput(Vector2 mouseMoveSinceSelection) {
		return mouseMoveSinceSelection + (this.PositionWhenSelected - (Vector2)Space.transform.position) / transform.lossyScale.x;
	}

	public bool TryMoveToPosition(Vector2 position, Vector2 delta, bool fromResidual = false) {
		if(Movable && !temporaryUnmovable) {
			bool centered = Centered;
			float deltaTimeSpeedCap = SpeedCap * Time.fixedDeltaTime;

			if(!centered) {
				if(transform.localPosition.magnitude < BaseThresholdSquared) {
					Move(transform.localPosition, Direction.None, false);
					centered = true;
				}
				else {
					Vector2 normalized = NormalizedPosition;
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
			}

			Direction direction = GetDirectionFromPosition(ref position);
			

			if (!fromResidual) {
				if (!centered && transform.localPosition.magnitude < BaseThreshold && Vector2.Dot(position, transform.localPosition) > 0) {
					position = transform.localPosition.normalized * Mathf.Min(transform.localPosition.magnitude, BaseThreshold - 0.001f);
				}
				else if (centered && position.magnitude < BaseThreshold) {
					return false;
				}
			}

			Vector3 moveAmount = position - (Vector2)transform.localPosition;
			float magBeforeLimiting = position.magnitude;
			// limit speed to max movement speed
			if (moveAmount.magnitude > deltaTimeSpeedCap) {
				moveAmount = moveAmount.normalized * deltaTimeSpeedCap;
				position = transform.localPosition + moveAmount;
			}

			// make sure we escape base threshold when uncentering
			float mag = position.magnitude;
			if (centered && magBeforeLimiting > BaseThreshold) {
				mag = Mathf.Max(BaseThreshold + 0.001f, position.magnitude);
				position = mag * position.normalized;
			}

			if(position.magnitude >= 1) {
				position = position.normalized;
			}

			HashSet<Tile> tilesToMove = new HashSet<Tile>();
			moveAmount = position - (Vector2)transform.localPosition;
			bool moved = false;

			bool canMove = CanMoveTo(ref moveAmount, tilesToMove, direction);
			if (canMove) {
				var movingTiles = tilesToMove.Reverse();
				foreach (Tile t in movingTiles) {
					var originalPosition = (Vector2)t.transform.localPosition;
					var tMoveAmount = position - originalPosition;

					t.Move(tMoveAmount, direction);
					if(fromResidual) {
						t.ResidualVelocity = this.ResidualVelocity;
					}
					
					Vector2 actualMovement = (Vector2)t.transform.localPosition - originalPosition;
					if(actualMovement.sqrMagnitude < tMoveAmount.sqrMagnitude) {
						position = originalPosition + actualMovement;
					}
				}
				moved = this.Move(moveAmount, direction);
			}

			// if movement is off by 90 degrees, it's changed direction from the residual velocity - throw it out
			if(!canMove || Mathf.Abs(Vector2.Dot(direction.Value, ResidualVelocity)) < 0.25f) {
				ResidualVelocity = Vector2.zero;
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
		SetMovable(initialMovable, false);
		SetTemporaryUnmovable(false);

		transform.localPosition = Vector2.zero; // parent set in tilespace.reset
		this.ResidualVelocity = Vector2.zero;
		this.ResetThisFrame = true;

		this.Rotation = 0f;
		EndRotation();

		if (Movable && audio.clip.name != MoveSound.name) {
			audio.clip = MoveSound;
			audio.pitch = 0.8f;
			audio.loop = true;
		}
	}

	public void SetTemporaryUnmovable(bool unmovable) {
		temporaryUnmovable = unmovable;
	}

	public void SetMovable(bool movable, bool animate = true) {
		temporaryUnmovable = false;
		Movable = movable;
		if(!movable) {
			if (Selected) {
				Select(false);
			}
			audio.Stop();
			StopCoroutine(SetImmobileAnimation());
			if(animate) {
				PlayStickySound();
				StartCoroutine(SetImmobileAnimation());
			}
			else {
				SetMobileShaderValue(0f);
			}
		}
		else {
			SetMobileShaderValue(1f);
		}
	}

	private void PlayStickySound() {
		audio.Stop();
		audio.clip = StickySound;
		audio.volume = 0.6f * GameManager.Instance.SaveData.FxVolume;
		audio.pitch = 0.8f;
		audio.loop = false;
		audio.time = 0f;
		audio.Play();
	}

	private void UpdateMaterialProperties() {
		float r = RotationPercent;
		bool positive = RotationDirection > 0;
		if (RotationInfo != null) {
			float rx = RotationInfo.rightPoint_x.Evaluate(r);
			float lx = RotationInfo.leftPoint_x.Evaluate(r);

			BoxBottom.sharedMaterial.SetFloat("_Y", RotationInfo.Y.Evaluate(r));
			BoxBottom.sharedMaterial.SetFloat("_LeftX", positive ? rx : lx);
			BoxBottom.sharedMaterial.SetFloat("_RightX", positive ? lx : rx);

			if (BoxSide != null) {
				rx = RotationInfo.rightPoint_x.Evaluate(1 - r);
				lx = RotationInfo.leftPoint_x.Evaluate(1 - r);

				BoxSide.sharedMaterial.SetFloat("_Y", RotationInfo.Y.Evaluate(1 - r));
				BoxSide.sharedMaterial.SetFloat("_RightX", positive ? rx : lx);
				BoxSide.sharedMaterial.SetFloat("_LeftX", positive ? lx : rx);
			}
		}
	}

	private void UpdateRotations() {
		Box.transform.localRotation = Quaternion.Euler(0, 0, Rotation);
		float diff = Rotation0to90;
		float bbr = BoxBottom.transform.eulerAngles.z;
		while (bbr - diff > 360) bbr -= 360;
		while (diff - bbr > 360) bbr += 360;
		diff -= bbr;
		BoxBottom.transform.RotateAround(Box.transform.position, Vector3.forward, diff);
		if (BoxSide != null) {
			BoxSide.transform.RotateAround(Box.transform.position, Vector3.forward, diff);
		}

		foreach (PlatformController p in childPlatforms) {
			var blocker = p.GetCurrentBlocker();
			if (blocker) {
				blocker.collider.GetComponent<IPlatformMoveBlocker>().MoveFromRotation(diff, blocker.point, this.transform.position);
				return;
			}
		}
	}

	public void BeginRotation(int direction, TileRotator rotator) {
		this.rotator = rotator;
		audio.volume = 0;
		RotationDirection = direction;
		SetTemporaryUnmovable(true);
		if (BoxSide == null) {
			CreateBoxSide();
		}

		BoxSide.sortingOrder += 10;
		BoxBottom.sortingOrder += 10;
		Box.sortingOrder += 10;

		var bbPos = BoxBottom.transform.localPosition;
		var boxPos = Box.transform.localPosition;
		var diff = boxPos.y - bbPos.y;
		if (direction > 0) {
			BoxSide.transform.localPosition = new Vector3(-diff, boxPos.y, bbPos.z);
			BoxSide.transform.localEulerAngles = new Vector3(0, 0, -90);
		}
		else {
			BoxSide.transform.localPosition = new Vector3(diff, boxPos.y, bbPos.z);
			BoxSide.transform.localEulerAngles = new Vector3(0, 0, 90);
		}
	}

	public void EndRotation() {
		rotator?.ClearEffectedTile();
		rotator = null;
		SetTemporaryUnmovable(false);
		RotationComplete();
	}

	private void RotationComplete() {
		UpdateMaterialProperties();

		Box.transform.localRotation = Quaternion.Euler(0, 0, Rotation);
		Box.sortingOrder -= 10;
		BoxBottom.transform.RotateAround(Box.transform.position, Vector3.forward, -BoxBottom.transform.eulerAngles.z);
		BoxBottom.sortingOrder -= 10;
		if(BoxSide != null) {
			BoxSide.transform.RotateAround(Box.transform.position, Vector3.forward, -90 - BoxSide.transform.eulerAngles.z);
			BoxSide.sortingOrder -= 10;
		}
	}

	private void CreateBoxSide() {
		var go = GameObject.Instantiate(BoxBottom.gameObject);
		BoxSide = go.GetComponent<SpriteRenderer>();
		BoxSide.material = new Material(BoxBottom.material);
		BoxSide.sortingOrder = BoxBottom.sortingOrder + 1;

		BoxSide.transform.parent = this.transform;
		BoxSide.transform.localScale = Vector3.one;

		UpdateMaterialProperties();
	}

	private float Convert0To90(float r) {
		while (r < 0) r += 360;
		return (r % 90);
	}

	protected virtual void SetMobileShaderValue(float v, Material mTop = null, Material mBottom = null) {
		mTop = mTop ?? new Material(top.sharedMaterial);
		mBottom = mBottom ?? new Material(bottom.sharedMaterial);
		mTop.SetFloat("_Mobile", v);
		mBottom.SetFloat("_Mobile", v);
		top.sharedMaterial = mTop;
		bottom.sharedMaterial = mBottom;
	}

	protected virtual IEnumerator SetImmobileAnimation() {
		float t = 0;
		float animationTime = 0.25f;
		Material mTop = new Material(top.sharedMaterial);
		Material mBottom = new Material(bottom.sharedMaterial);
		while(t < animationTime) {
			SetMobileShaderValue(1 - t / animationTime, mTop, mBottom);
			t += Time.deltaTime;
			yield return null;
		}
		SetMobileShaderValue(0, mTop, mBottom);
	}

	public bool IsPlayerOnTile() => Physics2D.OverlapBox(transform.position, transform.lossyScale, 0, playerMask) != null;
}

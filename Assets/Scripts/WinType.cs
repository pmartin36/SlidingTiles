using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum WinTypeAction {
	None,
	Menu, // up
	Reset, // left
	LevelSelect, // down
	Next // right
}

public abstract class WinType : MonoBehaviour {
	public Image[] Stars;
	public Image Background;
	public bool IsAnimating;

	protected readonly float scale = 18f;

	protected readonly float threshold = 90f * 90f;
	protected Vector2 ActionThreshold { get; private set; }
	public bool ActionSelected { get; private set; }

	[SerializeField]
	protected RectTransform frontPanel; 

	public GameObject Arrows;

	protected Vector2 Position {
		get => frontPanel.anchoredPosition;
		set => frontPanel.anchoredPosition = value;
	}

	protected bool Centered {
		get => Position.sqrMagnitude < 10;
	}

	public virtual void Start() {
		ActionThreshold = new Vector2(800, 450);
	}

	public virtual void Run(int stars) {
		frontPanel.gameObject.SetActive(true);
		IsAnimating = true;
		for (int i = 0; i < stars; i++) {
			Stars[i].color = Color.white;
		}
	}

	public WinTypeAction SetGrabPosition(Vector2 offset) {
		if(Centered) {
			offset = offset.SnapToAxis() * scale;
			if(offset.sqrMagnitude < threshold) {
				offset = Vector2.zero;
			}
		}		
		else {
			Vector2 snap = Mathf.Abs(Position.x) > Mathf.Abs(Position.y) ? Vector2.right : Vector2.up;
			offset = offset * snap * scale;
		}

		return SetPosition(offset);
	}

	public WinTypeAction SetPositionNoGrab(Vector2 grabReleasePoint) {
		Vector2 dir = Position.normalized;
		float moveAmount = 2000 * Time.deltaTime;

		Vector2 target = Vector2.zero;
		if((grabReleasePoint * dir * scale).sqrMagnitude > (ActionThreshold * dir).sqrMagnitude) {
			target = ActionThreshold * dir * 2f;
		}

		return SetPosition(target);
	}

	private WinTypeAction SetPosition(Vector2 targetPosition) {
		if(ActionSelected) {
			float moveAmount = 2000 * Time.deltaTime;
			Position += moveAmount * Position.normalized;
		}
		else {
			float moveAmount = 2000 * Time.deltaTime;
			Vector2 diff = (targetPosition - Position);
			if (diff.magnitude > moveAmount) {
				Position += diff.normalized * moveAmount;
			}
			else {
				Position = targetPosition;
			}

			if(!Centered && Position.sqrMagnitude > (ActionThreshold * Position.normalized).sqrMagnitude) {
				// start transition
				ActionSelected = true;
				if(Position.x > 100) {
					return WinTypeAction.Next;
				}
				else if(Position.x < -100) {
					return WinTypeAction.Reset;
				}
				else if(Position.y > 100) {
					return WinTypeAction.LevelSelect;
				}
				else if(Position.y < -100) {
					return WinTypeAction.Menu;
				}
			}	
		}
		return WinTypeAction.None;
	}

	public void Hide() {
		GetComponent<Animator>().Play("unblack");
	}

	public virtual void Reset() {
		frontPanel.gameObject.SetActive(false);
		Position = Vector2.zero;
		IsAnimating = false;
		ActionSelected = false;
	}

	public IEnumerator WhenTilesOffScreen(Action action = null) {
		var threshold = (ActionThreshold + Vector2.one) * 2; 
		yield return new WaitUntil(() => Mathf.Abs(Position.x) > threshold.x || Mathf.Abs(Position.y) > threshold.y);
		action?.Invoke();
	}
}


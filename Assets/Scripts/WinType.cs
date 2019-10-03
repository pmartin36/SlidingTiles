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
	[HideInInspector]
	public bool IsAnimating;

	public bool ActionSelected { get; private set; }
	protected Vector2 ActionMoveDirection;

	private Action<WinTypeAction> OnActionSelected;

	[SerializeField]
	protected RectTransform frontPanel; 
	protected Vector2 Position {
		get => frontPanel.anchoredPosition;
		set => frontPanel.anchoredPosition = value;
	}

	public virtual void Start() {
	
	}

	public virtual void Run(int stars, int availableStars = 3, Action<WinTypeAction> callback = null) {
		frontPanel.gameObject.SetActive(true);
		IsAnimating = true;
		for (int i = 0; i < Stars.Length; i++) {
			if (i >= availableStars) {
				Stars[i].color = Color.clear;
			}
			else if (stars > i) {
				Stars[i].color = new Color(1,1,0.6f);
			}
		}
		OnActionSelected = callback;
	}

	public virtual void SelectAction(WinTypeAction w) {
		ActionSelected = true;
		OnActionSelected?.Invoke(w);

		switch (w) {
			case WinTypeAction.Menu:
				Background.gameObject.SetActive(false);
				ActionMoveDirection = Vector2.up;
				break;
			case WinTypeAction.Reset:
				ActionMoveDirection = Vector2.left;
				break;
			case WinTypeAction.LevelSelect:
				ActionMoveDirection = Vector2.down;
				break;
			case WinTypeAction.Next:
				ActionMoveDirection = Vector2.right;
				break;
		}
	}

	protected void Update() {
		if(ActionSelected) {
			float moveAmount = 2000 * Time.deltaTime;
			Position += moveAmount * ActionMoveDirection;
		}
	}

	public void ShowLeaderboard() {
		Debug.Log("Leaderboard");
	}

	//public WinTypeAction SetGrabPosition(Vector2 offset) {
	//	if(Centered) {
	//		offset = offset.SnapToAxis() * scale;
	//		if(offset.sqrMagnitude < threshold) {
	//			offset = Vector2.zero;
	//		}
	//	}		
	//	else {
	//		Vector2 snap = Mathf.Abs(Position.x) > Mathf.Abs(Position.y) ? Vector2.right : Vector2.up;
	//		offset = offset * snap * scale;
	//	}

	//	return SetPosition(offset);
	//}

	//public WinTypeAction SetPositionNoGrab(Vector2 grabReleasePoint) {
	//	Vector2 dir = Position.normalized;
	//	float moveAmount = 2000 * Time.deltaTime;

	//	Vector2 target = Vector2.zero;
	//	if((grabReleasePoint * dir * scale).sqrMagnitude > (ActionThreshold * dir).sqrMagnitude) {
	//		target = ActionThreshold * dir * 2f;
	//	}

	//	return SetPosition(target);
	//}

	//private WinTypeAction SetPosition(Vector2 targetPosition) {
	//	if(ActionSelected) {
	//		float moveAmount = 2000 * Time.deltaTime;
	//		Position += moveAmount * Position.normalized;
	//	}
	//	else {
	//		float moveAmount = 2000 * Time.deltaTime;
	//		Vector2 diff = (targetPosition - Position);
	//		if (diff.magnitude > moveAmount) {
	//			Position += diff.normalized * moveAmount;
	//		}
	//		else {
	//			Position = targetPosition;
	//		}

	//		if(!Centered && Position.sqrMagnitude > (ActionThreshold * Position.normalized).sqrMagnitude) {
	//			// start transition
	//			ActionSelected = true;
	//			if(Position.x > 100) {
	//				return WinTypeAction.Next;
	//			}
	//			else if(Position.x < -100) {
	//				return WinTypeAction.Reset;
	//			}
	//			else if(Position.y > 100) {
	//				return WinTypeAction.LevelSelect;
	//			}
	//			else if(Position.y < -100) {
	//				return WinTypeAction.Menu;
	//			}
	//		}	
	//	}
	//	return WinTypeAction.None;
	//}

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
		var threshold = new Vector2(1601, 901); 
		yield return new WaitUntil(() => Mathf.Abs(Position.x) > threshold.x || Mathf.Abs(Position.y) > threshold.y);
		action?.Invoke();
	}
}


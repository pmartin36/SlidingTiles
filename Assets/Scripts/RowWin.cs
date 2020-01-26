using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RowWin : WinType
{
	public override void Run(TimeInfo timeInfo, int stars, int availableStars = 3, System.Action<WinTypeAction> callback = null) {
		Animator a = GetComponent<Animator>();
		a.SetFloat("direction", 1);
		a.Play("rowwin", 0, 0);
		base.Run(timeInfo, stars, availableStars, callback);
	}

	public override void Reset() {
		base.Reset();
	}

	public void SelectActionInspector(int w) {	
		bool actionSuccess = base.SelectAction((WinTypeAction)w);
		if(actionSuccess) {
			Hide();
		}
	}
}

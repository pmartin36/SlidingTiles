using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RowWin : WinType
{
	public override void Run(int stars, int availableStars = 3, System.Action<WinTypeAction> callback = null) {	
		GetComponent<Animator>().Play("rowwin");
		base.Run(stars, availableStars, callback);
	}

	public override void Reset() {
		base.Reset();
	}

	public void SelectActionInspector(int w) {
		base.SelectAction((WinTypeAction)w); /// needed to show up in inspector?
	}
}

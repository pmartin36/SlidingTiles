using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RowWin : WinType
{
	public override void Run(int stars) {	
		GetComponent<Animator>().Play("rowwin");
		base.Run(stars);
	}

	public override void Reset() {
		base.Reset();

	}
}

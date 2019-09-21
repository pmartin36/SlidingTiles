using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RowWin : WinType
{
	public override void Run(int stars, int availableStars = 3) {	
		GetComponent<Animator>().Play("rowwin");
		base.Run(stars, availableStars);
	}

	public override void Reset() {
		base.Reset();
	}
}

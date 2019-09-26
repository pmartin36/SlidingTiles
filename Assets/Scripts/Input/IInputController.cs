using UnityEngine;

public interface IInputController {
	void GetInput(InputPackage p, in InputPackage last, in Camera main);
}


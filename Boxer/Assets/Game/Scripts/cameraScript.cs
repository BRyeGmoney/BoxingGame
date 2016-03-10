using UnityEngine;
using System.Collections;

public class cameraScript : MonoBehaviour {

	public void CameraAnimationFinished()
    {
        OverlordScript.instance.currentGameState = GameState.GameScreen;
    }
}

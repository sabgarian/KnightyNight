using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLevelManager : MonoBehaviour
{
    private int lastLoadedPuzzle = -1;
    public int currentPuzzle = -1;
    public Vector3 newPuzzleCoord;
    public Vector3 oldPuzzleCoord;
    public float puzzleTransitionSpeed = 1f;
    private float puzzleTransitionTime = 0f;
    public GameObject[] puzzles;

    public GameObject curPuzzleGO = null;

    public float[] cameraZoom;

    public Transform playerTrans;
    public Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        playerTrans = GameObject.FindWithTag("Player").transform;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        StartCoroutine(LoadPuzzle());
    }

    public void NextPuzzle()
    {
        if (puzzleTransitionTime < 1f)
            return;
        ++currentPuzzle;
        StartCoroutine(LoadPuzzle());
    }

    public IEnumerator LoadPuzzle()
    {
        Vector3 newPoint = Vector3.zero;
        Vector3 oldPoint = newPoint;
        if (lastLoadedPuzzle < currentPuzzle)
        {
            newPoint = newPuzzleCoord;
            oldPoint = oldPuzzleCoord;
        }
        else
        {
            newPoint = oldPuzzleCoord;
            oldPoint = newPuzzleCoord;
        }
        lastLoadedPuzzle = currentPuzzle;
        puzzleTransitionTime = 0;
        GameObject newPuzzle = GameObject.Instantiate(puzzles[currentPuzzle]);

        PuzzlePlayerController playerController = playerTrans.gameObject.GetComponent<PuzzlePlayerController>();
        playerController.invulnerable = true;
        playerController.Freeze();

        Vector3 oldPlayerPos = playerTrans.transform.position;
        float oldCamSize = mainCamera.orthographicSize;
        while (puzzleTransitionTime < 1f)
        {
            puzzleTransitionTime += Time.deltaTime * puzzleTransitionSpeed;
            float smoothTime = Mathf.SmoothStep(0, 1f, puzzleTransitionTime);
            if (curPuzzleGO != null)
                curPuzzleGO.transform.position = Vector3.Lerp(Vector3.zero, oldPoint, smoothTime);
            newPuzzle.transform.position = Vector3.Lerp(newPoint, Vector3.zero, smoothTime);
            mainCamera.orthographicSize = Mathf.Lerp(oldCamSize, cameraZoom[currentPuzzle], smoothTime);
            playerTrans.transform.position = Vector3.Lerp(oldPlayerPos, newPuzzle.transform.GetChild(0).position, smoothTime);
            yield return new();
        }
        if (curPuzzleGO != null)
            Destroy(curPuzzleGO);

        curPuzzleGO = newPuzzle;

        playerController.invulnerable = false;
        playerController.Unfreeze();
    }
}

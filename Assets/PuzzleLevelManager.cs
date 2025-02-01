using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLevelManager : MonoBehaviour
{
    public int currentPuzzle = -1;
    public Vector3 newPuzzleCoord;
    public Vector3 oldPuzzleCoord;
    public float puzzleTransitionSpeed = 1f;
    private float puzzleTransitionTime = 0f;
    public GameObject[] puzzles;
    public float[] cameraZoom;

    public Transform playerTrans;
    public Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        playerTrans = GameObject.FindWithTag("Player").transform;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        StartCoroutine(NextPuzzle());
    }

    public void FinishPuzzle()
    {
        if (puzzleTransitionTime < 1f)
            return;
        StartCoroutine(NextPuzzle());
    }

    IEnumerator NextPuzzle()
    {
        int oldIndex = currentPuzzle;
        ++currentPuzzle;
        puzzleTransitionTime = 0;
        puzzles[currentPuzzle].SetActive(true);
        playerTrans.gameObject.GetComponent<PuzzlePlayerController>().Freeze();
        Vector3 oldPlayerPos = playerTrans.transform.position;
        float oldCamSize = mainCamera.orthographicSize;
        while (puzzleTransitionTime < 1f)
        {
            puzzleTransitionTime += Time.deltaTime * puzzleTransitionSpeed;
            float smoothTime = Mathf.SmoothStep(0, 1f, puzzleTransitionTime);
            if (oldIndex >= 0)
                puzzles[oldIndex].transform.position = Vector3.Lerp(Vector3.zero, oldPuzzleCoord, smoothTime);
            puzzles[currentPuzzle].transform.position = Vector3.Lerp(newPuzzleCoord, Vector3.zero, smoothTime);
            mainCamera.orthographicSize = Mathf.Lerp(oldCamSize, cameraZoom[currentPuzzle], smoothTime);
            playerTrans.transform.position = Vector3.Lerp(oldPlayerPos, puzzles[currentPuzzle].transform.GetChild(0).position, smoothTime);
            yield return new();
        }
        if (oldIndex >= 0)
            puzzles[oldIndex].SetActive(false);
        playerTrans.gameObject.GetComponent<PuzzlePlayerController>().Unfreeze();
    }
}

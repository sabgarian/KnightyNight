using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleLevelManager : MonoBehaviour
{
    private int lastLoadedPuzzle = -1;
    public int currentPuzzle = -1;
    public Vector3 newPuzzleCoord;
    public Vector3 oldPuzzleCoord;
    public float puzzleTransitionSpeed = 1f;
    private float puzzleTransitionTime = 0f;
    public GameObject[] puzzles;
    public Vector2 minMaxX;

    private GameObject curPuzzleGO = null;

    public float[] cameraZoom;

    public GameObject fightPlayerPref;
    private Transform playerTrans;
    private Camera mainCamera;

    private MusicManager musicBox;

    public int gameMode = 0; // 0 = Puzzle mode, 1 = fight mode
    private int songIntensity = 0;

    public Animator finalTransitionAnim;
    public float finalTransitionHalfway;
    public float finalTransitionEndway;
    public Vector3 fightCamPos;
    public Quaternion fightCamRot;

    // Start is called before the first frame update
    void Start()
    {
        playerTrans = GameObject.FindWithTag("Player").transform;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        musicBox = gameObject.GetComponent<MusicManager>();
        if (gameMode == 0)
            StartCoroutine(LoadPuzzle());
        else
            StartCoroutine(LoadFight());
    }

    public void SetGameMode(int newMode)
    {
        gameMode = newMode;
        switch (gameMode)
        {
            // Puzzle mode
            case 0:
                if (songIntensity == 0)
                {
                    musicBox.TransitionTo(0, 3, 0.5f);
                    musicBox.TransitionTo(1, -1, 0.5f);
                }
                else
                {
                    musicBox.TransitionTo(0, 3, 0.5f);
                    musicBox.TransitionTo(1, 2, 0.5f);
                }
                break;
            // Fight mode
            case 1:
                //Debug.Log("Play music");
                if (musicBox.currentSong[0] != 1)
                {
                    //Debug.Log("playing shit");
                    musicBox.TransitionTo(0, 0, 0, 1);
                    musicBox.TransitionTo(1, -1, 0);
                }
                break;
            default:
                break;
        }
    }

    public void NextPuzzle()
    {
        if (puzzleTransitionTime < 1f)
            return;
        ++currentPuzzle;

        if (currentPuzzle < puzzles.Length)
        {
            if (gameMode == 0)
                StartCoroutine(LoadPuzzle());
            else
                StartCoroutine(LoadFight());
        }
        else
        {
            // End game:
            StartCoroutine(EndGame());
        }
    }

    IEnumerator EndGame()
    {
        Debug.Log("Loading fight");
        FightPlayerController playerController = playerTrans.gameObject.GetComponent<FightPlayerController>();
        playerController.StartCutScene();
        playerController.normalizedInputs = new Vector2(-1f, 0f);
        musicBox.TransitionTo(0, -1, finalTransitionEndway);
        musicBox.TransitionTo(1, -1, 0);
        finalTransitionAnim.SetTrigger("Play");
        yield return new WaitForSeconds(finalTransitionEndway);
        SceneManager.LoadScene("GameOver");
    }

    public void SwitchToFight()
    {
        StartCoroutine(FightTransition());
    }

    IEnumerator FightTransition()
    {
        musicBox.TransitionTo(0, -1, 1);
        musicBox.TransitionTo(1, -1, 0);
        finalTransitionAnim.SetTrigger("Play");
        yield return new WaitForSeconds(finalTransitionHalfway);
        transform.position = fightCamPos;
        transform.rotation = fightCamRot;
        Destroy(curPuzzleGO);
        Destroy(playerTrans.gameObject);
        playerTrans = Instantiate(fightPlayerPref).transform;
        SetGameMode(1);
        puzzleTransitionTime = 1f;
        curPuzzleGO = null;
        yield return new();
        NextPuzzle();
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
        playerController.NoclipStart();

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
        SetGameMode(0);

        curPuzzleGO = newPuzzle;

        playerController.invulnerable = false;
        playerController.NoclipEnd();
        playerController.Unfreeze();
    }

    public IEnumerator LoadFight()
    {
        Debug.Log("Loading fight");
        FightPlayerController playerController = playerTrans.gameObject.GetComponent<FightPlayerController>();
        playerController.StartCutScene();
        playerController.normalizedInputs = new Vector2(-1f, 0f);
        if (curPuzzleGO != null)
        {
            while (playerController.transform.position.x >= minMaxX.x)
                yield return new();

            Destroy(curPuzzleGO);
        }

        GameObject newPuzzle = GameObject.Instantiate(puzzles[currentPuzzle]);
        Debug.Log("Instantiated");
        //Debug.Log("fuck me right in the pussy");
        mainCamera.orthographicSize = cameraZoom[currentPuzzle];
        SetGameMode(1);
        curPuzzleGO = newPuzzle;

        playerController.transform.position = curPuzzleGO.transform.GetChild(0).position;
        //GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        //Vector2 enemyInputs = new Vector2(1, 0);
        //for (int i = 0; i < allEnemies.Length; ++i)
        //{
        //    allEnemies[i].SendMessage("StartCutScene");
        //    allEnemies[i].SendMessage("SetInputs", enemyInputs);
        //}

        while (playerController.transform.position.x >= minMaxX.y)
            yield return new();
        puzzleTransitionTime = 1f;

        //for (int i = 0; i < allEnemies.Length; ++i)
        //    allEnemies[i].SendMessage("EndCutScene");

        playerController.EndCutScene();
    }
}

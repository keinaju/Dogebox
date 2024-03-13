using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    //Width and height as number of tiles:
    const int width = 100;
    const int height = 100;

    static Tile DogeTile;
    static List<Tile> Eggs;
    public static int CurrentLevel = 0;
    public static Tile[,] Tiles = new Tile[width, height];
    static TMP_Text LevelInfo;
    static TMP_Text TutorialInfo;
    static GameObject CompleteText;
    public Button ResetButton, PreviousButton, NextButton, UndoButton, QuitButton;
    const int DefaultZoom = 7;

    void Awake()
    {
        LevelInfo = GameObject.FindWithTag("LevelInfoTag").GetComponent<TMP_Text>();
        TutorialInfo = GameObject.FindWithTag("TutorialInfoTag").GetComponent<TMP_Text>();
        CompleteText = GameObject.FindWithTag("LevelCompleteTag");
        CompleteText.SetActive(false);

        ResetButton.onClick.AddListener(ResetTask);
        PreviousButton.onClick.AddListener(PreviousTask);
        NextButton.onClick.AddListener(NextTask);
        UndoButton.onClick.AddListener(UndoTask);
        QuitButton.onClick.AddListener(QuitTask);

        Initialize(); //Map initialization is needed only once per runtime.
        LoadLevel(CurrentLevel); //Load first level.
    }
    void Update()
    {
        if (CompleteText.activeSelf == true && Input.anyKeyDown == true)
        {
            CompleteText.SetActive(false);
            LoadLevel(CurrentLevel + 1);
        }
        else
        {
            //Read user inputs.
            if (Input.GetKeyDown(KeyCode.UpArrow)) MovePlayer(Tile.Direction.Up);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) MovePlayer(Tile.Direction.Down);
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) MovePlayer(Tile.Direction.Left);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) MovePlayer(Tile.Direction.Right);
            else if (Input.GetKeyDown(KeyCode.R)) ResetTask();
            else if (Input.GetKeyDown(KeyCode.PageDown)) PreviousTask();
            else if (Input.GetKeyDown(KeyCode.PageUp)) NextTask();
            else if (Input.GetKeyDown(KeyCode.Backspace)) UndoTask();
            else if (Input.GetKeyDown(KeyCode.Escape)) QuitTask();
        }
    }
    void ResetTask()
    {
        LoadLevel(CurrentLevel);
    }
    void PreviousTask()
    {
        LoadLevel(CurrentLevel - 1);
    }
    void NextTask()
    {
        LoadLevel(CurrentLevel + 1);
    }
    void UndoTask()
    {
        Undo();
    }
    public void QuitTask()
    {
        Application.Quit();
    }
    void Initialize()
    {
        //Initialize empty tiles. Resolution defined in constants width and height.
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                Tiles[column, row] = new Tile(column, row, Tile.BackType.Black, Tile.ForeType.None);
            }
        }
    }
    //Destroy all game objects in tilemap.
    static void ResetGameObjects()
    {
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                if (Tiles[column, row].backObject != null) Destroy(Tiles[column, row].backObject);
                if (Tiles[column, row].foreObject != null) Destroy(Tiles[column, row].foreObject);
            }
        }
    }
    public static void LoadLevel(int levelNumber)
    {
        ResetGameObjects();

        //Forget eggs.
        Eggs = new();

        //Forget move history.
        MoveList = new();

        //Remember current level.
        if (levelNumber < 0) levelNumber = Level.Levels.Count - 1;
        else if (levelNumber >= Level.Levels.Count) levelNumber = 0;
        CurrentLevel = levelNumber;

        LevelInfo.text = $"Puzzle {CurrentLevel + 1}/{Level.Levels.Count}: \"{Level.Levels[levelNumber].Name}\"";

        if (levelNumber == 0) TutorialInfo.gameObject.SetActive(true);
        else TutorialInfo.gameObject.SetActive(false);

        int x = 0;
        int y = 0;
        foreach (char c in Level.Levels[levelNumber].Content)
        {
            if (c == '\n') //Line break
            {
                y++;
                x = 0;
            }
            else if (c == '0') //Black/blocked tile
            {
                Tiles[x, y] = new Tile(x, y, Tile.BackType.Black, Tile.ForeType.Blocked);
                x++;
            }
            else if (c == ' ') //"Free to move" tile
            {
                Tiles[x, y] = new Tile(x, y, Tile.BackType.Floor, Tile.ForeType.None);
                x++;
            }
            else if (c == 'e') //Egg tile
            {
                Tiles[x, y] = new Tile(x, y, Tile.BackType.Egg, Tile.ForeType.None);
                Eggs.Add(Tiles[x, y]);
                x++;
            }
            else if (c == '2') //Egg tile with chicken
            {
                Tiles[x, y] = new Tile(x, y, Tile.BackType.Egg, Tile.ForeType.Chicken);
                Eggs.Add(Tiles[x, y]);
                x++;
            }
            else if (c == 'p') //Player spawn tile
            {
                Tiles[x, y] = new Tile(x, y, Tile.BackType.Floor, Tile.ForeType.Player);
                DogeTile = Tiles[x, y];
                x++;
            }
            else if (c == 'c') //Chicken tile
            {
                Tiles[x, y] = new Tile(x, y, Tile.BackType.Floor, Tile.ForeType.Chicken);
                x++;
            }
        }

        //Adjust camera according to offsets specified in level settings.
        GameObject.FindWithTag("MainCamera").transform.position +=
            (UnityEngine.Vector3.right * Level.Levels[levelNumber].xOff) + (UnityEngine.Vector3.down * Level.Levels[levelNumber].yOff);

        GameObject.FindWithTag("MainCamera").GetComponent<Camera>().orthographicSize = DefaultZoom + Level.Levels[levelNumber].Zoom;
    }
    static void WinCheck()
    {
        int succeeds = 0;
        foreach (Tile tile in Eggs)
        {
            if (tile.Fore == Tile.ForeType.Chicken) succeeds++;
        }
        if (succeeds == Eggs.Count)
        {
            CompleteText.SetActive(true);
        }
    }
    void MovePlayer(Tile.Direction direction)
    {
        //Move if there's room.
        if (DogeTile.GetNear(direction).Fore == Tile.ForeType.None)
        {
            DogeTile.Move(direction);
            DogeTile = DogeTile.GetNear(direction); //Update player's tile.
            MoveList.Push(new Move(direction, false)); //Add move to undo history.
        }

        //Move, and push if there's room behind chicken.
        else if (DogeTile.GetNear(direction).Fore == Tile.ForeType.Chicken
        && DogeTile.GetFar(direction).Fore == Tile.ForeType.None)
        {
            DogeTile.GetNear(direction).Move(direction);
            DogeTile.Move(direction);
            DogeTile = DogeTile.GetNear(direction); //Update player's tile.
            MoveList.Push(new Move(direction, true)); //Add move to undo history.
        }

        //Check if player has won.
        WinCheck();
    }


    static Stack<Move> MoveList = new();
    void Undo()
    {
        if (MoveList.Count > 0)
        {
            Move latest = MoveList.Pop();

            //Reverse direction.
            Tile.Direction reverseDirection;
            if (latest.Direction == Tile.Direction.Up) reverseDirection = Tile.Direction.Down;
            else if (latest.Direction == Tile.Direction.Down) reverseDirection = Tile.Direction.Up;
            else if (latest.Direction == Tile.Direction.Left) reverseDirection = Tile.Direction.Right;
            else reverseDirection = Tile.Direction.Left;

            DogeTile.Move(reverseDirection);
            DogeTile = DogeTile.GetNear(reverseDirection); //Update player's tile.
            if (reverseDirection == Tile.Direction.Left) DogeTile.foreObject.GetComponent<SpriteRenderer>().flipX = true;
            else if (reverseDirection == Tile.Direction.Right) DogeTile.foreObject.GetComponent<SpriteRenderer>().flipX = false;

            //If chicken was next to player, pull it back.
            if (latest.Push == true && DogeTile.GetFar(latest.Direction).Fore == Tile.ForeType.Chicken)
            {
                DogeTile.GetFar(latest.Direction).Move(reverseDirection);
            }
        }

    }

}

class Move
{
    public Tile.Direction Direction;
    public bool Push;
    public Move(Tile.Direction direction, bool push)
    {
        Direction = direction;
        Push = push;
    }
}
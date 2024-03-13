using UnityEngine;

public class Tile
{
    public enum BackType
    {
        Black, Floor, Egg
    }
    public enum ForeType
    {
        None, Player, Chicken, Blocked
    }
    public int X, Y;
    public BackType Back;
    public ForeType Fore;
    public GameObject backObject;
    public GameObject foreObject;
    public Tile(int xArg, int yArg, BackType backArg, ForeType foreArg)
    {
        X = xArg;
        Y = yArg;
        Back = backArg;
        Fore = foreArg;

        if (backArg == BackType.Black)
        {
            foreArg = ForeType.Blocked;
        }
        else
        {
            if (backArg == BackType.Floor)
            {
                //Every other tile will be generated from bright version, and every other from dark version.
                if((xArg + yArg) % 2 == 0) backObject = GameObject.Instantiate(GameObject.Find("FloorModelBright"));
                else backObject = GameObject.Instantiate(GameObject.Find("FloorModelDark"));
            }
            else if (backArg == BackType.Egg)
            {
                //Every other tile will be generated from bright version, and every other from dark version.
                if ((xArg + yArg) % 2 == 0) backObject = GameObject.Instantiate(GameObject.Find("EggModelBright"));
                else backObject = GameObject.Instantiate(GameObject.Find("EggModelDark"));
            }

            backObject.transform.position = Vector3.zero + (Vector3.right * xArg) + (Vector3.down * yArg);
        }

        if (foreArg == ForeType.Player)
        {
            foreObject = GameObject.Instantiate(GameObject.Find("Player"));
            //Set initial position for character.
            foreObject.transform.position = Vector3.zero + (Vector3.right * xArg) + (Vector3.down * yArg);
            //Set target vector for character.
            foreObject.GetComponent<Magnet>().Target = foreObject.transform.position;
            //Position camera so player is in the center.
            GameObject.FindWithTag("MainCamera").transform.position = Vector3.zero + (Vector3.right * xArg) + (Vector3.down * yArg) + Vector3.back;
        }
        else if (foreArg == ForeType.Chicken)
        {
            foreObject = GameObject.Instantiate(GameObject.Find("ChickenModel"));
            //Set initial position for character.
            foreObject.transform.position = Vector3.zero + (Vector3.right * xArg) + (Vector3.down * yArg);
            //Set target vector for character.
            foreObject.GetComponent<Magnet>().Target = foreObject.transform.position;
        }
    }
    public enum Direction
    {
        Up, Down, Left, Right
    }
    public Tile GetNear(Direction direction)
    {
        if (direction == Direction.Up) return Map.Tiles[this.X, this.Y - 1];
        else if (direction == Direction.Down) return Map.Tiles[this.X, this.Y + 1];
        else if (direction == Direction.Left) return Map.Tiles[this.X - 1, this.Y];
        else if (direction == Direction.Right) return Map.Tiles[this.X + 1, this.Y];
        else return null;
    }
    public Tile GetFar(Direction direction)
    {
        if (direction == Direction.Up) return Map.Tiles[this.X, this.Y - 2];
        else if (direction == Direction.Down) return Map.Tiles[this.X, this.Y + 2];
        else if (direction == Direction.Left) return Map.Tiles[this.X - 2, this.Y];
        else if (direction == Direction.Right) return Map.Tiles[this.X + 2, this.Y];
        else return null;
    }
    //Moves foreground object, distance of one tile, to specified direction.
    public void Move(Direction direction)
    {
        if (direction == Direction.Up)
        {
            Map.Tiles[this.X, this.Y -1].Fore = this.Fore;
            if (this.foreObject != null)
            {
                foreObject.GetComponent<Magnet>().Target += Vector3.up;
                Map.Tiles[this.X, this.Y -1].foreObject = this.foreObject;
            }
        }
        if (direction == Direction.Down)
        {
            Map.Tiles[this.X, this.Y +1].Fore = this.Fore;
            if (this.foreObject != null)
            {
                foreObject.GetComponent<Magnet>().Target += Vector3.down;
                Map.Tiles[this.X, this.Y +1].foreObject = this.foreObject;
            }
        }
        if (direction == Direction.Left)
        {
            Map.Tiles[this.X -1, this.Y].Fore = this.Fore;
            if (this.foreObject != null)
            {
                foreObject.GetComponent<Magnet>().Target += Vector3.left;
                Map.Tiles[this.X -1, this.Y].foreObject = this.foreObject;
                this.foreObject.GetComponent<SpriteRenderer>().flipX = false;
            }
        }
        if (direction == Direction.Right)
        {
            Map.Tiles[this.X +1, this.Y].Fore = this.Fore;
            if (this.foreObject != null)
            {
                foreObject.GetComponent<Magnet>().Target += Vector3.right;
                Map.Tiles[this.X +1, this.Y].foreObject = this.foreObject;
                this.foreObject.GetComponent<SpriteRenderer>().flipX = true;
            }
        }

        this.Fore = ForeType.None;
        this.foreObject = null;
    }
}

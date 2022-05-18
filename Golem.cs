using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golem : MonoBehaviour
{
    public enum Direction
    {
        north=0, south=1, west=2, east=3
    }

    public int posx;
    public int posy;
    public Direction direction;
    public Animator animator;

    GolemLanguage.FunctionRunner3<string> scriptRunner;

    // Start is called before the first frame update
    void Start()
    {
        scriptRunner = new GolemLanguage.FunctionRunner3<string>(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void MoveFoward()
    {
        switch (direction)
        {
            case Direction.north:
                posy += 1;
                break;
            case Direction.south:
                posy -= 1;
                break;
            case Direction.west:
                posx -= 1;
                break;
            case Direction.east:
                posx += 1;
                break;
        }

        this.transform.position = new Vector2(posx, posy);
    }

    public void TurnRight()
    {
        switch (direction)
        {
            case Direction.north:
                direction = Direction.east;
                break;
            case Direction.south:
                direction = Direction.west;
                break;
            case Direction.west:
                direction = Direction.north;
                break;
            case Direction.east:
                direction = Direction.south;
                break;
        }
        animator.SetInteger("dir", (int)direction);
    }

    public void TurnRight(int amount)
    {
        for (int i = 0; i < amount; i++)
            TurnRight();
    }

    public void TurnLeft()
    {
        switch (direction)
        {
            case Direction.north:
                direction = Direction.west;
                break;
            case Direction.south:
                direction = Direction.east;
                break;
            case Direction.west:
                direction = Direction.south;
                break;
            case Direction.east:
                direction = Direction.north;
                break;
        }
    }

    public void TurnLeft(int amount)
    {
        for (int i = 0; i < amount; i++)
            TurnLeft();
    }

    public void Turn(int amount)
    {
        if (amount <= 0)
        {
            TurnLeft(amount);
        }
        else
        {
            TurnRight(amount);
        }
    }
}

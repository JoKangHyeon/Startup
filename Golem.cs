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
    public float speed= 0.3f;

    GolemLanguage.FunctionRunner scriptRunner;

    public Queue<KeyValuePair< string, object>> inst;

    public GameObject PrintCanvas;
    public UnityEngine.UI.Text text;

    // Start is called before the first frame update
    void Start()
    {
        scriptRunner = new GolemLanguage.FunctionRunner(this);
        inst = new Queue<KeyValuePair<string, object>>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = new Vector2(posx, posy);
        if (!transform.position.Equals(pos)){
            Vector2 dir = pos - (Vector2)transform.position;

            if (dir.magnitude < speed * Time.deltaTime)
            {
                transform.position = pos;
                return;
            }


            dir=dir.normalized * speed*Time.deltaTime;

            this.transform.position += (Vector3)dir;
        }
        
    }


    public void Tick()
    {
        if (inst.Count == 0)
            return;
        KeyValuePair<string, object> currentInst = inst.Dequeue();

        switch (currentInst.Key)
        {
            case "TurnRight":
                runTurnRight((int)currentInst.Value);
                break;

            case "MoveFoward":
                runMoveFoward();
                break;
            case "Print":
                PrintCanvas.SetActive(true);
                text.text = currentInst.Value.ToString();
                Invoke("HideCanvas", 2);
                break;
        }
    }

    public void HideCanvas()
    {
        PrintCanvas.SetActive(false);
    }

    public void MoveFoward()
    {
        inst.Enqueue(new KeyValuePair<string, object>("MoveFoward",1));
    }

    public void Print(string value)
    {
        inst.Enqueue(new KeyValuePair<string, object>("Print", value));
    }

    private void runMoveFoward()
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

       // this.transform.position = new Vector2(posx, posy);
    }

    private void runTurnRight(int amount)
    {
        for (int i = 0; i < amount; i++)
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
        }
        animator.SetInteger("dir", (int)direction);
    }

    public void TurnRight()
    {
        inst.Enqueue(new KeyValuePair<string, object>("TurnRight", 1));
    }

    public void TurnRight(int amount)
    {
        inst.Enqueue(new KeyValuePair<string, object>("TurnRight", amount));
    }

}

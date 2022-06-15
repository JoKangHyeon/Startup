using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class Tester : MonoBehaviour
{
    static string data = "�׽�Ʈ1=3+(4*2)\n�׽�Ʈ2=�׽�Ʈ1+2\n�׽�Ʈ3=�׽�Ʈ4(1,2)";
    GolemLanguage.FunctionRunner runner;

    public Text debug;
    public TMPro.TMP_InputField debugInput;

    public Golem testGolem;
    public float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        //runner.RunFunction(data);
        runner = new GolemLanguage.FunctionRunner(testGolem);
    }

    public void RunFunc()
    {
        runner.RunLines(debugInput.text);
    }

    // Update is called once per frame
    void Update()
    {
        StringBuilder sb = new StringBuilder();
        foreach(string key in runner.runner.valueDic.Keys)
        {
            sb.Append(key + " : " + runner.runner.valueDic[key].ToString() + "\n");
        }

        debug.text = sb.ToString();

        timer += Time.deltaTime;

        if (timer > 2)
        {
            testGolem.Tick();
            timer = 0;
        }

    }
}

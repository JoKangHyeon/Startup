using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class Tester : MonoBehaviour
{
    static string data = "테스트1=3+(4*2)\n테스트2=테스트1+2\n테스트3=테스트4(1,2)";
    GolemLanguage.FunctionRunner3<int> runner = new GolemLanguage.FunctionRunner3<int>(new GolemLanguage());

    public Text debug;
    public InputField debugInput;

    // Start is called before the first frame update
    void Start()
    {
        //runner.RunFunction(data);
    }

    public void RunFunc()
    {
        runner.RunFunction(debugInput.text);
    }

    // Update is called once per frame
    void Update()
    {
        StringBuilder sb = new StringBuilder();
        foreach(string key in runner.runner.intDic.Keys)
        {
            sb.Append(key + " : " + runner.runner.intDic[key] + "\n");
        }

        debug.text = sb.ToString();
    }
}

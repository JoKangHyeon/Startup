using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class GolemLanguage
{

    public Stack<Object> callStack;
    private static readonly char[] operators = { '+', '-', '/','*' };

    public Dictionary<string, int> intDic = new Dictionary<string, int>();

    private Dictionary<string, InstructionCapsule> inst;

    public class FunctionRunner<T>
    {
        List<string> lines;

        T ret = default(T);

        public FunctionRunner(string func)
        {
            lines = new List<string>();

            lines.AddRange(func.Split('\n'));
        }

        public T RunFunction()
        {
            ret = default(T);

            for(int i = 0; i < lines.Count; i++)
            {
                LineRunner(i);
            }

            return ret;
        }

        public void LineRunner(int line)
        {
            Stack<string> st = new Stack<string>();
            StringBuilder sb = new StringBuilder();
            char[] chars = lines[line].ToCharArray();

            for(int i=0; i < chars.Length; i++)
            {
                switch (chars[i])
                {
                    case '(':
                        st.Push(sb.ToString());
                        Debug.Log(sb.ToString());
                        sb.Append(MiddleRunner(i, chars, st));
                        if (i == -1)
                        {
                            Debug.Log("ERROR");
                            return;
                        }
                        break;
                    case '=':
                        st.Push(sb.ToString());
                        Debug.Log(sb.ToString());
                        sb.Append(MiddleRunner(i, chars, st));
                        if (i == -1)
                        {
                            Debug.Log("ERROR");
                            return;
                        }
                        break;
                    case ')':
                        break;
                    default:
                        sb.Append(chars[i]);
                        break;
                }
            }

            Debug.Log(string.Join(",",st.ToArray()));
        }


        public string MiddleRunner(int position, char[] chars, Stack<string> st)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(chars[position]);
            for (int i = position+1; i < chars.Length; i++)
            {
                switch (chars[i])
                {
                    case '(':
                        //st.Push(sb.ToString());
                        sb.Append(MiddleRunner(i, chars, st));
                        Debug.Log(sb);
                        break;
                    case '=':
                        return "-1";
                    case ')':
                        sb.Append(chars[i]);

                        bool calcString = false;

                        if (sb[0] == '(')
                        {
                            sb.Remove(0, 1);
                        }

                        if (sb[sb.Length-1] == ')')
                        {
                            sb.Remove(sb.Length - 1, 1);
                        }
                        string str = sb.ToString();

                        List<string> splited = new List<string>();

                        if (str.Contains("\""))
                        {
                            calcString = true;
                        }

                        splited.AddRange(str.Split(operators));

                        for (int j=0; j < splited.Count; j++)
                        {
                            if (splited[j].All(char.IsDigit))
                            {
                                splited.RemoveAt(j);
                                j--;
                                continue;
                            }
                        }

                        Debug.Log("변수 : " + string.Join(",", splited));



                        //st.Push(sb.ToString());
                        Debug.Log(sb.ToString());
                        return sb.ToString();
                    default:
                        sb.Append(chars[i]);
                        break;
                }
            }

            return "";
        }
    }

    public class FunctionRunner2<T>
    {

        GolemLanguage caller;

        public FunctionRunner2(GolemLanguage caller)
        {
            this.caller = caller;
        }

        Object getValue(string code)
        {
            int state = 0;
            StringBuilder current = new StringBuilder();
            StringBuilder calc = new StringBuilder();
            string calctype = "null";
            int deep = 0;

            int cursor = 0;

            bool escape = false;

            while (true)
            {

                if (cursor < code.Length)
                {
                    switch (state)
                    {
                        case 0:
                            if (code[cursor] == '\"')
                            {
                                state = 3;
                            }
                            else if (char.IsDigit(code[cursor]))
                            {
                                state = 2;
                            }
                            else if (operators.Contains<char>(code[cursor]))
                            {
                                state = -1;
                            }
                            else
                            {
                                state = 1;
                            }

                            break;

                        case 1:
                            current.Append(code[cursor++]);
                            if (code[cursor] == '(')
                            {
                                if (caller.inst.ContainsKey(current.ToString()))
                                {
                                    //FUNCRUN
                                    break;
                                }
                                else
                                {
                                    state = -1;
                                    break;
                                }
                            }
                            else if (operators.Contains<char>(code[cursor])|| code[cursor] == ')')
                            {
                                state = 5;

                            }
                            else
                            {
                                current.Append(code[cursor++]);
                                state = 1;
                            }
                            break;

                        case 2:
                            //numbers

                            if(char.IsDigit(code[cursor]) || code[cursor] == '.')
                            {
                                current.Append(code[cursor++]);
                                state = 2;
                                break;
                            }else if (operators.Contains<char>(code[cursor]))
                            {
                                calc.Append(current.ToString());
                                calc.Append(code[cursor++]);
                                state = 3;
                                break;
                            }else if(code[cursor] == ')')
                            {
                                state = 10;
                                break;
                            }
                            else
                            {
                                state = -1;
                                break;
                            }

                        case 5:
                            if (caller.intDic.ContainsKey(current.ToString()) && (new List<string> { "null", "int" }).Contains(calctype))
                            {
                                calctype = "int";
                                calc.Append(caller.intDic[current.ToString()]);
                                calc.Append(code[cursor]);
                            }//다른 딕셔너리도 순회

                            else //모든 딕셔너리에 없으면
                            {
                                state = -1;
                                break;
                            }

                            if (code[cursor] == ')')
                            {
                                state = 10;
                            }
                            else
                            {
                                
                                state = 0;
                            }
                            break;
                    }

                    if (escape)
                    {
                        break;
                    }
                }
                else
                {
                    //EOL
                }
            }

            return null;
        }
    }

    public class FunctionRunner3<T>
    {
        T returnValue = default(T);
        public GolemLanguage runner;
        Golem gol;


        public enum Mode { start, unknown, insertValue, funcArgument };


        public FunctionRunner3(Golem g)
        {
            this.gol = g;
            this.runner = new GolemLanguage();
        }

        public T RunFunction(string lines)
        {
            returnValue = default(T);

            foreach (string line in lines.Split('\n'))
            {
                RunLine(line);
            }

            return returnValue;
        }

        public string RunFunc(string header, string value)
        {
            Debug.Log(value);
            StringBuilder sb = new StringBuilder();
            StringBuilder sbSub = new StringBuilder();
            List<string> args = new List<string>();
            int opened = 0;

            Debug.Log(value);

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == ')')
                {
                    if (opened == 0)
                    {
                        Debug.LogError("FUNC ERRROR");
                        return "-1";
                    }
                    else
                    {
                        opened--;
                    }
                }
                else if (value[i] == '(')
                {
                    opened++;
                }
                else if (value[i] == ',')
                {
                    if (opened == 0)
                    {
                        args.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }
                sb.Append(value[i]);

            }


            if (sb.Length != 0)
            {
                args.Add(sb.ToString());
            }


            for (int i = 0; i < args.Count; i++)
            {
                Debug.Log(header + "(" + value + ") : " + i + " : " + args[i]);

                args[i] = GetValue(args[i]);
                Debug.Log(header + "(" + value + ") : " + i + " replaced : " + args[i]);
            }

            switch (header)
            {
                case "합":
                    //REQ : args n (INT)

                    int sum = 0;
                    for (int i = 0; i < args.Count; i++)
                    {
                        sum += int.Parse(args[i]);
                    }

                    return sum.ToString();

                case "돌아":
                    //REQ : args 1 (INT)
                    if (args.Count != 1)
                    {
                        return null;
                    }
                    else
                    {
                        gol.TurnRight(int.Parse(args[0]));
                    }

                    return null;
                case "앞으로":
                    gol.MoveFoward();
                    return null;
                case "테스트4":
                    return "1";
            }

            return null;
        }

        private string GetValue(string line)
        {
            StringBuilder sbLine = new StringBuilder(line);
            StringBuilder sbContent = new StringBuilder();
            StringBuilder sbCalc = new StringBuilder();

            int submode = 0;

            for (int i = 0; i < sbLine.Length; i++)
            {
                if (char.IsDigit(sbLine[i]) && submode == 0)
                {
                    submode = 1;
                    sbContent.Append(sbLine[i]);
                }
                else if (sbLine[i] == '(' && submode == 0)
                {
                    sbCalc.Append(sbLine[i]);
                }
                else if (submode == 0)
                {
                    submode = 2;
                    sbContent.Append(sbLine[i]);
                }
                else if (GolemLanguage.operators.Contains<char>(sbLine[i]) || sbLine[i] == ')')
                {

                    if (submode == 1)
                    {
                        sbCalc.Append(sbContent.ToString());
                        sbCalc.Append(sbLine[i]);
                        sbContent.Clear();
                    }
                    else if (submode == 2)
                    {
                        string get = null;

                        if (runner.intDic.ContainsKey(sbContent.ToString()))
                        {
                            get = runner.intDic[sbContent.ToString()].ToString();
                        }


                        if (get == null)
                        {
                            throw new System.Exception("unable to find val" + sbContent.ToString());
                        }
                        else
                        {
                            sbCalc.Append(get);
                            sbCalc.Append(sbLine[i]);
                            sbContent.Clear();
                        }
                    }

                    //sbCalc.Append(sbLine[i]);
                    submode = 0;
                }
                else if (submode == 1)
                {
                    if (char.IsDigit(sbLine[i]) || (sbLine[i] == '.' && sbContent.ToString().Contains('.')!))
                    {
                        sbContent.Append(sbLine[i]);
                    }
                    else
                    {
                        throw new System.Exception("unable to calc in digit" + sbLine[i]);
                    }
                }
                else if (submode == 2)
                {
                    if (sbLine[i] == '(')
                    {
                        //Debug.Log(sbContent.ToString());
                        int opened = 0;
                        int j;
                        for (j = i; j < sbLine.Length; j++)
                        {
                            if (sbLine[j] == '(')
                            {
                                opened++;
                            }
                            else if (sbLine[j] == ')')
                            {
                                if (opened == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    opened--;
                                }
                            }
                        }


                        Debug.Log(sbLine.ToString(i, j - i));

                        sbCalc.Append(RunFunc(sbContent.ToString(), sbLine.ToString(i + 1, j - i - 2)));
                        sbContent.Clear();

                        i = j;
                        submode = 0;
                    }
                    else
                    {
                        sbContent.Append(sbLine[i]);
                    }

                }
                else
                {
                    throw new System.Exception("unknown string : " + sbLine[i]);
                }
            }

            //END OF FOR

            if (submode == 1 && sbContent.Length > 0)
            {
                sbCalc.Append(sbContent.ToString());
            }
            else if (submode == 2 && sbContent.Length > 0)
            {
                string get = null;

                if (runner.intDic.ContainsKey(sbContent.ToString()))
                {
                    get = runner.intDic[sbContent.ToString()].ToString();
                }


                if (get == null)
                {
                    throw new System.Exception("unable to find val : " + sbContent.ToString());
                }
                else
                {
                    sbCalc.Append(get);
                }
            }

            System.Data.DataTable dt = new System.Data.DataTable();

            //507호
            Debug.Log("calc:" + sbCalc.ToString());
            return ((int)dt.Compute(sbCalc.ToString(), "")).ToString();
        }

        private void RunLine(string line)
        {

            List<string> splited = new List<string>();
            StringBuilder sbLine = new StringBuilder(line);
            StringBuilder sbContent = new StringBuilder();
            StringBuilder sbCalc = new StringBuilder();

            Mode currentMode = Mode.start;

            int submode = 0;

            for (int i = 0; i < sbLine.Length; i++)
            {
                if (currentMode == Mode.start)
                {
                    if (char.IsDigit(sbLine[i]))
                    {
                        throw new System.Exception("digit at line start");
                    }
                    else if (GolemLanguage.operators.Contains<char>(sbLine[i]))
                    {
                        throw new System.Exception("op at line start");
                    }
                    else if (sbLine[i] == '"' || sbLine[i] == '(' || sbLine[i] == ')')
                    {
                        throw new System.Exception(sbLine[i] + " at line start");
                    }
                    else
                    {
                        sbContent.Clear();
                        sbContent.Append(sbLine[i]);
                        currentMode = Mode.unknown;
                    }
                }
                else if (currentMode == Mode.unknown)
                {
                    if (GolemLanguage.operators.Contains<char>(sbLine[i]))
                    {
                        throw new System.Exception("op while unknown");
                    }
                    else if (sbLine[i] == '"')
                    {
                        throw new System.Exception("\" while unknown");
                    }
                    else if (sbLine[i] == ')')
                    {
                        throw new System.Exception(") while unknown");
                    }
                    else if (sbLine[i] == ' ')
                    {
                        throw new System.Exception("blank while unknown");
                    }
                    else if (sbLine[i] == '=')
                    {
                        splited.Add(sbContent.ToString());
                        sbContent.Clear();
                        sbCalc.Clear();
                        currentMode = Mode.insertValue;
                    }
                    else if (sbLine[i] == '(')
                    {
                        splited.Add(sbContent.ToString());
                        sbContent.Clear();
                        sbCalc.Clear();
                        currentMode = Mode.funcArgument;
                    }
                    else
                    {
                        sbContent.Append(sbLine[i]);
                    }
                }
                else if (currentMode == Mode.insertValue)
                {
                    int value = int.Parse(GetValue(sbLine.ToString(i, sbLine.Length - i)));
                    if (runner.intDic.ContainsKey(splited[0]))
                    {
                        runner.intDic[splited[0]] = value;
                    }
                    else
                    {
                        runner.intDic.Add(splited[0], value);
                    }
                    break;
                }
                else if (currentMode == Mode.funcArgument)
                {
                    int opened = 0;

                    if (sbLine[i] == '(')
                    {
                        opened++;
                    }
                    else if (sbLine[i] == ')')
                    {
                        if (opened == 0)
                        {
                            RunFunc(splited[0], sbContent.ToString());
                            break;
                        }
                        else
                        {
                            opened--;
                        }
                    }
                    sbContent.Append(sbLine[i]);

                }


            }
        }
    }

    public abstract class InstructionCapsule
    {
        public List<System.Type> argumentType;
        public System.Type returnType;

        public Object Run(List<string> argu)
        {
            return null;
        }
    }
}
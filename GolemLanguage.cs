using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class GolemLanguage
{

    public Stack<Object> callStack;
    private static readonly char[] operators = { '+', '-', '/','*' ,'&','|','`','>','<'};

    public Dictionary<string, System.Object> valueDic = new Dictionary<string, System.Object>();

    private Dictionary<string, InstructionCapsule> inst;

    public class FunctionRunner
    {
        System.Object returnValue;
        public GolemLanguage runner;
        Golem gol;


        public enum Mode { start, unknown, insertValue, funcArgument };


        public FunctionRunner(Golem g)
        {
            this.gol = g;
            this.runner = new GolemLanguage();
        }

        public System.Object RunLines(string lines)
        {
            returnValue = null;

            Stack<int> indents = new Stack<int>();
            Stack<string> indentType = new Stack<string>();
            Stack<int> indentStartAt = new Stack<int>();

            int currentIndent = 0;
            string currentIndentType = "none";

            StringBuilder sb = new StringBuilder() ;
            int opened = 0;

            bool indentNeedChange = false;
            bool pass = false;


            string[] lineSplit = lines.Split('\n');
            for (int l=0; l< lineSplit.Length+1;l++)
            {
                sb.Clear();
                int i;

                if (l == lineSplit.Length)
                {
                    if (currentIndentType == "반복" && !pass)
                    {
                        currentIndent = indents.Pop();
                        currentIndentType = indentType.Pop();
                        l = indentStartAt.Pop();
                    }
                    else
                    {
                        break;
                    }
                }

                for (i=0; i< lineSplit[l].Length; i++)
                {
                    if(lineSplit[l][i]!=' ')
                    {
                        break;
                    }
                }

                if (indentNeedChange)
                {
                    if (currentIndent > i)
                    {
                        throw new System.Exception("indent error");
                    }

                    indents.Push(currentIndent);
                    currentIndent = i;
                    indentNeedChange = false;
                    indentStartAt.Push(l - 1);
                    Debug.Log(currentIndent + " : " + currentIndentType + " ENTER");
                }

                if (pass)
                {
                    if (currentIndent > i)
                    {
                        pass = false;
                        currentIndent = indents.Pop();
                        currentIndentType = indentType.Pop();
                        indentStartAt.Pop();
                    }
                    else
                    {
                        continue;
                    }
                }

                while (currentIndent != i )
                {
                    if ((currentIndent == 0 && i != 0 )|| indents.Count == 0)
                    {
                        throw new System.Exception("indent error");
                    }

                    Debug.Log(currentIndent + " : " + currentIndentType + " ESCAPE");

                    if (currentIndentType == "반복")
                    {
                        currentIndent = indents.Pop();
                        currentIndentType = indentType.Pop();
                        l=indentStartAt.Pop();
                    }
                    else
                    {
                        currentIndent = indents.Pop();
                        currentIndentType = indentType.Pop();
                        indentStartAt.Pop();
                    }
                    
                }


                if (lineSplit[l].Substring(i).StartsWith("반복"))
                {

                    indentNeedChange = true;
                    indentType.Push(currentIndentType);
                    currentIndentType = "반복";

                    for(int j = i+2; j < lineSplit[l].Length; j++)
                    {
                        if (lineSplit[l][j] == '(')
                        {
                            opened++;
                            sb.Append(lineSplit[l][j]);
                        }
                        else if (lineSplit[l][j] == ')')
                        {
                            if (opened == 0)
                            {
                                throw new System.Exception("반복 : 인자 오류");
                            }
                            else
                            {
                                opened--;
                            }

                            sb.Append(lineSplit[l][j]);
                        }
                        else
                        {
                            if (opened == 0)
                            {
                                if (lineSplit[l][j] == ' ' || lineSplit[l][j] =='\n')
                                    continue;
                                else
                                    throw new System.Exception("반복 : 괄호 없음");
                            }
                            else
                            {
                                sb.Append(lineSplit[l][j]);
                            }
                        }
                    }

                    bool cond = (bool)GetValue(sb.ToString());


                    if (!cond)
                    {
                        pass = true;
                    }
                    continue;
                }

                if (lineSplit[l].Substring(i).StartsWith("만약"))
                {
                    indentNeedChange = true;
                    indentType.Push(currentIndentType);
                    currentIndentType = "만약";

                    for (int j = i + 2; j < lineSplit[l].Length; j++)
                    {
                        if (lineSplit[l][j] == '(')
                        {
                            opened++;
                            sb.Append(lineSplit[l][j]);
                        }
                        else if (lineSplit[l][j] == ')')
                        {
                            if (opened == 0)
                            {
                                throw new System.Exception("만약 : 인자 오류");
                            }
                            else
                            {
                                opened--;
                            }

                            sb.Append(lineSplit[l][j]);
                        }
                        else
                        {
                            if (opened == 0)
                            {
                                if (lineSplit[l][j] == ' ' || lineSplit[l][j] == '\n')
                                    continue;
                                else
                                    throw new System.Exception("만약 : 괄호 없음");
                            }
                            else
                            {
                                sb.Append(lineSplit[l][j]);
                            }
                        }
                    }

                    bool cond = bool.Parse((string)GetValue(sb.ToString()));

                    if (!cond)
                    {
                        pass = true;
                    }
                    continue;
                }

                RunLine(lineSplit[l].Substring(i));
            }

            return returnValue;
        }

        public System.Object RunFunc(string header, string value)
        {
            Debug.Log(value);
            StringBuilder sb = new StringBuilder();
            StringBuilder sbSub = new StringBuilder();
            List<string> args = new List<string>();
            List<System.Object> argsCalc = new List<System.Object>();
            int opened = 0;

            Debug.Log(value);

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == ')')
                {
                    if (opened == 0)
                    {
                        Debug.LogError("FUNC ERRROR");
                        return null;
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

                argsCalc.Add(GetValue(args[i]));
            }

            switch (header)
            {
                case "합":
                    //REQ : args n (INT)

                    int sum = 0;
                    for (int i = 0; i < argsCalc.Count; i++)
                    {
                        if (argsCalc[i].GetType().Equals(typeof(System.Int32)))
                        {
                            sum += (int)argsCalc[i];
                        }
                        else
                        {
                            throw new System.Exception("SUM NEED INT");
                        }
                    }

                    return sum;

                case "돌아":
                    //REQ : args 1 (INT)
                    if (args.Count != 1)
                    {
                        return null;
                    }
                    else
                    {
                        
                        if (argsCalc[0].GetType().Equals(typeof(System.Int32)))
                        {
                            Debug.Log(argsCalc[0].GetType());
                            gol.TurnRight((int)argsCalc[0]);
                        }
                    }

                    return null;
                case "앞으로":
                    gol.MoveFoward();
                    return null;
                case "출력":
                    if (args.Count != 1)
                    {
                        return null;
                    }
                    else
                    {
                        gol.Print(argsCalc[0].ToString());
                    }
                    break;
                case "테스트4":
                    return 1;
            }

            return null;
        }

        private System.Object GetValue(string line)
        {
            StringBuilder sbContent = new StringBuilder();
            StringBuilder sbCalc = new StringBuilder();

            bool stringMode = false;

            int submode = 0;

            line = line.Replace("같다", "`").Replace("크다", ">").Replace("작다", "<").Replace("또는", "|").Replace("그리고", "&").Replace("==","`").Replace(" ","").Replace("\"","'");
            

            StringBuilder sbLine = new StringBuilder(line);

            for (int i = 0; i < sbLine.Length; i++)
            {
                if(sbLine[i] == '\'')
                {
                    if (stringMode)
                    {
                        stringMode = false;
                        sbContent.Append(sbLine[i]);
                        sbCalc.Append(sbContent);
                        sbContent.Clear();
                    }
                    else
                    {
                        stringMode = true;
                        sbContent.Append(sbLine[i]);   
                    }
                }
                else if (stringMode)
                {
                    sbContent.Append(sbLine[i]);
                }
                else if (char.IsDigit(sbLine[i]) && submode == 0)
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

                        if (runner.valueDic.ContainsKey(sbContent.ToString()))
                        {
                            get = runner.valueDic[sbContent.ToString()].ToString();
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

                if (runner.valueDic.ContainsKey(sbContent.ToString()))
                {
                    get = runner.valueDic[sbContent.ToString()].ToString();
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

            return (dt.Compute(sbCalc.ToString().Replace('`', '='), ""));
        }

        private void RunLine(string line)
        {

            List<string> splited = new List<string>();
            StringBuilder sbLine = new StringBuilder(line);
            StringBuilder sbContent = new StringBuilder();
            StringBuilder sbCalc = new StringBuilder();

            Mode currentMode = Mode.start;

            int submode = 0;

            bool blanked = false;

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
                        blanked = true;
                        continue;
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
                        if (blanked)
                        {
                            throw new System.Exception("띄어쓰기 이후에 결말이 안 남");
                        }

                        sbContent.Append(sbLine[i]);
                    }
                }
                else if (currentMode == Mode.insertValue)
                {
                    System.Object value = GetValue(sbLine.ToString(i, sbLine.Length - i));
                    if (runner.valueDic.ContainsKey(splited[0]))
                    {
                        runner.valueDic[splited[0]] = value;
                    }
                    else
                    {
                        runner.valueDic.Add(splited[0], value);
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
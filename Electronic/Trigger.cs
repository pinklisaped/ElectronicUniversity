using System;
using System.IO;

namespace Electronic
{
    public class Circuit
    {
        Input[] inputs;
        public bool Initial_Value;
        public string Name { get; set; }
        public bool[] Result { get; private set; }

        public Circuit(string objects)
        {
            string[] str = objects.Split('\n');
            Name = str[0].Substring(1, str[0].Length - 3);
            Initial_Value = new Random().Next(0,5) % 2 == 0;
            string[] obj = str[1].Split(',');
            string[] tmp = {"\\","/","0","[","]"," "};
            inputs = new Input[obj.Length];
            for (byte i = 0; i < obj.Length; i++)
            {
                Type tp = Type.direct;
                if (obj[i].Contains("0"))
                    tp = Type.inverse;
                else if (obj[i].Contains("\\"))
                    tp = Type.rear;
                else if (obj[i].Contains("/"))
                    tp = Type.front;
                foreach (var temp in tmp)
                    obj[i] = obj[i].Replace(temp, string.Empty);
                inputs[i] = new Input(obj[i],tp);
            }
                States = Generate();
            
        }
        public Input[] Inputs
        {
            get { return inputs; }
            set { inputs = value; }
        }
        public bool[,] States
        {
            get
            {
                bool[,] temp = new bool[inputs.Length, Params.Steps];
                for (int i = 0; i < inputs.Length; i++)
                {
                    for (int j = 0; j < inputs[i].States.Length; j++)
                    {
                        temp[i,j] = inputs[i].States[j];
                    }
                }
                return temp;
            }
            set 
            {
                for (int i = 0; i < value.GetLength(0); i++)
                {
                    bool[] temp=new bool[value.GetLength(1)];
                    for (int j = 0; j < value.GetLength(1); j++)
                    {
                        temp[j] = value[i, j];
                    }
                    inputs[i].States = temp;
                }
                Result_Calculator();
            }
        }
        void Result_Calculator()
        {
            Input[] inp_tempolary = new Input[inputs.Length];
            inputs.CopyTo(inp_tempolary, 0);
            bool last_state = Initial_Value, c_state = false;
            bool[] temp = new bool[Params.Steps];
            Array.Sort(inp_tempolary);
            for (int j = 0; j < Params.Steps; j++)
            {
                for (int i = 0; i < inp_tempolary.Length; i++)
                {
                    if ((inp_tempolary[i].Type == Type.front || inp_tempolary[i].Type == Type.direct) && inp_tempolary[i].States[j] == true)
                    {
                        if (j > 0 && inp_tempolary[i].Name == "C" && inp_tempolary[i].States[j - 1] == false)
                            c_state = !c_state;
                        switch (inp_tempolary[i].Name)
                        {
                            case "S": last_state = true; break;
                            case "R": last_state = false; break;
                            case "T": if (j > 0 && inp_tempolary[i].States[j - 1] == false) last_state = !last_state; break;
                            case "C": if (inp_tempolary[i].Type == Type.direct)
                                    last_state = inp_tempolary[i + 1].States[j];
                                else if (c_state) last_state = inp_tempolary[i + 1].States[j]; break;
                        }
                        break;
                    }
                    else if ((inp_tempolary[i].Type == Type.rear || inp_tempolary[i].Type == Type.inverse) && inp_tempolary[i].States[j] == false)
                    {
                        if (j > 0 && inp_tempolary[i].Name == "C" && inp_tempolary[i].States[j - 1] == true)
                            c_state = !c_state;
                        switch (inp_tempolary[i].Name)
                        {
                            case "S": last_state = true; break;
                            case "R": last_state = false; break;
                            case "T": if (j > 0 && inp_tempolary[i].States[j - 1] == true) last_state = !last_state; break;
                            case "C": if (inp_tempolary[i].Type == Type.inverse)
                                    last_state = inp_tempolary[i + 1].States[j];
                                else if (c_state) last_state = inp_tempolary[i + 1].States[j]; break;
                        }
                        break;
                    }
                }
                temp[j] = last_state;
            }
            Result= temp;
        }
        public bool[,] Generate()
        {
            Random rnd = new Random();
            bool[,] temp = new bool[inputs.Length, Params.Steps];
            for (int i = 0; i < inputs.Length; i++)
            {
                int tmp = rnd.Next(35, (int)Math.Pow(2, Params.Steps));
                for (int j = 0; j < Params.Steps; j++)
                {
                    if (tmp >= Math.Pow(2, Params.Steps - j - 1))
                    {
                        temp[i, j] = true;
                        tmp -= (int)Math.Pow(2, Params.Steps - j - 1);
                    }
                }
            }
            return temp;
        }
        public static Circuit[] Read()           
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory + "\\lib", "*.trg", SearchOption.TopDirectoryOnly);
            Circuit[] temp = new Circuit[files.Length];
            for (int i = 0; i < files.Length; i++)
                temp[i] = new Circuit(File.ReadAllText(files[i]));
            return temp;
        }
    }
    public enum Type {direct,inverse,front,rear}
    public class Input:IComparable
    {
        string name;
        Type type;
        bool[] states;
        public byte Priority;
        public Input(string name, Type type)
        {
            string[] priority_calc = {"S","R","T","C","D"};
            this.name = name;
            for (byte i = 0; i < priority_calc.Length;i++)
            {
                if (name.Contains(priority_calc[i]))
                { Priority = i; break; }
            }
            this.type = type;
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        internal Type Type
        {
            get { return type; }
            set { type = value; }
        }

        public bool[] States
        {
            get { return states; }
            set { states = value; }
        }

        public int CompareTo(object obj)
        {
            Input tmp = (Input)obj;
            if (tmp.Priority > this.Priority)
                return -1;
            else if (tmp.Priority < this.Priority)
                return 1;
            return 0;
        }
    }
   
}

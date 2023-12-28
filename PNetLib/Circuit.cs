using System;
using System.IO;

namespace ElectronicElements
{
    [Serializable]
    public class ElectronicCircuit
    {
        public bool Initial_Value;
        /// <summary>
        /// Служит для обнаружения сервером полной схемы
        /// </summary>
        public readonly int ID;   
        public string Name { get; set; }
        /// <summary>
        /// Коллекция входов схемы
        /// </summary>
        public Input[] Inputs { get; set; }    
        /// <summary>
        /// Состояния выходов схемы
        /// </summary>            
        public bool[] Output { get; set; }
        public ElectronicCircuit() { this.ID = this.GetHashCode() / 3;  }
        public ElectronicCircuit(Input[] inputs):this() { this.Inputs = inputs; }
        public ElectronicCircuit(int id, Input[] inputs):this(inputs) { ID = id;}
    }                
    public class TriggerCircuit:ElectronicCircuit
    {
        public TriggerCircuit(string objects)
        {
            Initial_Value = Environment.TickCount%2==0;
            string[] preview_string = objects.Split('\n');
            Name = preview_string[0].Substring(1, preview_string[0].Length - 3);
            if (preview_string[1].Length > 2)
                preview_string[1] = preview_string[1].Substring(1, preview_string[1].Length - 2);
            string[] preview_inputs = preview_string[1].Split(',');
            char[] tmp = { '\\', '/', '0', ' ' };
            Inputs = new Input[preview_inputs.Length];
            for (byte i = 0; i < preview_inputs.Length; i++)
            {
                CircuitInputType InputType;
                switch (preview_inputs[i][0])
                {
                    case '0': InputType = CircuitInputType.inverse; break;
                    case '\\': InputType = CircuitInputType.rear; break;
                    case '/': InputType = CircuitInputType.front; break;
                    default: InputType = CircuitInputType.direct; break; ;
                }
                preview_inputs[i] = preview_inputs[i].Trim(tmp);
                Inputs[i] = new Input(preview_inputs[i], InputType);
            }
            States = TriggerCircuit.Generate(Inputs.Length);
        }
        public bool[,] States
        {
            get
            {
                bool[,] temp = new bool[Inputs.Length, Params.Steps];
                for (int i = 0; i < Inputs.Length; i++)
                {
                    for (int j = 0; j < Inputs[i].States.Length; j++)
                    {
                        temp[i,j] = Inputs[i].States[j];
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
                    Inputs[i].States = temp;
                }
                Result_Calculator();
            }
        }
        void Result_Calculator()
        {
            Input[] input_temp = new Input[Inputs.Length];
            Output = new bool[Params.Steps];
            Inputs.CopyTo(input_temp, 0);
            bool last_state = Initial_Value, c_state = false;
            Array.Sort(input_temp);
            for (int j = 0; j < Params.Steps; j++)
            {
                for (int i = 0; i < input_temp.Length; i++)
                {
                    if ((input_temp[i].Type == CircuitInputType.front || input_temp[i].Type == CircuitInputType.direct) && input_temp[i].States[j] == true)
                    {
                        if (j > 0 && input_temp[i].Name == "C" && input_temp[i].States[j - 1] == false)
                            c_state = !c_state;
                        switch (input_temp[i].Name)
                        {
                            case "S": last_state = true; break;
                            case "R": last_state = false; break;
                            case "T": if (j > 0 && input_temp[i].States[j - 1] == false) last_state = !last_state; break;
                            case "C": if (input_temp[i].Type == CircuitInputType.direct)
                                    last_state = input_temp[i + 1].States[j];
                                else if (c_state==true) last_state = input_temp[i + 1].States[j]; break;
                        }
                        break;
                    }
                    else if ((input_temp[i].Type == CircuitInputType.rear || input_temp[i].Type == CircuitInputType.inverse) && input_temp[i].States[j] == false)
                    {
                        if (j > 0 && input_temp[i].Name == "C" && input_temp[i].States[j - 1] == true)
                            c_state = !c_state;
                        switch (input_temp[i].Name)
                        {
                            case "S": last_state = true; break;
                            case "R": last_state = false; break;
                            case "T": if (j > 0 && input_temp[i].States[j - 1] == true) last_state = !last_state; break;
                            case "C": if (input_temp[i].Type == CircuitInputType.inverse)
                                    last_state = input_temp[i + 1].States[j];
                                else if (c_state) last_state = input_temp[i + 1].States[j]; break;
                        }
                        break;
                    }
                }
                Output[j] = last_state;
            }
        }
        public override string ToString()
        {
            return Name;
        }
        public static bool[,] Generate(int lenght)
        {
            Random rnd = new Random();
            ushort max_base = (ushort)Math.Pow(2, Params.Steps);
            bool[,] temp = new bool[lenght, Params.Steps];
            for (int i = 0; i < lenght; i++)
            {
                int tmp = rnd.Next(35, max_base-33);
                System.Threading.Thread.Sleep(10);
                for (int j = 0, p = (int)Math.Pow(2, (int)Math.Log(tmp, 2)); tmp>0; j++, p /= 2)
                {
                    if (tmp < p)
                        continue;
                    temp[i, j] = true;
                    tmp -= p;
                }
            }
            return temp;
        }
        public static TriggerCircuit[] Read()           
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory + "\\lib", "*.trg", SearchOption.TopDirectoryOnly);
            TriggerCircuit[] temp = new TriggerCircuit[files.Length];
            for (int i = 0; i < files.Length; i++)
                temp[i] = new TriggerCircuit(File.ReadAllText(files[i]));
            return temp;
        }
        public ElectronicCircuit Cut() { return new ElectronicCircuit(this.ID, this.Inputs) { Initial_Value = this.Initial_Value }; }

    }
    public enum CircuitInputType:byte {direct,inverse,front,rear}
    [Serializable]
    public class Input:IComparable
    {
        public readonly byte Priority;
        public Input(string name, CircuitInputType type)
        {
            string[] priority_calc = {"S","R","T","C","D"};
            this.Name = name;
            for (byte i = 0; i < priority_calc.Length;i++)
            {
                if (name.Contains(priority_calc[i]))
                { Priority = i; break; }
            }
            this.Type = type;
        }
        public string Name { get; private set; }

        public CircuitInputType Type { get; private set; }
        /// <summary>
        /// Состояния входа
        /// </summary>
        public bool[] States { get; set; }

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

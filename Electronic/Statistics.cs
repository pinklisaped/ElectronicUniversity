using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Electronic
{
    [Serializable]
    public class StudentS
    {
        public string Name;
        string Group;
        Statistics Statistic;
        public StudentS(string name, string group, Statistics statistics)
        {
            this.Name = name; this.Group = group; Statistic = statistics;
        }
        public void SaveResult()
        {
            File.AppendAllText("Report.txt", string.Format("\r\nИмя: {0}, группа: {1}, результат: {2:P0}, закончил {3} из {4}, время:{5:H:mm:ss}", Name, Group, Statistic.FULLRESULT, Statistic.Count_Finished, Statistic.Count, DateTime.Now));
        }
    }
    [Serializable]
    public class Statistics
    {
        public float FULLRESULT { get { float result = 0; foreach (float Current in Result)result += Current; if (Count_Finished == 0)return 0; return result / Count_Finished; } }
        float[] Result;
        public Statistics(int Triggers_Count)
        {
            Result = new float[Triggers_Count];
        }
        public float this[int index]
        {
            get { return Result[index]; }
            set { if (Result[index] == 0) Result[index] = value; }
        }
        public ushort Count_Finished { get { ushort i = 0; foreach (float Current in Result) { if (Current > 0)i++; } return i; } }
        public ushort Count { get { return (ushort)Result.Length; } }
    }
    //[Serializable]
    //public class Report
    //{
    //    public Statistics statisctics;
    //    public Trigger[] triggers;
    //    public Persona student;
    //    DateTime end;
    //    public Report(Statistics statisctics_student, Trigger[] test_triggers, Persona person)
    //    {
    //        statisctics = statisctics_student; test_triggers = triggers; student = person;
    //    }
    //    public Report() { }
    //    public void Save()
    //    {
    //        end = DateTime.Now;
    //        FileStream fs = new FileStream(student.GetHashCode().ToString("x"), FileMode.Create);
    //        BinaryFormatter writer = new BinaryFormatter();
    //        writer.Serialize(fs, this);
    //    }
    //}
}

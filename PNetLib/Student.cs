using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PNetwork;

namespace UserElements
{                  
    public delegate void StudentEvent(object sender);     
    public enum StatusList:byte { Ожидает, Удалён, Завершил, Тестируется }
    /// <summary>
    /// Класс студента, хранит в себе все данные о всех действиях студента
    /// </summary>
    [Serializable]
     public class Student:INotifyPropertyChanged,IEquatable<Student>
    {

        [field: NonSerialized]
        public event StudentEvent EndTest;
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        [NonSerialized]
        System.Net.Sockets.Socket _Client;
        StatusList _Status;       
        public string ID;         
        public System.Net.Sockets.Socket Socket
        {
            get { return _Client; }
            set { _Client = value; }
        }       
        public Test_Type Type { get; set; }
        public Statistics Statistic { get; set; }
        public string Name { get;  set; }
        public string Group { get; set; }  
        public int Current_Task
        {
            get
            {

                if (Status == StatusList.Тестируется)
                {
                    if (Statistic.Step_Score == null) return 1;
                    else return Statistic.Step_Score.Count + 1;
                }
                else return 0;
            }
        }
        public StatusList Status { get { return _Status; } set { if (_Status != StatusList.Удалён) { _Status = value; NotifyPropertyChanged(); } } }
        public float Score { get { return Statistic.Score; } }
        public float Total_Score { get { return Statistic.Total_Score; } }
        public Student():this(string.Empty) { }
        public Student(string name) : this(name,null) { }        
        public Student(string name, string group) : this(name, group,Test_Type.Triggers) { }
        public Student(string name, string group,Test_Type type)
        {
            this.Statistic = new Statistics(5, 6);
            this.Name = name;
            this.Group = group;
            this.Type = type;
            this.ID = name.GetHashCode().ToString(); ;
        }
        private void NotifyPropertyChanged()
        {
            if (Status == StatusList.Ожидает)
                Status = StatusList.Тестируется;
            else if (Statistic.Step_Score.Count == Statistic.Capacity)
                Status = StatusList.Завершил;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Score"));
                PropertyChanged(this, new PropertyChangedEventArgs("Total_Score"));
                PropertyChanged(this, new PropertyChangedEventArgs("Current_Task"));
                PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                PropertyChanged(this, new PropertyChangedEventArgs("Statistic")); 
                PropertyChanged(this, new PropertyChangedEventArgs("Current_Task"));
            }
            if (Status == StatusList.Завершил && EndTest != null)
                EndTest(this);
        }

        /// <summary>
        /// Добавляет ответ студента в статистику
        /// </summary>
        /// <param name="answer">Ответ студента</param>
        /// <param name="true_answer">Правильный ответ</param>
        public void Add_Answer(bool?[] answer, bool[] true_answer)
        {
            Statistic.Add_Answer(answer,true_answer);
            NotifyPropertyChanged();
        }
        public void Add_Answer(bool?[] answer)
        {
            Statistic.Add_Answer(answer);
            NotifyPropertyChanged();
        }
        public void Add_Answer(bool[] true_answer)
        {
            Statistic.Add_Answer(true_answer);
        }
        public static bool operator ==(Student one, Student two) { if ((object)one == null && (object)two == null) return true; return one.Equals(two); }
        public static bool operator !=(Student one, Student two) { if ((object)one == null && (object)two == null) return false; return !one.Equals(two); }
        public bool Equals(Student other)
        {
            if ((object)other == null) return false;
            return Name == other.Name && Group == other.Group;
        }
    }

    /// <summary>
    /// Класс статистики, хранит все ответы и оценки на их основе
    /// </summary>
    [Serializable] 
    public class Statistics
    {
        List<Statistic_Answer> _Answers;
        public readonly float _Max_Score;
        /// <summary>
        /// Представляет промежуточную предполагаюемую оценку
        /// </summary>
        public float Score { get; private set; }
        /// <summary>
        /// Представляет конечную оценку, в зависимости от количества заданий 
        /// </summary>
        public float Total_Score { get; private set; }
        /// <summary>
        /// Коллекция результатов ответов
        /// </summary>
        public ObservableCollection<string> Step_Score { get; private set; }
        /// <summary>
        /// Отображает количество ответов для формирования конечной оценки
        /// </summary>
        [field: NonSerialized]
        public int Capacity { get { return _Answers.Capacity; } }
        public Statistic_Answer Last_Answer { get { if (_Answers.Count < 1)return default(Statistic_Answer); return _Answers[_Answers.Count - 1]; } }
        /// <param name="max_score">Максимальное количество баллов</param>
        /// <param name="tasks">Количество заданий</param>
        public Statistics(float max_score, int tasks)
        {             
            _Max_Score = max_score;
            _Answers = new List<Statistic_Answer>(tasks);
            Step_Score = new ObservableCollection<string>();
        }
        /// <summary>
        /// Добавляет ответ студента в статистику
        /// </summary>
        /// <param name="answer">Ответ пользователя, сопоставленный правильному ответу</param>
        void Add_Answer(Statistic_Answer answer)
        {
            _Answers.Add(answer);
            Calculate_Score();
        }
        /// <summary>
        /// Добавляет ответ студента в статистику
        /// </summary>
        /// <param name="answer">Ответ студента</param>
        /// <param name="true_answer">Правильный ответ</param>
        public void Add_Answer(bool?[] answer, bool[] true_answer)
        {
            Add_Answer(new Statistic_Answer(answer, true_answer)); 
        }
        public void Add_Answer(bool?[] answer)
        {
            if (_Answers.Count < 1 || _Answers[_Answers.Count - 1].True_Answer == null)
                throw new Exception("Перед добавлением ответа пользователя не был добавлен правильный ответ в статистику");
            _Answers[_Answers.Count - 1] = new Statistic_Answer(answer, _Answers[_Answers.Count - 1].True_Answer);
            Calculate_Score();
        }
        public void Add_Answer(bool[] true_answer)
        {
            _Answers.Add(new Statistic_Answer(true_answer));
        }
        void Calculate_Score()
        {
            float last_result = (float)_Answers[_Answers.Count - 1].Get_Errors_Count()/_Answers[_Answers.Count - 1].Answer.Length*_Max_Score;  
            Step_Score.Add(string.Format("{1} задание: {0:F1}", last_result, Step_Score.Count + 1));
            Score = _Answers.Count > 1 ? 2/(1/Score + 1/last_result) : last_result;//_Answers.Count > 1 ? (Score + last_result) / 2 : last_result;
            Total_Score = Total_Score + last_result/_Answers.Capacity;
        }

    }

    /// <summary>
    /// Класс-оболочка списка студентов
    /// </summary>
    [Serializable]
    public class StudentsList : ObservableCollection<Student>
    {
        public event StudentEvent Students_Changed;
        public int END_TEST { get; private set; }
        public StudentsList() : base() { }
        public new void Add(Student item)
        {
            item.EndTest += item_EndTest;
            base.Add(item);
            if (Students_Changed != null)
                Students_Changed(item);
        }
        void item_EndTest(object sender)
        {
            END_TEST++;
            if (Students_Changed != null)
                Students_Changed(sender);
        }
        public void Test_Start()
        {
            foreach (Student st in this.Items)
                st.Status = StatusList.Тестируется;       
        }
        public void Test_Stop()
        {
            foreach (Student st in this.Items)
                st.Status = StatusList.Завершил;
        }
    }

    [Serializable]
    public struct Statistic_Answer
    {
        bool?[] _Answer;
        public bool?[] Answer
        {
            get { return _Answer; }
            set
            {
                if (value.Length != True_Answer.Length)
                    throw new ArgumentOutOfRangeException("Answer", "Входной и правильный ответы имеют разную длину");
                _Answer = value;
            }
        }
        public bool[] True_Answer { get; private set; }
        public Statistic_Answer(bool[] true_answer) : this() { True_Answer = true_answer; }
        public Statistic_Answer(bool?[] answer, bool[] true_answer) : this(true_answer) { Answer = answer; }
        public int Get_Errors_Count()
        {
            int result = 0;
            for (int i = 0; i < Answer.Length; i++)
            {
                if (Answer[i] == True_Answer[i])
                    result++;
            }
            return result;
        }
    }
}

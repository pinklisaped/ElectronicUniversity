using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Electronic
{
    /// <summary>
    /// Логика взаимодействия для Input_Interface.xaml
    /// </summary>
    public partial class Input_Interface : UserControl
    {
        Cell[] CELLS;
        Polyline value;
        Rectangle cur;
        int index;
        public bool Initial_Value;
        public Input_Interface()
        {
            InitializeComponent();
            Foreground = new SolidColorBrush(Params.Grid_Color);
            value = new Polyline() { StrokeThickness = Params.Value_size, Stroke = new SolidColorBrush(Params.Value_Color) };
            //cur = new Polygon() {Opacity=0.8, Fill= new SolidColorBrush(Params.Grid_Color), Points = {new Point(),new Point(),new Point() } };
            this.KeyDown += Input_Interface_KeyDown;
            this.SizeChanged += (o, e) =>
            {
                if (CELLS == null)
                    return;
                Cell[] old = CELLS;
                int tmp = index;
                Initialize(Initial_Value);
                for (index = 0; index < Params.Steps; index++)
                {
                    CELLS[index].State = old[index].State;
                    Value_Paint();
                }
                index = tmp;
                Cursor_Paint();
            };
        }
        public void Initialize(bool state)
        {
            Initial_Value = state;
            index = 0;
            this.Height = 100 * (this.ActualWidth / 630);
            GridMain.Children.Clear(); 
            Cell_Layout();    
            cur = new Rectangle() { Opacity = 0.3, Fill = Brushes.SteelBlue, Margin = new Thickness(31, 0, 0, 2), Width = CELLS[0].Width, Height = this.Height, HorizontalAlignment = HorizontalAlignment.Left };
            GridMain.Children.Add(cur);
            Paint(); 
             value.Points.Clear();
            Initial_Value_Paint();     
            GridMain.Children.Add(value);
            Cursor_Paint();
        }
        void Input_Interface_KeyDown(object sender, KeyEventArgs e)
        {
            if (Visibility != System.Windows.Visibility.Visible)
                return;
            switch (e.Key)
            {
                case Key.Up:
                case Key.W: { CELLS[index].State = true; Value_Paint(); } break;
                case Key.Down:
                case Key.S: { CELLS[index].State = false; Value_Paint(); } break;
                case Key.Left:
                case Key.A: if (index > 0) index--; break;
                case Key.Right:
                case Key.D: if (CELLS[0].State == null) return; if (index < CELLS.Length - 1) index++;
                    if (CELLS[index - 1].State == null & index > 0) { CELLS[index - 1].State = CELLS[index - 2].State; index--; Value_Paint(); index++; }
                    break;
                case Key.D1: CELLS[index].State = true; Value_Paint(); if (index < CELLS.Length - 1) index++; break;
                case Key.D0: CELLS[index].State = false; Value_Paint(); if (index < CELLS.Length - 1) index++; break;
            }
            Cursor_Paint();
        }
        void Cell_Layout()
        {
            CELLS = new Cell[Params.Steps];
            int row_height = (int)this.ActualHeight,
                column_width = ActualWidth > 30 ? (int)((this.ActualWidth - 30) / Params.Steps) : 0;

            for (int i = 0; i < Params.Steps; i++)
            {
                CELLS[i] = new Cell(i * column_width+30, row_height, row_height, column_width);
            }
        }
        void Paint()
        {
            Line[] borders = new Line[CELLS.Length];
            Label qlabel = new Label() {Foreground=Brushes.SteelBlue, FontSize=20,FontWeight=FontWeights.SemiBold, Margin=new Thickness(0,this.ActualHeight/2-20,0,0), Content="Q"};
            GridMain.Children.Add(qlabel);
            for (int i = 0; i < Params.Steps; i++)
            {
                GridMain.Children.Add(new Line() { X1 = CELLS[i].Left_Border, X2 = CELLS[i].Left_Border, Y1 = 0, Y2 = ActualHeight, StrokeThickness = Params.Border_size, Stroke = Foreground });
            }
            GridMain.Children.Add(new Line() { X1 = 30, X2 = GridMain.ActualWidth, Y1 = ActualHeight - 1, Y2 = ActualHeight - 1, StrokeThickness = Params.Border_size, Stroke = Foreground });
        }
        void Cursor_Paint()
        {
            cur.Margin=new Thickness(CELLS[index].Left_Border,0,0,2);
            //int bottom=(int)this.ActualHeight,size=CELLS[index].Left_Border+CELLS[index].Width;
            //cur.Points[0]= new Point(size - 15,bottom);
            //cur.Points[1] = new Point(size+2, bottom - 15);
            //cur.Points[2] = new Point(size+2, bottom);
        }
        void Initial_Value_Paint()
        {
            Point[] Initial_Value_Graphic = new Point[2];
            Initial_Value_Graphic[0] = Initial_Value == true ? new Point(20, this.ActualHeight / 2) : new Point(20, this.ActualHeight - 2);
            Initial_Value_Graphic[1] = Initial_Value == true ? new Point(30, this.ActualHeight / 2) : new Point(30, this.ActualHeight - 2);
            value.Points.Add(Initial_Value_Graphic[0]);
            value.Points.Add(Initial_Value_Graphic[1]);
        }
        void Value_Paint()
        {
            Point[] temp;
            if (CELLS[index].State == true) temp = One(); else if (CELLS[index].State == false) temp = Zero(); else return;
            if (value.Points.Count < index * 2+3)
            {
                foreach (Point p in temp)
                    value.Points.Add(p);
            }
            else
            {
                value.Points[index * 2+2] = temp[0];
                value.Points[index * 2+3] = temp[1];    
            }
        }
        Point[] One()
        {
            Point[] one={
            new Point(CELLS[index].Left_Border, CELLS[index].Bottom_Border - CELLS[index].Height / 2),
            new Point(CELLS[index].Left_Border + CELLS[index].Width, CELLS[index].Bottom_Border - CELLS[index].Height / 2)};
            return one;
        }
        Point[] Zero()
        {
            Point[] zero ={
            new Point(CELLS[index].Left_Border, CELLS[index].Bottom_Border - 2),
            new Point(CELLS[index].Left_Border + CELLS[index].Width, CELLS[index].Bottom_Border - 2)};
            return zero;
        }
        public bool?[] States { get { return CELLS.Select(c => c.State).ToArray<bool?>(); } }

    }

   
}

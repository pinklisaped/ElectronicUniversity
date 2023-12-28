using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ElectronicElements;

namespace Electronic
{
    public partial class Out_Interface : UserControl
    {
        public static DependencyProperty CIRCUIT;
        public ElectronicCircuit Circuit { get { return (ElectronicCircuit)GetValue(CIRCUIT); } set { SetValue(CIRCUIT, value); } }
        Cell[,] CELLS;
        ElectronicCircuit SELECTED_TRIGGER;
        static Out_Interface()
        {
            CIRCUIT = DependencyProperty.Register("SELECTED_TRIGGER", typeof(ElectronicCircuit), typeof(Out_Interface), new PropertyMetadata(new PropertyChangedCallback(On_Trigger_Changed)));
        }

        public Out_Interface()
        {
            InitializeComponent();
            this.Foreground = new SolidColorBrush(Params.Grid_Color);
        }
        private static void On_Trigger_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Out_Interface OUT = (Out_Interface)d;
            OUT.SELECTED_TRIGGER = (ElectronicCircuit)e.NewValue;
            OUT.Initialize();
        }
        public void Initialize()
        {
            this.Height = SELECTED_TRIGGER.Inputs.Length * 100 * (this.ActualWidth / 630);
            GridMain.Children.Clear();
            Cell_Layout();
            Paint();
        }
        void Cell_Layout()
        {
            CELLS = new Cell[SELECTED_TRIGGER.Inputs.Length, SELECTED_TRIGGER.Inputs[0].States.Length];
            float row_height = (float)((this.ActualHeight - 1) / SELECTED_TRIGGER.Inputs.Length),
                column_width = (float)((this.ActualWidth - 30) / Params.Steps);
            for (int i = 0; i < SELECTED_TRIGGER.Inputs.Length; i++)
            {
                for (int j = 0; j < Params.Steps; j++)
                {
                    CELLS[i, j] = new Cell(SELECTED_TRIGGER.Inputs[i].States[j], j * column_width + 30, (i + 1) * row_height, row_height - 10, column_width);
                }
            }
        }
        void Paint()
        {
            //Line[,] borders = new Line[CELLS.GetLength(0), CELLS.GetLength(1)];
            DoubleCollection borders = new DoubleCollection();                       
            borders.Add((CELLS[0,0].Height)/4);
            borders.Add(2.5);                      
            for (int j = 0; j < Params.Steps; j++)
            {
                GridMain.Children.Add(new Label() { Content = j + 1, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(CELLS[0, j].Left_Border + CELLS[0, 0].Width / 2 - 8, 0, 0, 0), FontWeight = FontWeights.SemiBold, FontSize = 18, Foreground = Brushes.OrangeRed });
                GridMain.Children.Add(new Line() { X1 = CELLS[0, j].Left_Border, X2 = CELLS[0, j].Left_Border, Y1 = CELLS[0, j].Bottom_Border - CELLS[0, j].Height, Y2 = CELLS[Circuit.Inputs.Length-1, j].Bottom_Border, StrokeDashArray = borders, StrokeThickness = Params.Border_size, Stroke = Foreground });
            }
            for (int i = 0; i < SELECTED_TRIGGER.Inputs.Length; i++)
            {
                GridMain.Children.Add(new Line() { X1 = 28, X2 = GridMain.ActualWidth, Y1 = CELLS[i, 0].Bottom_Border - 2, Y2 = CELLS[i, 0].Bottom_Border - 2, StrokeThickness = Params.Border_size, Stroke = Foreground });

                Label qlabel = new Label() { Foreground = Brushes.SteelBlue, FontSize = 20, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, this.CELLS[i, 0].Bottom_Border - this.CELLS[i, 0].Height / 2 - 20, 0, 0), Content = SELECTED_TRIGGER.Inputs[i].Name };
                GridMain.Children.Add(qlabel);
            }
            Paint_Value();
        }
        void Paint_Value()
        {
            for (int i = 0; i < SELECTED_TRIGGER.Inputs.Length; i++)
            {
                byte shift = (SELECTED_TRIGGER.Inputs[i].Type == CircuitInputType.front || SELECTED_TRIGGER.Inputs[i].Type == CircuitInputType.rear) ? (byte)10 : (byte)0;
                Polyline pl = new Polyline() { StrokeThickness = Params.Value_size, Stroke = new SolidColorBrush(Params.Value_Color), StrokeLineJoin = PenLineJoin.Bevel };
                for (int j = 0; j < Params.Steps; j++)
                {
                    if (CELLS[i, j].State == true)
                        One(CELLS[i, j], pl, shift);
                    else Zero(CELLS[i, j], pl, shift);
                    if (SELECTED_TRIGGER.Inputs[i].Type == CircuitInputType.front || SELECTED_TRIGGER.Inputs[i].Type == CircuitInputType.rear)
                    {
                    }
                }
                GridMain.Children.Add(pl);
            }
        }
        void One(Cell c, Polyline p, byte shift)
        {
            if (c.Left_Border > 30)
                p.Points.Add(new Point(c.Left_Border + shift, c.Bottom_Border - c.Height / 2));
            else
                p.Points.Add(new Point(c.Left_Border, c.Bottom_Border - c.Height / 2));
            p.Points.Add(new Point(c.Left_Border + c.Width + shift, c.Bottom_Border - c.Height / 2));
        }
        void Zero(Cell c, Polyline p, byte shift)
        {
            if (c.Left_Border > 30)
                p.Points.Add(new Point(c.Left_Border + shift, c.Bottom_Border - 2));
            else
                p.Points.Add(new Point(c.Left_Border, c.Bottom_Border - 2));
            p.Points.Add(new Point(c.Left_Border + c.Width + shift, c.Bottom_Border - 2));
        }

        private void GridMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (SELECTED_TRIGGER != null)
            {
                GridMain.Children.Clear();
                Initialize();
            }
        }
    }
}

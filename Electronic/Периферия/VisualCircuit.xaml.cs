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
using ElectronicElements;


namespace Electronic
{
    /// <summary>
    /// Логика взаимодействия для VisualCircuit.xaml
    /// </summary>
    public partial class VisualCircuit : UserControl
    {
        public static readonly DependencyProperty CIRCUIT;
        public ElectronicCircuit Circuit { get { return (ElectronicCircuit)GetValue(CIRCUIT); } set { SetValue(CIRCUIT, value); } }
        ElectronicCircuit SELECTED_TRIGGER;
        static VisualCircuit()
        {
            CIRCUIT = DependencyProperty.Register("CIRCUIT", typeof(ElectronicCircuit), typeof(VisualCircuit), new PropertyMetadata(new PropertyChangedCallback(On_Trigger_Changed)));
        } 
        private static void On_Trigger_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VisualCircuit vs = (VisualCircuit)d;
            vs.SELECTED_TRIGGER = (ElectronicCircuit)e.NewValue;
            vs.Paint();
        }
        
        public VisualCircuit()
        {
            InitializeComponent();
            this.SizeChanged += (o, e) =>
            {
                this.Height = (this.ActualWidth * 1.2);
                if (SELECTED_TRIGGER != null) Paint();
            };
        }
        void Paint()
        {
            CANVAS.Children.Clear();
            Brush brush = Brushes.SteelBlue; LABELTRIGGER.Content = "T";
            foreach (Input i in SELECTED_TRIGGER.Inputs)
                if (i.Name.ToLower() == "t") LABELTRIGGER.Content = "TT";
            int length = (int)this.ActualHeight / SELECTED_TRIGGER.Inputs.Length;
            int position=length/2;
            foreach(Input i in SELECTED_TRIGGER.Inputs)
            {
                Label letter = new Label() {Content=i.Name,FontSize=22,FontWeight=FontWeights.SemiBold, Foreground=brush};
                if(i.Name.ToLower()=="d"&&i!=SELECTED_TRIGGER.Inputs[0])
                	CANVAS.Children.Add(new Line() { X1 = 70, X2 = 20, Y1 = position-length/2, Y2 = position-length/2, Stroke=brush,StrokeThickness=4});
                if(i.Name.ToLower()=="c"&&i!=SELECTED_TRIGGER.Inputs[SELECTED_TRIGGER.Inputs.Length-1])
                	CANVAS.Children.Add(new Line() { X1 = 70, X2 = 20, Y1 = position+length/2, Y2 = position+length/2, Stroke=brush,StrokeThickness=4});
                CANVAS.Children.Add(letter);Canvas.SetLeft(letter, 30); Canvas.SetTop(letter, position - 20);
                CANVAS.Children.Add(new Line() { X1 = 0, X2 = 20, Y1 = position, Y2 = position, Stroke=brush,StrokeThickness=4});
                
                switch(i.Type)
                {
                    case CircuitInputType.inverse: Ellipse e = new Ellipse() { Height = 15, Width = 15, StrokeThickness = 4, Stroke = brush, Fill = Brushes.White }; 
                        Canvas.SetLeft(e,14); Canvas.SetTop(e,position-7);
                        CANVAS.Children.Add(e);break;
                    case CircuitInputType.rear: CANVAS.Children.Add(new Line() { X1 = 15, X2 = 28, Y1 = position - 7, Y2 = position + 8, Stroke = brush, StrokeThickness = 4, StrokeStartLineCap = PenLineCap.Triangle, StrokeEndLineCap = PenLineCap.Triangle }); break;
                    case CircuitInputType.front: CANVAS.Children.Add(new Line() { X1 = 15, X2 = 28, Y1 = position + 8, Y2 = position - 7, Stroke = brush, StrokeThickness = 4, StrokeStartLineCap = PenLineCap.Triangle, StrokeEndLineCap = PenLineCap.Triangle }); break;
                }
                position+=length;
            }
        }

    }

}

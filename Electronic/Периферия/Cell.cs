using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Controls;

namespace Electronic
{
    public class Cell
    {
        public Cell(float border_left, float border_bottom, float height, float width) : this(null, border_left, border_bottom, height, width) { }
        public Cell(bool? value, float border_left, float border_bottom, float height, float width)
        {
            State = value;
            Width = width; Height = height;
            Bottom_Border = border_bottom;
            Left_Border = border_left;
        }
        public bool? State { get; set; }
        public float Left_Border { get; set; }
        public float Bottom_Border { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using PNetwork;


/// <summary>
/// Основные настройки, необходимые для отрисовки элементов
/// </summary>
public static class Params
{
    public const int Steps = 8;
    public static Color Background_Color = Colors.AliceBlue;//Color.FromArgb(0x16, 0x1A, 0x1E);//16-ричный формат, можно указать в десятичном (21,26,30)
    public static Color Grid_Color = Colors.SkyBlue;//Color.FromArgb(0x16, 0x1A, 0x1E);//16-ричный формат, можно указать в десятичном (21,26,30)
    public static Color Value_Color = Colors.Gold;//Color.FromArgb(0x16, 0x1A, 0x1E);//16-ричный формат, можно указать в десятичном (21,26,30)
    /// <summary>
    /// Толщина линий
    /// </summary>
    public const ushort Value_size = 5;
    public const ushort Border_size = 4;
    public const ushort Constanta = 30;

    public static Test_Mode Mode = Test_Mode.Password; 
}


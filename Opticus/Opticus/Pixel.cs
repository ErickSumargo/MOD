using System;
using System.Drawing;

namespace Opticus
{
    class Pixel
    {
        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        public Point Position { get; set; }

        public Color color { get; set; }

        /*----------------------------------------------------------------------------------------------------------*/

        public Pixel(Point Position, Color color)
        {
            this.Position = Position;
            this.color = color;
        }
    }
}
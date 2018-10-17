using System;
using System.Drawing;

namespace Opticus
{
    class Canvas
    {
        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        Bitmap canvas;

        Graphics graphics;

        Rectangle rectangle;

        /*----------------------------------------------------------------------------------------------------------*/

        public Bitmap Blank(int sizeX, int sizeY)
        {
            canvas = new Bitmap(sizeX, sizeY);

            using (graphics = Graphics.FromImage(canvas))
            {
                rectangle = new Rectangle(0, 0, sizeX, sizeY);

                graphics.FillRectangle(Brushes.Black, rectangle);
            }

            return canvas;
        }
    }
}
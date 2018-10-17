using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;

namespace Opticus
{
    class Restoration
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        int sizeX, sizeY;
        int metric;
        int halfMetric;
        int step;

        byte[] pixels_noiseFiltered;
        byte[] pixels_binaryImage;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        Canvas canvas;

        Bitmap binaryImage, noiseFiltered;

        LockBitmap lbm_binaryImage, lbm_noiseFiltered;

        List<int> binaryIntensity;

        /*----------------------------------------------------------------------------------------------------------*/

        public Restoration()
        {
            sizeX = Transfer.sizeX;
            sizeY = Transfer.sizeY;

            canvas = new Canvas();

            binaryIntensity = new List<int>();
        }

        public void MedianFilter(ref Bitmap binaryImage, int metric)
        {
            this.metric = metric;
            halfMetric = metric / 2;

            noiseFiltered = canvas.Blank(sizeX, sizeY);

            this.binaryImage = binaryImage;

            lbm_noiseFiltered = new LockBitmap(noiseFiltered);
            lbm_noiseFiltered.LockBits();

            lbm_binaryImage = new LockBitmap(this.binaryImage);
            lbm_binaryImage.LockBits();

            step = lbm_noiseFiltered.step;

            pixels_noiseFiltered = lbm_noiseFiltered.Pixels;
            pixels_binaryImage = lbm_binaryImage.Pixels;

            Parallel.Invoke(
                () =>
                {
                    MedianFilterThreaded(halfMetric, halfMetric, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    MedianFilterThreaded(sizeX / 2, halfMetric, sizeX - halfMetric, sizeY / 2);
                },
                () =>
                {
                    MedianFilterThreaded(halfMetric, sizeY / 2, sizeX / 2, sizeY - halfMetric);
                },
                () =>
                {
                    MedianFilterThreaded(sizeX / 2, sizeY / 2, sizeX - halfMetric, sizeY - halfMetric);
                }
            );

            lbm_noiseFiltered.UnlockBits();
            lbm_binaryImage.UnlockBits();

            binaryImage = new Bitmap(noiseFiltered);
        }

        private void MedianFilterThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            int layer = -1;

            List<int> binaryIntensity = new List<int>();

            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    for (int k = -halfMetric; k <= halfMetric; k++)
                    {
                        for (int l = -halfMetric; l <= halfMetric; l++)
                        {
                            layer = ((j + l) * sizeX + (i + k)) * step;

                            binaryIntensity.Add(pixels_binaryImage[layer]);
                        }
                    }

                    binaryIntensity.Sort();

                    int medianValue = binaryIntensity[metric * metric / 2];

                    pixels_noiseFiltered[layer] = pixels_noiseFiltered[layer + 1] = pixels_noiseFiltered[layer + 2] = (byte)medianValue;

                    binaryIntensity.Clear();
                }
            }
        }
    }
}
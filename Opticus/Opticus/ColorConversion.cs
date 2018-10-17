using System;
using System.Threading.Tasks;
using System.Drawing;

namespace Opticus
{
    class ColorConversion
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        int sizeX, sizeY;
        int DS;
        int SS;
        int step;

        int[,] T;

        byte[] pixels_RGB;
        byte[] pixels_Gray;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        LockBitmap lbm_RGBImage, lbm_GrayImage;

        /*----------------------------------------------------------------------------------------------------------*/

        public ColorConversion()
        {
            sizeX = Transfer.sizeX;
            sizeY = Transfer.sizeY;
        }

        public void HSL(Bitmap RGBImage)
        {
            lbm_RGBImage = new LockBitmap(RGBImage);
            lbm_RGBImage.LockBits();

            step = lbm_RGBImage.step;

            pixels_RGB = lbm_RGBImage.Pixels;

            Parallel.Invoke(
                () =>
                {
                    HSLThreaded(0, 0, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    HSLThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                },
                () =>
                {
                    HSLThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                },
                () =>
                {
                    HSLThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                }
            );

            lbm_RGBImage.UnlockBits();
        }

        public void HSLThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            double R, G, B;
            double H, S, L;

            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = ((j * sizeX) + i) * step;

                    R = pixels_RGB[layer + 2] / 255.0;
                    G = pixels_RGB[layer + 1] / 255.0;
                    B = pixels_RGB[layer] / 255.0;

                    double minRGB = Math.Min(R, Math.Min(G, B));
                    double maxRGB = Math.Max(R, Math.Max(G, B));

                    if (maxRGB == minRGB)
                    {
                        H = 0; S = 0; L = 0;
                    }

                    else
                    {
                        L = (minRGB + maxRGB) / 2;

                        double d = maxRGB - minRGB;

                        if (L <= 0.5)
                        {
                            S = d / (maxRGB + minRGB);
                        }

                        else
                        {
                            S = d / (2 - minRGB - maxRGB);
                        }

                        if (R == maxRGB)
                        {
                            H = (G - B) / d;
                        }

                        else if (G == maxRGB)
                        {
                            H = 2 + (B - R) / d;
                        }

                        else
                        {
                            H = 4 + (R - G) / d;
                        }

                        H *= 60;

                        if (H < 0)
                        {
                            H += 360;
                        }
                    }

                    pixels_RGB[layer + 2] = (byte)(H / 360 * 255);
                    pixels_RGB[layer + 1] = (byte)(S * 255);
                    pixels_RGB[layer] = (byte)(L * 255);
                }
            }
        }

        public void GrayScale(Bitmap RGBImage)
        {
            lbm_RGBImage = new LockBitmap(RGBImage);
            lbm_RGBImage.LockBits();

            step = lbm_RGBImage.step;

            pixels_RGB = lbm_RGBImage.Pixels;

            Parallel.Invoke(
                () =>
                {
                    GrayScaleThreaded(0, 0, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    GrayScaleThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                },
                () =>
                {
                    GrayScaleThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                },
                () =>
                {
                    GrayScaleThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                }
            );

            lbm_RGBImage.UnlockBits();
        }

        private void GrayScaleThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = ((j * sizeX) + i) * step;

                    int gray = (pixels_RGB[layer] + pixels_RGB[layer + 1] + pixels_RGB[layer + 2]) / 3;

                    pixels_RGB[layer] = pixels_RGB[layer + 1] = pixels_RGB[layer + 2] = (byte)gray;
                }
            }
        }

        public void BinaryScale_AbsoluteDifference(Bitmap GrayImage)
        {
            T = Transfer.TD;
            DS = Transfer.DS;

            lbm_GrayImage = new LockBitmap(GrayImage);
            lbm_GrayImage.LockBits();

            step = lbm_GrayImage.step;

            pixels_Gray = lbm_GrayImage.Pixels;

            Parallel.Invoke(
                () =>
                {
                    BinaryScale_AbsoluteDifferenceThreaded(0, 0, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    BinaryScale_AbsoluteDifferenceThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                },
                () =>
                {
                    BinaryScale_AbsoluteDifferenceThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                },
                () =>
                {
                    BinaryScale_AbsoluteDifferenceThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                }
            );

            lbm_GrayImage.UnlockBits();
        }

        private void BinaryScale_AbsoluteDifferenceThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = ((j * sizeX) + i) * step;

                    if (pixels_Gray[layer] >= T[i, j] + DS)
                    {
                        pixels_Gray[layer] = pixels_Gray[layer + 1] = pixels_Gray[layer + 2] = 255;
                    }

                    else
                    {
                        pixels_Gray[layer] = pixels_Gray[layer + 1] = pixels_Gray[layer + 2] = 0;
                    }
                }
            }
        }

        public void BinaryScale_Segmentation(Bitmap GrayImage)
        {
            T = Transfer.TS;
            DS = Transfer.DS;
            SS = Transfer.SS;

            lbm_GrayImage = new LockBitmap(GrayImage);
            lbm_GrayImage.LockBits();

            step = lbm_GrayImage.step;

            pixels_Gray = lbm_GrayImage.Pixels;

            Parallel.Invoke(
                () =>
                {
                    BinaryScale_SegmentationThreaded(0, 0, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    BinaryScale_SegmentationThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                },
                () =>
                {
                    BinaryScale_SegmentationThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                },
                () =>
                {
                    BinaryScale_SegmentationThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                }
            );

            lbm_GrayImage.UnlockBits();
        }

        private void BinaryScale_SegmentationThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = ((j * sizeX) + i) * step;

                    if (pixels_Gray[layer] >= T[i, j] + (DS - SS))
                    {
                        pixels_Gray[layer] = pixels_Gray[layer + 1] = pixels_Gray[layer + 2] = 255;
                    }

                    else
                    {
                        pixels_Gray[layer] = pixels_Gray[layer + 1] = pixels_Gray[layer + 2] = 0;
                    }
                }
            }
        }
    }
}
using System;
using System.Drawing;

namespace Opticus
{
    class Correlation
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        int sizeX, sizeY;

        double sumMean_Gray_B, sumMean_Gray_RF;
        double mean_Gray_B, mean_Gray_RF;
        double sumDeviation_Gray_B, sumDeviation_Gray_RF;
        double std_Gray_B, std_Gray_RF;
        double sumCovariance;
        double covariance;
        double PC;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        Bitmap Gray_B, Gray_RF, Binary_AD;

        LockBitmap lbm_Gray_B, lbm_Gray_RF, lbm_Binary_AD;

        /*----------------------------------------------------------------------------------------------------------*/

        public Correlation()
        {
            sizeX = Transfer.sizeX;
            sizeY = Transfer.sizeY;

            Gray_B = Transfer.Gray_B;
        }

        public void Pearson()
        {
            LoadInitializationSimilarityValue();

            LoadMeanValue();
            LoadDeviationValue();
            LoadCovarianceValue();
        }

        private void LoadInitializationSimilarityValue()
        {
            sumMean_Gray_B = 0; sumMean_Gray_RF = 0;
            mean_Gray_B = 0; mean_Gray_RF = 0;
            sumDeviation_Gray_B = 0; sumDeviation_Gray_RF = 0;
            std_Gray_B = 0; std_Gray_RF = 0;
            sumCovariance = 0;
            covariance = 0;
            PC = 0;

            Gray_RF = Transfer.Gray_RF;
            Binary_AD = Transfer.Binary_AD;
        }

        private void LoadMeanValue()
        {
            lbm_Gray_B = new LockBitmap(Gray_B);
            lbm_Gray_B.LockBits();

            lbm_Gray_RF = new LockBitmap(Gray_RF);
            lbm_Gray_RF.LockBits();

            lbm_Binary_AD = new LockBitmap(Binary_AD);
            lbm_Binary_AD.LockBits();

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    sumMean_Gray_B += lbm_Gray_B.GetPixel(i, j).R;
                    sumMean_Gray_RF += lbm_Gray_RF.GetPixel(i, j).R;
                }
            }

            lbm_Gray_B.UnlockBits();
            lbm_Gray_RF.UnlockBits();
            lbm_Binary_AD.UnlockBits();

            mean_Gray_B = sumMean_Gray_B / (sizeX * sizeY);
            mean_Gray_RF = sumMean_Gray_RF / (sizeX * sizeY);
        }

        private void LoadDeviationValue()
        {
            lbm_Gray_B = new LockBitmap(Gray_B);
            lbm_Gray_B.LockBits();

            lbm_Gray_RF = new LockBitmap(Gray_RF);
            lbm_Gray_RF.LockBits();

            lbm_Binary_AD = new LockBitmap(Binary_AD);
            lbm_Binary_AD.LockBits();

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    sumDeviation_Gray_B += Math.Pow((lbm_Gray_B.GetPixel(i, j).R - mean_Gray_B), 2);
                    sumDeviation_Gray_RF += Math.Pow((lbm_Gray_RF.GetPixel(i, j).R - mean_Gray_RF), 2);
                }
            }

            lbm_Gray_B.UnlockBits();
            lbm_Gray_RF.UnlockBits();
            lbm_Binary_AD.UnlockBits();

            std_Gray_B = Math.Sqrt(sumDeviation_Gray_B / (sizeX * sizeY - 1));
            std_Gray_RF = Math.Sqrt(sumDeviation_Gray_RF / (sizeX * sizeY - 1));
        }

        private void LoadCovarianceValue()
        {
            lbm_Gray_B = new LockBitmap(Gray_B);
            lbm_Gray_B.LockBits();

            lbm_Gray_RF = new LockBitmap(Gray_RF);
            lbm_Gray_RF.LockBits();

            lbm_Binary_AD = new LockBitmap(Binary_AD);
            lbm_Binary_AD.LockBits();

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    sumCovariance += (lbm_Gray_B.GetPixel(i, j).R - mean_Gray_B) *
                                     (lbm_Gray_RF.GetPixel(i, j).R - mean_Gray_RF);
                }
            }

            lbm_Gray_B.UnlockBits();
            lbm_Gray_RF.UnlockBits();
            lbm_Binary_AD.UnlockBits();

            covariance = Math.Abs(sumCovariance / (sizeX * sizeY - 1));

            PC = (covariance / (std_Gray_B * std_Gray_RF)) * 100;

            Transfer.PC = PC;
        }
    }
}
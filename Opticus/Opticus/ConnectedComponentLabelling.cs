using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Opticus
{
    class ConnectedComponentLabelling : IConnectedComponentLabelling
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        private int _width;
        private int _height;

        private int[,] _board;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        private Bitmap _input;

        public Dictionary<int, List<Pixel>> patterns;

        /*----------------------------------------------------------------------------------------------------------*/

        public IDictionary<int, Bitmap> Process(Bitmap input)
        {
            _input = input;
            _width = input.Width;
            _height = input.Height;
            _board = new int[_width, _height];

            patterns = Find();

            var images = new Dictionary<int, Bitmap>();

            foreach (KeyValuePair<int, List<Pixel>> pattern in patterns)
            {
                Bitmap bmp = CreateBitmap(pattern.Value);

                images.Add(pattern.Key, bmp);
            }

            return images;
        }

        private Bitmap CreateBitmap(List<Pixel> pattern)
        {
            int minX = pattern.Min(p => p.Position.X);
            int maxX = pattern.Max(p => p.Position.X);

            int minY = pattern.Min(p => p.Position.Y);
            int maxY = pattern.Max(p => p.Position.Y);

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            var bmp = new Bitmap(width, height);

            foreach (Pixel pix in pattern)
            {
                bmp.SetPixel(pix.Position.X - minX, pix.Position.Y - minY, pix.color);
            }

            return bmp;
        }

        protected virtual bool CheckIsBackGround(Pixel currentPixel)
        {
            return currentPixel.color.A == 255 && currentPixel.color.R == 0;
        }

        private Dictionary<int, List<Pixel>> Find()
        {
            int labelCount = 1;

            var allLabels = new Dictionary<int, ILabel>();

            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    Pixel currentPixel = new Pixel(new Point(j, i), _input.GetPixel(j, i));

                    if (CheckIsBackGround(currentPixel))
                    {
                        continue;
                    }

                    IEnumerable<int> neighboringLabels = GetNeighboringLabels(currentPixel);

                    int currentLabel;

                    if (!neighboringLabels.Any())
                    {
                        currentLabel = labelCount;

                        allLabels.Add(currentLabel, new ILabel(currentLabel));

                        labelCount++;
                    }

                    else
                    {
                        currentLabel = neighboringLabels.Min(n => allLabels[n].GetRoot().Name);

                        ILabel root = allLabels[currentLabel].GetRoot();

                        foreach (var neighbor in neighboringLabels)
                        {
                            if (root.Name != allLabels[neighbor].GetRoot().Name)
                            {
                                allLabels[neighbor].Join(allLabels[currentLabel]);
                            }
                        }
                    }

                    _board[j, i] = currentLabel;
                }
            }

            Dictionary<int, List<Pixel>> patterns = AggregatePatterns(allLabels);

            return patterns;
        }

        private IEnumerable<int> GetNeighboringLabels(Pixel pix)
        {
            var neighboringLabels = new List<int>();

            for (int i = pix.Position.Y - 1; i <= pix.Position.Y + 2 && i < _height - 1; i++)
            {
                for (int j = pix.Position.X - 1; j <= pix.Position.X + 2 && j < _width - 1; j++)
                {
                    if (i > -1 && j > -1 && _board[j, i] != 0)
                    {
                        neighboringLabels.Add(_board[j, i]);
                    }
                }
            }

            return neighboringLabels;
        }

        private Dictionary<int, List<Pixel>> AggregatePatterns(Dictionary<int, ILabel> allLabels)
        {
            var patterns = new Dictionary<int, List<Pixel>>();

            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    int patternNumber = _board[j, i];

                    if (patternNumber != 0)
                    {
                        patternNumber = allLabels[patternNumber].GetRoot().Name;

                        if (!patterns.ContainsKey(patternNumber))
                        {
                            patterns[patternNumber] = new List<Pixel>();
                        }

                        patterns[patternNumber].Add(new Pixel(new Point(j, i), Color.White));
                    }
                }
            }

            return patterns;
        }
    }
}
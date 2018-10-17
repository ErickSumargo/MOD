using System;
using System.Collections.Generic;
using System.Drawing;

namespace Opticus
{
    public interface IConnectedComponentLabelling
    {
        IDictionary<int, Bitmap> Process(Bitmap input);
    }
}

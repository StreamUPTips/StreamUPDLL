using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
    public int CropCalculation(int maxSourceWidth, double current, double goal)
    {
        double goalPercent = current / goal;
        double cropPercent = maxSourceWidth * goalPercent;
        int cropValue = maxSourceWidth - (int)cropPercent;
        
        return  cropValue < 0 ? 0 : cropValue;
    }

    }
}
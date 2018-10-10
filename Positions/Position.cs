using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiiWandz.Positions
{
    public class Position
    {
        public WiimoteInfo.Point point;
        public DateTime time;

        public Position(WiimoteInfo.Point point, DateTime time)
        {
            this.point = point;
            this.time = time;
        }

        public System.Drawing.Point SystemPoint()
        {
            return new System.Drawing.Point((1023-point.X)/4, (760-point.Y)/4);
        }

    }
}

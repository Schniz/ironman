using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    class MedianStuff
    {
        Point lastAveragePoint = Point.Empty;
        List<Point> points = new List<Point>();

        public Point add(Point point) {
            if (points.Count < 5) {
                this.points.Add(point);
                return lastAveragePoint;
            };

            int x = 0;
            int y = 0;

            foreach (Point pt in points)
            {
                x += pt.X;
                y += pt.Y;
            }

            x = x / points.Count;
            y = y / points.Count;

            this.lastAveragePoint = new Point
            {
                X = x,
                Y = y
            };

            this.points.Clear();
            
            return lastAveragePoint;
        }
    }
}

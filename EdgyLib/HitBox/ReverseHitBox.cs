﻿using System;
using Aardvark.Base;
using Emgu.CV;
using Emgu.CV.Structure;
using Geometry.Base;
using RenderGeometry.Base;
using Vector = FlightLib.Vector;

namespace EdgyLib.HitBox
{
    internal class ReverseHitBox : IHit
    {
        protected Polygon InternalPolygon;

        public ReverseHitBox(int width, int height, double centerWidth = 0.065, double centerHeight = 0.2,
            double centerHeightOffset = 0.25)
        {
            Width = width;
            Height = height;

            double x1 = width * (1 - (1 - centerWidth) / 2);
            double y1 = height * ((1 - centerHeight) / 2 - centerHeightOffset);
            double x2 = width * ((1 - centerWidth) / 2);
            double y2 = height * (1 - (1 - centerHeight) / 2 - centerHeightOffset);

            InternalPolygon = new Polygon(
                new V2d(x1, y1),
                new V2d(x2, y1),
                new V2d(x2, y2),
                new V2d(x1, y2),
                new V2d(x1, y1)
            );
        }

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public bool Hit(Point point, Vector vector)
        {
            if (InternalPolygon.Contains(point))
            {
                return false;
            }

            if (0 <= point.X && point.X <= Width)
            {
                vector.Yaw = HorizontalMovement(point.X) * 2;
            }

            return true;
        }

        public void Render(Image<Bgr, byte> frame)
        {
            ((PolyRender) InternalPolygon).Render(frame);
        }

        protected int HorizontalMovement(double movement)
        {
            double center = (double) Width / 2;
            int force = (int) Math.Round((center - movement) / center * 100, 0);

            return -force.Clamp(-100, 100);
        }
    }
}
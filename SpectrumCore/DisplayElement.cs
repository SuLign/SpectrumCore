using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SpectrumCore
{
    public class DisplayElement
    {
        private Pen pen;
        private double[] points;

        /// <summary>
        /// The pen
        /// </summary>
        public Pen Pen { get => pen; set => pen = value; }

        public double[] Points { get => points; set => points = value; }
    }
}

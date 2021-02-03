using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpectrumCore
{
    /// <summary>
    /// WaterFallGraphics.xaml
    /// </summary>
    public partial class WaterFallGraphics : UserControl
    {
        private List<DisplayElement> lastElements;

        private double endFrequency;
        private double startAmplite;
        private double startFrequency;
        private double endAmplite;

        private double startPoint;
        private double endPoint;

        private int dataFrame;
        private PointF fixPoint = new PointF(0, 0);
        private double displayResolutionX;
        private double displayResolutionY;

        private double zoomSizeX = 1;
        private double origionZoomSizeX = 1;
        private double zoomSizeY = 1;
        private double totalBandwidth;

        private double spectrumResolution;

        #region Properties
        public static readonly DependencyProperty MaxAmpliteProperty = DependencyProperty.Register("MaxAmplite", typeof(double), typeof(WaterFallGraphics), new PropertyMetadata(100000.0));
        public static readonly DependencyProperty BackGroundProperty = DependencyProperty.Register("BackGround", typeof(System.Windows.Media.Color), typeof(WaterFallGraphics), new PropertyMetadata(System.Windows.Media.Colors.Transparent));

        public double MaxAmplite { get => (double)GetValue(MaxAmpliteProperty); set => SetValue(MaxAmpliteProperty, value); }
        public double StartFrequency { get => startFrequency; set => startFrequency = value; }
        public double StartAmplite { get => startAmplite; set => startAmplite = value; }
        public double EndFrequency { get => endFrequency; set => endFrequency = value; }
        public double EndAmplite { get => endAmplite; set => endAmplite = value; }
        public Pen BackGround { get => new Pen((System.Drawing.Color.FromArgb(((Color)GetValue(BackGroundProperty)).A, ((Color)GetValue(BackGroundProperty)).R, ((Color)GetValue(BackGroundProperty)).G, ((Color)GetValue(BackGroundProperty)).B))); set => SetValue(BackGroundProperty, System.Windows.Media.Color.FromArgb(((Pen)value).Color.A, ((Pen)value).Color.R, ((Pen)value).Color.G, ((Pen)value).Color.B)); }
        public double ZoomSize { get => zoomSizeX; set => zoomSizeX = value; }
        #endregion

        #region event
        //public new event MouseEventHandler MouseMove;
        //public new event MouseButtonEventHandler Drop;

        #endregion

        public WaterFallGraphics()
        {
            InitializeComponent();
        }

        #region Override
        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            displayResolutionX = (float)(this.ActualWidth) / dataFrame;
            displayResolutionY = (float)(this.ActualHeight) / MaxAmplite;
            startPoint = 0;
            endPoint = this.ActualWidth;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            fixPoint = new PointF((float)Mouse.GetPosition(this).X, (float)Mouse.GetPosition(this).Y);
            if (displayResolutionX * dataFrame * zoomSizeX + startPoint < this.ActualWidth) fixPoint = new PointF((float)this.ActualWidth, (float)Mouse.GetPosition(this).Y);
            origionZoomSizeX = zoomSizeX;
            var value = zoomSizeX * (e.Delta > 0 ? 1.1 : (1 / 1.1));
            zoomSizeX = value > 400 ? 400 : (value < 1 ? 1 : value);
            if (zoomSizeX == 1) startPoint = 0;
            else startPoint = startPoint * zoomSizeX / origionZoomSizeX + fixPoint.X * (1 - (zoomSizeX / origionZoomSizeX));
            DrawElements(lastElements.ToArray());
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }
        #endregion

        public void Initialize(int startXAxis, int endXAxis, int startYAxis, int endYAxis, int frameLength)
        {
            this.startFrequency = startXAxis;
            this.endFrequency = endXAxis;
            this.startAmplite = startYAxis;
            this.EndAmplite = endYAxis;

            totalBandwidth = endXAxis - startXAxis;
            spectrumResolution = totalBandwidth / frameLength;

            dataFrame = frameLength;
        }

        /// <summary>
        /// Draw the elements on the board.
        /// </summary>
        /// <param name="elements">Elements to draw.</param>
        public void DrawElements(params DisplayElement[] elements)
        {
            if (displayResolutionX == 0 || displayResolutionX == double.NaN) return;
            (lastElements ??= new List<DisplayElement>()).Clear();
            lastElements.AddRange(elements);
            SpecCore.DrawOnMap((graphics) =>
            {
                for (int index = 0; index < elements.Length; index++)
                {
                    List<PointF> pointFs = new List<PointF>();
                    for (long i = 0; i < elements[index].Points.Length; i++)
                    {
                        pointFs.Add(GetPointF(i, (float)elements[index].Points[i]));
                    }
                    graphics.DrawLines(elements[index].Pen, pointFs.ToArray());
                }
            });
        }

        private System.Drawing.PointF GetPointF(long index, float value)
        {
            var X = displayResolutionX * index * zoomSizeX + startPoint;
            var Y = displayResolutionY * value * zoomSizeY + fixPoint.Y * (1 - zoomSizeY);
            return new System.Drawing.PointF((float)X, (float)Y);
        }
    }
}

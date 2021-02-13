using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

using System.ComponentModel;
using SpectrumCore;
using SpectrumCore.Utils;
using System.Windows.Input;
using System.Windows.Controls;

namespace Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BindContent _content;
        CancellationTokenSource _tokenSource;

        [DllImport("SignalGenerator.dll", CallingConvention = CallingConvention.Cdecl)]
        internal extern static IntPtr CreateInstance(int count, int Fs);

        [DllImport("SignalGenerator.dll", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void GenerateData(IntPtr handle, ulong centFreq, int amp, ref IntPtr dataResult);

        [DllImport("SignalGenerator.dll", CallingConvention = CallingConvention.Cdecl)]
        internal extern static void Destory(IntPtr handle);


        public MainWindow()
        {
            _content = new BindContent();
            InitializeComponent();
            this.DataContext = _content;
            _tokenSource = new CancellationTokenSource();

            #region Display text
            DoFFTTestAsync();
            #endregion
        }

        private void DoFFTTestAsync()
        {
            Task t = Task.Run(() =>
            {
                int length = (int)Math.Pow(2, 15);
                var datas = new ConcurrentQueue<double[]>();
                var dat = IntPtr.Zero;
                IntPtr handle = CreateInstance(length, 102400000);
                //ulong startFr = 1;
                Task.Run(() =>
                {
                    while (!_tokenSource.IsCancellationRequested)
                    {
                        GenerateData(handle, ulong.Parse(_content.SyncFrequency), 1, ref dat);
                        var SignalDatas = new double[length * 2];
                        Marshal.Copy(dat, SignalDatas, 0, SignalDatas.Length);
                        FftSharp.Window.Apply(FftSharp.Window.Hanning(SignalDatas.Length), SignalDatas);
                        var result = FftSharp.Transform.FFTpower(SignalDatas);
                        Array.Resize(ref result, result.Length / 2);
                        datas.Enqueue(result);
                        Thread.Sleep(120);
                    }
                });
                Grapicpannel.Initialize(0, 50000000, 0, 400, length / 2);
                DisplayElement spectrum;
                Task.Run(async () =>
                {
                    while (!_tokenSource.IsCancellationRequested)
                    {
                        datas.TryDequeue(out double[] data);
                        if (data == null) continue;
                        data[data.Length / 2] *= 3;
                        //#region Median filter
                        //int handleDataLength = 71;
                        ////var slideWindow = new double[handleDataLength];

                        //double maxValue;
                        //double minValue;
                        //double slideSum = 0;
                        //double sum;
                        //var medianFilteringData = new double[data.Length];
                        //for (int position = 0; position < (int)handleDataLength / 2; position++)
                        //{
                        //    medianFilteringData[position] = data[position];
                        //    medianFilteringData[data.Length - 1 - position] = data[data.Length - 1 - position];
                        //}

                        //for (int position = (int)handleDataLength / 2; position < data.Length - Math.Ceiling(handleDataLength / 2.0); position++)
                        //{
                        //    sum = 0;
                        //    slideSum -= Math.Abs(data[position - (int)handleDataLength / 2] - medianFilteringData[position - (int)handleDataLength / 2]);
                        //    slideSum += Math.Abs(data[position - 1] - medianFilteringData[position - 1]);
                        //    maxValue = data[position];
                        //    minValue = data[position];
                        //    for (int index = (int)(position - handleDataLength / 2.0); index < position + handleDataLength / 2 + 1; index++)
                        //    {
                        //        //slideWindow[index + (int)(position - Math.Ceiling(handleDataLength / 2.0))] = data[index];
                        //        if (data[index] > maxValue) maxValue = data[index];
                        //        else if (data[index] < minValue) minValue = data[index];
                        //        sum += data[index];
                        //    }
                        //    //medianFilteringData[position] = (sum - maxValue - minValue) / (handleDataLength - 2) + slideSum / (handleDataLength / 2.0 + 1);
                        //    //Array.Sort(slideWindow);
                        //    //medianFilteringData[position] = slideWindow[slideWindow.Length / 2];s
                        //}

                        //#endregion

                        //var thr = new DisplayElement()
                        //{
                        //    Pen = Pens.LightBlue,
                        //    Points = medianFilteringData,
                        //};

                        spectrum = new DisplayElement()
                        {
                            Pen = Pens.Yellow,
                            Points = data
                        };
                        Grapicpannel.DrawElements(spectrum/*, thr*/);
                        await Task.Delay(100);
                    }
                });
            });
        }

        private class BindContent : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private string _infoLog = "";
            private string _syncFrequency = "15000000";

            public string InfoLog
            {
                get => _infoLog;
                set { _infoLog = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InfoLog")); }
            }

            public string SyncFrequency
            {
                get => _syncFrequency;
                set { _syncFrequency = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SyncFrequency")); }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;
        const int HTCAPTION = 0x0002;
        public void MoveForm(object sender, MouseButtonEventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            ReleaseCapture();
            SendMessage(hwnd, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var editor = sender as TextBox;
            _content.SyncFrequency = editor.Text;
        }
    }
}

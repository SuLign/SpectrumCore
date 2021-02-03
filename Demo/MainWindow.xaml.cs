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
                IntPtr handle = CreateInstance(32768, 51200000);
                Task.Run(() =>
                {
                    while (!_tokenSource.IsCancellationRequested)
                    {
                        GenerateData(handle, 150000, 2, ref dat);
                        var SignalDatas = new double[65536];
                        Marshal.Copy(dat, SignalDatas, 0, SignalDatas.Length);
                        var result = FastFourierTransform.DoastFourierTransformRealAsync(SignalDatas).Result;
                        //result = FastFourierTransform.DoastFourierTransformRealAsync(result).Result;
                        datas.Enqueue(result);
                    }
                });
                Grapicpannel.Initialize(0, 50000000, 0, 400, length);
                DisplayElement spectrum;
                Task.Run(async () =>
                {
                    while (!_tokenSource.IsCancellationRequested)
                    {
                        datas.TryDequeue(out double[] data);
                        if (data == null) continue;
                        data[data.Length / 2] *= 3;
                        #region Median filter
                        int handleDataLength = 71;

                        double maxValue;
                        double minValue;
                        double slideSum = 0;
                        double sum;
                        var medianFilteringData = new double[data.Length];
                        for (int position = 0; position < (int)handleDataLength / 2; position++)
                        {
                            medianFilteringData[position] = data[position];
                            medianFilteringData[data.Length - 1 - position] = data[data.Length - 1 - position];
                        }

                        for (int position = (int)handleDataLength / 2; position < data.Length - Math.Ceiling(handleDataLength / 2.0); position++)
                        {
                            sum = 0;
                            slideSum -= Math.Abs(data[position - (int)handleDataLength / 2] - medianFilteringData[position - (int)handleDataLength / 2]);
                            slideSum += Math.Abs(data[position - 1] - medianFilteringData[position - 1]);
                            maxValue = data[position];
                            minValue = data[position];
                            for (int index = (int)(position - handleDataLength / 2.0); index < position + handleDataLength / 2 + 1; index++)
                            {
                                if (data[index] > maxValue) maxValue = data[index];
                                else if (data[index] < minValue) minValue = data[index];
                                sum += data[index];
                            }
                            medianFilteringData[position] = (sum - maxValue - minValue) / (handleDataLength - 2) + slideSum / (handleDataLength / 2.0 + 1);
                        }
                        
                        #endregion
                        spectrum = new DisplayElement()
                        {
                            Pen = Pens.Yellow,
                            Points = data
                        };

                        var thr = new DisplayElement()
                        {
                            Pen = Pens.LightBlue,
                            Points = medianFilteringData,
                        };
                        Grapicpannel.DrawElements(spectrum, thr);
                        await Task.Delay(1);
                    }
                });
            });
        }

        private class BindContent : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string _infoLog = "";

            public string InfoLog
            {
                get => _infoLog;
                set { _infoLog = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InfoLog")); }
            }
        }
    }
}

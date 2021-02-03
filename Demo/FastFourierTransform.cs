using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    public class FastFourierTransform
    {
        public static async Task<Complex[]> DoastFourierTransformAsync(double[] Data)
        {
            if (Data.Length == 1 || (Data.Length & (Data.Length - 1)) == 0)
            {
                var _result = new Complex[Data.Length];
                int _length = Data.Length;
                //copy input to output 
                var image = new double();
                for (int n = 0; n < Data.Length; n++) _result[n] = new Complex(Data[n], image);
                //rearrange input sequence - fft butterfly (bit reverse)
                //Example (8 points = 3 bits -> 2^3 = 8)
                //input order  => 0 1 2 3 4 5 6 7
                //output order => 0 4 2 6 1 5 3 7
                //Index   Index-binary (input)   Index-binary (output)
                //----------------------------------------------------
                //  0         000                    000
                //  1         001                    100
                //  2         010                    010
                //  3         011                    110
                //  4         100                    001
                //  5         101                    101
                //  6         110                    011
                //  7         111                    111
                int i = 1;
                for (int n = 1; n < _length - 1; n++)
                {
                    if (n < i)
                    {
                        var _tmp = _result[i - 1];
                        _result[i - 1] = _result[n - 1];
                        _result[n - 1] = _tmp;
                    }
                    int _half = _length >> 1;
                    while (_half < i)
                    {
                        i -= _half;
                        _half >>= 1;
                    }
                    i += _half;
                }
                //step count; while-loop is faster than log2(n)
                int _stepCount = 0;
                int _step = _length;
                while (_step > 1)
                {
                    _step >>= 1;
                    _stepCount++;
                }
                //FFT
                Task task = Task.Run(() =>
                {
                    Parallel.For(0, _stepCount - 1, (n) =>
                    {
                        int _s = 1 << n;
                        double _sr = 1.0;
                        double _si = 0.0;
                        double _wr = Math.Cos(Math.PI / _s);
                        double _wi = -Math.Sin(Math.PI / _s);
                        for (int m = 0; m < _s; m++)
                        {
                            int _k = m;
                            while (_k < _length)
                            {
                                Complex _tmp = Complex.Zero;
                                _tmp = new Complex(_result[_k + _s].Real * _sr - _result[_k + _s].Imaginary * _si, _result[_k + _s].Imaginary * _sr + _result[_k + _s].Real * _si);
                                _result[_k + _s] = new Complex(_result[_k].Real - _tmp.Real, _result[_k].Imaginary - _tmp.Imaginary);
                                _result[_k] = new Complex(_result[_k].Real + _tmp.Real, _result[_k].Imaginary + _tmp.Imaginary);
                                _k += (_s << 1);
                            }
                            double _tmpd = _sr * _wr - _si * _wi;
                            _si = _si * _wr + _sr * _wi;
                            _sr = _tmpd;
                        }
                    });
                });
                await task;
                return _result;
            }
            throw new ArgumentOutOfRangeException("FFT Length must be Power of Two!");
        }

        public static async Task<double[]> DoastFourierTransformRealAsync(double[] Data)
        {
            if (Data.Length == 1 || (Data.Length & (Data.Length - 1)) == 0)
            {
                var _result = new Complex[Data.Length];
                var _data = new double[Data.Length];
                int _length = Data.Length;
                //copy input to output 
                var image = new double();
                for (int n = 0; n < Data.Length; n++) _result[n] = new Complex(Data[n], image);
                //rearrange input sequence - fft butterfly (bit reverse)
                //Example (8 points = 3 bits -> 2^3 = 8)
                //input order  => 0 1 2 3 4 5 6 7
                //output order => 0 4 2 6 1 5 3 7
                //Index   Index-binary (input)   Index-binary (output)
                //----------------------------------------------------
                //  0         000                    000
                //  1         001                    100
                //  2         010                    010
                //  3         011                    110
                //  4         100                    001
                //  5         101                    101
                //  6         110                    011
                //  7         111                    111
                int i = 1;
                for (int n = 1; n < _length - 1; n++)
                {
                    if (n < i)
                    {
                        var _tmp = _result[i - 1];
                        _result[i - 1] = _result[n - 1];
                        _result[n - 1] = _tmp;
                    }
                    int _half = _length >> 1;
                    while (_half < i)
                    {
                        i -= _half;
                        _half >>= 1;
                    }
                    i += _half;
                }
                //step count; while-loop is faster than log2(n)
                int _stepCount = 0;
                int _step = _length;
                while (_step > 1)
                {
                    _step >>= 1;
                    _stepCount++;
                }
                //FFT
                Task task = Task.Run(() =>
                {
                    Parallel.For(0, _stepCount - 1, (n) =>
                    {
                        int _s = 1 << n;
                        double _sr = 1.0;
                        double _si = 0.0;
                        double _wr = Math.Cos(Math.PI / _s);
                        double _wi = -Math.Sin(Math.PI / _s);
                        for (int m = 0; m < _s; m++)
                        {
                            int _k = m;
                            while (_k < _length)
                            {
                                Complex _tmp = Complex.Zero;
                                _tmp = new Complex(_result[_k + _s].Real * _sr - _result[_k + _s].Imaginary * _si, _result[_k + _s].Imaginary * _sr + _result[_k + _s].Real * _si);
                                _result[_k + _s] = new Complex(_result[_k].Real - _tmp.Real, _result[_k].Imaginary - _tmp.Imaginary);
                                _data[_k + _s] = _result[_k + _s].Real;
                                _result[_k] = new Complex(_result[_k].Real + _tmp.Real, _result[_k].Imaginary + _tmp.Imaginary);
                                _data[_k] = _result[_k].Real;
                                _k += (_s << 1);
                            }
                            double _tmpd = _sr * _wr - _si * _wi;
                            _si = _si * _wr + _sr * _wi;
                            _sr = _tmpd;
                        }
                    });
                });
                await task;
                return _data;
            }
            throw new ArgumentOutOfRangeException("FFT Length must be Power of Two!");
        }
    }
}

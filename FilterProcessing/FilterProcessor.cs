﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterProcessing
{
    public class FilterProcessor
    {
        private double[][] _matrix;
        private bool _separation;

        public FilterProcessor(double[][] matrix, bool separation = false)
        {
            if (!separation) if (!CheckMatrixIsCorrect(matrix)) throw new ArgumentException("Некорректная матрица"); else ;
            else if (matrix.Length != 1) throw new ArgumentException("Некорректная матрица");
            _matrix = matrix;
            _separation = separation;
        }

        public FilterProcessor()
        {
            _matrix = null;
            _separation = false;
        }

        public bool UseSeparation { get { return _separation; } set { _separation = value; } }

        public bool SetMatrix(double[][] matrix)
        {
            if (CheckMatrixIsCorrect(matrix))
                return (_matrix = matrix) == _matrix;
            return false;
        }

        private bool CheckMatrixIsCorrect(double[][] matrix)
        {
            if (matrix.Length == 0) return false;
            for (int i = 0; i < matrix.Length; ++i) if (matrix.Length != matrix[i].Length) return false;
            return true;
        }

        public System.Drawing.Bitmap Process(System.Drawing.Bitmap original)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, original.Width, original.Height);
            System.Drawing.Imaging.BitmapData bmpData = original.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, original.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] values = new byte[bytes];
            byte[] newValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            if (!_separation)
            {
                for (int y = 0; y < bmpData.Height; ++y)
                    for (int x = 0; x < bmpData.Width; ++x)
                    {
                        int index = GetIndex(x, y, bmpData.Width);
                        double r = 0, g = 0, b = 0;
                        double sum = 0;
                        for (int j = -_matrix.Length / 2; j <= _matrix.Length/2; ++j)
                            for (int i = -_matrix.Length / 2; i <= _matrix.Length / 2; ++i)
                            {
                                if ((x + i < 0) || (x + i >= bmpData.Width) || (y + j < 0) || (y + j >= bmpData.Height)) continue;
                                int matrIndex = GetIndex(x + i, y + j, bmpData.Width);
                                double weight = _matrix[i + _matrix.Length / 2][j + _matrix.Length / 2];
                                r += values[matrIndex + 2] * weight;
                                g += values[matrIndex + 1] * weight;
                                b += values[matrIndex + 0] * weight;
                                sum += weight;
                            }

                        if (sum == 0) sum = 1;
                        r /= sum;
                        g /= sum;
                        b /= sum;

                        newValues[index + 3] = (byte)255;
                        newValues[index + 2] = (byte)r;
                        newValues[index + 1] = (byte)g;
                        newValues[index + 0] = (byte)b;
                    }
            }
            else
            {

            }

            System.Runtime.InteropServices.Marshal.Copy(newValues, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
        }

        int GetIndex(int x, int y, int width)
        {
            return (x + y * width) * 4;
        }
    }
}

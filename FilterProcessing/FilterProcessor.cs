using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilterProcessing
{
    public class FilterProcessor
    {
        private double[][] _matrix;
        private bool _separation;
        private int _channelCount;

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

        private bool CheckMatrixIsCorrect(double[][] matrix)
        {
            if (matrix.Length == 0) return false;
            for (int i = 0; i < matrix.Length; ++i) if (matrix.Length != matrix[i].Length) return false;
            if (matrix.Length % 2 != 1) return false;
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

            _channelCount = values.Length / (bmpData.Width * bmpData.Height); // 3 либо 4
            
            // Дальше многа букафф, но я не смог придумать, как это записать более красиво без введения ещё кучи параметров
            if (!_separation)
            {
                // обычная квадратная матрица без особых неожиданностей. То, что она квадратная, гарантирует конструктор/свойство
                for (int y = 0; y < bmpData.Height; ++y)
                    for (int x = 0; x < bmpData.Width; ++x)
                    {
                        int index = GetIndex(x, y, bmpData.Width); // куды вставлять
                        double r = 0, g = 0, b = 0, sum = 0;
                        for (int j = -_matrix.Length / 2; j <= _matrix.Length/2; ++j)
                            for (int i = -_matrix.Length / 2; i <= _matrix.Length / 2; ++i)
                            {
                                // Идем по ядру. Если пиксель находится за пределами изображение, то я ничего не отражаю, не заворачиваю, а тупо игнорирую
                                // Ну, не совсем тупо. Коэффициент нормировки пересчитывается на такой случай.
                                // Вывод -- не нужно подавать на вход уже отнормированную руками матрицу. Не надо. Получится не совсем то :)
                                // Т.е для box-фильтра нужно подать именно матрицу из единиц, а не из 1/9

                                if ((x + i < 0) || (x + i >= bmpData.Width) || (y + j < 0) || (y + j >= bmpData.Height)) continue;
                                int matrIndex = GetIndex(x + i, y + j, bmpData.Width); // влияющий сейчас пиксель
                                double weight = _matrix[i + _matrix.Length / 2][j + _matrix.Length / 2];
                                SumValues(values, ref sum, ref r, ref g, ref b, matrIndex, weight);
                            }
                        NormalizeValues(ref r, ref g, ref b, ref sum); // там нормировка
                        WriteNewValues(newValues, index, r, g, b);
                    }
            }
            else
            {
                // сепарабельный фильтр
                byte[] tmpValues = new byte[bytes];

                // проход по строкам
                for (int y = 0; y < bmpData.Height; ++y)
                {
                    for (int x = 0; x < bmpData.Width; ++x)
                    {
                        int index = GetIndex(x, y, bmpData.Width);
                        double sum = 0, r = 0, g = 0, b = 0;
                        for (int k = -_matrix[0].Length / 2; k < _matrix[0].Length / 2; ++k) // здесь матрица суть вектор, инфа 100%
                        {
                            int i = x + k;
                            if ((i < 0) || (i >= bmpData.Width)) continue;
                            int matrIndex = GetIndex(i, y, bmpData.Width);
                            double weight = _matrix[0][k + _matrix[0].Length / 2];
                            SumValues(values, ref sum, ref r, ref g, ref b, matrIndex, weight);
                        }
                        NormalizeValues(ref r, ref g, ref b, ref sum);
                        WriteNewValues(tmpValues, index, r, g, b);
                    }
                }

                // проход по столбцам
                for (int x = 0; x < bmpData.Width; ++x)
                    for (int y = 0; y < bmpData.Height; ++y)
                    {
                        int index = GetIndex(x, y, bmpData.Width);
                        double sum = 0, r = 0, g = 0, b = 0;
                        for (int k = -_matrix[0].Length / 2; k < _matrix[0].Length / 2; ++k)
                        {
                            int i = y + k;
                            if ((i < 0) || (i >= bmpData.Height)) continue;
                            int matrIndex = GetIndex(x, i, bmpData.Width);
                            double weight = _matrix[0][k + _matrix[0].Length / 2];
                            SumValues(tmpValues, ref sum, ref r, ref g, ref b, matrIndex, weight);
                        }
                        NormalizeValues(ref r, ref g, ref b, ref sum);
                        WriteNewValues(newValues, index, r, g, b);
                    }
            }

            System.Runtime.InteropServices.Marshal.Copy(newValues, 0, ptr, bytes);
            original.UnlockBits(bmpData);
            return original;
        }

        private void WriteNewValues(byte[] newValues, int index, double r, double g, double b)
        {
            if (index + 2 >= newValues.Length) return;
            if (_channelCount == 4) newValues[index + 3] = (byte)255;
            newValues[index + 2] = (byte)r;
            newValues[index + 1] = (byte)g;
            newValues[index + 0] = (byte)b;
        }

        private void NormalizeValues(ref double r, ref double g, ref double b, ref double sum)
        {
            if (sum == 0) sum = 1;
            r /= sum;
            g /= sum;
            b /= sum;
        }

        private void SumValues(byte[] values, ref double sum, ref double r, ref double g, ref double b, int matrIndex, double weight)
        {
            if (matrIndex + 2 >= values.Length) return;
            r += values[matrIndex + 2] * weight;
            g += values[matrIndex + 1] * weight;
            b += values[matrIndex + 0] * weight;
            sum += weight;
        }

        int GetIndex(int x, int y, int width)
        {
            return (x + y * width) * _channelCount;
        }
    }
}

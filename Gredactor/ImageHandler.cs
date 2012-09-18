﻿using System;
using System.Drawing;
using System.IO;

namespace Gredactor
{
    /// <summary>
    /// Хранит и обрабатывает изображение, также занимается выделение (зона интереса)
    /// </summary>
    public class ImageHandler
    {
        /// <summary>
        /// В любой момент времени работа идет только с одним изображением. Паттерн "singletone"
        /// </summary>
        private static ImageHandler instance;

        /// <summary>
        /// Текущее изображение
        /// </summary>
        private Bitmap _currentImage;

        /// <summary>
        /// Зона интереса представлена как отдельное изображение для простоты обработки
        /// </summary>
        private Bitmap _selection;

        /// <summary>
        /// Откуда было открыто изображение, чтоб потом туда сохранить
        /// </summary>
        private string _filename;

        private System.Collections.Stack _undoStack;

        private bool _changed;

        /// <summary>
        /// Изображение было изменено
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Возвращает рабочий экземпляр
        /// </summary>
        /// <returns></returns>
        public static ImageHandler GetInstanse()
        {
            if (instance == null)
            {
                instance = new ImageHandler();
            }
            return instance;
        }

        private ImageHandler()
        {
            _undoStack = new System.Collections.Stack();
            _changed = false;
        }

        /// <summary>
        /// Сбрасывает все в дефолт
        /// </summary>
        /// Вообще писалось для тестов, ну да мне не жалко оставить
        public void Reset()
        {
            if (_currentImage != null)
                _currentImage.Dispose();
            if (_selection != null)
                _selection.Dispose();
            _currentImage = null;
            _selection = null;
            _filename = null;
            _undoStack.Clear();
            _changed = false;
        }

        /// <summary>
        /// Изображение, с которым происходит работа
        /// </summary>
        public Bitmap Image
        {
            get { return _currentImage; }
            set { _currentImage = value; }
        }

        /// <summary>
        /// Индикатор того, что изображение было изменено
        /// </summary>
        public bool WasChanged
        {
            get { return _changed; }
            private set
            {
                _changed = value;
                if (_changed)
                {
                    EventHandler handler = Changed;
                    if (handler != null)
                        handler(this, new EventArgs());
                }
            }
        }

        #region Selection
        /// <summary>
        /// Текущее выделение либо null
        /// </summary>
        public Bitmap Selection
        {
            get
            {
                return _selection;
            }
        }

        /// <summary>
        /// Установить выделение
        /// </summary>
        /// <param name="x">Начальная координата по горизонтали</param>
        /// <param name="y">Начальная координата по вертикали</param>
        /// <param name="width">Ширина</param>
        /// <param name="height">Высота</param>
        public void SetSelection(int x, int y, int width, int height)
        {
            SetSelection(new Rectangle(x, y, width, height));
        }
        /// <summary>
        /// Установить выделение
        /// </summary>
        /// <param name="rect">Прямоугольник выделения</param>
        public void SetSelection(Rectangle rect)
        {
            if (_currentImage == null) throw new InvalidOperationException();
            try
            {
                _selection = _currentImage.Clone(rect, _currentImage.PixelFormat);
            }
            catch (OutOfMemoryException)
            {
                throw new ArgumentOutOfRangeException("Выделение находится за границами изображения");
            }
        }

        /// <summary>
        /// Сбросить выделение
        /// </summary>
        public void ResetSelection()
        {
            if (_selection != null)
                _selection.Dispose();
            _selection = null;
        }
        #endregion

        /// <summary>
        /// Открыть файл
        /// </summary>
        /// <param name="filename"></param>
        public void Open(string filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException("Файл не найден");
            _currentImage = new Bitmap(filename);
            _filename = filename;
            _undoStack.Clear();
            _changed = false;
        }

        /// <summary>
        /// Сохранить изображение в тот же файл, из которого оно было открыто
        /// </summary>
        public void Save()
        {
            if (_filename == null) throw new InvalidOperationException();
            _currentImage.Save(_filename);
            this.WasChanged = false;
        }

        /// <summary>
        /// Сохранить изображение в файл, отличный от исходного и переключиться на него
        /// </summary>
        /// <param name="filename"></param>
        public void SaveAs(string filename)
        {
            if (_filename == null) throw new InvalidOperationException();
            _currentImage.Save(_filename = filename);
            this.WasChanged = false;
        }

        /// <summary>
        /// Выбрать нужную область, которую отдать в эффект
        /// </summary>
        /// <returns>Все изображение, либо выделенную область</returns>
        public Bitmap GetImageForEffect()
        {
            return (_selection == null) ? _currentImage : _selection;
        }

        public void ApplyEffect(IEffect e)
        {
            _undoStack.Push(_currentImage);
            _currentImage = e.Apply(GetImageForEffect());
            this.WasChanged = true;
        }

        public void Undo()
        {
            if (CanUndo)
                _currentImage = (Bitmap)_undoStack.Pop();
        }

        public bool CanUndo
        {
            get 
            {
                return _undoStack.Count > 0;
            }
        }
    }
}

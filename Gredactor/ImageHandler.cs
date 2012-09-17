using System;
using System.Drawing;
using System.IO;

namespace Gredactor
{
    public class ImageHandler
    {
        private static ImageHandler instance;

        private Bitmap _currentImage;
        private Bitmap _selection;
        private string _filename;
        private bool _changed;

        public event EventHandler Changed;

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
        }

        public void Reset()
        {
            if (_currentImage != null)
                _currentImage.Dispose();
            if (_selection != null)
                _selection.Dispose();
            _currentImage = null;
            _selection = null;
            _filename = null;
        }

        public Bitmap Image
        {
            get { return _currentImage; }
            set { _currentImage = value; }
        }

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
        public Bitmap Selection
        {
            get
            {
                return _selection;
            }
        }

        public void SetSelection(int x, int y, int width, int height)
        {
            SetSelection(new Rectangle(x, y, width, height));
        }
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

        public void ResetSelection()
        {
            if (_selection != null)
                _selection.Dispose();
            _selection = null;
        }
        #endregion

        public void Open(string filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException("Файл не найден");
            _currentImage = new Bitmap(filename);
            _filename = filename;
        }

        public void Save()
        {
            if (_filename == null) throw new InvalidOperationException();
            _currentImage.Save(_filename);
            this.WasChanged = false;
        }

        public void SaveAs(string filename)
        {
            if (_filename == null) throw new InvalidOperationException();
            _currentImage.Save(_filename = filename);
            this.WasChanged = false;
        }

        public Bitmap GetImageForEffect()
        {
            return (_selection == null) ? _currentImage : _selection;
        }
    }
}

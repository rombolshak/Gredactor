using System;
using System.Drawing;
using System.IO;

namespace Gredactor
{
    public class ImageHandler
    {
        private static ImageHandler instance;

        private Bitmap _currentImage;
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
            _currentImage = null;
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
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.Common.IO
{

    public class FileContentEnumerator : IEnumerator<string>, IEnumerable<string>
    {
        StreamReader _reader;

        string _current;

        FileInfo _fi;

        public Boolean _open = false;

        public int _count;


        public FileContentEnumerator(String filePath)
            : this(new FileInfo(filePath))
        {

        }

        public FileContentEnumerator(FileInfo fi)
        {
            _fi = fi;
            _count = Convert.ToInt32(_fi.CountLines());
            Reset();
        }

        public string Current
        {
            get
            {
                return _current;
            }
        }

        public void Dispose()
        {
            _reader.DisposeIfNotNull();
        }

        object IEnumerator.Current
        {
            get { return _current; }
        }

        public bool MoveNext()
        {
            try
            {
                _current = _reader.ReadLine();
                CurrentIteration++;
            }
            catch(ObjectDisposedException)
            {
                _current = null;
            }

            if(_current == null)
            {
                Close();
                return false;
            }
            return true;
        }

        public void Reset()
        {
            Close();

            _reader = _fi.OpenText();

            _open = true;
        }

        public void Close()
        {
            if(_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
            }

            CurrentIteration = 0;
            _open = false;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public bool IsOpen
        {
            get
            {
                return _open;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public int CurrentIteration
        {
            get;
            private set;
        }
    }
}

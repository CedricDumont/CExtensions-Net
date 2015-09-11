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


        public FileContentEnumerator(String filePath)
            : this(new FileInfo(filePath))
        {

        }

        public FileContentEnumerator(FileInfo fi)
        {
            _fi = fi;
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
            _current = _reader.ReadLine();

            if(_current == null)
            {
                _reader.Close();
                return false;
            }
            return true;
        }

        public void Reset()
        {
            try
            {
                _reader.Close();
            }
            catch (Exception ex) { }

            _reader = _fi.OpenText();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}

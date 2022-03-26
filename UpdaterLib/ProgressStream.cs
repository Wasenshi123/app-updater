using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpdaterLib
{
    public class ProgressStream : Stream
    {
        public long BytesRead { get; set; }
        Stream _baseStream;
        public ProgressStream(Stream s)
        {
            _baseStream = s;
        }
        public override bool CanRead
        {
            get { return _baseStream.CanRead; }
        }
        public override bool CanSeek
        {
            get { return false; }
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override void Flush()
        {
            _baseStream.Flush();
        }
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }
        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int rc = _baseStream.Read(buffer, offset, count);
            BytesRead += rc;
            return rc;
        }
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int rc = await _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
            BytesRead += rc;
            return rc;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.IO;

namespace IDeliverable.Donuts.Helpers
{
    public class PlaceholderStream : Stream
    {
        public PlaceholderStream(Stream responseStream)
        {
            mStream = responseStream;
            mCacheStream = new MemoryStream(5000); // TODO: Is it really wise to limit capacity like this?
        }

        private Stream mStream;
        private MemoryStream mCacheStream;

        /// <summary>
        /// Determines whether the Write method is outputting data immediately, or delaying output until Flush() is fired.
        /// </summary>
        private bool IsOutputDelayed => TransformStream != null;

        /// <summary>
        /// This event allows capturing and transformation of the entire output stream.
        /// The Transformation takes place after the markup has been output cached, but before the markup is returned as a response.
        /// </summary>
        public event Func<MemoryStream, MemoryStream> TransformStream;

        protected virtual MemoryStream OnTransformCompleteStream(MemoryStream ms)
        {
            if (TransformStream != null)
                return TransformStream(ms);

            return ms;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => 0;

        public override long Position { get; set; }

        public override long Seek(long offset, SeekOrigin direction) => mStream.Seek(offset, direction);

        public override void SetLength(long length) => mStream.SetLength(length);

        public override void Close() => mStream.Close();

        /// <summary>
        /// Override flush by writing out the cached stream data.
        /// </summary>
        public override void Flush()
        {
            if (IsOutputDelayed && mCacheStream.Length > 0)
            {
                // Check for transform implementations.
                mCacheStream = OnTransformCompleteStream(mCacheStream);

                mStream.Write(mCacheStream.ToArray(), 0, (int)mCacheStream.Length);

                // Clear the cache once we've written it out.
                mCacheStream.SetLength(0);
            }

            mStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) => mStream.Read(buffer, offset, count);

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (IsOutputDelayed)
                // Copy to holding buffer only - we'll write out later.
                mCacheStream.Write(buffer, 0, count);
            else
                mStream.Write(buffer, offset, buffer.Length);
        }
    }
}
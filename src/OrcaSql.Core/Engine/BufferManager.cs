using System;
using System.Collections.Generic;
using System.IO;

namespace OrcaSql.Core.Engine
{
    /// <summary>
    /// The buffer manager caches 8kb byte arrays in memory and reads from disk if necessary. It does not know about about
    /// pages themselves, just 8kb data chunks.
    /// </summary>
    public class BufferManager// : IDisposable
    {
        private readonly Database database;
        private readonly Dictionary<short, Dictionary<long, byte[]>> buffer = new Dictionary<short, Dictionary<long, byte[]>>();
        private readonly Dictionary<short, Dictionary<long, byte[]>> headersBuffer = new Dictionary<short, Dictionary<long, byte[]>>();
        private readonly Dictionary<short, ReadAheadWindow> readAhead = new Dictionary<short, ReadAheadWindow>();

        // Pages that aren't cached (table scans) are read ahead in blocks of this many pages once the reads turn
        // sequential, so scanning consecutive pages costs one read per block instead of one per page.
        private const int ReadAheadPages = 64;

        private class ReadAheadWindow
        {
            public readonly byte[] Data = new byte[ReadAheadPages * 8192];
            public long FirstPage = -1;
            public int PageCount;
            public long LastMissedPage = -2;
        }

        internal BufferManager(Database db)
        {
            database = db;

            // For each file, instantiate a buffer
            foreach (var file in db.Files)
            {
                buffer.Add(file.Key, new Dictionary<long, byte[]>());
                headersBuffer.Add(file.Key, new Dictionary<long, byte[]>());
                readAhead.Add(file.Key, new ReadAheadWindow());
            }
        }

        internal byte[] GetPageBytes(short fileID, long pageID, bool putResultToCache)
        {
            // Ensure file is part of the database
            if (!buffer.ContainsKey(fileID))
                throw new ArgumentOutOfRangeException("File with ID " + fileID + " is not part of this database.");

            
            if (!putResultToCache)
            {
                var page = getPageReadingAhead(fileID, pageID);
                return page;
            }

            // Read bytes from file if not already in buffer
            if (!buffer[fileID].ContainsKey(pageID))
            {
                lock (buffer[fileID])
                {
                    if (buffer[fileID].ContainsKey(pageID))
                        return buffer[fileID][pageID];

                    var page = getPageFromDisk(fileID, pageID);

                    buffer[fileID].Add(pageID, page);
                }
            }

            // Return result from buffer
            return buffer[fileID][pageID];
        }

        /// <summary>
        /// Like getPageFromDisk, but once the requested pages are consecutive it reads ReadAheadPages pages at a time
        /// and serves the following requests from that block. Random reads still read a single page.
        /// </summary>
        private byte[] getPageReadingAhead(short fileID, long pageID)
        {
            var window = readAhead[fileID];

            lock (window)
            {
                if (pageID < window.FirstPage || pageID >= window.FirstPage + window.PageCount)
                {
                    bool sequential = pageID == window.LastMissedPage + 1 || pageID == window.FirstPage + window.PageCount;
                    window.LastMissedPage = pageID;

                    if (!sequential)
                        return getPageFromDisk(fileID, pageID);

                    var fs = database.Files[fileID];
                    fs.Seek(pageID * 8192, SeekOrigin.Begin);

                    int read = 0, n;
                    while (read < window.Data.Length && (n = fs.Read(window.Data, read, window.Data.Length - read)) > 0)
                        read += n;

                    window.FirstPage = pageID;
                    window.PageCount = read / 8192;

                    if (window.PageCount == 0)
                        return getPageFromDisk(fileID, pageID);
                }

                var bytes = new byte[8192];
                Buffer.BlockCopy(window.Data, (int)(pageID - window.FirstPage) * 8192, bytes, 0, 8192);
                return bytes;
            }
        }

        /// <summary>
        /// Reads the 8kb data chunk from the specified file on disk.
        /// </summary>
        private byte[] getPageFromDisk(short fileID, long pageID)
        {
            var fs = database.Files[fileID];

            var bytes = new byte[8192];
            fs.Seek(pageID * 8192, SeekOrigin.Begin);
            fs.Read(bytes, 0, 8192);

            return bytes;
        }
    }
}
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

        internal BufferManager(Database db)
        {
            database = db;

            // For each file, instantiate a buffer
            foreach (var file in db.Files)
            {
                buffer.Add(file.Key, new Dictionary<long, byte[]>());
                headersBuffer.Add(file.Key, new Dictionary<long, byte[]>());
            }
        }

        internal byte[] GetPageBytes(short fileID, long pageID, bool putResultToCache)
        {
            // Ensure file is part of the database
            if (!buffer.ContainsKey(fileID))
                throw new ArgumentOutOfRangeException("File with ID " + fileID + " is not part of this database.");

            
            if (!putResultToCache)
            {
                var page = getPageFromDisk(fileID, pageID);
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using OrcaSql.Core.Engine.Pages;
using OrcaSql.Core.Engine.Pages.PFS;
using OrcaSql.Core.MetaData;
using OrcaSql.Core.MetaData.Exceptions;
using OrcaSql.Core.MetaData.TableValuedDictionaries;

namespace OrcaSql.Core.Engine
{
    public class Database : IDisposable
    {
        public string Name { get; }
        public short ID { get; }
        public Guid BindingID { get; private set; }
        public DmvGenerator Dmvs { get; }

        internal Dictionary<short, Stream> Files = new Dictionary<short, Stream>();
        public BaseTableData BaseTables;
        internal Dictionary<string, object> ObjectCache = new Dictionary<string, object>();

        private readonly object metaDataLock = new object();
        private DatabaseMetaData metaData;
        private readonly BufferManager _bufferManager;
        public string DefaultCollation { get; }
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Instantiates a database using just a single data file.
        /// </summary>
        public Database(string file)
            : this(new[] { file })
        { }

        /// <summary>
        /// Instantiates a new database. Each data file that's part of the database must be included in the files parameter.
        /// The order of the files is irrelevant.
        /// </summary>
        public Database(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);

                InitDatabaseStream(fs, file);
            }

            // Instantiate buffer manager
            _bufferManager = new BufferManager(this);

            // Read boot page properties
            var bootPage = GetBootPage();
            Name = bootPage.DatabaseName;
            ID = bootPage.DBID;
            var collation = Collations.GetByCollationId((int) (bootPage.CollationId >> 32));
            DefaultCollation = collation.Name;
            Encoding = Collations.GetEncodingForCollation(collation);
            // Parse vital base tables
            BaseTables = new BaseTableData(this);

            // Startup dmv generator
            Dmvs = new DmvGenerator(this);
        }

        public Database(IEnumerable<Stream> streams)
        {
            foreach (var stream in streams)
            {
                InitDatabaseStream(stream);
            }

            // Instantiate buffer manager
            _bufferManager = new BufferManager(this);

            // Read boot page properties
            var bootPage = GetBootPage();
            Name = bootPage.DatabaseName;
            ID = bootPage.DBID;
            var collation = Collations.GetByCollationId((int)(bootPage.CollationId >> 32));
            DefaultCollation = collation.Name;
            Encoding = Collations.GetEncodingForCollation(collation);
            // Parse vital base tables
            BaseTables = new BaseTableData(this);

            // Startup dmv generator
            Dmvs = new DmvGenerator(this);
        }

        private void InitDatabaseStream(Stream fs, string file = "")
        {
            var fileHeaderBytes = new byte[8192];
            fs.Read(fileHeaderBytes, 0, 8192);
            // This is kindy hackish as "this" isn't operational yet. As such, any calls to "this" will fail.
            // We know however that, currently, FileHeaderPage doesn't use the reference. Should be changed
            // later on.
            var fileHeaderPage = new FileHeaderPage(fileHeaderBytes, this);

            if (BindingID == Guid.Empty)
                BindingID = fileHeaderPage.BindingID;

            if (BindingID != fileHeaderPage.BindingID)
                throw new BindingIDMismatchException(file, BindingID, fileHeaderPage.BindingID);

            fs.Seek(fs.Position - 8192, SeekOrigin.Begin);

            Files.Add(fileHeaderPage.FileID, fs);
        }

        internal DatabaseMetaData GetMetaData()
        {
            if (metaData != null) return metaData;

            lock (metaDataLock)
            {
                if (metaData == null)
                    metaData = new DatabaseMetaData(this);

                Thread.MemoryBarrier();
            }

            return metaData;
        }

        [DebuggerStepThrough]
        internal PrimaryRecordPage GetPrimaryRecordPage(PagePointer loc, CompressionContext compression, bool isSysTable = true)
        {
            Debug.WriteLine("Loading Primary Record Page " + loc);

            return new PrimaryRecordPage(GetPageBytes(loc, isSysTable), compression, this);
        }

        private byte[] GetPageBytes(PagePointer pagePointer, bool putResultsToCache = true)
        {
            return _bufferManager.GetPageBytes(pagePointer.FileID, pagePointer.PageID, putResultsToCache);
        }

        [DebuggerStepThrough]
        internal CompressedRecordPage GetCompressedRecordPage(PagePointer loc, CompressionContext compression)
        {
            if (compression.CompressionLevel == CompressionLevel.None)
                throw new ArgumentException("Can't load compressed page with a compression level of none.");

            Debug.WriteLine("Loading compressed record page " + loc);

            return new CompressedRecordPage(GetPageBytes(loc), compression, this);
        }

        [DebuggerStepThrough]
        internal Page GetPage(PagePointer loc)
        {
            Debug.WriteLine("Loading Generic Page " + loc);

            return new Page(GetPageBytes(loc), this);
        }

        [DebuggerStepThrough]
        internal FileHeaderPage GetFileHeaderPage(PagePointer loc)
        {
            Debug.WriteLine("Loading File Header Page");

            return new FileHeaderPage(GetPageBytes(loc), this);
        }

        [DebuggerStepThrough]
        internal NonclusteredIndexPage GetNonclusteredIndexPage(PagePointer loc)
        {
            Debug.WriteLine("Loading Nonclustered Index Page " + loc);

            return new NonclusteredIndexPage(GetPageBytes(loc), this);
        }

        [DebuggerStepThrough]
        internal ClusteredIndexPage GetClusteredIndexPage(PagePointer loc, bool isSysTable)
        {
            Debug.WriteLine("Loading Clustered Index Page " + loc);

            return new ClusteredIndexPage(GetPageBytes(loc, isSysTable), this);
        }

        [DebuggerStepThrough]
        internal TextMixPage GetTextMixPage(PagePointer loc)
        {
            Debug.WriteLine("Loading TextMix Page " + loc);

            return new TextMixPage(GetPageBytes(loc, false), this);
        }

        [DebuggerStepThrough]
        internal IamPage GetIamPage(PagePointer loc, bool useCache)
        {
            Debug.WriteLine("Loading IAM Page " + loc);

            return new IamPage(GetPageBytes(loc, useCache), this);
        }

        [DebuggerStepThrough]
        internal BootPage GetBootPage()
        {
            Debug.WriteLine("Loading Boot Page");

            return new BootPage(GetPageBytes(new PagePointer(1, 9)), this);
        }

        internal SgamPage GetSgamPage(PagePointer loc)
        {
            Debug.WriteLine("Loading SGAM Page " + loc);

            if (loc.PageID % 511230 != 3)
                throw new ArgumentException("Invalid SGAM index: " + loc.PageID);

            return new SgamPage(GetPageBytes(loc), this);
        }

        internal GamPage GetGamPage(PagePointer loc)
        {
            Debug.WriteLine("Loading GAM Page " + loc);

            if (loc.PageID % 511230 != 2)
                throw new ArgumentException("Invalid GAM index: " + loc.PageID);

            return new GamPage(GetPageBytes(loc), this);
        }

        internal PfsPage GetPfsPage(PagePointer loc)
        {
            Debug.WriteLine("Loading PFS Page " + loc);

            // We know PFS pages are present every 8088th page, except for the very first one
            if (loc.PageID != 1 && loc.PageID % 8088 != 0)
                throw new ArgumentException("Invalid PFS index: " + loc.PageID);

            return new PfsPage(GetPageBytes(loc), this);
        }

        public void Dispose()
        {
            foreach (var stream in Files)
            {
                stream.Value.Dispose();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using OrcaMDF.Core.Engine.Pages;

namespace OrcaMDF.Core.Engine
{
    internal class PageLocator
    {
        private readonly BufferManager _bufferManager;
        private readonly bool _backupMode;
        private readonly Dictionary<short, Dictionary<long, PagePointer>> _backupPagesMap;

        public PageLocator(BufferManager bufferManager, bool backupMode)
        {
            _bufferManager = bufferManager;
            _backupMode = backupMode;
            _backupPagesMap = new Dictionary<short, Dictionary<long, PagePointer>>();
        }

        public byte[] GetPageBytes(PagePointer loc, bool putResultToCache = true)
        {

            if(!_backupMode)
                return _bufferManager.GetPageBytes(loc.FileID, loc.PageID, putResultToCache);

            //var pagesInFile = (int)(bufferManager.GetFileLen(loc.FileID) / 8192);

            //var pages = Enumerable.Range(0, pagesInFile).Select(x => new PagePointer(loc.FileID, x)).ToArray();

            //var result = ( from pagePointer in pages
            //               let pageHeaderBytes = bufferManager.GetHeaderBytes(pagePointer.FileID, pagePointer.PageID)
            //               let pageHeader = new PageHeader(pageHeaderBytes)
            //               select new Tuple<PagePointer, PagePointer>(pagePointer, pageHeader.Pointer)
            //              ).ToList();

            //File.WriteAllText("test.loc.txt", string.Join(Environment.NewLine, result.Select(x => x.Item1 + " -> " + x.Item2)));


            if (_backupPagesMap.TryGetValue(loc.FileID, out var loc1) && loc1.TryGetValue(loc.PageID/8, out var loc2))
            {
                return _bufferManager.GetPageBytes(loc2.FileID, loc2.PageID  + loc.PageID % 8, putResultToCache);
            }

            var pointer = BinarySearch(x => new PageHeader(_bufferManager.GetHeaderBytes(loc.FileID, x*8)).Pointer,
                0, (long)_bufferManager.GetFileLen(loc.FileID) / (8192*8), loc);

            return _bufferManager.GetPageBytes(pointer.FileID, pointer.PageID + loc.PageID % 8, putResultToCache);
        }

        private PagePointer BinarySearch(Func<long, PagePointer> getPagePointerFunc, long l, long r, PagePointer x)
        {
            var range = new Range();
            var firstExtentPage = new PagePointer(x.FileID, x.PageID/8*8);

            if (!_backupPagesMap.ContainsKey(x.FileID)) { _backupPagesMap[x.FileID] = new Dictionary<long, PagePointer>(); }

            l = _backupPagesMap[x.FileID].Select(y => new { Diff = firstExtentPage.PageID/8 - y.Key, Key = (long?)y.Value.PageID / 8 })
                    .Where(y => y.Diff >= 0)
                    .OrderBy(y => y.Diff)
                    .Select(y => y.Key)
                    .FirstOrDefault() ?? l;

            r = _backupPagesMap[x.FileID].Select(y => new { Diff = y.Key - firstExtentPage.PageID/8, Key = (long?)y.Value.PageID / 8 })
                    .Where(y => y.Diff >= 0)
                    .OrderBy(y => y.Diff)
                    .Select(y => y.Key)
                    .FirstOrDefault() ?? r;

            if (l + 1 == firstExtentPage.PageID / 8)
            {
                var pagePointer = getPagePointerFunc(l+1);

                _backupPagesMap[pagePointer.FileID][pagePointer.PageID / 8] = new PagePointer(pagePointer.FileID, (l + 1) * 8);

                if (pagePointer == firstExtentPage)
                    return new PagePointer(pagePointer.FileID, (l + 1) * 8);
            }

            while (l <= r)
            {
                var m = l + (r -l) / 2;

                var pagePointer = getPagePointerFunc(m);

                if(pagePointer != PagePointer.Zero)
                    _backupPagesMap[pagePointer.FileID][pagePointer.PageID / 8] = new PagePointer(pagePointer.FileID, m*8);

                // Check if x is present at mid 
                if (pagePointer == firstExtentPage)
                    return new PagePointer(pagePointer.FileID, m * 8);

                // If x greater, ignore left half   
                if (pagePointer.PageID < firstExtentPage.PageID)// && pagePointer != PagePointer.Zero)
                    l = m + 1;

                // If x is smaller, ignore right half  
                else
                    r = m - 1;
            }

            // We reach here when element is not present in array 
            return PagePointer.Zero;
        }
    }

    internal class Range
    {
        public long LeftBound { get; set; }
        public long RightBound { get; set; }

        public long GetDistance(long value)
        {
            var leftDistance = LeftBound - value;

            if (leftDistance <= 0) return leftDistance;

            var rightDistance = value - RightBound;

            if (rightDistance >= 0) return rightDistance;

            return 0;
        }
    }
}

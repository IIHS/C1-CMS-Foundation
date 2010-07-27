﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Composite.Collections.Generic;
using Composite.Data.Caching;
using Composite.Data.Foundation;
using Composite.Logging;
using Composite.StringExtensions;
using Composite.Types;
using NullableGuid = Composite.Types.ExtendedNullable<System.Guid>;

namespace Composite.Data.Types
{
    /// <summary>
    /// Provides basic data access to IPage and IPageStructure data
    /// </summary>
	internal static class PageManager
	{
        private static readonly string LogTitle = "PageManager";

        private class PageStructureRecord
        {
            public Guid ParentId { get; set; }
            public int LocalOrdering { get; set; }
        }

        private static readonly int PageCacheSize = 5000;
        private static readonly int PageStructureCacheSize = 3000;
        private static readonly int ChildrenCacheSize = 2000;
        private static readonly int PagePlaceholderCacheSize = 500;

        private static readonly Cache<string, ExtendedNullable<IPage>> _pageCache = new Cache<string, ExtendedNullable<IPage>>("Pages", PageCacheSize);
        private static readonly Cache<string, ExtendedNullable<PageStructureRecord>> _pageStructureCache = new Cache<string, ExtendedNullable<PageStructureRecord>>("Page structure", PageStructureCacheSize);
        private static readonly Cache<string, ReadOnlyCollection<Guid>> _childrenCache = new Cache<string, ReadOnlyCollection<Guid>>("Child pages", ChildrenCacheSize);
        private static readonly Cache<string, ReadOnlyList<IPagePlaceholderContent>> _placeholderCache = new Cache<string, ReadOnlyList<IPagePlaceholderContent>>("Page placeholders", PagePlaceholderCacheSize);

        static PageManager()
        {
            SubscribeToEvents();
        }

        #region Public methods

        public static IPage GetPageById(Guid id)
        {
            return GetPageById(id, false);
        }

        public static IPage GetPageById(Guid id, bool readonlyValue)
        {
            IPage result;

            string cacheKey = GetCacheKey<IPage>(id);
            var cachedValue = _pageCache.Get(cacheKey);

            if (cachedValue != null)
            {
                result = cachedValue.Value;
            }
            else
            {
                result = (from page in DataFacade.GetData<IPage>(false)
                                where page.Id == id
                                select page).FirstOrDefault();

                _pageCache.Add(cacheKey, new ExtendedNullable<IPage> { Value = result });
            }

            if(result == null)
            {
                return null;
            }

            return readonlyValue ? result : CreateWrapper(result);
        }

        public static Guid GetParentID(Guid pageId)
        {
            PageStructureRecord pageStructure = GetPageStructureRecord(pageId);
            return pageStructure != null ? pageStructure.ParentId : Guid.Empty;
        }

         public static int GetLocalOrdering(Guid pageId)
         {
             PageStructureRecord pageStructure = GetPageStructureRecord(pageId);
             return pageStructure != null ? pageStructure.LocalOrdering : 0;
         }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentPageId">Empty guild will yield all root pages</param>
        /// <returns></returns>
        public static ReadOnlyCollection<Guid> GetChildrenIDs(Guid parentPageId)
        {
            var cacheKey = GetCacheKey<IPageStructure>(parentPageId);
            var cachedValue = _childrenCache.Get(cacheKey);

            if (cachedValue != null)
            {
                return cachedValue;
            }

            List<Guid> children = (from ps in DataFacade.GetData<IPageStructure>()
                                   where ps.ParentId == parentPageId
                                   orderby ps.LocalOrdering
                                   select ps.Id).ToList();

            var readonlyList = new ReadOnlyCollection<Guid>(children);

            _childrenCache.Add(cacheKey, readonlyList);

            return readonlyList;
        }



        public static IEnumerable<IPagePlaceholderContent> GetPlaceholderContent(Guid pageId)
        {
            string cacheKey = GetCacheKey<IPagePlaceholderContent>(pageId);
            var cachedValue = _placeholderCache.Get(cacheKey);
            if(cachedValue != null)
            {
                return cachedValue;
            }

            var result = DataFacade.GetData<IPagePlaceholderContent>(f => f.PageId == pageId).ToList();
            _placeholderCache.Add(cacheKey, new ReadOnlyList<IPagePlaceholderContent>(result));
            return result;
        }

        #endregion Public

        #region Private

        private static PageStructureRecord GetPageStructureRecord(Guid pageId)
        {
            string cacheKey = GetCacheKey<IPageStructure>(pageId);
            ExtendedNullable<PageStructureRecord> cachedValue = _pageStructureCache.Get(cacheKey);

            if (cachedValue != null)
            {
                Verify.That(cachedValue.HasValue, "Incorrect usage of cache.");
                return cachedValue.Value;
            }

            PageStructureRecord result =
                         (from ps in DataFacade.GetData<IPageStructure>(false)
                          where ps.Id == pageId
                          select new PageStructureRecord
                              {
                                  ParentId = ps.ParentId,
                                  LocalOrdering = ps.LocalOrdering
                              }).FirstOrDefault();

            if(result == null)
            {
                LoggingService.LogWarning(LogTitle, 
                    "No IPageStructure entries found for Page with Id '{0}'".FormatWith(pageId));
            }

            _pageStructureCache.Add(cacheKey, result);

            return result;
        }

        private static string GetCacheKey<T>(Guid id)
        {
            string localizationInfo = LocalizationScopeManager.MapByType(typeof(T)).ToString();
            string dataScope = DataScopeManager.MapByType(typeof (T)).Name;
            return id + dataScope + localizationInfo;
        }

        private static string GetCacheKey(Guid id, DataSourceId dataSourceId)
        {
            string localizationInfo = dataSourceId.LocaleScope.ToString();
            string dataScope = dataSourceId.DataScopeIdentifier.Name;
            return id + dataScope + localizationInfo;
        }

        private static void OnPageChanged(DataEventArgs args)
        {
            IPage page = args.Data as IPage;
            if (page == null)
            {
                return;
            }

            _pageCache.Remove(GetCacheKey(page.Id, page.DataSourceId));
        }        
        
        private static void OnPagePlaceholderChanged(DataEventArgs args)
        {
            var placeHolder = args.Data as IPagePlaceholderContent;
            if (placeHolder == null)
            {
                return;
            }

            _placeholderCache.Remove(GetCacheKey(placeHolder.PageId, placeHolder.DataSourceId));
        }

        private static void OnPageStructureChanged(DataEventArgs args)
        {
            var ps = args.Data as IPageStructure;
            if (ps == null)
            {
                return;
            }

            _pageStructureCache.Remove(GetCacheKey(ps.Id, ps.DataSourceId));
            _childrenCache.Remove(GetCacheKey(ps.ParentId, ps.DataSourceId));
        }

        private static IPage CreateWrapper(IPage page)
        {
            return DataWrappingFacade.Wrap(page);
        }

        private static void SubscribeToEvents()
        {
            DataEventSystemFacade.SubscribeToDataDeleted<IPagePlaceholderContent>(OnPagePlaceholderChanged, true);
            DataEventSystemFacade.SubscribeToDataAfterUpdate<IPagePlaceholderContent>(OnPagePlaceholderChanged, true);
            DataEventSystemFacade.SubscribeToDataAfterAdd<IPagePlaceholderContent>(OnPagePlaceholderChanged, true);

            DataEventSystemFacade.SubscribeToDataAfterUpdate<IPage>(OnPageChanged, true);
            DataEventSystemFacade.SubscribeToDataAfterAdd<IPage>(OnPageChanged, true);
            DataEventSystemFacade.SubscribeToDataDeleted<IPage>(OnPageChanged, true);

            DataEventSystemFacade.SubscribeToDataAfterAdd<IPageStructure>(OnPageStructureChanged, true);
            DataEventSystemFacade.SubscribeToDataBeforeUpdate<IPageStructure>(OnPageStructureChanged, true);
            DataEventSystemFacade.SubscribeToDataAfterUpdate<IPageStructure>(OnPageStructureChanged, true);
            DataEventSystemFacade.SubscribeToDataDeleted<IPageStructure>(OnPageStructureChanged, true);
        }

        #endregion Private
    }
}

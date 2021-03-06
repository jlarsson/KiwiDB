using System;
using System.Collections.Generic;
using System.Linq;
using Kiwi.Json;
using Kiwi.Json.Untyped;
using KiwiDb.Gist.Extensions;
using KiwiDb.Gist.Tree;
using KiwiDb.JsonDb.Filter;
using KiwiDb.Storage;

namespace KiwiDb.JsonDb.Index
{
    public class IndexCatalog : Gist<string, IJsonValue>, IIndexCatalog
    {
        private Dictionary<string, IndexWrapper> _indexCache;

        public IndexCatalog(IBlockCollection blocks, IIndexValueFactory indexValueFactory, IMasterTable masterTable)
            : base(
                blocks.ApplicationBlockReference,
                new GistConfig<string, IJsonValue>
                    {
                        Blocks = blocks,
                        Ext =
                            new OrderedGistExtension<string, IJsonValue>(
                            new GistStringType(), new GistJsonType()),
                        UpdateStrategy = UpdateStrategy<string, IJsonValue>.UpdateKey
                    })
        {
            Blocks = blocks;
            IndexValueFactory = indexValueFactory;
            MasterTable = masterTable;
        }

        public IBlockCollection Blocks { get; private set; }
        public IIndexValueFactory IndexValueFactory { get; private set; }
        public IMasterTable MasterTable { get; private set; }

        protected Dictionary<string, IndexWrapper> IndexCache
        {
            get
            {
                return _indexCache ?? (_indexCache = Scan().ToDictionary(
                    kv => kv.Key,
                    kv =>
                    new IndexWrapper(this, JSON.ToObject<IndexDefinition>(kv.Value.ToString()))));
            }
        }

        #region IIndexCatalog Members

        public IEnumerable<KeyValuePair<string, IndexOptions>> EnumerateIndices
        {
            get { return IndexCache.Select(kv => new KeyValuePair<string, IndexOptions>(kv.Key, kv.Value.IndexDefinition.Options)); }
        }

        public IIndex GetIndex(string memberPath)
        {
            IndexWrapper indexWrapper;
            return IndexCache.TryGetValue(memberPath, out indexWrapper) ? indexWrapper : null;
        }

        public bool EnsureIndex(IndexDefinition indexDefinition)
        {
            // Try to not rebuild index, so we check if there exists an equal defintion already
            IndexWrapper existing;
            if (IndexCache.TryGetValue(indexDefinition.Path, out existing))
            {
                if (indexDefinition.Options.Equals(existing.IndexDefinition.Options))
                {
                    // phew, existing index was already matching. bail out
                    return false;
                }
                DropIndex(indexDefinition.Path);
            }

            // An index can not both exclude certain values and at the same time include certain values
            var hasIncludeValues = (indexDefinition.Options.IncludeValues != null) &&
                                   (indexDefinition.Options.IncludeValues.Length > 0);
            var hasExcludeValues = (indexDefinition.Options.ExcludeValues != null) &&
                                   (indexDefinition.Options.ExcludeValues.Length > 0);

            if (hasIncludeValues && hasExcludeValues)
            {
                throw new KiwiDbException("An index can not have an exclusion list and an inclusion list at the same time.");
            }

            var index = new IndexWrapper(this, indexDefinition) {IsChanged = true};
            IndexCache.Add(indexDefinition.Path, index);
            Insert(indexDefinition.Path, JSON.FromObject(indexDefinition));

            // rebuild index
            foreach (var record in MasterTable.Scan())
            {
                UpdateIndex(record.Key, null, record.Value);
            }
            return true;
        }

        public bool DropIndex(string memberPath)
        {
            IndexWrapper index;
            if (IndexCache.TryGetValue(memberPath, out index))
            {
                IndexCache.Remove(memberPath);
                Remove(memberPath, _ => true);
                index.Drop();
                return true;
            }
            return false;
        }

        public void UpdateIndex(string key, IJsonValue oldValue, IJsonValue newValue)
        {
            // TODO: Special IndexValue comparison is not in effect...
            var oldIndex = new HashSet<Tuple<IndexWrapper, IndexValue>>(GetObjectIndexValues(oldValue));
            var newIndex = new HashSet<Tuple<IndexWrapper, IndexValue>>(GetObjectIndexValues(newValue));

            var add = newIndex.Except(oldIndex);
            var remove = oldIndex.Except(newIndex);

            foreach (var tuple in remove)
            {
                tuple.Item1.RemoveIndex(key, tuple.Item2);
            }
            foreach (var tuple in add)
            {
                tuple.Item1.AddIndex(key, tuple.Item2);
            }
        }

        public IEnumerable<KeyValuePair<string, IJsonValue>> FindIndexedObjects(IJsonValue obj)
        {
            HashSet<string> keys = null;

            var indexPaths = new HashSet<string>();
            foreach (var g in from indexValue in GetObjectIndexValues(obj)
                              group indexValue by indexValue.Item1
                              into g select g)
            {
                indexPaths.Add(g.Key.MemberPath);

                HashSet<string> keysForThisIndex = null;
                foreach (var tuple in g)
                {
                    if (keysForThisIndex == null)
                    {
                        keysForThisIndex = new HashSet<string>(g.Key.FindKeys(tuple.Item2));
                    }
                    else
                    {
                        keysForThisIndex.IntersectWith(g.Key.FindKeys(tuple.Item2));
                    }
                    if (keysForThisIndex.Count == 0)
                    {
                        return Enumerable.Empty<KeyValuePair<string, IJsonValue>>();
                    }
                }

                if (keys == null)
                {
                    keys = keysForThisIndex;
                }
                else
                {
                    keys.IntersectWith(keysForThisIndex);
                }
                if (keys.Count == 0)
                {
                    return Enumerable.Empty<KeyValuePair<string, IJsonValue>>();
                }
            }
            if (keys == null)
            {
                // table scan
                var filter = new JsonFilter(obj.JsonPathValues());
                return from rec in MasterTable.Scan()
                       where filter.Matches(rec.Value)
                       select rec;
            }
            else
            {
                var filter = new JsonFilter(obj.JsonPathValues().Where(pv => !indexPaths.Contains(pv.Path.Path)));
                return from key in keys
                       from rec in MasterTable.Find(key)
                       where filter.Matches(rec.Value)
                       select rec;
            }
        }

        public void SaveChanges()
        {
            if (_indexCache != null)
            {
                foreach (var index in _indexCache.Select(kv => kv.Value).Where(v => v.IsChanged))
                {
                    Insert(index.MemberPath, JSON.FromObject(index.IndexDefinition));
                }
            }
        }

        #endregion

        private IEnumerable<Tuple<IndexWrapper, IndexValue>> GetObjectIndexValues(IJsonValue obj)
        {
            return obj == null
                       ? Enumerable.Empty<Tuple<IndexWrapper, IndexValue>>()
                       : from index in IndexCache
                         let valueFilter = CreateIndexValueFilter(index.Value.IndexDefinition.Options)
                         from indexValue in IndexValueFactory.GetIndexValues(obj, index.Key)
                         where valueFilter.Filter(indexValue)
                         select Tuple.Create(index.Value, indexValue);
        }

        private IIndexValueFilter CreateIndexValueFilter(IndexOptions options)
        {
            if ((options.ExcludeValues != null) && (options.ExcludeValues.Length > 0))
            {
                return new ExcludeIndexValueFilter(options.ExcludeValues);
            }
            if ((options.IncludeValues != null) && (options.IncludeValues.Length > 0))
            {
                return new IncludeIndexValueFilter(options.IncludeValues);
            }
            return new NullIndexValueFilter();
        }

        private class ExcludeIndexValueFilter : IIndexValueFilter
        {
            private readonly HashSet<object> _values;
            public ExcludeIndexValueFilter(IEnumerable<object> excludeValues)
            {
                _values = new HashSet<object>(excludeValues);
            }

            public bool Filter(IndexValue indexValue)
            {
                return !_values.Contains(indexValue.Value);
            }
        }

        private class IncludeIndexValueFilter : IIndexValueFilter
        {
            private readonly HashSet<object> _values;
            public IncludeIndexValueFilter(IEnumerable<object> includeValues)
            {
                _values = new HashSet<object>(includeValues);
            }

            public bool Filter(IndexValue indexValue)
            {
                return _values.Contains(indexValue.Value);
            }
        }

        private class NullIndexValueFilter : IIndexValueFilter
        {
            public bool Filter(IndexValue indexValue)
            {
                return true;
            }
        }

        private interface IIndexValueFilter
        {
            bool Filter(IndexValue indexValue);
        }

        #region Nested type: IndexWrapper

        protected class IndexWrapper : IIndex, IBlockReference
        {
            private readonly IndexCatalog _owner;
            private IIndex _index;

            public IndexWrapper(IndexCatalog owner, IndexDefinition indexDefinition)
            {
                IndexDefinition = indexDefinition;
                _owner = owner;
            }

            public IndexDefinition IndexDefinition { get; set; }

            protected IIndex Index
            {
                get { return _index ?? (_index = new Index(this, IndexDefinition)); }
            }

            public bool IsChanged { get; set; }

            #region IBlockReference Members

            IBlockCollection IBlockReference.Blocks
            {
                get { return _owner.Blocks; }
            }

            int IBlockReference.BlockId
            {
                get { return IndexDefinition.BlockId; }
                set
                {
                    IndexDefinition.BlockId = value;
                    IsChanged = true;
                }
            }

            #endregion

            #region IIndex Members

            public string MemberPath
            {
                get { return IndexDefinition.Path; }
            }

            public void Visit(Action<KeyValuePair<IndexValue, string>> visitor)
            {
                Index.Visit(visitor);
            }

            public IEnumerable<string> FindKeys(IndexValue indexValue)
            {
                return Index.FindKeys(indexValue);
            }

            public void AddIndex(string key, IndexValue indexValue)
            {
                Index.AddIndex(key, indexValue);
            }

            public void RemoveIndex(string key, IndexValue indexValue)
            {
                Index.RemoveIndex(key, indexValue);
            }

            public void Drop()
            {
                Index.Drop();
            }

            #endregion
        }

        #endregion
    }
}
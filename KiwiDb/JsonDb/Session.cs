using KiwiDb.JsonDb.Index;
using KiwiDb.Storage;

namespace KiwiDb.JsonDb
{
    public class Session : ISession
    {
        private readonly IBlockCollection _blocks;
        private IIndexCatalog _indexCatalog;
        private IMasterTable _masterTable;

        public Session(IBlockCollection blocks, IIndexValueFactory indexValueFactory)
        {
            IndexValueFactory = indexValueFactory;
            _blocks = blocks;
        }

        protected IIndexValueFactory IndexValueFactory { get; private set; }

        #region ISession Members

        public bool AutoCommit
        {
            get { return _blocks.AutoCommit; }
            set { _blocks.AutoCommit = value; }
        }

        public IMasterTable MasterTable
        {
            get { return _masterTable ?? (_masterTable = new MasterTable(_blocks)); }
        }

        public IIndexCatalog IndexCatalog
        {
            get { return _indexCatalog ?? (_indexCatalog = new IndexCatalog(_blocks, IndexValueFactory, MasterTable)); }
        }

        public void Dispose()
        {
            if (_indexCatalog != null)
            {
                _indexCatalog.SaveChanges();
            }
            _blocks.Dispose();
        }

        #endregion
    }
}
namespace KiwiDb.Gist.Tree
{
    public interface IUpdateActions
    {
        void FailIfKeyExists();
        void UpdateExistingKey();
        void AppendNewKey();
    }
}
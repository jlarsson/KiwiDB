namespace KiwiDb
{
    public class DuplicateKeyException: KiwiDbException
    {
        public DuplicateKeyException(object duplicateKeyValue)
            : base(string.Format("A record with key \"{0}\" already exists", duplicateKeyValue))
        {
        }
    }
}

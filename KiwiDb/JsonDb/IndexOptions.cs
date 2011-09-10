namespace KiwiDb.JsonDb
{
    public class IndexOptions
    {
        public IndexOptions()
        {
            StringIndexOptions = new StringIndexOptions();
            DateIndexOptions = new DateIndexOptions();
        }

        public bool IsUnique { get; set; }
        public StringIndexOptions StringIndexOptions { get; set; }
        public DateIndexOptions DateIndexOptions { get; set; }

        public IndexOptions SetIgnoreCase(bool ignoreCase)
        {
            StringIndexOptions.IgnoreCase = ignoreCase;
            return this;
        }

        public IndexOptions SetUnique(bool isUnique)
        {
            IsUnique = isUnique;
            return this;
        }

        public IndexOptions SetIgnoreTimeOfDay(bool ignoreTimeOfDay)
        {
            DateIndexOptions.IgnoreTimeOfDay = ignoreTimeOfDay;
            return this;
        }
    }
}
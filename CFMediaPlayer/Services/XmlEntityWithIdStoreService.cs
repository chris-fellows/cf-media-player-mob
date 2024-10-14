using CFMediaPlayer.Utilities;

namespace CFMediaPlayer.Services
{
    /// <summary>
    /// Base service that stores entities with an Id in XML files.
    /// </summary>
    /// <typeparam name="TEntityType"></typeparam>
    /// <typeparam name="TIdType"></typeparam>
    public abstract class XmlEntityWithIdStoreService<TEntityType, TIdType>
    {
        protected readonly string _folder;
        protected readonly string _getAllFilePattern;                                       // E.g. "UserSettings.*.xml"
        protected readonly Func<TEntityType, string> _getEntityFileNameByEntityFunction;    // Returns file name from entity
        protected readonly Func<TIdType, string> _getEntityFileNameByIdFunction;            // Returns file name from id

        public XmlEntityWithIdStoreService(string folder, 
                                    string getAllFilePattern,
                                    Func<TEntityType, string> getEntityFileNameByEntityFunction,
                                    Func<TIdType, string> getEntityFileNameByIdFunction)

        {
            _folder = folder;
            _getAllFilePattern = getAllFilePattern;
            _getEntityFileNameByEntityFunction = getEntityFileNameByEntityFunction;
            _getEntityFileNameByIdFunction = getEntityFileNameByIdFunction;
        }

        public List<TEntityType> GetAll()
        {
            var items = new List<TEntityType>();
            foreach(var file in Directory.GetFiles(_folder, _getAllFilePattern))
            {
                items.Add(XmlUtilities.DeserializeFromString<TEntityType>(File.ReadAllText(file)));                
            }
            return items;
        }

        public TEntityType? GetById(TIdType id)
        {
            var file = Path.Combine(_folder, _getEntityFileNameByIdFunction(id));
            return File.Exists(file) ?
                    XmlUtilities.DeserializeFromString<TEntityType>(File.ReadAllText(file)) : default(TEntityType);
        }

        public void Update(TEntityType entity)
        {
            var file = Path.Combine(_folder, _getEntityFileNameByEntityFunction(entity));
            File.WriteAllText(file, XmlUtilities.SerializeToString(entity));
        }

        public void Delete(TIdType id)
        {
            var file = Path.Combine(_folder, _getEntityFileNameByIdFunction(id));
            if (File.Exists(file)) File.Delete(file);
        }
    }
}

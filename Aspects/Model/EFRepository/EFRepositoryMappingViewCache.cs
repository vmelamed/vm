using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.MappingViews;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Class EFRepositoryMappingViewCache.
    /// Based on https://msdn.microsoft.com/en-us/library/dn469601(v=vs.113).aspx
    /// </summary>
    /// <typeparam name="T">The type of the actual <see cref="EFRepositoryBase"/> derived repository.</typeparam>
    /// <seealso cref="DbMappingViewCache" />
    public class EFRepositoryMappingViewCache<T> : DbMappingViewCache where T : EFRepositoryBase, new()
    {
        const string CacheFileSuffix = ".cache";

        readonly string _cachePath;
        readonly string _cacheFilePath;
        SerializableViews _serializableViews;

        /// <summary>
        /// Initializes a new instance of the <see cref="EFRepositoryMappingViewCache{T}"/> class.
        /// </summary>
        public EFRepositoryMappingViewCache()
        {
            var repositoryType =
            _cachePath = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                nameof(EFRepository),
                                typeof(T).Namespace);

            _cacheFilePath = Path.Combine(
                                _cachePath,
                                typeof(T).Name+CacheFileSuffix);

            // Load from an existing file
            if (File.Exists(_cacheFilePath))
                using (Stream stream = File.Open(_cacheFilePath, FileMode.Open))
                    _serializableViews = (SerializableViews)new BinaryFormatter().Deserialize(stream);

            if (_serializableViews == null)
                Generate();
        }

        /// <summary>
        /// Generates the mapping views cache.
        /// </summary>
        /// <exception cref="EdmSchemaException"></exception>
        public void Generate()
        {
            if (!Directory.Exists(_cachePath))
                Directory.CreateDirectory(_cachePath);

            var repository        = new T();
            var mappingCollection = (StorageMappingItemCollection)repository.ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
            var mappingHashValue  = mappingCollection.ComputeMappingHashValue();

            if (mappingHashValue == _serializableViews?.HashValue)
                return;

            // generate the views
            var errors = new List<EdmSchemaError>();
            var views  = mappingCollection.GenerateViews(errors);

            if (errors.Any())
                throw new EdmSchemaException(errors);

            // serialize them
            _serializableViews = new SerializableViews(mappingHashValue, views);

            using (Stream stream = File.Open(_cacheFilePath, FileMode.Create))
                new BinaryFormatter().Serialize(stream, _serializableViews);
        }

        /// <summary>
        /// Gets a hash value computed over the mapping closure.
        /// </summary>
        public override string MappingHashValue => _serializableViews?.HashValue;

        /// <summary>
        /// Gets a view corresponding to the specified extent.
        /// </summary>
        /// <param name="extent">An <see cref="EntitySetBase" /> that specifies the extent.</param>
        /// <returns>A <see cref="DbMappingView" /> that specifies the mapping view,
        /// or null if the extent is not associated with a mapping view.</returns>
        public override DbMappingView GetView(
            EntitySetBase extent)
        {
            DbMappingView view = null;

            _serializableViews?.Cache?.TryGetValue(extent.Name, out view);

            return view;
        }
    }
}

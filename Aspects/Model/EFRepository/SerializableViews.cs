﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace vm.Aspects.Model.EFRepository
{
    [Serializable]
    class SerializableViews : ISerializable
    {
        const string Hash       = nameof(Hash);
        const string CacheCount = nameof(CacheCount);
        const string CacheKey   = nameof(CacheKey);
        const string CacheValue = nameof(CacheValue);

        readonly string _hash;
        readonly IReadOnlyDictionary<string, DbMappingView> _cache;

        public string HashValue => _hash;

        public IReadOnlyDictionary<string, DbMappingView> Cache => _cache;

        public SerializableViews(
            string hash,
            IDictionary<EntitySetBase, DbMappingView> cache)
        {
            if (hash.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(hash));
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            _hash  = hash;
            _cache = new ReadOnlyDictionary<string, DbMappingView>(
                            cache.ToDictionary(
                                    kv => kv.Key.Name,
                                    kv => new DbMappingView(kv.Value.EntitySql)));
        }

        protected SerializableViews(
            SerializationInfo info,
            StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _hash = info.GetString(Hash);

            var count = info.GetInt32(CacheCount);
            var cache = new Dictionary<string, DbMappingView>(count);

            for (var i = 0; i<count; i++)
                cache.Add(
                    info.GetString(CacheKey+i),
                    new DbMappingView(info.GetString(CacheValue+i)));

            _cache = new ReadOnlyDictionary<string, DbMappingView>(cache);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(
            SerializationInfo info,
            StreamingContext context)
        {
            info.AddValue(Hash, _hash);
            info.AddValue(CacheCount, _cache.Count());

            var i=0;

            foreach (var kv in _cache)
            {
                info.AddValue(CacheKey+i, kv.Key);
                info.AddValue(CacheValue+i, kv.Value.EntitySql);
                i++;
            }
        }
    }
}

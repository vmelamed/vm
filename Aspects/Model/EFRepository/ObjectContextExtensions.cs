﻿using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Class ObjectContextExtensions.
    /// </summary>
    public static class ObjectContextExtensions
    {
        /// <summary>
        /// Gets the name of the entity set of the type <paramref name="type"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">The type for which to find the entity set name.</param>
        /// <returns>The name of the entity set.</returns>
        public static string GetEntitySetName(
            this ObjectContext context,
            Type type)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            for (var t = type; t != typeof(object); t = t.BaseType)
            {
                var typeName = t.Name;

                if (typeName.Length > 64) // skip over the proxy types generated by EF
                    continue;

                var entitySetName = context.MetadataWorkspace
                                           .GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace)
                                           .BaseEntitySets
                                           .Where(meta => meta.ElementType.Name == typeName)
                                           .Select(m => m.Name)
                                           .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(entitySetName))
                    return entitySetName;
            }

            return null;
        }

        /// <summary>
        /// Gets the name of the entity set of the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type for which to find the entity set name.</typeparam>
        /// <param name="context">The context.</param>
        /// <returns>The name of the entity set.</returns>
        public static string GetEntitySetName<T>(
            this ObjectContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            for (var t = typeof(T); t != typeof(object); t = t.BaseType)
            {
                var typeName = t.Name;

                if (typeName.Length > 64) // skip over the proxy types generated by EF
                    continue;

                var entitySetName = context.MetadataWorkspace
                                           .GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace)
                                           .BaseEntitySets
                                           .Where(meta => meta.ElementType.Name == typeName)
                                           .Select(m => m.Name)
                                           .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(entitySetName))
                    return entitySetName;
            }

            return null;
        }
    }
}

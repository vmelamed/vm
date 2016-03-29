using System;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Changes the default type of <see cref="DateTime"/> properties to the 'datetime2' SQL Server type. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public sealed class DateTimeSqlTypeConvention : Convention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSqlTypeConvention"/> class.
        /// </summary>
        public DateTimeSqlTypeConvention()
        {
            Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
        }
    }
}

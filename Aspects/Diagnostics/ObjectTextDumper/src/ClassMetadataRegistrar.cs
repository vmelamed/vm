using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;

using vm.Aspects.Diagnostics.ExternalMetadata;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Class ClassMetadataRegistrar - helper for registering external dump metadata and type related <see cref="DumpAttribute"/>-s in a fluent API style.
    /// </summary>
    public class ClassMetadataRegistrar
    {
        /// <summary>
        /// Registers the metadata defined in <see cref="ExternalMetadata"/>.
        /// Allows for chaining further registering more dump metadata.
        /// </summary>
        /// <returns>ClassMetadataRegistrar.</returns>
        public static ClassMetadataRegistrar RegisterMetadata()
        {
            ClassMetadataResolver.SetClassDumpMetadata(typeof(Task<>), typeof(TaskGenericDumpMetadata));

            return new ClassMetadataRegistrar()
                .Register<Type, TypeDumpMetadata>()
                .Register<Exception, ExceptionDumpMetadata>()
                .Register<ArgumentException, ArgumentExceptionDumpMetadata>()
                .Register<SecurityException, SecurityExceptionDumpMetadata>()
                .Register<CultureInfo, CultureInfoDumpMetadata>()
                .Register<Task, TaskDumpMetadata>()
                .Register<ClaimsIdentity, ClaimsIdentityMetadata>()
                .Register<Claim, ClaimMetadata>()

                .Register<Expression, ExpressionDumpMetadata>()
                .Register<LambdaExpression, LambdaExpressionDumpMetadata>()
                .Register<ParameterExpression, ParameterExpressionDumpMetadata>()
                .Register<BinaryExpression, BinaryExpressionDumpMetadata>()
                .Register<ConstantExpression, ConstantExpressionDumpMetadata>()
                ;

            // Do not extend the BCL dependency, but the client can call also:
            //
            //using System.Data;
            //using System.Data.Metadata.Edm;
            //using System.Data.SqlClient;
            //using System.Net;
            //using Microsoft.Practices.EnterpriseLibrary.Validation;
            //using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
            //
            //.Register<SqlException, SqlExceptionDumpMetadata>()
            //.Register<SqlError, SqlErrorDumpMetadata>()
            //.Register<ArgumentValidationException, ArgumentValidationExceptionDumpMetadata>()
            //.Register<MetadataItem, MetadataItemDumpMetadata>()
            //.Register<UpdateException, UpdateExceptionDumpMetadata>()
            //.Register<ValidationResult, ValidationResultDumpMetadata>()
            //.Register<ValidationResults, ValidationResultsDumpMetadata>()
            //.Register<ConfigurationErrorsException, ConfigurationErrorsExceptionDumpMetadata>()
            //.Register<WebException, WebExceptionDumpMetadata>()
            // ;
        }

        /// <summary>
        /// Registers the dump metadata and <see cref="DumpAttribute" /> instance related to the specified type.
        /// </summary>
        /// <param name="type">The type for which the metadata is being registered.</param>
        /// <param name="metadataType">The dump metadata type.</param>
        /// <param name="dumpAttribute">The dump attribute.</param>
        /// <param name="replace">
        /// If set to <see langword="false" /> and there is already dump metadata associated with the <paramref name="type"/>
        /// the method will throw exception of type <see cref="InvalidOperationException"/>;
        /// otherwise it will silently override the existing metadata with <paramref name="metadataType"/> and <paramref name="dumpAttribute"/>.
        /// </param>
        /// <returns>The current instance of ClassMetadataRegistrar.</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="type" /> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="replace"/> is <see langword="false"/> and there is already metadata associated with the <paramref name="type"/>.
        /// </exception>
        public ClassMetadataRegistrar Register(
            Type type,
            Type? metadataType = null,
            DumpAttribute? dumpAttribute = null,
            bool replace = false)
        {
            ClassMetadataResolver.SetClassDumpMetadata(type, metadataType, dumpAttribute, replace);
            return this;
        }

        /// <summary>
        /// Registers the dump metadata and <see cref="DumpAttribute"/> instance related to the specified type.
        /// </summary>
        /// <typeparam name="T">The type for which the metadata is being registered.</typeparam>
        /// <typeparam name="TMetadata">The dump metadata type.</typeparam>
        /// <param name="dumpAttribute">The dump attribute.</param>
        /// <param name="replace">
        /// If set to <see langword="false" /> and there is already dump metadata associated with the <typeparamref name="T"/>
        /// the method will throw exception of type <see cref="InvalidOperationException"/>;
        /// otherwise it will silently override the existing metadata with <typeparamref name="TMetadata"/> and the <paramref name="dumpAttribute"/>.
        /// </param>
        /// <returns>The current instance of ClassMetadataRegistrar.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="replace"/> is <see langword="false"/> and there is already metadata associated with the <typeparamref name="T"/>.
        /// </exception>
        public ClassMetadataRegistrar Register<T, TMetadata>(
            DumpAttribute? dumpAttribute = null,
            bool replace = false) => Register(typeof(T), typeof(TMetadata), dumpAttribute, replace);

        /// <summary>
        /// Registers the specified dump attribute.
        /// </summary>
        /// <typeparam name="T">The type for which the dump attribute is being registered.</typeparam>
        /// <param name="dumpAttribute">The dump attribute.</param>
        /// <param name="replace">
        /// If set to <see langword="false" /> and there is already dump metadata associated with the <typeparamref name="T"/>
        /// the method will throw exception of type <see cref="InvalidOperationException"/>;
        /// otherwise it will silently override the existing metadata with itself - <typeparamref name="T"/> and the <paramref name="dumpAttribute"/>.
        /// </param>
        /// <returns>The current instance of ClassMetadataRegistrar.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="replace"/> is <see langword="false"/> and there is already metadata associated with the <typeparamref name="T"/>.
        /// </exception>
        public ClassMetadataRegistrar Register<T>(
            DumpAttribute? dumpAttribute,
            bool replace = false) => Register(typeof(T), null, dumpAttribute, replace);
    }
}

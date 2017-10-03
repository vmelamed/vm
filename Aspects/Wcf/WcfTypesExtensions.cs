using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Behaviors;
using vm.Aspects.Wcf.Bindings;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Class ServiceTypeExtensions. Introduces several extension methods to <see cref="Type"/> assuming that it represents
    /// the type of a WCF service, client or interface.
    /// </summary>
    public static class WcfTypesExtensions
    {
        /// <summary>
        /// Traverses all methods and their parameters of a type (usually service contract). 
        /// For each method return type, in or out parameter it calls a callback predicate which should return
        /// <see langword="true"/> if the traversal should stop or <see langword="false"/> to continue. 
        /// Based on the types of the return value and/or parameter the caller can determine if the interface requires 
        /// streaming transfer mode on the binding.
        /// </summary>
        /// <param name="type">The type to be traversed.</param>
        /// <param name="callback">The callback which examines each parameter type and return value type.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="type"/> does not represent interface or class type.</exception>
        static void TraverseParameters(
            Type type,
            Func<Type, bool, bool> callback)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!type.IsInterface && !type.IsClass)
                throw new ArgumentException("The parameter must represent either an interface or a class type.");

            // get all operations
            var operations = type.GetMethods()
                                 .Where(m => m.GetCustomAttribute<OperationContractAttribute>() != null)
                                 .ToArray();

            // look at the parameters for input stream
            foreach (var n in operations)
            {
                // pass the type of the parameter:
                var parameters = n.GetParameters();

                // streaming requires that we pass only one input or one output parameter
                if (parameters.Length > 1)
                    continue;

                // pass the type of the return value:
                if (callback(n.ReturnType, true))
                    return;

                if (parameters.Length == 0)
                    continue;

                var param = parameters[0];

                // pass the type of the parameter
                if (callback(param.ParameterType, param.IsOut||param.IsRetval))
                    return;
            }
        }

        /// <summary>
        /// Determines what type of transfer mode should be used for the specified class or interface.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><see langword="true"/> if the specified type is streaming; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The method enumerates all parameter types and the return value type looking for a parameter of type <see cref="T:System.IO.Stream"/> or
        /// a custom type with attribute <see cref="T:System.ServiceModel.MessageContractAttribute"/> and a property of type <see cref="T:System.IO.Stream"/> 
        /// in the message body.
        /// </remarks>
        public static TransferMode RequiredTransferMode(
            this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!type.IsInterface && !type.IsClass)
                throw new ArgumentException("The parameter must represent either an interface or a class type.");

            var transferMode = TransferMode.Buffered;

            TraverseParameters(
                type,
                (paramType, isOut) =>
                {
                    // the transfer mode that we might add to transferMode
                    var addMode = isOut ? TransferMode.StreamedResponse : TransferMode.StreamedRequest;

                    // if we know that the transfer mode already requires the mode that we might add - continue.
                    if (transferMode.HasFlag(addMode))
                        return false;

                    if (paramType == typeof(Stream))
                    {
                        transferMode |= addMode;

                        // stop the traversal if we already know that it should be fully streamed
                        return transferMode.HasFlag(TransferMode.Streamed);
                    }

                    // only message contract parameters can have Stream properties/fields
                    if (paramType.GetCustomAttribute<MessageContractAttribute>() == null)
                        return false;

                    // if the parameter type has any public properties which are Stream-s and are message body members - add the mode
                    if (paramType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Any(p => p.PropertyType == typeof(Stream)  &&
                                      p.GetCustomAttribute<MessageBodyMemberAttribute>(true) != null))
                    {
                        transferMode |= addMode;

                        // stop the traversal if we already know that it is fully streamed
                        return transferMode.HasFlag(TransferMode.Streamed);
                    }

                    // if the parameter type has any public fields which are Stream-s and are message body members - add the mode
                    if (paramType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                            .Any(f => f.FieldType == typeof(Stream)  &&
                                      f.GetCustomAttribute<MessageBodyMemberAttribute>(true) != null))
                    {
                        transferMode |= addMode;

                        // stop the traversal if we already know that it is fully streamed
                        return transferMode.HasFlag(TransferMode.Streamed);
                    }

                    return false;
                });

            return transferMode;
        }

        /// <summary>
        /// Determines whether the specified (service) type requires streaming mode.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><see langword="true"/> if the specified type is streaming; otherwise, <see langword="false"/>.</returns>
        public static bool RequiresStreaming(
            this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!type.IsInterface && !type.IsClass)
                throw new ArgumentException("The parameter must represent either an interface or a class type.");

            return type.RequiredTransferMode() != TransferMode.Buffered;
        }

        /// <summary>
        /// Extracts the resolve name of the service (or related type) from <see cref="DIBehaviorAttribute"/> if applied,
        /// and if a name or attribute is not found tries to extract it from <see cref="ResolveNameAttribute"/> if applied.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>The resolve name if found, otherwise <see langword="null"/></returns>
        public static string GetServiceResolveName(
            this Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            var serviceDIBehaviorAttribute = serviceType.GetCustomAttribute<DIBehaviorAttribute>(false);

            if (serviceDIBehaviorAttribute != null)
                return serviceDIBehaviorAttribute.ResolveName;
            else
                return serviceType.GetCustomAttribute<ResolveNameAttribute>(false)?.Name;
        }

        /// <summary>
        /// Extracts the messaging pattern of the service, client or interface (service contract) from <see cref="MessagingPatternAttribute"/> if applied.
        /// </summary>
        /// <param name="type">Type of the service, client or interface.</param>
        /// <returns>The resolve name if found, otherwise <see langword="null"/> - the default pattern request-response.</returns>
        public static string GetMessagingPattern(
            this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GetCustomAttribute<MessagingPatternAttribute>(false)?.Name;
        }

        /// <summary>
        /// Disposes correctly any communication object.
        /// </summary>
        /// <param name="co">The co.</param>
        public static void DisposeCommunicationObject(
            this ICommunicationObject co)
        {
            if (co == null)
                return;

            switch (co.State)
            {
            case CommunicationState.Opening:
            case CommunicationState.Opened:
            case CommunicationState.Created:
                try
                {
                    co.Close();
                }
                catch (CommunicationException ex)
                {
                    co.Abort();

                    // it would be would be nice to log and swallow this exception but 
                    // do not re-throw it as most likely there is another exception - 
                    // the root cause of this one - and we want that one handled properly.
                    Facility.LogWriter.ExceptionError(ex);
                }
                break;

            case CommunicationState.Closing:
            case CommunicationState.Faulted:
                co.Abort();
                break;

            case CommunicationState.Closed:
                break;
            }
        }

        /// <summary>
        /// Disposes correctly any communication object.
        /// </summary>
        /// <param name="co">The co.</param>
        public static Task DisposeCommunicationObjectAsync(
            this ICommunicationObject co)
        {

            if (co == null)
                return Task.FromResult(true);

            switch (co.State)
            {
            case CommunicationState.Opening:
            case CommunicationState.Opened:
            case CommunicationState.Created:
                try
                {
                    return Task.Factory.FromAsync(co.BeginClose, co.EndClose, null);
                }
                catch (CommunicationException ex)
                {
                    co.Abort();

                    // it would be would be nice to log and swallow this exception but 
                    // do not re-throw it as most likely there is another exception - 
                    // the root cause of this one - and we want that one handled properly.
                    Facility.LogWriter.ExceptionError(ex);
                }
                break;

            case CommunicationState.Closing:
            case CommunicationState.Faulted:
                co.Abort();
                break;

            case CommunicationState.Closed:
                break;
            }

            return Task.FromResult(true);
        }
    }
}

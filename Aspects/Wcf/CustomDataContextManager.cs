using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Helper class for managing the contexts' bindings for channels with custom data context.
    /// Based on Juval Lowe's code from [Programming WCF]
    /// </summary>
    public static class CustomDataContextManager
    {
        /// <summary>
        /// Creates a context.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>IDictionary&lt;System.String, System.String&gt;.</returns>
        public static IDictionary<string, string> CreateContext(
            string key,
            string value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return new SortedDictionary<string, string>
            {
                { key, value }
            };
        }

        /// <summary>
        /// Creates a new context out of an existing one by adding the new key-value pair.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>IDictionary&lt;System.String, System.String&gt;.</returns>
        public static IDictionary<string, string> CreateContext(
            IDictionary<string, string> context,
            string key,
            string value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return new SortedDictionary<string, string>(context)
            {
                { key, value }
            };
        }

        /// <summary>
        /// Creates a data context and sets it in the channel.
        /// </summary>
        /// <param name="innerChannel">The inner channel.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetContext(
            this IChannel innerChannel,
            string key,
            string value)
        {
            if (innerChannel == null)
                throw new ArgumentNullException(nameof(innerChannel));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            SetContext(innerChannel, CreateContext(key, value));
        }

        /// <summary>
        /// Sets an existing context in the channel.
        /// </summary>
        /// <param name="innerChannel">The inner channel.</param>
        /// <param name="context">The context.</param>
        public static void SetContext(
            this IChannel innerChannel,
            IDictionary<string, string> context)
        {
            if (innerChannel == null)
                throw new ArgumentNullException(nameof(innerChannel));

            innerChannel.GetProperty<IContextManager>().SetContext(context);
        }

        /// <summary>
        /// Updates the channel's existing context with the key-value pair.
        /// </summary>
        /// <param name="innerChannel">The inner channel.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>IDictionary&lt;System.String, System.String&gt;.</returns>
        public static IDictionary<string, string> UpdateContext(
            this IChannel innerChannel,
            string key,
            string value)
        {
            if (innerChannel == null)
                throw new ArgumentNullException(nameof(innerChannel));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return CreateContext(innerChannel.GetProperty<IContextManager>().GetContext(), key, value);
        }

        /// <summary>
        /// Sets the context in the proxy's channel.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proxy">The proxy.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetContext<T>(
            this ClientBase<T> proxy,
            string key,
            string value) where T : class
        {
            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            SetContext(proxy.InnerChannel, key, value);
        }

        /// <summary>
        /// Updates the context in the proxy's channel.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proxy">The proxy.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void UpdateContext<T>(
            this ClientBase<T> proxy,
            string key,
            string value) where T : class
        {
            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            UpdateContext(proxy.InnerChannel, key, value);
        }

        /// <summary>
        /// Gets the context from the current operation's context.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.String.</returns>
        public static string GetContext(
            string key)
        {
            if (OperationContext.Current != null &&
                OperationContext.Current.IncomingMessageProperties.TryGetValue(ContextMessageProperty.Name, out var contextProperty) &&
                ((ContextMessageProperty)contextProperty).Context.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }
    }
}

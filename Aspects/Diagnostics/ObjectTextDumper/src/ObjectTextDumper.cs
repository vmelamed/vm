using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Threading;

using vm.Aspects.Diagnostics.Implementation;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Delegate Script is the type of the generated dump script that can be cached and reused.
    /// </summary>
    /// <param name="instance">The instance being dumped.</param>
    /// <param name="classDumpMetadata">The data containing the metadata type and the dump attribute.</param>
    /// <param name="dumper">The dumper which has the current writer.</param>
    /// <param name="dumpState">The current state of the dump.</param>
    internal delegate void Script(
        object instance,
        ClassDumpMetadata classDumpMetadata,
        ObjectTextDumper dumper,
        DumpState dumpState);

    /// <summary>
    /// Class ObjectTextDumper. This class cannot be inherited. The main class which dumps the requested object.
    /// </summary>
    public sealed partial class ObjectTextDumper : IDisposable
    {
        static readonly ReaderWriterLockSlim _syncDefaultDumpSettings = new();
        static DumpSettings _defaultDumpSettings                      = DumpSettings.Default;

        /// <summary>
        /// Gets or sets the object dumper settings.
        /// </summary>
        public static DumpSettings DefaultDumpSettings
        {
            get
            {
                using var _ = new ReaderSlimSync(_syncDefaultDumpSettings);
                return _defaultDumpSettings;
            }

            set
            {
                using var _ = new WriterSlimSync(_syncDefaultDumpSettings);
                _defaultDumpSettings = value;
            }
        }

        #region Fields
        /// <summary>
        /// The current indent.
        /// </summary>
        internal int _indentLevel;

        /// <summary>
        /// The number of spaces in a single indent.
        /// </summary>
        internal readonly int _indentSize;
        #endregion

        #region Internal properties for access by the DumpState
        readonly ReaderWriterLockSlim _syncSettings = new();
        DumpSettings _settings                      = DumpSettings.Default;

        /// <summary>
        /// Gets the settings for the current instance.
        /// </summary>
        public DumpSettings InstanceSettings
        {
            get
            {
                using var _ = new ReaderSlimSync(_syncSettings);
                return _settings;
            }

            set
            {
                using var _ = new WriterSlimSync(_syncSettings);
                _settings = value;
            }
        }

        internal DumpSettings Settings { get; set; } = DefaultDumpSettings;

        /// <summary>
        /// The dump writer.
        /// </summary>
        internal TextWriter Writer { get; }

        internal int MaxDepth { get; private set; } = int.MinValue;

        /// <summary>
        /// Gets the member information comparer, used to order the dumped members in the desired sort order.
        /// </summary>
        internal IMemberInfoComparer MemberInfoComparer { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the currently dumped instance is sub-expression of a previously dumped expression.
        /// </summary>
        internal bool IsSubExpression { get; set; }

        /// <summary>
        /// Contains references to all dumped objects to avoid infinite dumping due to cyclical references.
        /// </summary>
        internal DumpedObjects DumpedObjects { get; } = new();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTextDumper" /> class with a text writer where to dump the object and initial indent
        /// level.
        /// </summary>
        /// <param name="writer">The text writer where to dump the object to.</param>
        /// <param name="memberInfoComparer">
        /// The member comparer used to order the dumped members in the desired sort order.
        /// If <see langword="null"/> the created here dumper instance will use the default member sorting order.
        /// </param>
        /// <exception cref="ArgumentNullException">writer</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="writer" /> is <c>null</c>.</exception>
        public ObjectTextDumper(
            TextWriter writer,
            IMemberInfoComparer? memberInfoComparer = null)
        {
            // wrap the writer in DumpTextWriter - handles better the indentations and
            // limits the dump output to DumpTextWriter.DefaultMaxLength
            Writer             = writer is StringWriter wr ? new DumpTextWriter(wr, Settings.MaxDumpLength) : writer;
            MemberInfoComparer = memberInfoComparer ?? new MemberInfoComparer();
            _indentSize        = Settings.IndentSize;
        }
        #endregion

        #region Public method/s
        /// <summary>
        /// Dumps the specified object in a text form to this object's <see cref="TextWriter" /> instance.
        /// </summary>
        /// <param name="value">The object to be dumped.</param>
        /// <param name="dumpMetadata">Optional metadata class to use to extract the dump parameters, options and settings. If not specified, the dump metadata will be
        /// extracted from the <see cref="MetadataTypeAttribute" /> attribute applied to <paramref name="value" />'s class if specified otherwise from
        /// the attributes applied within the class itself.</param>
        /// <param name="dumpAttribute">An explicit dump attribute to be applied at a class level. If not specified the <see cref="MetadataTypeAttribute" /> attribute applied to
        /// <paramref name="value" />'s class or <see cref="DumpAttribute.Default" /> will be assumed.</param>
        /// <param name="initialIndentLevel">The initial indent level.</param>
        /// <returns>The <paramref name="value" /> parameter.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ObjectTextDumper Dump(
            object? value,
            Type? dumpMetadata = null,
            DumpAttribute? dumpAttribute = null,
            int initialIndentLevel = DumpSettings.DefaultInitialIndentLevel)
        {
            try
            {
                // dump
                _indentLevel = initialIndentLevel >= DumpSettings.DefaultInitialIndentLevel ? initialIndentLevel : DumpSettings.DefaultInitialIndentLevel;
                MaxDepth     = int.MinValue;
                Settings     = InstanceSettings;

                Debug.Assert(DumpedObjects.IsEmpty);

                DumpObject(value, dumpMetadata, dumpAttribute);
            }
            catch (SecurityException)
            {
                Writer.WriteLine();
                Writer.Write(Resources.CallerDoesNotHavePermissionFormat, value?.ToString() ?? DumpUtilities.Null);
            }
            catch (TypeAccessException)
            {
                Writer.WriteLine();
                Writer.Write(Resources.CallerDoesNotHavePermissionFormat, value?.ToString() ?? DumpUtilities.Null);
            }
            catch (Exception x)
            {
                var message = @$"

ATTENTION:
The TextDumper threw an exception:
{x}";

                Writer.WriteLine(message);
                Debug.WriteLine(message);
            }
            finally
            {
                // clear the dumped objects register
                DumpedObjects.Clear();

                // prepare our dump text writer for a new dump
                (Writer as DumpTextWriter)?.Reset();
            }

            return this;
        }
        #endregion

        internal int IncrementMaxDepth() => MaxDepth++;

        internal int DecrementMaxDepth() => MaxDepth--;

        /// <summary>
        /// Increases the indentation of the writer by <see cref="_indentSize"/>.
        /// </summary>
        internal void Indent() => Writer.Indent(++_indentLevel, _indentSize);

        /// <summary>
        /// Decreases the indentation of the writer by <see cref="_indentSize"/>.
        /// </summary>
        internal void Unindent() => Writer.Unindent(--_indentLevel, _indentSize);

        #region Internal methods
        internal void DumpObject(
            object? obj,
            Type? dumpMetadata = null,
            DumpAttribute? dumpAttribute = null,
            DumpState? parentState = null)
        {
            if (Writer.DumpedBasicValue(obj, dumpAttribute)  ||
                Writer.DumpedBasicNullable(obj, dumpAttribute))
                return;

            // DumpedBasic... dumps null objects too, so we now know that objct is not null
            Debug.Assert(obj is not null);

            // resolve the class metadata and the dump attribute
            var objectType = obj.GetType();
            var classDumpMetadata = ClassMetadataResolver.GetClassDumpMetadata(objectType, dumpMetadata, dumpAttribute);

            // if we're too deep - stop here.
            if (MaxDepth == int.MinValue)
                MaxDepth = classDumpMetadata.DumpAttribute.MaxDepth;

            if (MaxDepth < 0)
            {
                Writer.Write(Resources.DumpReachedMaxDepth);
                return;
            }

            // save the IsSubExpression flag in the local variable,
            // as it will change if obj is Expression and IsSubExpression==false.
            // in the end of this method we restore the value from this local variable. See below.
            var isSubExpressionStore = IsSubExpression;

            // slow dump or compile and run a script?
            Script? script  = null; // the compiled dumping script
            var buildScript = Settings.UseDumpScriptCache  &&  (!obj.IsDynamicObject() || obj is ExpandoObject);
            using var state = new DumpState(this, obj, classDumpMetadata, buildScript);

            if (buildScript)
            {
                // does the script exist or is it in process of building
                if (DumpScriptCache.TryFind(this, obj, classDumpMetadata, out script))
                {
                    if (script != null)
                    {
                        if (parentState is null)
                            Writer.Indent(_indentLevel, _indentSize)
                                  .WriteLine();

                        script(obj, classDumpMetadata, this, state);
                    }

                    return;
                }

                DumpScriptCache.BuildingScriptFor(this, objectType, classDumpMetadata);
            }
            else
                if (parentState is null)
                Writer.Indent(_indentLevel, _indentSize)
                      .WriteLine();

            if (!state.DumpedAlready())     // the object has been dumped already (block circular and repeating references)
            {
                // this object will be dumped below.
                // Add it to the dumped objects now so that if nested property refers back to it, it won't be dumped in an infinite recursive chain.
                DumpedObjects.Add(obj);

                if (!state.DumpedCollection(classDumpMetadata.DumpAttribute, false))   // custom collections are dumped after dumping all other properties (see below*)
                {
                    var statesWithRemainingProperties = new Stack<DumpState>();
                    var statesWithTailProperties = new Queue<DumpState>();

                    // recursively dump all properties with non-negative order in class inheritance descending order (base classes' properties first)
                    // and if there are more properties  to be dumped put them in the stack
                    if (!DumpedTopProperties(state, statesWithRemainingProperties))
                    {
                        // dump all properties with negative order in class ascending order (derived classes' properties first)
                        DumpRemainingProperties(statesWithRemainingProperties, statesWithTailProperties);

                        // dump all properties with Order=int.MinValue in ascending order (derived classes' properties first)
                        DumpTailProperties(statesWithTailProperties);

                        // * if the object implements IEnumerable and the state allows it - dump the elements.
                        state.DumpedCollection(classDumpMetadata.DumpAttribute, true);
                    }

                    // we are done dumping
                    state.Unindent();
                }
            }

            if (buildScript  &&  state.DumpScript is not null)
                script = DumpScriptCache.Add(this, objectType, classDumpMetadata, state.DumpScript);

            if (!buildScript)
            {
                if (parentState is null)
                    Writer.Unindent(_indentLevel, _indentSize);
            }
            else
            {
                if (script is not null)
                {
                    if (parentState is null)
                        Writer.Indent(_indentLevel, _indentSize)
                              .WriteLine();

                    script(obj, classDumpMetadata, this, state);
                }
            }

            // restore here the IsSubExpression flag as it have changed if obj is Expression and IsSubExpression==false.
            IsSubExpression = isSubExpressionStore;
        }

        /// <summary>
        /// Dumps the properties with non-negative dump order.
        /// </summary>
        /// <param name="state">The current dump state.</param>
        /// <param name="statesWithRemainingProperties">The stack containing the states which have remaining properties.</param>
        /// <returns><c>true</c> if this is a dump tree leaf object, the current dump item is one of:
        /// <list type="bullet">
        /// <item><c>typeof(System.Object)</c></item><item><see cref="MemberInfo" /></item>
        /// <item>a delegate</item>
        /// <item>should not be dumped marked with <c>DumpAttribute(false)</c></item>
        /// </list>
        /// ; otherwise <c>false</c>.</returns>
        bool DumpedTopProperties(
            DumpState state,
            Stack<DumpState> statesWithRemainingProperties)
        {
            if (state.DumpedDelegate()    ||
                state.DumpedMemberInfo()  ||
                state.DumpedRootClass())
            {
                // we reached the root of the hierarchy,
                // indent and while unwinding the stack dump the properties from each descending class
                state.Indent();
                state.Dispose();
                return true;
            }

            var baseDumpState = state.GetBaseTypeDumpState();

            // 1. dump the properties of the base class with non-negative order
            DumpedTopProperties(baseDumpState, statesWithRemainingProperties);

            // 2. dump the non-negative order properties of the current class
            while (state.MoveNext())
            {
                // if we reached a negative order property,
                if (state.CurrentDumpAttribute!.Order < 0)
                {
                    // put the state in the stack of states with remaining properties to be dumped
                    // and suspend the dumping at this inheritance level
                    statesWithRemainingProperties.Push(state);
                    return false;
                }

                // otherwise dump the property
                state.DumpProperty();
            }

            // if we are here all properties have been dumped (no negative order properties)
            // and we do not need the state anymore.
            state.Dispose();

            return false;
        }

        /// <summary>
        /// Dumps the properties with negative dump order.
        /// </summary>
        /// <param name="statesWithRemainingProperties">The stack containing the states which have remaining properties.</param>
        /// <param name="statesWithTailProperties">The queue containing the states which have tail properties.</param>
        static void DumpRemainingProperties(
            Stack<DumpState> statesWithRemainingProperties,
            Queue<DumpState> statesWithTailProperties)
        {
            bool isTailProperty;

            while (statesWithRemainingProperties.Count > 0)
            {
                // pop the highest inheritance tree dump state from the stack
                var state = statesWithRemainingProperties.Pop();

                do
                {
                    isTailProperty = state.CurrentDumpAttribute!.Order == int.MinValue;

                    if (isTailProperty)
                    {
                        // the properties with order int.MinValue are to be dumped last (at the dump tail)
                        // put the state in the tail queue and suspend the dumping again
                        statesWithTailProperties.Enqueue(state);
                        break;
                    }

                    // otherwise dump the current property
                    state.DumpProperty();
                }
                while (state.MoveNext());

                // if all properties have been dumped (no int.MinValue order properties) we do not need the state anymore.
                if (!isTailProperty)
                    state.Dispose();
            }
        }

        /// <summary>
        /// Dumps the properties with order int.MinValue
        /// </summary>
        /// <param name="statesWithTailProperties">The bottom properties collection (usually a queue).</param>
        static void DumpTailProperties(
            IEnumerable<DumpState> statesWithTailProperties)
        {
            foreach (var state in statesWithTailProperties)
                using (state)
                    do
                        state.DumpProperty();
                    while (state.MoveNext());
        }
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag is being set when the object gets disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        int _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // these will be called only if the instance is not disposed and is not in a process of disposing.
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            // if the writer wraps foreign StringWriter, it is smart enough not to dispose it
            (Writer as DumpTextWriter)?.Dispose();

            _syncSettings.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

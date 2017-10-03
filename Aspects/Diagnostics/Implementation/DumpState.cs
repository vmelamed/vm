using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics.Implementation
{
    class DumpState : IEnumerator<MemberInfo>
    {
        #region fileds
        readonly ObjectTextDumper _dumper;
        readonly bool _isTopLevelClass;
        /// <summary>
        /// The current field or property's property info object
        /// </summary>
        MemberInfo _currentMemberInfo;
        /// <summary>
        /// The current field or property's dump attribute
        /// </summary>
        DumpAttribute _currentDumpAttribute;
        #endregion

        public DumpScript DumpScript { get; }

        public DumpState(
            ObjectTextDumper dumper,
            object instance,
            ClassDumpData classDumpData,
            bool buildScript)
            : this(dumper, instance, instance.GetType(), classDumpData, classDumpData.DumpAttribute, null, true)
        {
            if (dumper == null)
                throw new ArgumentNullException(nameof(dumper));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            // let's not create script if we don't need it or are not doing anything here.
            if (buildScript  &&  CurrentType != typeof(object))
                DumpScript = new DumpScript(instance.GetType());

            DecrementMaxDepth();
        }

        private DumpState(
            ObjectTextDumper dumper,
            object instance,
            Type type,
            ClassDumpData classDumpData,
            DumpAttribute instanceDumpAttribute,
            DumpScript dumpScript,
            bool isTopLevelClass)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (instanceDumpAttribute == null)
                throw new ArgumentNullException(nameof(instanceDumpAttribute));
            if (dumper == null)
                throw new ArgumentNullException(nameof(dumper));

            _dumper               = dumper;
            _isTopLevelClass      = isTopLevelClass;
            Instance              = instance;
            InstanceType          = instance.GetType();
            CurrentType           = type;
            ClassDumpData         = classDumpData;
            InstanceDumpAttribute = instanceDumpAttribute;
            DumpScript            = dumpScript;

            if (_isTopLevelClass)
            {
                var defaultProperty = DefaultProperty;

                if (!defaultProperty.IsNullOrWhiteSpace())
                {
                    var pi = CurrentType.GetProperty(defaultProperty);

                    Enumerator = pi != null
                                    ? (new MemberInfo[] { pi }).AsEnumerable().GetEnumerator()
                                    : (new MemberInfo[] { }).AsEnumerable().GetEnumerator();
                    return;
                }
            }

            Enumerator = CurrentType.GetProperties(_dumper.PropertiesBindingFlags | BindingFlags.DeclaredOnly)
                         .Union<MemberInfo>(
                         CurrentType.GetFields(_dumper.FieldsBindingFlags | BindingFlags.DeclaredOnly))
                            .Where(mi => !mi.Name.StartsWith("<", StringComparison.Ordinal))
                            .OrderBy(p => p, ServiceResolver
                                                .Default
                                                .GetInstance<IMemberInfoComparer>()
                                                .SetMetadata(ClassDumpData.Metadata))
                            .GetEnumerator();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public DumpState GetBaseTypeDumpState()
            => new DumpState(
                    _dumper,
                    Instance,
                    CurrentType.BaseType,
                    ClassMetadataResolver.GetClassDumpData(CurrentType.BaseType),
                    InstanceDumpAttribute,
                    DumpScript,
                    false);

        /// <summary>
        /// Gets a value indicating whether the state is in a slow dumping mode (no dump script generated - all is dumped via reflection).
        /// </summary>
        public bool IsInDumpingMode => DumpScript == null;

        /// <summary>
        /// Gets a value indicating whether the state is in a building dump script mode.
        /// </summary>
        public bool IsInScriptingMode => DumpScript != null;

        /// <summary>
        /// Gets or sets the currently dumped instance.
        /// </summary>
        public object Instance { get; }

        /// <summary>
        /// Gets or sets the current type (can be one of the base types) of the instance being dumped.
        /// </summary>
        public Type CurrentType { get; }

        /// <summary>
        /// Gets or sets the (most derived) type of the instance being dumped.
        /// </summary>
        public Type InstanceType { get; }

        /// <summary>
        /// Gets or sets the dump attribute applied to the instance .
        /// </summary>
        /// <value>
        /// The instance dump attribute.
        /// </value>
        public DumpAttribute InstanceDumpAttribute { get; }

        /// <summary>
        /// Gets or sets the class dump data pair - the metadata type and the class dump attribute.
        /// </summary>
        public ClassDumpData ClassDumpData { get; }

        /// <summary>
        /// Gets the current property being dumped.
        /// </summary>
        public MemberInfo CurrentProperty => Current;

        /// <summary>
        /// Gets the property dump attribute applied to the current property being dumped.
        /// </summary>
        public DumpAttribute CurrentPropertyDumpAttribute => _currentDumpAttribute;

        /// <summary>
        /// Calculates whether null property values of the current instance should be dumped.
        /// </summary>
        public ShouldDump DumpNullValues => ClassDumpData.DumpNullValues(InstanceDumpAttribute);

        /// <summary>
        /// Calculates whether to dump recursively the current instance.
        /// </summary>
        /// <value>This property never returns <see cref="ShouldDump.Default"/>.</value>
        public ShouldDump RecurseDump => ClassDumpData.RecurseDump(InstanceDumpAttribute);

        /// <summary>
        /// Gets the representative property of the current type that should not be dumped recursively.
        /// </summary>
        public string DefaultProperty => ClassDumpData.DefaultProperty(InstanceDumpAttribute);

        #region IEnumerator<MemberInfo> Members
        public IEnumerator<MemberInfo> Enumerator { get; }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public MemberInfo Current => _currentMemberInfo;
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_isTopLevelClass)
                IncrementMaxDepth();

            Enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members
        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            if (!Enumerator.MoveNext())
            {
                _currentMemberInfo    = null;
                _currentDumpAttribute = null;
                return false;
            }

            _currentMemberInfo    = Enumerator.Current;
            _currentDumpAttribute = PropertyDumpResolver.GetPropertyDumpAttribute(_currentMemberInfo, ClassDumpData.Metadata);
            return true;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            Enumerator.Reset();
            _currentMemberInfo    = null;
            _currentDumpAttribute = null;
        }

        #endregion

        public bool DumpedAlready()
        {
            var type = Instance.GetType();

            // stop the recursion at circular references
            if (!_dumper.DumpedObjects.Contains(new DumpedObject(Instance, type)))
                return false;

            DumpSeenAlready();

            return true;
        }

        public void DumpSeenAlready()
        {
            if (IsInDumpingMode)
            {
                var type = Instance.GetType();

                _dumper.Writer.Write(
                    DumpFormat.CyclicalReference,
                    type.GetTypeName(),
                    type.Namespace,
                    type.AssemblyQualifiedName);
            }
            else
                DumpScript.AddDumpSeenAlready();
        }

        public void DumpType()
        {
            if (IsInDumpingMode)
            {
                var type = Instance.GetType();

                _dumper.Writer.Write(
                    DumpFormat.Type,
                    type.GetTypeName(),
                    type.Namespace,
                    type.AssemblyQualifiedName);
            }
            else
                DumpScript.AddDumpType();
        }

        public void DumpExpressionCSharpText()
        {
            var expression = Instance as Expression;

            if (expression == null  ||  _dumper.IsSubExpression)
                return;

            // this is the highest level expression. all
            _dumper.IsSubExpression = true;

            var cSharpText = expression.DumpCSharpText();

            if (IsInDumpingMode)
            {
                _dumper.Indent();
                _dumper.Writer.WriteLine();
                _dumper.Writer.Write(DumpFormat.CSharpDumpLabel);
                _dumper.Indent();
                _dumper.Writer.WriteLine();
                _dumper.Writer.Write(cSharpText);
                _dumper.Unindent();
                _dumper.Unindent();
            }
            else
                DumpScript.AddDumpExpressionText(cSharpText);
        }

        internal void IncrementMaxDepth()
        {
            if (IsInDumpingMode)
                _dumper._maxDepth++;
            else
                DumpScript.AddIncrementMaxDepth();
        }

        internal void DecrementMaxDepth()
        {
            if (IsInDumpingMode)
                _dumper._maxDepth--;
            else
                DumpScript.AddDecrementMaxDepth();
        }

        internal void Indent()
        {
            if (IsInDumpingMode)
                _dumper.Indent();
            else
                DumpScript.AddIndent();
        }

        internal void Unindent()
        {
            if (IsInDumpingMode)
                _dumper.Unindent();
            else
                DumpScript.AddUnindent();
        }

        /// <summary>
        /// The dumping traverses depth-first a dump tree consisting of the object's properties its base classes' properties and the properties' 
        /// properties etc. This method determines if the recursion reached a leaf in the dump tree and that it should stop drilling down and return to 
        /// dump other branches of the dump tree. Recursion stops when:
        /// <list type="bullet">
        /// <item>
        /// The current examined type is <see cref="object"/>. The method dumps the object contained in <see cref="Instance"/>-s type name.
        /// </item>
        /// <item>
        /// The current examined base class has <see cref="DumpAttribute"/> with property <see cref="DumpAttribute.RecurseDump"/> set to <see cref="ShouldDump.Skip"/>. 
        /// If the attribute also defines the property <see cref="DumpAttribute.DefaultProperty"/> it will dump that property only as a representing
        /// property of the entire class.
        /// </item>
        /// <item>
        /// The examined object is a delegate. The method will dump the delegate type.
        /// </item>
        /// <item>
        /// The method determines that the current object has already been dumped (discovers circular reference). The method will dump a short 
        /// reference text.
        /// </item>
        /// </list>
        /// The current base class has class dump attribute with  examined is System.Object
        /// </summary>
        /// <returns><c>true</c> if the recursion should stop; otherwise <c>false</c>.</returns>
        public bool IsAtDumpTreeLeaf() => DumpedRootClass()  ||  // Stop the recursion if: at System.Object
                                          DumpedDelegate()   ||  // at delegates
                                          DumpedMemberInfo();    // at MemberInfo values

        public bool DumpedRootClass()
        {
            if (CurrentType!=typeof(object))
                return false;

            DumpType();
            DumpExpressionCSharpText();
            return true;
        }

        public bool DumpedDelegate()
        {
            if (!typeof(Delegate).IsAssignableFrom(CurrentType))
                return false;

            // it will be dumped at the descendant level
            if (CurrentType == typeof(MulticastDelegate) ||
                CurrentType == typeof(Delegate))
                return true;

            if (Instance == null)
                return false;

            if (IsInDumpingMode)
                _dumper.Writer.Dumped((Delegate)Instance);
            else
                DumpScript.AddDumpedDelegate();

            return true;
        }

        public bool DumpedMemberInfo()
        {
            var mi = Instance as MemberInfo;

            if (mi == null)
                return false;

            if (IsInDumpingMode)
                _dumper.Writer.Dumped(Instance as MemberInfo);
            else
                DumpScript.AddDumpedMemberInfo();

            return true;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "It's OK in the dumper")]
        public void DumpProperty()
        {
            // should we dump it at all?
            if (!CurrentProperty.CanRead()  ||  CurrentPropertyDumpAttribute.Skip == ShouldDump.Skip)
                return;

            var pi = CurrentProperty as PropertyInfo;

            // can't dump indexers
            if (pi != null  &&  pi.GetIndexParameters().Length > 0)
                return;

            if (pi != null  &&  pi.IsVirtual())
            {
                // for virtual properties dump the instance value at the the least derived class level that declares the property for first time.
                if (CurrentType.BaseType.GetProperty(CurrentProperty.Name, _dumper.PropertiesBindingFlags) != null)
                    return;

                pi = InstanceType.GetProperty(CurrentProperty.Name, _dumper.PropertiesBindingFlags);
            }

            var fi       = CurrentProperty as FieldInfo;
            Type type    = null;
            object value = null;

            try
            {
                if (pi != null)
                {
                    type  = pi.PropertyType;
                    value = pi.GetValue(Instance, null);
                }
                else
                {
                    type  = fi.FieldType;
                    value = fi.GetValue(Instance);
                }
            }
            catch (Exception x)
            {
                // this should not happen but...
                value = $"<{x.Message}>";
            }

            var dontDumpNulls = CurrentPropertyDumpAttribute.DumpNullValues==ShouldDump.Skip  ||
                                CurrentPropertyDumpAttribute.DumpNullValues==ShouldDump.Default && DumpNullValues==ShouldDump.Skip;

            if (IsInDumpingMode)
            {
                // should we dump a null value of the current property
                if (value == null  &&  dontDumpNulls)
                    return;

                // write the property header
                _dumper.Writer.WriteLine();
                _dumper.Writer.Write(
                    CurrentPropertyDumpAttribute.LabelFormat,
                    CurrentProperty.Name);

                if (!(DumpedPropertyCustom(value, type)                                        ||    // dump the property value using caller's customization (see ValueFormat="ToString", DumpClass, DumpMethod) if any.
                      _dumper.Writer.DumpedBasicValue(value, CurrentPropertyDumpAttribute)     ||
                      _dumper.Writer.DumpedBasicNullable(value, CurrentPropertyDumpAttribute)  ||
                      _dumper.Writer.Dumped(value as Delegate)                                 ||
                      _dumper.Writer.Dumped(value as MemberInfo)                               ||
                      DumpedCollection(value, CurrentProperty, CurrentPropertyDumpAttribute)))
                {
                    // dump a property representing an associated class or struct object
                    var currentPropertyDumpAttribute = !CurrentPropertyDumpAttribute.IsDefaultAttribute() ? CurrentPropertyDumpAttribute : null;

                    _dumper.DumpObject(value, null, currentPropertyDumpAttribute, this);
                }
            }
            else
            {
                // write the property header
                DumpScript.BeginDumpProperty(CurrentProperty, CurrentPropertyDumpAttribute);

                if (!DumpedPropertyCustom(value, type))                                             // dump the property value using caller's customization (see ValueFormat="ToString", DumpClass, DumpMethod) if any.
                    DumpScript.AddDumpPropertyOrCollectionValue(CurrentProperty, CurrentPropertyDumpAttribute);

                DumpScript.EndDumpProperty(CurrentProperty, dontDumpNulls);
            }
        }

        public bool DumpedCollection(
            DumpAttribute dumpAttribute,
            bool enumerateCustom)
            => DumpedCollection(Instance, null, dumpAttribute, enumerateCustom, true);

        bool DumpedCollection(
            object value,
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            bool enumerateCustom = false,
            bool newLineForCustom = false)
        {
            var sequence = value as IEnumerable;

            if (sequence == null)
                return false;

            return DumpedDictionary(sequence, mi, dumpAttribute)  ||
                   DumpedSequence(sequence, mi, dumpAttribute, enumerateCustom, newLineForCustom);
        }

        bool DumpedDictionary(
            IEnumerable sequence,
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (dumpAttribute == null)
                throw new ArgumentNullException(nameof(dumpAttribute));

            if (sequence.IsDynamicObject())
                return false;

            if (IsInDumpingMode)
                return _dumper.Writer.DumpedDictionary(
                                            sequence,
                                            dumpAttribute,
                                            o => _dumper.DumpObject(o, null, null, this),
                                            _dumper.Indent,
                                            _dumper.Unindent);
            else
            {
                if (sequence.GetType().DictionaryTypeArguments() == null)
                    return false;

                if (mi != null)
                    DumpScript.AddDumpedDictionary(mi, dumpAttribute);
                else
                    DumpScript.AddDumpedDictionary(dumpAttribute);
            }

            return true;
        }

        bool DumpedSequence(
            IEnumerable sequence,
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            bool enumerateCustom = false,
            bool newLineForCustom = false)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (dumpAttribute == null)
                throw new ArgumentNullException(nameof(dumpAttribute));

            var sequenceType = sequence.GetType();
            var isCustom     = !sequenceType.IsArray  &&  !sequenceType.IsFromSystem();
            var dumpCustom   = enumerateCustom  &&  dumpAttribute.Enumerate == ShouldDump.Dump;

            if (isCustom  &&  !dumpCustom)
                return false;

            if (IsInDumpingMode)
            {
                if (isCustom  &&  newLineForCustom)
                    _dumper.Writer.WriteLine();

                return _dumper.Writer.DumpedCollection(
                                            sequence,
                                            dumpAttribute,
                                            o => _dumper.DumpObject(o, null, null, this),
                                            _dumper.Indent,
                                            _dumper.Unindent);
            }
            else
            {
                if (mi != null)
                    DumpScript.AddDumpedCollection(mi, dumpAttribute);
                else
                    DumpScript.AddDumpedCollection(dumpAttribute);

                return true;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "vm.Aspects.Diagnostics.DumpImplementation.DumpScript.AddWrite(System.String,System.String,System.Int32)")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "vm.Aspects.Diagnostics.DumpImplementation.DumpScript.AddWrite(System.String)")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It's OK.")]
        bool DumpedPropertyCustom(
            object value,
            Type type)
        {
            if (value == null)
                return false;

            // did they specify DumpAttribute.ValueFormat="ToString"?
            if (CurrentPropertyDumpAttribute.ValueFormat == Resources.ValueFormatToString)
            {
                if (IsInDumpingMode)
                    _dumper.Writer.Write(value.ToString());
                else
                    DumpScript.AddCustomDumpPropertyOrField(CurrentProperty, null);

                return true;
            }

            // did they specify DumpAttribute.DumpMethod?
            var dumpMethodName = CurrentPropertyDumpAttribute.DumpMethod;
            var dumpClass      = CurrentPropertyDumpAttribute.DumpClass;

            if (dumpClass == null  &&  dumpMethodName.IsNullOrWhiteSpace())
                return false;

            if (dumpMethodName.IsNullOrWhiteSpace())
                dumpMethodName = "Dump";

            MethodInfo dumpMethod  = null;  // best match
            MethodInfo dumpMethod2 = null;  // second best

            // try the external class if specified
            if (dumpClass != null)
            {
                foreach (var mi in dumpClass.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                            .Where(mi => mi.Name == dumpMethodName  &&
                                                         mi.ReturnType == typeof(string)  &&
                                                         mi.GetParameters().Count() == 1))
                {
                    if (mi.GetParameters()[0].ParameterType == type)
                    {
                        // exact match
                        dumpMethod = mi;
                        break;
                    }

                    if (mi.GetParameters()[0].ParameterType.IsAssignableFrom(type))
                        // candidate
                        dumpMethod = mi;
                }

                if (dumpMethod != null)
                {
                    if (IsInDumpingMode)
                        _dumper.Writer.Write((string)dumpMethod.Invoke(null, new object[] { value }));
                    else
                        DumpScript.AddCustomDumpPropertyOrField(CurrentProperty, dumpMethod);
                }
                else
                {
                    var message0 = string.Format(CultureInfo.InvariantCulture, Resources.CouldNotFindCustomDumpers, dumpMethodName, type.FullName, dumpClass.FullName);

                    if (IsInDumpingMode)
                        _dumper.Writer.Write(message0);
                    else
                        DumpScript.AddWrite(message0);
                }

                return true;
            }

            // try the property's class or base class, or metadata class
            foreach (var mi in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                                   .Where(mi => mi.Name == dumpMethodName  &&
                                                mi.ReturnType == typeof(string))
                               .Union(
                               type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                   .Where(mi => mi.Name == dumpMethodName  &&
                                                mi.ReturnType == typeof(string)  &&
                                                mi.GetParameters().Count() == 1))
                               .Union(
                               ClassDumpData.Metadata
                                   .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                   .Where(mi => mi.Name == dumpMethodName  &&
                                                mi.ReturnType == typeof(string)  &&
                                                mi.GetParameters().Count() == 1)))
            {
                // found an instance method
                if (!mi.IsStatic && mi.GetParameters().Count() == 0)
                {
                    dumpMethod = mi;
                    break;
                }

                if (mi.IsStatic)
                {
                    if (mi.GetParameters()[0].ParameterType == type)
                        dumpMethod = mi;
                    else
                    {
                        if (mi.GetParameters()[0].ParameterType.IsAssignableFrom(type))
                            dumpMethod2 = mi;
                    }
                }
            }

            if (dumpMethod == null)
                dumpMethod = dumpMethod2;

            if (dumpMethod != null)
            {
                if (IsInDumpingMode)
                {
                    var text = dumpMethod.IsStatic
                                    ? (string)dumpMethod.Invoke(null, new object[] { value })
                                    : (string)dumpMethod.Invoke(value, null);

                    _dumper.Writer.Write(text);
                }
                else
                    DumpScript.AddCustomDumpPropertyOrField(CurrentProperty, dumpMethod);

                return true;
            }

            var message = string.Format(CultureInfo.InvariantCulture, Resources.CouldNotFindCustomDumpers, dumpMethodName, type.FullName, type.FullName);

            if (IsInDumpingMode)
                _dumper.Writer.Write(message);
            else
                DumpScript.AddWrite(message);

            return true;
        }
    }
}

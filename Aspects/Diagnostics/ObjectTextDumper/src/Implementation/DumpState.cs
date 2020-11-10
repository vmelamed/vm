using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics.Implementation
{
    class DumpState : IEnumerator<MemberInfo?>
    {
        public DumpState(
            ObjectTextDumper dumper,
            object instance,
            ClassDumpData classDumpData,
            bool buildScript)
            : this(dumper, instance, instance.GetType(), classDumpData, classDumpData.DumpAttribute)
        {
            // let's not create script if we don't need it or we are not doing anything here (super type is object).
            if (buildScript  &&
                (instance.GetType() == typeof(object)  ||  SuperType != typeof(object)))
                DumpScript = new DumpScript(instance.GetType());

            DecrementMaxDepth();
        }

        private DumpState(
            ObjectTextDumper dumper,
            object instance,
            Type superType,
            ClassDumpData classDumpData,
            DumpAttribute instanceDumpAttribute,
            DumpScript? dumpScript = null)
        {
            Dumper                = dumper;
            Instance              = instance;
            InstanceType          = instance.GetType();
            SuperType             = superType;
            IsTopLevelClass       = InstanceType == SuperType;
            ClassDumpData         = classDumpData;
            InstanceDumpAttribute = ClassMetadataResolver.CombineDumpAttributes(instanceDumpAttribute, ClassDumpData.DumpAttribute);
            DumpScript            = dumpScript;

            if (InstanceDumpAttribute.RecurseDump == ShouldDump.Skip)
            {
                // dump only the default property, if any
                var pi = InstanceDumpAttribute.DefaultProperty is not ""
                            ? SuperType.GetProperty(InstanceDumpAttribute.DefaultProperty)
                            : null;

                Enumerator = pi is not null
                                ? (new MemberInfo[] { pi }).AsEnumerable().GetEnumerator()
                                : (Array.Empty<MemberInfo>()).AsEnumerable().GetEnumerator();

                return;
            }

            // dump all properties and fields
            Enumerator = SuperType.GetProperties(Dumper.Settings.PropertyBindingFlags | BindingFlags.DeclaredOnly)
                                    .Union<MemberInfo>(
                         SuperType.GetFields(Dumper.Settings.FieldBindingFlags | BindingFlags.DeclaredOnly))
                                    .Where(mi => !mi.Name.StartsWith("<", StringComparison.Ordinal))
                                    .OrderBy(p => p, dumper.MemberInfoComparer.SetMetadata(classDumpData.Metadata))
                                    .ToList()
                                    .GetEnumerator();
            CurrentMember = Enumerator.Current;
        }

        public DumpState GetBaseTypeDumpState() =>
            new DumpState(
                    Dumper,
                    Instance,
                    SuperType.BaseType ?? throw new ArgumentException("System.Object does not have base type."),
                    ClassMetadataResolver.GetClassDumpData(SuperType.BaseType, dumpAttribute: InstanceDumpAttribute),
                    InstanceDumpAttribute,
                    DumpScript);

        public ObjectTextDumper Dumper { get; }

        public DumpScript? DumpScript { get; }

        public bool IsTopLevelClass { get; }

        /// <summary>
        /// Gets a value indicating whether the state is in a slow dumping mode (no dump script generated - all is dumped via reflection).
        /// </summary>
        public bool IsInDumpingMode => DumpScript is null;

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
        public Type SuperType { get; }

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
        /// Gets the current member being dumped.
        /// </summary>
        public MemberInfo? CurrentMember { get; private set; }

        /// <summary>
        /// Gets the property dump attribute applied to the current property being dumped.
        /// </summary>
        public DumpAttribute? CurrentDumpAttribute { get; private set; }

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
        public MemberInfo? Current => Enumerator.Current;
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsTopLevelClass)
                IncrementMaxDepth();

            Enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members
        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        object? IEnumerator.Current => CurrentMember;

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
                CurrentMember        = null;
                CurrentDumpAttribute = null;
                return false;
            }
            else
            {
                CurrentMember        = Enumerator.Current;
                CurrentDumpAttribute = PropertyDumpResolver.GetPropertyDumpAttribute(CurrentMember, ClassDumpData.Metadata);
                return true;
            }
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            Enumerator.Reset();
            CurrentMember        = null;
            CurrentDumpAttribute = null;
        }

        #endregion

        public bool DumpedAlready()
        {
            var type = Instance.GetType();

            // stop the recursion at circular references
            if (!Dumper.DumpedObjects.Contains(new DumpedObject(Instance, type)))
                return false;

            DumpSeenAlready();
            return true;
        }

        public void DumpSeenAlready()
        {
            if (DumpScript is null)
                Dumper.Writer.Write(
                    DumpFormat.CyclicalReference,
                    InstanceType.GetTypeName(),
                    InstanceType.Namespace,
                    InstanceType.AssemblyQualifiedName);
            else
                DumpScript.AddDumpSeenAlready();
        }

        public void DumpType()
        {
            if (DumpScript is null)
                Dumper.Writer.Write(
                    DumpFormat.Type,
                    InstanceType.GetTypeName(),
                    InstanceType.Namespace,
                    InstanceType.AssemblyQualifiedName);
            else
                DumpScript.AddDumpType();
        }

        public void DumpExpressionCSharpText()
        {
            if (Instance is Expression expression  &&  !Dumper.IsSubExpression)
            {
                // this is the highest level expression.
                Dumper.IsSubExpression = true;

                var cSharpText = expression.DumpCSharpText();

                if (DumpScript is null)
                {
                    Dumper.Indent();
                    Dumper.Writer.WriteLine();
                    Dumper.Writer.Write(DumpFormat.CSharpDumpLabel);
                    Dumper.Indent();
                    Dumper.Writer.WriteLine();
                    Dumper.Writer.Write(cSharpText);
                    Dumper.Unindent();
                    Dumper.Unindent();
                }
                else
                    DumpScript.AddDumpExpressionText(cSharpText);
            }
        }

        internal void IncrementMaxDepth()
        {
            if (DumpScript is null)
                Dumper.IncrementMaxDepth();
            else
                DumpScript.AddIncrementMaxDepth();
        }

        internal void DecrementMaxDepth()
        {
            if (DumpScript is null)
                Dumper.DecrementMaxDepth();
            else
                DumpScript.AddDecrementMaxDepth();
        }

        internal void Indent()
        {
            if (DumpScript is null)
                Dumper.Indent();
            else
                DumpScript.AddIndent();
        }

        internal void Unindent()
        {
            if (DumpScript is null)
                Dumper.Unindent();
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
            if (SuperType!=typeof(object))
                return false;

            DumpType();
            DumpExpressionCSharpText();
            return true;
        }

        public bool DumpedDelegate()
        {
            if (!typeof(Delegate).IsAssignableFrom(SuperType))
                return false;

            // it will be dumped at the descendant level
            if (SuperType == typeof(MulticastDelegate) ||
                SuperType == typeof(Delegate))
                return true;

            if (Instance == null)
                return false;

            if (DumpScript is null)
                Dumper.Writer.Dumped((Delegate)Instance);
            else
                DumpScript.AddDumpedDelegate();

            return true;
        }

        public bool DumpedMemberInfo()
        {
            if (Instance is not MemberInfo)
                return false;

            if (DumpScript is null)
                Dumper.Writer.Dumped(Instance as MemberInfo);
            else
                DumpScript.AddDumpedMemberInfo();

            return true;
        }

        public void DumpProperty()
        {
            Debug.Assert(CurrentMember is not null && CurrentDumpAttribute is not null);

            // should we dump it at all?
            if (!CurrentMember.CanRead()  ||  CurrentDumpAttribute.Skip == ShouldDump.Skip)
                return;

            var pi = CurrentMember as PropertyInfo;

            // can't dump indexers
            if (pi?.GetIndexParameters().Length > 0)
                return;

            if (pi?.IsVirtual() is true)
            {
                // for virtual properties dump the instance value at the the least derived class level that declares the property for first time.
                if (SuperType.BaseType?.GetProperty(CurrentMember.Name, Dumper.Settings.PropertyBindingFlags) != null)
                    return;

                pi = InstanceType.GetProperty(CurrentMember.Name, Dumper.Settings.PropertyBindingFlags);
            }

            var fi = CurrentMember as FieldInfo;
            Type type;
            object? value;

            try
            {
                if (pi != null)
                {
                    type  = pi.PropertyType;
                    value = pi.GetValue(Instance, null);
                }
                else
                {
                    Debug.Assert(fi is not null);
                    type  = fi.FieldType;
                    value = fi.GetValue(Instance);
                }
            }
            catch (Exception x)
            {
                // this should not happen but...
                type  = typeof(void);
                value = $"<{x.Message}>";
            }

            if (DumpScript is null)
            {
                var dontDumpNulls = CurrentDumpAttribute.DumpNullValues==ShouldDump.Skip  ||
                                    CurrentDumpAttribute.DumpNullValues==ShouldDump.Default && DumpNullValues==ShouldDump.Skip;

                // should we dump a null value of the current property
                if (value == null  &&  dontDumpNulls)
                    return;

                // write the property header
                Dumper.Writer.WriteLine();
                Dumper.Writer.Write(
                    CurrentDumpAttribute.LabelFormat,
                    CurrentMember.Name);

                if (!(DumpedPropertyCustom(value, type)                               ||    // dump the property value using caller's customization (see ValueFormat="ToString", DumpClass, DumpMethod) if any.
                      Dumper.Writer.DumpedBasicValue(value, CurrentDumpAttribute)     ||
                      Dumper.Writer.DumpedBasicNullable(value, CurrentDumpAttribute)  ||
                      Dumper.Writer.Dumped(value as Delegate)                         ||
                      Dumper.Writer.Dumped(value as MemberInfo)                       ||
                      DumpedCollection(value, CurrentMember, CurrentDumpAttribute)))
                {
                    // dump a property representing an associated class or struct object
                    var currentPropertyDumpAttribute = !CurrentDumpAttribute.IsDefaultAttribute() ? CurrentDumpAttribute : null;

                    Dumper.DumpObject(value, null, currentPropertyDumpAttribute, this);
                }
            }
            else
            {
                var dontDumpNulls = CurrentDumpAttribute.DumpNullValues==ShouldDump.Skip  ||
                                    CurrentDumpAttribute.DumpNullValues==ShouldDump.Default && DumpNullValues==ShouldDump.Skip;

                // write the property header
                DumpScript.BeginDumpProperty(CurrentMember, CurrentDumpAttribute);

                if (!DumpedPropertyCustom(value, type))                                             // dump the property value using caller's customization (see ValueFormat="ToString", DumpClass, DumpMethod) if any.
                    DumpScript.AddDumpPropertyOrCollectionValue(CurrentMember, CurrentDumpAttribute);

                DumpScript.EndDumpProperty(CurrentMember, dontDumpNulls);
            }
        }

        public bool DumpedCollection(
            DumpAttribute dumpAttribute,
            bool enumerateCustom) => DumpedCollection(Instance, null, dumpAttribute, enumerateCustom, true);

        internal bool DumpedCollection(
            object? value,
            MemberInfo? mi,
            DumpAttribute dumpAttribute,
            bool enumerateCustom = false,
            bool newLineForCustom = false) =>
                value is IEnumerable sequence  &&
                (DumpedDictionary(sequence, mi, dumpAttribute)  ||
                 DumpedSequence(sequence, mi, dumpAttribute, enumerateCustom, newLineForCustom));

        bool DumpedDictionary(
            IEnumerable sequence,
            MemberInfo? mi,
            DumpAttribute dumpAttribute)
        {
            if (sequence.IsDynamicObject())
                return false;

            if (DumpScript is null)
                return Dumper.Writer.DumpedDictionary(
                                            sequence,
                                            dumpAttribute,
                                            o => Dumper.DumpObject(o, null, null, this),
                                            Dumper.Indent,
                                            Dumper.Unindent);

            if (sequence.GetType().DictionaryTypeArguments().keyType == typeof(void))
                return false;

            _ = mi is null
                ? DumpScript.AddDumpedDictionary(dumpAttribute)
                : DumpScript.AddDumpedDictionary(mi, dumpAttribute);

            return true;
        }

        bool DumpedSequence(
            IEnumerable sequence,
            MemberInfo? mi,
            DumpAttribute dumpAttribute,
            bool enumerateCustom = false,
            bool newLineForCustom = false)
        {
            var sequenceType = sequence.GetType();
            var isCustom     = !sequenceType.IsArray  &&  !sequenceType.IsFromSystem();
            var dumpCustom   = enumerateCustom  &&  dumpAttribute.Enumerate == ShouldDump.Dump;

            if (isCustom  &&  !dumpCustom)
                return false;

            if (DumpScript is null)
            {
                if (isCustom  &&  newLineForCustom)
                    Dumper.Writer.WriteLine();
                return Dumper.Writer.DumpedCollection(
                                            sequence,
                                            dumpAttribute,
                                            o => Dumper.DumpObject(o, null, null, this),
                                            Dumper.Indent,
                                            Dumper.Unindent);
            }
            else
            {
                _ = mi is null
                    ? DumpScript.AddDumpedCollection(dumpAttribute)
                    : DumpScript.AddDumpedCollection(mi, dumpAttribute);
                return true;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It's OK.")]
        internal bool DumpedPropertyCustom(
            object? value,
            Type type)
        {
            if (value is null)
                return false;

            Debug.Assert(CurrentDumpAttribute is not null);

            // did they specify DumpAttribute.ValueFormat="ToString"?
            if (CurrentDumpAttribute.ValueFormat == Resources.ValueFormatToString)
            {
                if (DumpScript is null)
                    Dumper.Writer.Write(value.ToString());
                else
                    DumpScript.AddCustomDumpPropertyOrField(CurrentMember!);

                return true;
            }

            // did they specify DumpAttribute.DumpMethod?
            if (!CurrentDumpAttribute.HasDumpClass  &&  !CurrentDumpAttribute.HasDumpMethod)
                return false;

            var dumpMethodName = CurrentDumpAttribute.HasDumpMethod ? CurrentDumpAttribute.DumpMethod : "Dump";

            MethodInfo? dumpMethod  = null;  // best match
            MethodInfo? dumpMethod2 = null;  // second best

            // try the external class if specified
            if (CurrentDumpAttribute.HasDumpClass)
            {
                var dumpClass = CurrentDumpAttribute.DumpClass;

                foreach (var mi in dumpClass.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                            .Where(mi => mi.Name                   == dumpMethodName  &&
                                                         mi.ReturnType             == typeof(string)  &&
                                                         mi.GetParameters().Length == 1))
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

                if (dumpMethod is not null)
                {
                    if (DumpScript is null)
                    {
                        var dumpString = dumpMethod.Invoke(null, new object[] { value }) as string;

                        if (dumpString is not null)
                            Dumper.Writer.Write(dumpString);
                    }
                    else
                        DumpScript.AddCustomDumpPropertyOrField(CurrentMember!, dumpMethod);

                    return true;
                }

                var message0 = string.Format(
                                        CultureInfo.InvariantCulture,
                                        Resources.CouldNotFindCustomDumpers,
                                        dumpMethodName,
                                        type.FullName,
                                        dumpClass.FullName);

                if (DumpScript is null)
                    Dumper.Writer.Write(message0);
                else
                    DumpScript.AddWrite(message0);

                return true;
            }

            // try the property's class or base class, or the metadata class
            foreach (var mi in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                                   .Where(mi => mi.Name                   == dumpMethodName  &&
                                                mi.ReturnType             == typeof(string))
                               .Union(
                               type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                   .Where(mi => mi.Name                   == dumpMethodName  &&
                                                mi.ReturnType             == typeof(string)  &&
                                                mi.GetParameters().Length == 1))
                               .Union(
                               ClassDumpData.Metadata
                                   .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                   .Where(mi => mi.Name                   == dumpMethodName  &&
                                                mi.ReturnType             == typeof(string)  &&
                                                mi.GetParameters().Length == 1)))
            {
                // found an instance method - done!
                if (!mi.IsStatic && mi.GetParameters().Length == 0)
                {
                    dumpMethod = mi;
                    break;
                }

                if (mi.IsStatic)
                {
                    // found static method but keep looking for instance method
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
                if (DumpScript is null)
                {
                    var text = (dumpMethod.IsStatic
                                    ? dumpMethod.Invoke(null, new object[] { value })
                                    : dumpMethod.Invoke(value, null)) as string;

                    if (text is not null)
                    {
                        Dumper.Writer.Write(text);
                        return true;
                    }
                }
                else
                {
                    DumpScript.AddCustomDumpPropertyOrField(CurrentMember!, dumpMethod);
                    return true;
                }
            }

            var message = string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.CouldNotFindCustomDumpers,
                            dumpMethodName, type.FullName, type.FullName);

            if (DumpScript is null)
                Dumper.Writer.Write(message);
            else
                DumpScript.AddWrite(message);

            return true;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics.Implementation
{
    class DumpState : IEnumerator<MemberInfo?>
    {
        #region Constructors
        public DumpState(
            ObjectTextDumper dumper,
            object instance,
            ClassDumpMetadata classDumpMetadata,
            bool buildScript)
            : this(dumper, instance, instance.GetType(), classDumpMetadata, classDumpMetadata.DumpAttribute, null, buildScript)
        {
            DecrementMaxDepth();
        }

        // The non-nullable MemberDumper will be initialized in the public constructor, dep. on the type of dump
        private DumpState(
            ObjectTextDumper dumper,
            object instance,
            Type superType,
            ClassDumpMetadata classDumpMetadata,
            DumpAttribute instanceDumpAttribute,
            DumpScript? dumpScript = null,
            bool buildScript = false)
        {
            Dumper                = dumper;
            Instance              = instance;
            InstanceType          = instance.GetType();
            SuperType             = superType;
            IsTopLevelClass       = InstanceType == SuperType;
            ClassDumpMetadata     = classDumpMetadata;
            InstanceDumpAttribute = ClassMetadataResolver.CombineDumpAttributes(instanceDumpAttribute, ClassDumpMetadata.DumpAttribute);
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
            }
            else
            {
                // dump all properties and fields
                Enumerator = SuperType.GetProperties(Dumper.Settings.PropertyBindingFlags | BindingFlags.DeclaredOnly)
                                        .Union<MemberInfo>(
                             SuperType.GetFields(Dumper.Settings.FieldBindingFlags | BindingFlags.DeclaredOnly))
                                        .Where(mi => !mi.Name.StartsWith("<", StringComparison.Ordinal))
                                        .OrderBy(p => p, dumper.MemberInfoComparer.SetMetadata(classDumpMetadata.Metadata))
                                        .ToList()
                                        .GetEnumerator();
                CurrentMember = Enumerator.Current;
            }

            // let's not create script if we don't need it or we are not doing anything here (super type is object).
            if (buildScript is true  &&  DumpScript is null  &&
                (instance.GetType() == typeof(object)  ||  SuperType != typeof(object)))
                DumpScript = new DumpScript(instance.GetType());

            MemberDumper = DumpScript is not null
                                ? new ScriptMemberDumper(this)
                                : new WriterMemberDumper(this);
        }
        #endregion

        #region Properties
        IMemberDumper MemberDumper { get; }

        public ObjectTextDumper Dumper { get; }

        public DumpScript? DumpScript { get; }

        public bool IsTopLevelClass { get; }

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
        public ClassDumpMetadata ClassDumpMetadata { get; }

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
        public ShouldDump DumpNullValues => ClassDumpMetadata.DumpNullValues(InstanceDumpAttribute);

        /// <summary>
        /// Calculates whether to dump recursively the current instance.
        /// </summary>
        /// <value>This property never returns <see cref="ShouldDump.Default"/>.</value>
        public ShouldDump RecurseDump => ClassDumpMetadata.RecurseDump(InstanceDumpAttribute);

        /// <summary>
        /// Gets the representative property of the current type that should not be dumped recursively.
        /// </summary>
        public string DefaultProperty => ClassDumpMetadata.DefaultProperty(InstanceDumpAttribute);
        #endregion

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
                CurrentDumpAttribute = PropertyDumpResolver.GetPropertyDumpAttribute(CurrentMember, ClassDumpMetadata.Metadata);
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

        public DumpState GetBaseTypeDumpState() =>
            new(Dumper,
                Instance,
                SuperType.BaseType ?? throw new ArgumentException("System.Object does not have base type."),
                ClassMetadataResolver.GetClassDumpMetadata(SuperType.BaseType, dumpAttribute: InstanceDumpAttribute),
                InstanceDumpAttribute,
                DumpScript);

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
            MemberDumper.DumpSeenAlready();
        }

        public void DumpType()
        {
            MemberDumper.DumpType();
        }

        public void DumpExpressionCSharpText()
        {
            if (Instance is Expression expression  &&  !Dumper.IsSubExpression)
            {
                // this is the highest level expression.
                Dumper.IsSubExpression = true;

                var cSharpText = expression.DumpCSharpText();
                MemberDumper.DumpExpressionCSharpText(cSharpText);
            }
        }

        public void IncrementMaxDepth()
        {
            MemberDumper.IncrementMaxDepth();
        }

        public void DecrementMaxDepth()
        {
            MemberDumper.DecrementMaxDepth();
        }

        public void Indent()
        {
            MemberDumper.Indent();
        }

        public void Unindent()
        {
            MemberDumper.Unindent();
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

            MemberDumper.DumpDelegate();
            return true;
        }

        public bool DumpedMemberInfo()
        {
            if (Instance is not MemberInfo)
                return false;

            MemberDumper.DumpedMemberInfo();
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

            MemberDumper.DumpProperty(value, type);
        }

        public bool DumpedCollection(
            DumpAttribute dumpAttribute,
            bool enumerateCustom) => DumpedCollection(Instance, null, dumpAttribute, enumerateCustom, true);

        public bool DumpedCollection(
            object? value,
            MemberInfo? mi,
            DumpAttribute dumpAttribute,
            bool enumerateCustom = false,
            bool newLineForCustom = false) =>
            value is IEnumerable sequence  &&
            (DumpedExpando(sequence, dumpAttribute)         ||
             DumpedDictionary(sequence, mi, dumpAttribute)  ||
             DumpedSequence(sequence, mi, dumpAttribute, enumerateCustom, newLineForCustom));

        bool DumpedExpando(
            IEnumerable sequence,
            DumpAttribute dumpAttribute) =>
            sequence is ExpandoObject  &&
            MemberDumper.DumpExpando(sequence, dumpAttribute);

        bool DumpedDictionary(
            IEnumerable sequence,
            MemberInfo? mi,
            DumpAttribute dumpAttribute) =>
            !sequence.IsDynamicObject()  &&
            MemberDumper.DumpDictionary(sequence, dumpAttribute, mi);

        bool DumpedSequence(
            IEnumerable sequence,
            MemberInfo? mi,
            DumpAttribute dumpAttribute,
            bool enumerateCustom = false,
            bool newLineForCustom = false)
        {
            var sequenceType = sequence.GetType();

            return (sequenceType.IsArray         ||      // it is a system collection: Array or from System.*
                    sequenceType.IsFromSystem()  ||
                    enumerateCustom  &&  dumpAttribute.Enumerate == ShouldDump.Dump)  &&  // the dump attribute tells us to
                   MemberDumper.DumpSequence(sequence, dumpAttribute, mi, newLineForCustom);
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
                MemberDumper.DumpToString(value);
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
                               ClassDumpMetadata.Metadata
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

            if (dumpMethod is null)
                dumpMethod = dumpMethod2;
            if (dumpMethod is not null)
                return MemberDumper.CustomDumpProperty(value, dumpMethod);

            MemberDumper.Write(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.CouldNotFindCustomDumpers,
                                dumpMethodName, type.FullName, type.FullName));
            return true;
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    static class XNames
    {
        /// <summary>
        /// The XML namespace of the W3C schema definition - http://www.w3.org/2001/XMLSchema
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="XNamespace is immutable.")]
        public static readonly XNamespace Xsd  = XNamespace.Get("http://www.w3.org/2001/XMLSchema");

        /// <summary>
        /// The XML namespace of the W3C instance schema definition - http://www.w3.org/2001/XMLSchema-instance
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="XNamespace is immutable.")]
        public static readonly XNamespace Xsi  = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

        /// <summary>
        /// The XML namespace of the Microsoft serialization schema definition - http://schemas.microsoft.com/2003/10/Serialization/
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="XNamespace is immutable.")]
        public static readonly XNamespace Ser  = XNamespace.Get("http://schemas.microsoft.com/2003/10/Serialization/");

        /// <summary>
        /// The XML namespace object representing the namespace of the data contracts - http://schemas.datacontract.org/2004/07/System
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="XNamespace is immutable.")]
        public static readonly XNamespace Dcs = XNamespace.Get("http://schemas.datacontract.org/2004/07/System");

        /// <summary>
        /// The XML namespace object representing the namespace of the Aspects' expression serialization - urn:schemas-vm-com:Aspects.Linq.Expression
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="XNamespace is immutable.")]
        public static readonly XNamespace Xxp = XNamespace.Get("urn:schemas-vm-com:Aspects.Linq.Expression");

        public static class Elements
        {
            public static readonly XName Expression             = Xxp + "expression";

            public static readonly XName Boolean                = Xxp + "boolean";
            public static readonly XName UnsignedByte           = Xxp + "unsignedByte";
            public static readonly XName Byte                   = Xxp + "byte";
            public static readonly XName Short                  = Xxp + "short";
            public static readonly XName UnsignedShort          = Xxp + "unsignedShort";
            public static readonly XName Int                    = Xxp + "int";
            public static readonly XName UnsignedInt            = Xxp + "unsignedInt";
            public static readonly XName Long                   = Xxp + "long";
            public static readonly XName UnsignedLong           = Xxp + "unsignedLong";
            public static readonly XName Float                  = Xxp + "float";
            public static readonly XName Double                 = Xxp + "double";
            public static readonly XName Decimal                = Xxp + "decimal";
            public static readonly XName Guid                   = Xxp + "guid";
            public static readonly XName AnyURI                 = Xxp + "anyURI";
            public static readonly XName Duration               = Xxp + "duration";
            public static readonly XName String                 = Xxp + "string";
            public static readonly XName Char                   = Xxp + "char";
            public static readonly XName DateTime               = Xxp + "dateTime";
            public static readonly XName DBNull                 = Xxp + "dbNull";

            public static readonly XName Nullable               = Xxp + "nullable";
            public static readonly XName Enum                   = Xxp + "enum";
            public static readonly XName Custom                 = Xxp + "custom";
            public static readonly XName Anonymous              = Xxp + "anonymous";

            public static readonly XName Body                   = Xxp + "body";
            public static readonly XName Indexes                = Xxp + "indexes";
            public static readonly XName IsLiftedToNull         = Xxp + "isLiftedToNull";
            public static readonly XName Left                   = Xxp + "left";
            public static readonly XName Method                 = Xxp + "method";
            public static readonly XName Parameters             = Xxp + "parameters";
            public static readonly XName Right                  = Xxp + "right";
            public static readonly XName Variables              = Xxp + "variables";

            public static readonly XName Arguments              = Xxp + "arguments";
            public static readonly XName ArrayIndex             = Xxp + "arrayIndex";
            public static readonly XName Add                    = Xxp + "add";
            public static readonly XName AddAssign              = Xxp + "addAssign";
            public static readonly XName AddAssignChecked       = Xxp + "addAssignChecked";
            public static readonly XName AddChecked             = Xxp + "addChecked";
            public static readonly XName And                    = Xxp + "and";
            public static readonly XName AndAlso                = Xxp + "andAlso";
            public static readonly XName AndAssign              = Xxp + "andAssign";
            public static readonly XName ArrayLength            = Xxp + "arrayLength";
            public static readonly XName Assign                 = Xxp + "assign";
            public static readonly XName Bindings               = Xxp + "bindings";
            public static readonly XName Block                  = Xxp + "block";
            public static readonly XName Bounds                 = Xxp + "bounds";
            public static readonly XName BreakLabel             = Xxp + "breakLabel";
            public static readonly XName Case                   = Xxp + "case";
            public static readonly XName Catch                  = Xxp + "catch";
            public static readonly XName Call                   = Xxp + "call";
            public static readonly XName Coalesce               = Xxp + "coalesce";
            public static readonly XName Conditional            = Xxp + "conditional";
            public static readonly XName Constant               = Xxp + "constant";
            public static readonly XName Constructor            = Xxp + "constructor";
            public static readonly XName ContinueLabel          = Xxp + "continueLabel";
            public static readonly XName Convert                = Xxp + "convert";
            public static readonly XName ConvertChecked         = Xxp + "convertChecked";
            public static readonly XName DebugInfo              = Xxp + "debugInfo";
            public static readonly XName Decrement              = Xxp + "decrement";
            public static readonly XName Default                = Xxp + "default";
            public static readonly XName DefaultCase            = Xxp + "defaultCase";
            public static readonly XName Divide                 = Xxp + "divide";
            public static readonly XName DivideAssign           = Xxp + "divideAssign";
            public static readonly XName Dynamic                = Xxp + "dynamic";
            public static readonly XName ElementInit            = Xxp + "elementInit";
            public static readonly XName ArrayElements          = Xxp + "elements";
            public static readonly XName Equal                  = Xxp + "equal";
            public static readonly XName Event                  = Xxp + "event";
            public static readonly XName Exception              = Xxp + "exception";
            public static readonly XName ExclusiveOr            = Xxp + "exclusiveOr";
            public static readonly XName ExclusiveOrAssign      = Xxp + "exclusiveOrAssign";
            public static readonly XName Extension              = Xxp + "extension";
            public static readonly XName Fault                  = Xxp + "fault";
            public static readonly XName Field                  = Xxp + "field";
            public static readonly XName Filter                 = Xxp + "filter";
            public static readonly XName Finally                = Xxp + "finally";
            public static readonly XName Goto                   = Xxp + "goto";
            public static readonly XName GreaterThan            = Xxp + "greaterThan";
            public static readonly XName GreaterThanOrEqual     = Xxp + "greaterThanOrEqual";
            public static readonly XName Increment              = Xxp + "increment";
            public static readonly XName Index                  = Xxp + "index";
            public static readonly XName Invoke                 = Xxp + "invoke";
            public static readonly XName IsFalse                = Xxp + "isFalse";
            public static readonly XName IsTrue                 = Xxp + "isTrue";
            public static readonly XName Label                  = Xxp + "label";
            public static readonly XName Lambda                 = Xxp + "lambda";
            public static readonly XName LeftShift              = Xxp + "leftShift";
            public static readonly XName LeftShiftAssign        = Xxp + "leftShiftAssign";
            public static readonly XName LessThan               = Xxp + "lessThan";
            public static readonly XName LessThanOrEqual        = Xxp + "lessThanOrEqual";
            public static readonly XName ListInit               = Xxp + "listInit";
            public static readonly XName Loop                   = Xxp + "loop";
            public static readonly XName LabelTarget            = Xxp + "labelTarget";
            public static readonly XName MemberAccess           = Xxp + "memberAccess";
            public static readonly XName MemberInit             = Xxp + "memberInit";
            public static readonly XName Members                = Xxp + "members";
            public static readonly XName Modulo                 = Xxp + "modulo";
            public static readonly XName ModuloAssign           = Xxp + "moduloAssign";
            public static readonly XName Multiply               = Xxp + "multiply";
            public static readonly XName MultiplyAssign         = Xxp + "multiplyAssign";
            public static readonly XName MultiplyAssignChecked  = Xxp + "multiplyAssignChecked";
            public static readonly XName MultiplyChecked        = Xxp + "multiplyChecked";
            public static readonly XName Negate                 = Xxp + "negate";
            public static readonly XName NegateChecked          = Xxp + "negateChecked";
            public static readonly XName New                    = Xxp + "new";
            public static readonly XName NewArrayBounds         = Xxp + "newArrayBounds";
            public static readonly XName NewArrayInit           = Xxp + "newArrayInit";
            public static readonly XName Not                    = Xxp + "not";
            public static readonly XName NotEqual               = Xxp + "notEqual";
            public static readonly XName OnesComplement         = Xxp + "onesComplement";
            public static readonly XName Or                     = Xxp + "or";
            public static readonly XName OrAssign               = Xxp + "orAssign";
            public static readonly XName OrElse                 = Xxp + "orElse";
            public static readonly XName Parameter              = Xxp + "parameter";
            public static readonly XName PostDecrementAssign    = Xxp + "postDecrementAssign";
            public static readonly XName PostIncrementAssign    = Xxp + "postIncrementAssign";
            public static readonly XName Power                  = Xxp + "power";
            public static readonly XName PowerAssign            = Xxp + "powerAssign";
            public static readonly XName PreDecrementAssign     = Xxp + "preDecrementAssign";
            public static readonly XName PreIncrementAssign     = Xxp + "preIncrementAssign";
            public static readonly XName Property               = Xxp + "property";
            public static readonly XName Quote                  = Xxp + "quote";
            public static readonly XName RightShift             = Xxp + "rightShift";
            public static readonly XName RightShiftAssign       = Xxp + "rightShiftAssign";
            public static readonly XName RuntimeVariables       = Xxp + "runtimeVariables";
            public static readonly XName Subtract               = Xxp + "subtract";
            public static readonly XName SubtractAssign         = Xxp + "subtractAssign";
            public static readonly XName SubtractAssignChecked  = Xxp + "subtractAssignChecked";
            public static readonly XName SubtractChecked        = Xxp + "subtractChecked";
            public static readonly XName Switch                 = Xxp + "switch";
            public static readonly XName Throw                  = Xxp + "throw";
            public static readonly XName Try                    = Xxp + "try";
            public static readonly XName TypeAs                 = Xxp + "typeAs";
            public static readonly XName TypeEqual              = Xxp + "typeEqual";
            public static readonly XName TypeIs                 = Xxp + "typeIs";
            public static readonly XName UnaryPlus              = Xxp + "unaryPlus";
            public static readonly XName Unbox                  = Xxp + "unbox";
            public static readonly XName Value                  = Xxp + "value";

            public static readonly XName AssignmentBinding      = Xxp + "assignmentBinding";
            public static readonly XName MemberMemberBinding    = Xxp + "memberMemberBinding";
            public static readonly XName ListBinding            = Xxp + "listBinding";
        }

        public static class Attributes
        {
            public static readonly XName Uid                    = "uid";
            public static readonly XName Uidref                 = "uidref";
            public static readonly XName Type                   = "type";
            public static readonly XName DelegateType           = "delegateType";
            public static readonly XName Kind                   = "kind";
            public static readonly XName Name                   = "name";
            public static readonly XName IsByRef                = "isByRef";
            public static readonly XName IsOut                  = "isOut";
            public static readonly XName TailCall               = "tailCall";
            public static readonly XName Property               = "property";
            public static readonly XName IsLiftedToNull         = "isLiftedToNull";
            public static readonly XName IsNull                 = "isNull";
            public static readonly XName Static                 = "static";
            public static readonly XName Visibility             = "visibility";
            public static readonly XName Private                = "private";
            public static readonly XName Family                 = "family";
            public static readonly XName Assembly               = "assembly";
            public static readonly XName FamilyAndAssembly      = "familyAndAssembly";
            public static readonly XName FamilyOrAssembly       = "familyOrAssembly";
        };
    }
}

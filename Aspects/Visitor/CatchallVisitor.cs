using System;
using System.Globalization;

namespace vm.Aspects.Visitor
{
    /// <summary>
    /// Class CatchAllVisitor can serve as a base class for any visitor. Because <see cref="IVisitor{T}"/> is contra-variant it
    /// provides the catch-all case <c>IVisitor&lt;object&gt;</c> for any visited class that accepts the visitor but
    /// does not have corresponding visitor implementation <see cref="IVisitor&lt;T&gt;.Visit(T)"/> in it.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System;
    /// using vm.Aspects.Visitor;
    /// 
    /// namespace VisitorExample
    /// {
    ///     abstract class BaseVisited : IVisited<BaseVisited>
    ///     {
    ///         public void Accept(IVisitor<BaseVisited> visitor)
    ///         {
    ///             Console.WriteLine("{0} accepts {1} visitor.", GetType().Name, visitor.GetType().Name);
    ///             visitor.Visit(this);
    ///         }
    ///     }
    /// 
    ///     class Derived1 : BaseVisited, IVisited<Derived1>
    ///     {
    ///         public void Accept(IVisitor<Derived1> visitor)
    ///         {
    ///             Console.WriteLine("{0} accepts {1} visitor.", GetType().Name, visitor.GetType().Name);
    ///             visitor.Visit(this);
    ///         }
    ///     }
    /// 
    ///     class Derived2 : BaseVisited
    ///     {
    ///     }
    /// 
    ///     class Other : IVisited<Other>
    ///     {
    ///         public void Accept(IVisitor<Other> visitor)
    ///         {
    ///             Console.WriteLine("{0} accepts {1} visitor.", GetType().Name, visitor.GetType().Name);
    ///             visitor.Visit(this);
    ///         }
    ///     }
    /// 
    ///     class HasNoVisitor : IVisited<HasNoVisitor>
    ///     {
    ///         public void Accept(IVisitor<HasNoVisitor> visitor)
    ///         {
    ///             Console.WriteLine("{0} accepts {1} visitor.", GetType().Name, visitor.GetType().Name);
    ///             visitor.Visit(this);
    ///         }
    ///     }
    /// 
    ///     class ExampleVisitor : CatchallVisitor,
    ///                            IVisitor<BaseVisited>,
    ///                            IVisitor<Derived1>,
    ///                            IVisitor<Other>
    ///     {
    ///         void IVisitor<BaseVisited>.Visit(BaseVisited visited)
    ///         {
    ///             Console.WriteLine("Visitor {0} visits {1} object.", GetType().Name, visited.GetType().Name);
    ///         }
    /// 
    ///         void IVisitor<Derived1>.Visit(Derived1 visited)
    ///         {
    ///             Console.WriteLine("Visitor {0} visits {1} object.", GetType().Name, visited.GetType().Name);
    ///         }
    /// 
    ///         void IVisitor<Other>.Visit(Other visited)
    ///         {
    ///             Console.WriteLine("Visitor {0} visits {1} object.", GetType().Name, visited.GetType().Name);
    ///         }
    ///     }
    /// 
    ///     class Program
    ///     {
    ///         static void Main(string[] args)
    ///         {
    ///             try
    ///             {
    ///                 var derived1 = new Derived1();
    ///                 var derived2 = new Derived2();
    ///                 var other    = new Other();
    ///                 var hasNone  = new HasNoVisitor();
    ///                 var visitor  = new ExampleVisitor();
    /// 
    ///                 derived1.Accept(visitor);
    ///                 derived2.Accept(visitor);
    ///                 other.Accept(visitor);
    ///                 hasNone.Accept(visitor);
    ///             }
    ///             catch (NotImplementedException x)
    ///             {
    ///                 Console.WriteLine("EXCEPTION: {0}", x.Message);
    ///             }
    /// 
    ///             Console.Write("Press any key to exit...");
    ///             Console.ReadKey(true);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public abstract class CatchallVisitor : IVisitor<object>
    {
        /// <summary>
        /// Visits the specified object.
        /// </summary>
        /// <param name="visited">The visited object.</param>
        /// <exception cref="System.NotImplementedException">Always thrown.</exception>
        void IVisitor<object>.Visit(
            object visited)
        {
            if (visited == null)
                throw new ArgumentNullException("visited");

            throw new NotImplementedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "The visitor {0} does not implement IVisitor<{1}>.",
                    GetType().Name,
                    visited.GetType().Name));
        }
    }
}

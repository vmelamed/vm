
namespace vm.Aspects.Visitor
{
    /// <summary>
    /// Interface IVisitor defines the behavior of the visitor role from the G4 visitor design pattern.
    /// </summary>
    /// <typeparam name="TVisited">The type of the visited object.</typeparam>
    /// <seealso cref="T:IVisited{TVisited}"/>
    /// <seealso cref="T:CatchallVisitor"/>
    public interface IVisitor<in TVisited> where TVisited : class
    {
        /// <summary>
        /// Visits the specified visited object.
        /// </summary>
        /// <param name="visited">The visited object.</param>
        /// <returns>The visited object.</returns>
        /// <remarks>
        /// This method is supposed to be invoked by the visited object from within its <see cref="M:IVisited{T}.Accept"/> method.
        /// Therefore it is recommended that all implementations of the interface are explicit (<c>void IVisitor&lt;A&gt;.Visit(A visited) {...}</c>).
        /// It is here where the concrete processing logic should take place.
        /// Note that the <typeparamref name="TVisited"/> is qualified with <c>in</c> modifier, i.e. the type parameter is contravariant and
        /// can be replaced by more derived types, de facto implementing inheritance of the action performed by the visitor.
        /// </remarks>
        void Visit(TVisited visited);
    }
}

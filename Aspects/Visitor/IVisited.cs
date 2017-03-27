
namespace vm.Aspects.Visitor
{
    /// <summary>
    /// Interface IVisited defines the behavior of the visited objects from the G4 visitor design pattern.
    /// </summary>
    /// <typeparam name="TVisited">The type of the visitor.</typeparam>
    /// <seealso cref="IVisited{TVisited}"/>
    /// <seealso cref="CatchallVisitor"/>
    public interface IVisited<TVisited> where TVisited : class
    {
        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <returns>The visited object.</returns>
        /// <remarks>
        /// This method is implemented by the visited types and usually is as simple as:
        /// <code>
        /// <![CDATA[
        /// public void Accept(IVisitor<ThisClass> visitor)
        /// {
        ///     visitor.Visit(this);
        /// }
        /// ]]>
        /// </code>
        /// </remarks>
        TVisited Accept(IVisitor<TVisited> visitor);
    }
}

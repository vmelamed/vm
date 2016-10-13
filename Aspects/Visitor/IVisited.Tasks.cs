
using System.Threading.Tasks;

namespace vm.Aspects.Visitor
{
    /// <summary>
    /// Interface IVisitedTasks defines the behavior of the asynchronous visited objects from the G4 visitor design pattern.
    /// </summary>
    /// <typeparam name="TVisited">The type of the visitor.</typeparam>
    public interface IVisitedTasks<TVisited> where TVisited : class
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
        /// public void AcceptAsync(IVisitorTasks<ThisClass> visitor)
        /// {
        ///     await visitor.VisitAsync(this);
        /// }
        /// ]]>
        /// </code>
        /// </remarks>
        Task<TVisited> AcceptAsync(IVisitorTasks<TVisited> visitor);
    }
}

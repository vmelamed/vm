
using System.Threading.Tasks;

namespace vm.Aspects.Visitor
{
    /// <summary>
    /// Interface IVisitor defines the behavior of the visitor role from the G4 visitor design pattern.
    /// </summary>
    /// <typeparam name="TVisited">The type of the visited object.</typeparam>
    public interface IVisitorTasks<in TVisited> where TVisited : class
    {
        /// <summary>
        /// Visits the specified visited object.
        /// </summary>
        /// <param name="visited">The visited object.</param>
        /// <returns>The visited object.</returns>
        Task VisitAsync(TVisited visited);
    }
}

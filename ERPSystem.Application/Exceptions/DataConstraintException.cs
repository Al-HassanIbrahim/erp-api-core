using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Exceptions
{
    /// <summary>
    /// Thrown when a persistence operation violates a database constraint
    /// (unique index, foreign key, check constraint, etc.).
    /// Raised at the Infrastructure boundary (UnitOfWork) so the Application
    /// and API layers never reference EF Core types directly.
    /// </summary>
    public sealed class DataConstraintException : Exception
    {
        public DataConstraintException(string message, Exception? innerException = null)
            : base(message, innerException) { }
    }
}

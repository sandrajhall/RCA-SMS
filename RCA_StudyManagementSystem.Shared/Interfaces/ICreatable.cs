using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Shared.Interfaces
{
    public interface ICreatable
    {
        /// <summary>
        /// Gets or sets the created user identifier.
        /// </summary>
        /// <value>
        /// The created user identifier.
        /// </value>
        Guid CreatedUserId { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        /// <value>
        /// The created date.
        /// </value>
        DateTime CreatedDate { get; set; }
    }
}

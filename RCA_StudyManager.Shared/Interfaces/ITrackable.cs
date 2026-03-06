using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManager.Shared.Interfaces
{
    public interface ITrackable : ICreatable
    {
        /// <summary>
        /// Gets or sets the modified user identifier.
        /// </summary>
        /// <value>
        /// The modified user identifier.
        /// </value>
        Guid? ModifiedUserId { get; set; }

        /// <summary>
        /// Gets or sets the modified date.
        /// </summary>
        /// <value>
        /// The modified date.
        /// </value>
        DateTime? ModifiedDate { get; set; }
    }
}

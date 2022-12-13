using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class DbDeleteOperationProductResult : DbOperationProductResult
    {
        public bool AlreadyMadeChanges { get; set; }
        /// <summary>
        /// Признак того, что документ, с которым проводилась операция, уже подтвержден
        /// </summary>
        public bool DocIsConfirmed { get; set; }
    }
}

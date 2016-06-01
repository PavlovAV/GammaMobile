using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class DocShipmentOrder
    {
        /// <summary>
        /// ID приказа 1С
        /// </summary>
        
        public Guid DocShipmentOrderId { get; set; }
        /// <summary>
        /// Номер приказа 1С
        /// </summary> 
        public string Number { get; set; }
    }
}

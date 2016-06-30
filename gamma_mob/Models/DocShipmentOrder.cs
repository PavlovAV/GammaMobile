using System;

namespace gamma_mob.Models
{
    public class DocShipmentOrder
    {
        /// <summary>
        ///     ID приказа 1С
        /// </summary>
        public Guid DocShipmentOrderId { get; set; }

        /// <summary>
        ///     Номер приказа 1С
        /// </summary>
        public string Number { get; set; }

        public string Buyer { get; set; }
    }
}
using System;
using gamma_mob.Common;

namespace gamma_mob.Models
{
    public class DocOrder
    {
        /// <summary>
        ///     ID приказа 1С
        /// </summary>
        public Guid DocOrderId { get; set; }

        public OrderType OrderType { get; set; }

        /// <summary>
        ///     Номер приказа 1С
        /// </summary>
        public string Number { get; set; }

        public string Consignee { get; set; }
        public string OutPlaceName { get; set; }
        public string InPlaceName { get; set; }
        public int? OutPlaceID { get; set; }
        public int? InPlaceID { get; set; }
        public bool IsControlExec { get; set; }
        public DateTime? StartExec { get; set; }
        public DateTime? EndExec { get; set; }
        public string State { get; set; }
    }
}
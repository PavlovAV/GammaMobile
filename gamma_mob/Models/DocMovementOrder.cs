using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class DocMovementOrder
    {
        public Guid DocId { get; set; }
        public string Number { get; set; }
        public string PlaceTo { get; set; }
        public string PlaceFrom { get; set; }
    }
}

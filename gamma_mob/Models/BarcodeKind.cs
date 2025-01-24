using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class BarcodeKind
    {
        public string BarcodeKindId { get; set; }
        public string Name { get; set; }
        public int? BarcodeTypeId{ get; set; }
        public int? LastValue { get; set; }
        public string Template { get; set; }
        public int? BarcodeGroupId { get; set; }
    }
}

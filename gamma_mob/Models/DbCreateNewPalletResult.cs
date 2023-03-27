using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.Models
{
    public class DbCreateNewPalletResult 
    {
        public Guid? ProductID { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public string Person { get; set; }
        public string ResultMessage { get; set; }
    }
}


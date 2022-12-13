using System;

namespace gamma_mob.Models
{
    public class DbMoveOperationProductResult : DbOperationProductResult
    {
        //public string ResultMessage { get; set; }
        public bool AlreadyAdded { get; set; }
        public string OutPlace { get; set; }
        public Guid? DocMovementId { get; set; }
        public DateTime? Date { get; set; }
        public string NumberAndInPlaceZone { get; set; }

    }
}
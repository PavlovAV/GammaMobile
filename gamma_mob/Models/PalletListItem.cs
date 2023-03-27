using System;

namespace gamma_mob.Models
{
    public class PalletListItem
    {
        public Guid ProductId { get; private set; }
        public string Number { get; private set; }
        public DateTime Date { get; private set; }
        public string Person { get; private set; }

        public PalletListItem(Guid productId, string number, DateTime date, string person)
        {
            ProductId = productId;
            Number = number;
            Date = date;
            Person = person;
        }
    }
}

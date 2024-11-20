using System;

namespace gamma_mob.Models
{
    public class Person
    {
        public Guid PersonID { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public bool? b1 { get; set; }
        public bool? b2 { get; set; }
        public bool? b3 { get; set; }
        public bool? b4 { get; set; }
        public int? i1 { get; set; }
        public int? i2 { get; set; }
        public string s1 { get; set; }
        public string s2 { get; set; }
        public int PlaceID { get; set; }
    }
}
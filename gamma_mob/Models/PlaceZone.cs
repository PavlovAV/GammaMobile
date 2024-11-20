using System;

namespace gamma_mob.Models
{
    public class PlaceZone
    {
        public int PlaceId { get; set; }
        public Guid PlaceZoneId { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public Guid? PlaceZoneParentId { get; set; }
        public bool IsValid { get; set; }
        public int? RootPlaceId { get; set; }

        /// <summary>
        /// Разрешение на выбор зоны не из склада отгрузки (приемки) - из группы складов по RootPlaceID
        /// </summary>
        public bool AllowedUseZonesOfPlaceGroup { get; set; }

        /// <summary>
        /// Уже существует перемещение какой-либо продукции в данную зону в текущей смене текущим пользователем
        /// </summary>
        public bool IsExistMovementToZone { get; set; }
    }
}

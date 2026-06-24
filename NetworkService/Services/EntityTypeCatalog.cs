using System.Collections.Generic;
using NetworkService.Models;

namespace NetworkService.Services
{

    public static class EntityTypeCatalog
    {
        public static EntityType Rtd { get; } = new EntityType("RTD", "RtdImage");

        public static EntityType TermoSprega { get; } = new EntityType("TermoSprega", "TermoSpregaImage");

        public static IReadOnlyList<EntityType> All { get; } = new List<EntityType>
        {
            Rtd,
            TermoSprega
        };
    }
}

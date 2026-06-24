using System.Collections.Generic;
using NetworkService.Models;

namespace NetworkService.Services
{
    /// <summary>
    /// Holds the predefined entity types for the selected theme
    /// (T7 - Reactor temperature). Types are fixed in advance and offered in a
    /// ComboBox when creating a new entity. Each type has a predefined image.
    /// </summary>
    public static class EntityTypeCatalog
    {
        public static EntityType Rtd { get; } = new EntityType("RTD", "RtdImage");

        public static EntityType TermoSprega { get; } = new EntityType("TermoSprega", "TermoSpregaImage");

        /// <summary>All available types, in display order.</summary>
        public static IReadOnlyList<EntityType> All { get; } = new List<EntityType>
        {
            Rtd,
            TermoSprega
        };
    }
}

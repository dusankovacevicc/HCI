using System.Collections.Generic;
using NetworkService.Models;

namespace NetworkService.Services
{
    /// <summary>
    /// Provides a few predefined, theme-relevant entities (reactor temperature)
    /// so that the application starts with valid sample data, as required for the
    /// defense ("minimum three already created entities with valid data").
    /// </summary>
    public static class SeedData
    {
        public static IEnumerable<Entity> CreateInitialEntities()
        {
            return new List<Entity>
            {
                new Entity
                {
                    Id = 17,
                    Name = "Reaktor-A jezgro",
                    Type = EntityTypeCatalog.Rtd,
                    LastValue = 300.0,
                    HasMeasurement = true
                },
                new Entity
                {
                    Id = 18,
                    Name = "Reaktor-B obloga",
                    Type = EntityTypeCatalog.TermoSprega,
                    LastValue = 280.0,
                    HasMeasurement = true
                },
                new Entity
                {
                    Id = 24,
                    Name = "Sekundarni krug",
                    Type = EntityTypeCatalog.Rtd,
                    LastValue = 265.0,
                    HasMeasurement = true
                },
                new Entity
                {
                    Id = 19,
                    Name = "Parni generator",
                    Type = EntityTypeCatalog.TermoSprega,
                    LastValue = 320.0,
                    HasMeasurement = true
                }
            };
        }
    }
}

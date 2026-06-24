namespace NetworkService.Models
{
    /// <summary>
    /// Describes the type of a measuring entity. For the chosen theme
    /// (T7 - Reactor temperature) the available types are RTD and TermoSprega.
    /// Each type carries a display name and a key that maps to a predefined
    /// vector image used throughout the UI.
    /// </summary>
    public class EntityType
    {
        public EntityType(string name, string imageKey)
        {
            Name = name;
            ImageKey = imageKey;
        }

        /// <summary>Display name of the type (e.g. "RTD", "TermoSprega").</summary>
        public string Name { get; }

        /// <summary>
        /// Resource key of the predefined image for this type. It is resolved to
        /// a DrawingImage defined in Resources/TypeImages.xaml via a converter.
        /// </summary>
        public string ImageKey { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}

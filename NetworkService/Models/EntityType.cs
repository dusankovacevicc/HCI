namespace NetworkService.Models
{

    public class EntityType
    {
        public EntityType(string name, string imageKey)
        {
            Name = name;
            ImageKey = imageKey;
        }


        public string Name { get; }


        public string ImageKey { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}

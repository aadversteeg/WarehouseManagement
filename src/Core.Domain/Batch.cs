namespace Core
{
    public class Batch : Entity
    {
        public string Name { get; set; }

        public bool IsPharma { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Id})";
        }
    }
}

namespace SampleDomain {
    public class Checkpoint {
        //todo: fix this to actually work, some sort of value object and comparison here
        public static Checkpoint Start { get { return new Checkpoint("*", default); } }
        public static Checkpoint End { get { return new Checkpoint("*", ulong.MaxValue); } }
        public string Stream { get; }
        public ulong? Position { get; }
        public Checkpoint(string stream, ulong? position) {
            Stream = stream;
            Position = position;
        }
    }
}
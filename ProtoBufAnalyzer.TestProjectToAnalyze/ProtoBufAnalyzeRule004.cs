using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract] // squiggly
    public class ProtoBufAnalyzeRule004
    {
        [ProtoMember(2)]
        public int Prop { get; set; }
        [ProtoMember(3)]
        public int Prop2 { get; set; }
        [ProtoMember(-1)]
        public int Prop3 { get; set; } // squiggly
        [ProtoMember(6)]
        public int Prop4 { get; set; }
        [ProtoMember(10)]
        public int Prop5 { get; set; }
    }
}

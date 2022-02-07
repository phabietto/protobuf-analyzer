using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract]
    [ProtoReserved(1)]
    [ProtoReserved(3,5)]
    public class ProtoBufAnalyzeRule005SingleReservation
    {
        [ProtoMember(1)]
        public int Prop { get; set; }
        [ProtoMember(2)]
        public int Prop2 { get; set; }  //  squiggly
        [ProtoMember(3)]
        public int Prop3 { get; set; }
        [ProtoMember(4)]
        public int Prop4 { get; set; }  //  squiggly
    }
}

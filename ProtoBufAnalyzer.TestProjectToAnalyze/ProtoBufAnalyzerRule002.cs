using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract]
    public class ProtoBufAnalyzerRule002
    {
        [ProtoMember(1)] public int Id { get; set; }
        [ProtoMember(2)] public int Order { get; set; }
        [ProtoMember(3)] public string Title { get; set; }
        [ProtoMember(2)] public string Description { get; set; }
    }
}

using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract, TestA]
    [TestB]
    public class ProtoBufAnalyzerRule001
    {
        [ProtoMember(0)] public int Id { get; set; }
        [ProtoMember(-1)] public int Order { get; set; }
    }
}

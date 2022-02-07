using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract]
    public class ProtoBufAnalyzerRule002MultipleClasses
    {
        [ProtoMember(1)] public int Id { get; set; }
        [ProtoMember(2)] public int Order { get; set; }
        [ProtoMember(3)] public string Title { get; set; }

        [ProtoContract]
        public class ProtoBufAnalyzerRule002MultipleClassesWithin
        {
            [ProtoMember(1)] public int Id { get; set; }
            [ProtoMember(2)] public int Order { get; set; }
            [ProtoMember(3)] public string Title { get; set; }
        }
    }

    [ProtoContract]
    public class ProtoBufAnalyzerRule002MultipleClassesA
    {
        [ProtoMember(1)] public int Id { get; set; }
        [ProtoMember(2)] public int Order { get; set; }
        [ProtoMember(3)] public string Title { get; set; }
        [ProtoMember(3)] public string Description { get; set; } // this should be squiggly
    }
}

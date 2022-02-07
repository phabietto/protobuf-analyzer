using System.Collections.Generic;
using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract]
    public class ProtoBufAnalyzerRule003Simple
    {
        [ProtoMember(1)]
        public int[] Items1 { get; set; } // this should be squiggly
        [ProtoMember(2)]
        public string Title { get; set; }
        [ProtoMember(3)]
        public List<int> Items2 { get; set; } // this should be squiggly
        [ProtoMember(4)]
        public IEnumerable<int> Items3 { get; set; } // this should be squiggly
    }
}

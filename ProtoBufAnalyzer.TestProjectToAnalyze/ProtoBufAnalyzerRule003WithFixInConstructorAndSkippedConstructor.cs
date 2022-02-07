using System.Collections.Generic;
using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract(SkipConstructor = true)]
    public class ProtoBufAnalyzerRule003WithFixInConstructorAndSkippedConstructor
    {
        public ProtoBufAnalyzerRule003WithFixInConstructorAndSkippedConstructor()
        {
            Ids = new List<int>();
        }

        [ProtoMember(1)]
        public List<int> Ids { get; set; } // this should be squiggly
        [ProtoMember(2)]
        public int[] Items { get; set; } // this should be squiggly
    }
}

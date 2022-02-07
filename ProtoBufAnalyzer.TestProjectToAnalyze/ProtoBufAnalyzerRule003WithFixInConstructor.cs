using System.Collections.Generic;
using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract(SkipConstructor = false)]
    public class ProtoBufAnalyzerRule003WithFixInConstructor : A
    {
        public ProtoBufAnalyzerRule003WithFixInConstructor()
        {
            Ids = new List<int>();
        }

        public ProtoBufAnalyzerRule003WithFixInConstructor(int test)
        {
            Test = test;
        }

        public int Test { get; set; }

        [ProtoMember(1)]
        public List<int> Ids { get; set; } // this should not be squiggly
        [ProtoMember(2)]
        public int[] Items { get; set; } // this should be squiggly (no fix in constructor)
    }

    [ProtoContract]
    public class A
    {
        [ProtoMember(1)]
        public int Id { get; set; }
    }
}

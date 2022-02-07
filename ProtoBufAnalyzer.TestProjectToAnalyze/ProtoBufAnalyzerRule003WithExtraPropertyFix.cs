using System.Collections.Generic;
using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract]
    public class ProtoBufAnalyzerRule003WithExtraPropertyFix
    {
        [ProtoMember(1)]
        public List<int> Ids { get; set; } // this should not be squiggly
        [ProtoMember(2)]
        private bool IsIdsEmpty
        {
            get => Ids != null && Ids.Count == 0;
            set { if (value) { Ids = new List<int>(); } }
        }
        [ProtoMember(3)]
        public List<int> Items { get; set; } // this be squiggly (no property fix)
    }
}

using System.Collections.Generic;
using ProtoBuf;

namespace ProtoBufAnalyzer.TestProjectToAnalyze
{
    [ProtoContract]
    public class ProtoBufAnalyzerRule003ToDo
    {
        private List<string> _prop;
        private List<int> _prop2;
        private List<int> _prop3;

        public ProtoBufAnalyzerRule003ToDo()
        {
            _prop = new List<string>();
        }

        public ProtoBufAnalyzerRule003ToDo(int id)
        {
            Id = id;
            _prop2 = new List<int>();
        }

        [ProtoMember(1)]
        public List<string> Prop => _prop;  // this should not be squiggly
        [ProtoMember(2)]
        public List<int> Prop2 // this should be squiggly
        {
            get { return _prop2; }
            set { _prop2 = value; }
        }
        [ProtoMember(3)]
        public List<int> Prop3 // this should be squiggly
        {
            get { return _prop3; }
            set { _prop3 = value; }
        }

        public int Id { get; set; }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using VerifyCS = ProtoBufAnalyzer.Test.CSharpCodeFixVerifier<ProtoBufAnalyzer.ProtoBufAnalyzer,ProtoBufAnalyzer.ProtoBufAnalyzerCodeFixProvider>;

namespace ProtoBufAnalyzer.Test
{
    [TestClass]
    public class ProtoBufAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
using System;
using System.Collections.Generic;
namespace ProtoBuf {
    public class ProtoMemberAttribute : Attribute {
        private int _tag;
        public ProtoMemberAttribute(int tag){
            _tag = tag;
        }
    }
    public class ProtoContractAttribute : Attribute {
        public ProtoContractAttribute() { }
        public bool SkipConstructor { get; set; }
    }
    public class ProtoReservedAttribute : Attribute {
        private int _field;
        private int _from;
        private int _to;
        public ProtoReservedAttribute(int field){
            _field = field;
        }
        public ProtoReservedAttribute(int from, int to){
            _from = from;
            _to = to;
        }
    }
    public class TestAAttribute : Attribute
    {
    }
    public class TestBAttribute : Attribute
    {
    }
}

//[ProtoBuf.ProtoContract(SkipConstructor = false)]
//public class A
//{
//    [ProtoBuf.ProtoMember(1)]
//    public int CompanyId { get; set; }
//}

//[ProtoBuf.ProtoContract(SkipConstructor = false)]
//public class ProtoBufAnalyzerRule003WithFixInConstructor:A
//{
//    private List<string> _prop;

//    public ProtoBufAnalyzerRule003WithFixInConstructor()
//    {
//        Ids = new List<int>();
//        //Items = new int[0];
//    }
//    public ProtoBufAnalyzerRule003WithFixInConstructor(int test):this()
//    { 
//        _prop = new List<string>();
//    }

//    [ProtoBuf.ProtoMember(1)]
//    public List<int> Ids { get; set; } // this should not be squiggly
//    [ProtoBuf.ProtoMember(2)]
//    public int[] Items { get; set; } // this should be squiggly (no fix in constructor)
//    [ProtoBuf.ProtoMember(3)]
//    public List<string> Prop => _prop;  // this should not be squiggly
//}

    [ProtoBuf.ProtoContract]
    [ProtoBuf.ProtoReserved(2)]
    [ProtoBuf.ProtoReserved(6,10)]
    public class ProtoBufAnalyzeRule004
    {
        [ProtoBuf.ProtoMember(1)]
        public int Prop { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public int Prop2 { get; set; } // squiggly
        [ProtoBuf.ProtoMember(3)]
        public int Prop3 { get; set; }
        [ProtoBuf.ProtoMember(4)]
        public int Prop4 { get; set; } // squiggly
    }

            ";

            var dd = new DiagnosticDescriptor("PTA002", "ProtoBuf Analyzer", "List '{0}' is a member of ProtoBuf contract: you should add code to manage Empty lists.", "Usage", DiagnosticSeverity.Warning, true);

            var expected = VerifyCS.Diagnostic(dd);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}

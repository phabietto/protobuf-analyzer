# ProtoBuf Analyzer

A Roslyn analyzer to detect common mistakes and gotchas with protobuf-net

## Rules

- PBA001: Tag must be greater than 0
- PBA002: duplicate Tags in a class
- PBA003\*: collection property with no "hack" to manage Empty collections
- PBA004: gap(s) in Tag's assignment
- PBA005: use of a reserved Tag

(\*) protobuf makes no distinction between an *empty* and a *null* collection. So either you initialize the collection in the constructor (if you can do it) or you use the "hack" proposed on [Stack Overflow](https://stackoverflow.com/questions/2378666/protobuf-net-serializing-an-empty-list/2379390#2379390)
The Analyzer for this rule detects the "hack" by the naming of the property: in essence the property should be named following the pattern 'Is<PropToHackName>Empty', e.g.
```csharp
[ProtoMember(1)]
public List<int> Ids { get; set; } // this won't be squiggly
[ProtoMember(2)]
private bool IsIdsEmpty
{
    get => Ids != null && Ids.Count == 0;
    set { if (value) { Ids = new List<int>(); } }
}
```

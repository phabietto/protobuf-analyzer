using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProtoBufAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ProtoBufAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "ProtoBuf Analyzer";
        private const string Description = "Detect common mistakes and gotchas related to ProtoBuf";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor ProtoMemberTagNumbering = new DiagnosticDescriptor(
            "PBA001",
            Title,
            "Tag '{0}' cannot be assigned to a Contract (Tag must be greater than 0).",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);
        private static readonly DiagnosticDescriptor ProtoMemberDuplicateTag = new DiagnosticDescriptor(
            "PBA002",
            Title,
            "Tag '{0}' has been already assigned to this Contract.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);
        private static readonly DiagnosticDescriptor ProtoBufEmptyCollectionWarning = new DiagnosticDescriptor(
            "PBA003",
            Title,
            "Collection '{0}' is a member of a ProtoBuf Contract. You should add code to manage Empty collections.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);
        private static readonly DiagnosticDescriptor ProtoMemberTagGapInNumbering = new DiagnosticDescriptor(
            "PBA004",
            Title,
            "There's at least a gap in Tag's assignment: '{0}' are missing. Expected Tags: '1..{1}'.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);
        private static readonly DiagnosticDescriptor ProtoMemberReservedTagUsedInNumbering = new DiagnosticDescriptor(
            "PBA005",
            Title,
            "Cannot use Tag '{0}' because it's reseved.",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            ProtoBufEmptyCollectionWarning,
            ProtoMemberTagNumbering,
            ProtoMemberDuplicateTag,
            ProtoMemberTagGapInNumbering,
            ProtoMemberReservedTagUsedInNumbering);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSemanticModelAction(AnalizeTags);
        }

        private void AnalizeTags(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot();
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
            if (!classes.Any())
            {
                return;
            }

            foreach (var @class in classes)
            {
                if (!@class.AttributeLists.Any())
                {
                    continue;
                }
                var protoContract = @class.AttributeLists
                    .SelectMany(items => items.Attributes
                        .Where(attribute => attribute.DescendantNodes().OfType<IdentifierNameSyntax>()
                            .Any(an => an.DescendantTokens()
                                .Any(token => token.Kind() == SyntaxKind.IdentifierToken && token.ValueText.StartsWith("protocontract", StringComparison.InvariantCultureIgnoreCase))
                            )
                        )
                    ).FirstOrDefault();

                var reservedTags = new List<int>();
                var protoReservedAttributes = @class.AttributeLists
                    .SelectMany(lists => lists.Attributes
                        .Where(attribute => attribute.DescendantNodes().OfType<IdentifierNameSyntax>()
                            .Any(an => an.DescendantTokens()
                                .Any(token => token.Kind() == SyntaxKind.IdentifierToken && token.ValueText.StartsWith("protoreserved", StringComparison.InvariantCultureIgnoreCase))
                            )
                        )
                    );
                foreach (var attribute in protoReservedAttributes)
                {
                    var numericArgs = attribute.ArgumentList.Arguments.Where(arg => arg.DescendantTokens().Any(dt => dt.Kind() == SyntaxKind.NumericLiteralToken)).ToList();
                    if (!numericArgs.Any())
                    {
                        continue;
                    }

                    if (numericArgs.Count == 1)
                    {
                        var value = GetArgumentIntegerValue(numericArgs[0]);
                        if (value <= 0)
                        {
                            var diagnostic = Diagnostic.Create(ProtoMemberTagNumbering, attribute.GetLocation(), value.ToString());
                            context.ReportDiagnostic(diagnostic);
                            continue;
                        }
                        reservedTags.Add(value);
                    }
                    else
                    {
                        var value1 = GetArgumentIntegerValue(numericArgs[0]);
                        var value2 = GetArgumentIntegerValue(numericArgs[1]);
                        if (value1 <= 0)
                        {
                            var diagnostic = Diagnostic.Create(ProtoMemberTagNumbering, attribute.GetLocation(), value1.ToString());
                            context.ReportDiagnostic(diagnostic);
                            continue;
                        }
                        if (value2 <= 0)
                        {
                            var diagnostic = Diagnostic.Create(ProtoMemberTagNumbering, attribute.GetLocation(), value2.ToString());
                            context.ReportDiagnostic(diagnostic);
                            continue;
                        }
                        reservedTags.AddRange(Enumerable.Range(Math.Min(value1, value2), Math.Abs(value2 - value1) + 1));
                    }
                }

                if (protoContract == null)
                {
                    continue;
                }

                //  Should ProtoBuf skip the constructor ?
                var skipConstructor = false;
                var skipConstructorArgument = protoContract.ArgumentList?.Arguments
                    .FirstOrDefault(arg => arg.DescendantTokens()
                        .Any(token => token.Kind() == SyntaxKind.IdentifierToken && token.ValueText.StartsWith("skipconstructor", StringComparison.InvariantCultureIgnoreCase)));

                if (skipConstructorArgument != null)
                {
                    skipConstructor = skipConstructorArgument.DescendantTokens().Any(token => token.Kind() == SyntaxKind.TrueKeyword);
                }

                //  Do we have any collection initialization ?
                var collectionInitializations = @class.ChildNodes().OfType<ConstructorDeclarationSyntax>()
                    .Where(node => node.ChildNodes().OfType<ParameterListSyntax>().Any(plsNode => !plsNode.ChildNodes().OfType<ParameterSyntax>().Any()))
                    .SelectMany(declaration => declaration.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                        .Where(o => o.DescendantNodes().OfType<GenericNameSyntax>().Any() || o.ChildNodes().OfType<ArrayCreationExpressionSyntax>().Any())
                    ).ToList();
                var collectionInitializationsPropertyNames = collectionInitializations
                    .Select(assignment => assignment.DescendantTokens().First(token => token.Kind() == SyntaxKind.IdentifierToken).ValueText)
                    .ToList();

                var attributes = @class.DescendantNodes().OfType<AttributeSyntax>()
                    .Where(s => s.DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Any(an => an.DescendantTokens()
                            .Any(token => token.Kind() == SyntaxKind.IdentifierToken &&
                                          token.ValueText.StartsWith("protomember",
                                              StringComparison.InvariantCultureIgnoreCase))
                        )
                    ).ToList();

                //  Check tag-related rules
                var list = new List<int>();
                foreach (var attribute in attributes)
                {
                    // assumption: there's only one attribute's argument that is and integer
                    var argument = attribute.ArgumentList.Arguments.FirstOrDefault(arg => arg.DescendantTokens().Any(dt => dt.Kind() == SyntaxKind.NumericLiteralToken));

                    if (argument != null)
                    {
                        var value = GetArgumentIntegerValue(argument);
                        if (value <= 0)
                        {
                            var diagnostic = Diagnostic.Create(ProtoMemberTagNumbering, attribute.GetLocation(), value.ToString());
                            context.ReportDiagnostic(diagnostic);
                        }

                        if (list.Contains(value))
                        {
                            var diagnostic = Diagnostic.Create(ProtoMemberDuplicateTag, attribute.GetLocation(), value.ToString());
                            context.ReportDiagnostic(diagnostic);
                        }

                        if (reservedTags.Contains(value))
                        {
                            var diagnostic = Diagnostic.Create(ProtoMemberReservedTagUsedInNumbering, attribute.GetLocation(), value.ToString());
                            context.ReportDiagnostic(diagnostic);
                        }

                        if (value > 0)
                        {
                            list.Add(value);
                        }
                    }
                }

                var missingTags = Enumerable.Range(1, list.Max()).Except(list).Except(reservedTags).ToList();
                if (attributes.Count > 0 && missingTags.Any())
                {
                    var diagnostic = Diagnostic.Create(ProtoMemberTagGapInNumbering, protoContract.GetLocation(), string.Join(",", missingTags), attributes.Count);
                    context.ReportDiagnostic(diagnostic);
                }

                //  Check collection-related rule
                var protobufProperties = @class
                    .DescendantNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .Where(pds => pds.DescendantNodes().OfType<AttributeSyntax>()
                        .Any(attrs => attrs.DescendantTokens()
                            .Any(token => token.Kind() == SyntaxKind.IdentifierToken && token.ValueText.StartsWith("protomember", StringComparison.InvariantCultureIgnoreCase))
                        )
                    )
                    .ToList();
                var protobufPropertyNames = protobufProperties.Select(pds => pds.Identifier.Value).ToList();
                var protobufCollectionProperties = protobufProperties.Where(pds => pds.ChildNodes().OfType<GenericNameSyntax>().Any() || pds.ChildNodes().OfType<ArrayTypeSyntax>().Any()).ToList();

                foreach (var property in protobufCollectionProperties)
                {
                    var propertySymbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, property);
                    var propertyFieldNames = property.ChildNodes().OfType<ArrowExpressionClauseSyntax>()
                        .SelectMany(node => node.DescendantNodes().OfType<IdentifierNameSyntax>()
                            .SelectMany(ins => ins.DescendantTokens()
                                .Where(token => token.Kind() == SyntaxKind.IdentifierToken)
                                .Select(token => token.ValueText)
                            )
                        ).ToList();
                    propertyFieldNames.AddRange(property.ChildNodes().OfType<AccessorListSyntax>()
                        .SelectMany(node => node.DescendantNodes().OfType<IdentifierNameSyntax>()
                            .SelectMany(ins => ins.DescendantTokens()
                                .Where(token => token.Kind() == SyntaxKind.IdentifierToken)
                                .Select(token => token.ValueText)
                            )
                        ).ToList());
                    var expectedListFixPropertyName = $"Is{propertySymbol.Name}Empty";
                    if (protobufPropertyNames.Contains(expectedListFixPropertyName))
                    {
                        continue;
                    }

                    if (!skipConstructor && (collectionInitializationsPropertyNames.Contains(propertySymbol.Name) || propertyFieldNames.Any(field => collectionInitializationsPropertyNames.Contains(field))))
                    {
                        continue;
                    }

                    var diagnostic = Diagnostic.Create(ProtoBufEmptyCollectionWarning, propertySymbol.Locations[0], propertySymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static int GetArgumentIntegerValue(AttributeArgumentSyntax argument)
        {
            var isNegative = argument.DescendantTokens().Any(token => token.Kind() == SyntaxKind.MinusToken);
            var value = (int)argument.DescendantTokens().FirstOrDefault(dt => dt.Kind() == SyntaxKind.NumericLiteralToken).Value * (isNegative ? -1 : 1);
            return value;
        }
    }
}

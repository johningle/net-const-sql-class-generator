﻿using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Generators
{
    /// <summary>
    /// Finds all files ending with .sql in the project.
    /// Generates a public static partial class named SqlFiles with const static string fields.
    /// Each field has the normalized name of a single .sql file and contains its contents.
    /// </summary>
    /// <example>var content = SqlFiles.NameOfSqlFile</example>
    [Generator(LanguageNames.CSharp)]
    public class SqlGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            // find all additional files that end with .sql
            var sqlFiles = initContext.AdditionalTextsProvider
                .Where(static file => file.Path.EndsWith(".sql"))
                .Select((text, cancellationToken) => (Path.GetFileNameWithoutExtension(text.Path), SymbolDisplay.FormatLiteral(text.GetText(cancellationToken)!.ToString(), true)))
                .Select((nameAndContent, cancellationToken) => $"\tpublic const string {nameAndContent.Item1} = {nameAndContent.Item2};").Collect();
            
            initContext.RegisterSourceOutput(sqlFiles, (spc, fieldDeclaration) =>
            {
                var sb = new StringBuilder();

                foreach (var item in fieldDeclaration)
                {
                    sb.AppendLine(item);
                }

                spc.AddSource($"Experiments.CanIHazSourceGeneratedSql.SqlFiles.g.cs", $@"// <auto-generated/>
namespace Experiments.CanIHazSourceGeneratedSql;

public static partial class SqlFiles
{{
{$"{sb.ToString()}"}}}");
            });
        }
    }
}

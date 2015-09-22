//-----------------------------------------------------------------------
// <copyright file="TestGroundFIle1.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.CodeAnalysis.CSharp.TestGround
{
    using Symbols;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// Definition for TestGroundFIle1
    /// </summary>
    public static class TestGroundFile1
    {
        internal static Action<string> ConsoleWriter;
        public static void Main(
            string[] args,
            Action<string> writeLine)
        {
            ConsoleWriter = writeLine;
            var tree = CSharpSyntaxTree.ParseText(
                @"
using System;
namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World"");
        }
    }
}
");
            var mscorLib = MetadataReference.CreateFromFile(
                @"C:\windows\Microsoft.Net\assembly\GAC_32\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll");

            var compilation = CSharpCompilation.Create("HelloWorld")
                .AddReferences(mscorLib)
                .AddSyntaxTrees(tree);

            foreach(var ns in compilation.GlobalNamespace.GetNamespaceMembers())
            {
                TestGroundFile1.Dispatch(ns);
            }

            var metadata = mscorLib.GetMetadata() as AssemblyMetadata;
            var typeNames = metadata.GetAssembly().ManifestModule.TypeNames;
            var imports = compilation.GlobalImports;
        }

        private static void Dispatch(
            ISymbol symbol,
            string indentLevel = "")
        {
            ConsoleWriter(indentLevel + $"kind: {symbol.Kind}, name: {symbol.Name}");
            switch (symbol.Kind)
            {
                case SymbolKind.Namespace:
                    TestGroundFile1.IterateChildren(
                        (NamespaceSymbol)symbol,
                        indentLevel + " ");
                    break;
                case SymbolKind.NamedType:
                    TestGroundFile1.IterateChildren(
                        (TypeSymbol)symbol,
                        indentLevel + " ");
                    break;
                case SymbolKind.Method:
                    TestGroundFile1.IterateChildren(
                        (MethodSymbol)symbol,
                        indentLevel + " ");
                    break;
                case SymbolKind.Parameter:
                    TestGroundFile1.IterateChildren(
                        (ParameterSymbol)symbol,
                        indentLevel + " ");
                    break;
                case SymbolKind.Field:
                    TestGroundFile1.IterateChildren(
                        (FieldSymbol)symbol,
                        indentLevel + " ");
                    break;
                case SymbolKind.Property:
                    TestGroundFile1.IterateChildren(
                        (PropertySymbol)symbol,
                        indentLevel + " ");
                    break;
                case SymbolKind.Alias:
                case SymbolKind.ArrayType:
                case SymbolKind.Assembly:
                case SymbolKind.DynamicType:
                case SymbolKind.ErrorType:
                case SymbolKind.Event:
                case SymbolKind.Label:
                case SymbolKind.Local:
                case SymbolKind.NetModule:
                case SymbolKind.PointerType:
                case SymbolKind.RangeVariable:
                case SymbolKind.TypeParameter:
                case SymbolKind.Preprocessing:
                default:
                    break;
            }
        }

        internal static void WriteAttributes(
            ImmutableArray<CSharpAttributeData> attributes,
            string indentLevel)
        {
            if (attributes.Length == 0)
            { return; }
            ConsoleWriter(indentLevel + "Attributes");
            var tmpIndent = indentLevel + " ";
            foreach (var attr in attributes)
            {
                ConsoleWriter(tmpIndent + $"TypeName: {attr.AttributeClass.Name}");

                foreach (var args in attr.ConstructorArguments)
                { ConsoleWriter(tmpIndent + $"Arg: {args.Value}"); }

                foreach (var nProp in attr.NamedArguments)
                { ConsoleWriter(tmpIndent + $"NameArg: {nProp.Key}, val: {nProp.Value.Value}"); }
            }
        }

        internal static void IterateChildren(
            ParameterSymbol symbol,
            string indentLevel)
        {
            ConsoleWriter(indentLevel + $"RefKind {symbol.Kind}");
            ConsoleWriter(indentLevel + $"Type {symbol.Type.Name}");
            WriteAttributes(symbol.GetAttributes(), indentLevel);
        }

        internal static void IterateChildren(
            PropertySymbol symbol,
            string indentLevel)
        {
            ConsoleWriter(indentLevel + $"TypeName {symbol.GetType().Name}");
            ConsoleWriter(indentLevel + $"Refkind {symbol.ParameterRefKinds}");
            WriteAttributes(symbol.GetAttributes(), indentLevel);

            if (symbol.GetMethod != null)
            {
                ConsoleWriter(indentLevel + "GetMethod");
                Dispatch(symbol.GetMethod, indentLevel);
            }

            if (symbol.SetMethod != null)
            {
                ConsoleWriter(indentLevel + "SetMethod");
                Dispatch(symbol.SetMethod, indentLevel);
            }

            WriteAttributes(symbol.GetAttributes(), indentLevel);
        }

        internal static void IterateChildren(
            FieldSymbol symbol,
            string indentLevel)
        {
            ConsoleWriter(indentLevel + $"TypeName {symbol.GetType().Name}");

            WriteAttributes(symbol.GetAttributes(), indentLevel);
        }

        internal static void IterateChildren(
            MethodSymbol symbol,
            string indentLevel)
        {
            ConsoleWriter(indentLevel + $"MethodKind {symbol.MethodKind}");
            ConsoleWriter(indentLevel + $"TypeName {symbol.GetType().Name}");

            WriteAttributes(symbol.GetAttributes(), indentLevel);

            if (!symbol.ReturnsVoid)
            { ConsoleWriter(indentLevel + $"ReturnType: {symbol.ReturnType.Name}"); }

            foreach (var args in symbol.Parameters)
            { Dispatch(args, indentLevel); }
        }

        private static void IterateChildren(
            TypeSymbol symbol,
            string indentLevel)
        {
            ConsoleWriter(indentLevel + $"TypeKind {symbol.TypeKind}");
            ConsoleWriter(indentLevel + $"TypeName {symbol.GetType().Name}");

            WriteAttributes(symbol.GetAttributes(), indentLevel);
            switch (symbol.TypeKind)
            {
                case TypeKind.Unknown:
                case TypeKind.Array:
                case TypeKind.Class:
                case TypeKind.Delegate:
                case TypeKind.Dynamic:
                case TypeKind.Enum:
                case TypeKind.Error:
                case TypeKind.Interface:
                case TypeKind.Module:
                case TypeKind.Pointer:
                case TypeKind.Structure:
                case TypeKind.TypeParameter:
                case TypeKind.Submission:
                default:
                    break;
            }

            foreach (var member in symbol.GetMembers())
            {
                Dispatch(
                    member,
                    indentLevel);
            }
        }

        private static void IterateChildren(
            NamespaceSymbol symbol,
            string indentLevel)
        {
            ConsoleWriter(indentLevel + $"Assembly: {symbol.ContainingAssembly.Name}");
            ConsoleWriter(indentLevel + $"NamespaceKind: {symbol.NamespaceKind}");
            ConsoleWriter(indentLevel + $"TypeName {symbol.GetType().Name}");

            foreach(var torn in symbol.GetMembers())
            {
                TestGroundFile1.Dispatch(torn, indentLevel);
            }
        }
    }
}

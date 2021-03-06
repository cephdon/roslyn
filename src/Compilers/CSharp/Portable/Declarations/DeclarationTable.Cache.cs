﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp
{
    internal partial class DeclarationTable
    {
        // The structure of the DeclarationTable provides us with a set of 'old' declarations that
        // stay relatively unchanged and a 'new' declaration that is repeatedly added and removed.
        // This mimics the expected usage pattern of a user repeatedly typing in a single file.
        // Because of this usage pattern, we can cache information about these 'old' declarations
        // and keep that around as long as they do not change.  For example, we keep a single 'merged
        // declaration' for all those root declarations as well as sets of interesting information
        // (like the type names in those decls). 
        private class Cache
        {
            // The merged root declaration for all the 'old' declarations.
            internal readonly Lazy<MergedNamespaceDeclaration> MergedRoot;

            // All the simple type names for all the types in the 'old' declarations.
            internal readonly Lazy<ISet<string>> TypeNames;
            internal readonly Lazy<ISet<string>> NamespaceNames;
            internal readonly Lazy<ImmutableArray<ReferenceDirective>> ReferenceDirectives;
            internal readonly Lazy<ImmutableArray<Diagnostic>> ReferenceDirectiveDiagnostics;

            public Cache(DeclarationTable table)
            {
                this.MergedRoot = new Lazy<MergedNamespaceDeclaration>(
                    () => MergedNamespaceDeclaration.Create(table._allOlderRootDeclarations.InInsertionOrder.AsImmutable<SingleNamespaceDeclaration>()));

                this.TypeNames = new Lazy<ISet<string>>(
                    () => GetTypeNames(this.MergedRoot.Value));

                this.NamespaceNames = new Lazy<ISet<string>>(
                    () => GetNamespaceNames(this.MergedRoot.Value));

                this.ReferenceDirectives = new Lazy<ImmutableArray<ReferenceDirective>>(
                    () => MergedRoot.Value.Declarations.OfType<RootSingleNamespaceDeclaration>().SelectMany(r => r.ReferenceDirectives).AsImmutable());

                this.ReferenceDirectiveDiagnostics = new Lazy<ImmutableArray<Diagnostic>>(
                    () => MergedRoot.Value.Declarations.OfType<RootSingleNamespaceDeclaration>().SelectMany(r => r.ReferenceDirectiveDiagnostics).AsImmutable());
            }
        }
    }
}

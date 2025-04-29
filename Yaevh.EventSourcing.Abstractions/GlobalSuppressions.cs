// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    category: "Style",
    checkId: "IDE0130:Namespace does not match folder structure",
    Justification = "All the classes in this project are public APIs and should reside in the top-level project namespace",
    Scope = "namespace",
    Target = "~N:Yaevh.EventSourcing")]

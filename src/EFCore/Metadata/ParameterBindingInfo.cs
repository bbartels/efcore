// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Carries information about a parameter binding.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public readonly struct ParameterBindingInfo
{
    /// <summary>
    ///     Creates a new <see cref="ParameterBindingInfo" /> to define a parameter binding.
    /// </summary>
    /// <param name="structuralType">The entity or complex type for this binding.</param>
    /// <param name="materializationContextExpression">The expression tree from which the parameter value will come.</param>
    public ParameterBindingInfo(
        ITypeBase structuralType,
        Expression materializationContextExpression)
    {
        Check.NotNull(structuralType);
        Check.NotNull(structuralType);

        StructuralType = structuralType;
        InstanceName = "instance";
        MaterializationContextExpression = materializationContextExpression;
    }

    /// <summary>
    ///     Creates a new <see cref="ParameterBindingInfo" /> to define a parameter binding.
    /// </summary>
    /// <param name="materializerSourceParameters">Parameters for the materialization that is happening.</param>
    /// <param name="materializationContextExpression">The expression tree from which the parameter value will come.</param>
    public ParameterBindingInfo(
        StructuralTypeMaterializerSourceParameters materializerSourceParameters,
        Expression materializationContextExpression)
    {
        StructuralType = materializerSourceParameters.StructuralType;
        QueryTrackingBehavior = materializerSourceParameters.QueryTrackingBehavior;
        InstanceName = materializerSourceParameters.InstanceName;
        MaterializationContextExpression = materializationContextExpression;
    }

    /// <summary>
    ///     Creates a new <see cref="ParameterBindingInfo" /> to define a parameter binding.
    /// </summary>
    /// <param name="materializerSourceParameters">Parameters for the materialization that is happening.</param>
    /// <param name="materializationContextExpression">The expression tree from which the parameter value will come.</param>
    /// <param name="materializerSource">The materializer source used to generate expressions for nested complex type bindings.</param>
    public ParameterBindingInfo(
        StructuralTypeMaterializerSourceParameters materializerSourceParameters,
        Expression materializationContextExpression,
        IStructuralTypeMaterializerSource? materializerSource)
    {
        StructuralType = materializerSourceParameters.StructuralType;
        QueryTrackingBehavior = materializerSourceParameters.QueryTrackingBehavior;
        InstanceName = materializerSourceParameters.InstanceName;
        MaterializationContextExpression = materializationContextExpression;
        MaterializerSource = materializerSource;
    }

    /// <summary>
    ///     The entity or complex type for this binding.
    /// </summary>
    public ITypeBase StructuralType { get; }

    /// <summary>
    ///     The name of the instance being materialized.
    /// </summary>
    public string InstanceName { get; }

    /// <summary>
    ///     The query tracking behavior, or <see langword="null" /> if this materialization is not from a query.
    /// </summary>
    public QueryTrackingBehavior? QueryTrackingBehavior { get; }

    /// <summary>
    ///     The expression tree from which the parameter value will come.
    /// </summary>
    public Expression MaterializationContextExpression { get; }

    /// <summary>
    ///     The materializer source used for generating expressions when binding complex property parameters.
    ///     Set when materializing types that may have complex property constructor parameters.
    /// </summary>
    public IStructuralTypeMaterializerSource? MaterializerSource { get; }

    /// <summary>
    ///     Expressions holding initialized instances for service properties.
    /// </summary>
    public List<ParameterExpression> ServiceInstances { get; } = [];

    /// <summary>
    ///     Gets the index into the <see cref="ValueBuffer" /> where the property value can be found.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The index where its value can be found.</returns>
    public int GetValueBufferIndex(IPropertyBase property)
        => property.GetIndex();
}

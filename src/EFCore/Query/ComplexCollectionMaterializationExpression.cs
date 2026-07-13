// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Represents a complex collection that must be materialized by the query provider before it can be supplied
///     to a structural type constructor.
/// </summary>
/// <remarks>
///     This type is typically used by database providers (and other extensions). It is generally not used in application code.
/// </remarks>
public sealed class ComplexCollectionMaterializationExpression(
    IComplexProperty complexProperty,
    Expression materializationContextExpression) : Expression, IPrintableExpression
{
    /// <summary>
    ///     The complex collection property being materialized.
    /// </summary>
    public IComplexProperty ComplexProperty { get; } = complexProperty;

    /// <summary>
    ///     The materialization context for the containing structural type.
    /// </summary>
    public Expression MaterializationContextExpression { get; } = materializationContextExpression;

    /// <inheritdoc />
    public override Type Type
        => ComplexProperty.ClrType;

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var visitedMaterializationContextExpression = visitor.Visit(MaterializationContextExpression);

        return visitedMaterializationContextExpression == MaterializationContextExpression
            ? this
            : new ComplexCollectionMaterializationExpression(ComplexProperty, visitedMaterializationContextExpression);
    }

    /// <inheritdoc />
    public void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(nameof(ComplexCollectionMaterializationExpression));
        expressionPrinter.Append("(");
        expressionPrinter.Append(ComplexProperty.DeclaringType.DisplayName());
        expressionPrinter.Append(".");
        expressionPrinter.Append(ComplexProperty.Name);
        expressionPrinter.Append(")");
    }
}

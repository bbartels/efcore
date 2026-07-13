// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Describes the binding from an <see cref="IComplexProperty" /> to a parameter in a constructor, factory method,
///     or similar. When bound, the complex type instance is materialized from the value buffer and passed as the
///     constructor argument.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public class ComplexPropertyParameterBinding : ParameterBinding
{
    private static readonly MethodInfo CreateCollectionMethod
        = typeof(IClrIndexedCollectionAccessor).GetMethod(nameof(IClrIndexedCollectionAccessor.Create))!;

    /// <summary>
    ///     Creates a new <see cref="ComplexPropertyParameterBinding" /> instance for the given <see cref="IComplexProperty" />.
    /// </summary>
    /// <param name="complexProperty">The complex property to bind.</param>
    public ComplexPropertyParameterBinding(IComplexProperty complexProperty)
        : base(complexProperty.ClrType, complexProperty)
    {
    }

    /// <summary>
    ///     Creates an expression tree representing the binding of the value of a complex property from a
    ///     materialization expression to a parameter of the constructor, factory method, etc.
    /// </summary>
    /// <param name="bindingInfo">The binding information.</param>
    /// <returns>The expression tree.</returns>
    public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
    {
        var complexProperty = (IComplexProperty)ConsumedProperties[0];

        if (complexProperty.IsCollection)
        {
            if (bindingInfo.IsEmptyMaterializer)
            {
                return Convert(
                    Call(
                        Constant(((IRuntimePropertyBase)complexProperty).GetIndexedCollectionAccessor()),
                        CreateCollectionMethod,
                        Constant(0)),
                    complexProperty.ClrType);
            }

            return new ComplexCollectionMaterializationExpression(
                complexProperty,
                bindingInfo.MaterializationContextExpression);
        }

        Check.DebugAssert(
            bindingInfo.MaterializerSource != null,
            "MaterializerSource must be set on ParameterBindingInfo to bind complex property constructor parameters.");

        return bindingInfo.MaterializerSource.CreateMaterializeExpression(
            new StructuralTypeMaterializerSourceParameters(
                complexProperty.ComplexType,
                "complexType",
                complexProperty.ClrType,
                complexProperty.IsNullable,
                QueryTrackingBehavior: null),
            bindingInfo.MaterializationContextExpression);
    }

    /// <summary>
    ///     Creates a copy that contains the given consumed properties.
    /// </summary>
    /// <param name="consumedProperties">The new consumed properties.</param>
    /// <returns>A copy with replaced consumed properties.</returns>
    public override ParameterBinding With(IPropertyBase[] consumedProperties)
        => new ComplexPropertyParameterBinding((IComplexProperty)consumedProperties.Single());
}

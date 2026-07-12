// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

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
    /// <summary>
    ///     Creates a new <see cref="ComplexPropertyParameterBinding" /> instance for the given <see cref="IComplexProperty" />.
    /// </summary>
    /// <param name="complexProperty">The complex property to bind.</param>
    public ComplexPropertyParameterBinding(IComplexProperty complexProperty)
        : base(complexProperty.ClrType, complexProperty)
    {
        if (complexProperty.IsCollection)
        {
            throw new ArgumentException(
                CoreStrings.ComplexCollectionConstructorBinding(
                    complexProperty.DeclaringType.DisplayName(), complexProperty.Name),
                nameof(complexProperty));
        }
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

/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using Remotion.TypePipe.Dlr.Dynamic.Utils;
using Remotion.TypePipe.MutableReflection;

#if SILVERLIGHT
using System.Core;
#endif

#if TypePipe
namespace Remotion.TypePipe.Dlr.Ast {
#else
namespace System.Linq.Expressions {
#endif

    /// <summary>
    /// Represents an expression that has a constant value.
    /// </summary>
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(Expression.ConstantExpressionProxy))]
#endif
    public class ConstantExpression : Expression {
        // Possible optimization: we could have a Constant<T> subclass that
        // stores the unboxed value.
        private readonly object _value;

        internal ConstantExpression(object value) {
            _value = value;
        }

        internal static ConstantExpression Make(object value, Type type) {
            if ((value == null && type == typeof(object)) || (value != null && value.GetType() == type)) {
                return new ConstantExpression(value);
            } else {
                return new TypedConstantExpression(value, type);
            }
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        public override Type Type {
            get {
                if (_value == null) {
                    return typeof(object);
                }
                return _value.GetType();
            }
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Constant; }
        }
        /// <summary>
        /// Gets the value of the constant expression.
        /// </summary>
        public object Value {
            get { return _value; }
        }

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        protected internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitConstant(this);
        }
    }

    internal class TypedConstantExpression : ConstantExpression {
        private readonly Type _type;

        internal TypedConstantExpression(object value, Type type)
            : base(value) {
            _type = type;
        }

        public sealed override Type Type {
            get { return _type; }
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="ConstantExpression"/> that has the <see cref="P:ConstantExpression.Value"/> property set to the specified value. .
        /// </summary>
        /// <param name="value">An <see cref="System.Object"/> to set the <see cref="P:ConstantExpression.Value"/> property equal to.</param>
        /// <returns>
        /// A <see cref="ConstantExpression"/> that has the <see cref="P:Expression.NodeType"/> property equal to 
        /// <see cref="F:ExpressionType.Constant"/> and the <see cref="P:Expression.Value"/> property set to the specified value.
        /// </returns>
        public static ConstantExpression Constant(object value) {
            return ConstantExpression.Make(value, value == null ? typeof(object) : value.GetType());
        }


        /// <summary>
        /// Creates a <see cref="ConstantExpression"/> that has the <see cref="P:ConstantExpression.Value"/> 
        /// and <see cref="P:ConstantExpression.Type"/> properties set to the specified values. .
        /// </summary>
        /// <param name="value">An <see cref="System.Object"/> to set the <see cref="P:ConstantExpression.Value"/> property equal to.</param>
        /// <param name="type">A <see cref="System.Type"/> to set the <see cref="P:Expression.Type"/> property equal to.</param>
        /// <returns>
        /// A <see cref="ConstantExpression"/> that has the <see cref="P:Expression.NodeType"/> property equal to 
        /// <see cref="F:ExpressionType.Constant"/> and the <see cref="P:ConstantExpression.Value"/> and 
        /// <see cref="P:Expression.Type"/> properties set to the specified values.
        /// </returns>
        public static ConstantExpression Constant(object value, Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            if (value == null && type.IsValueType && !TypeUtils.IsNullableType(type)) {
                throw Error.ArgumentTypesMustMatch();
            }
            if (value != null && !type.IsTypePipeAssignableFrom(value.GetType())) {
                throw Error.ArgumentTypesMustMatch();
            }
            return ConstantExpression.Make(value, type);
        }
    }
}

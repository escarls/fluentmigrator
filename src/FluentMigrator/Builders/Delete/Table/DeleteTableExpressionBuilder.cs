#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.Table
{
    /// <summary>
    /// An expression builder for a <see cref="DeleteTableExpression"/>
    /// </summary>
    public class DeleteTableExpressionBuilder : ExpressionBuilderBase<DeleteTableExpression>, IIfExistsOrInSchemaSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteTableExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public DeleteTableExpressionBuilder(DeleteTableExpression expression)
            : base(expression)
        {
            Expression.IfExists = false;
        }

        public IInSchemaSyntax IfExists()
        {
            Expression.IfExists = true;
            return this;
        }

        /// <inheritdoc />
        public void InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
        }
    }
}

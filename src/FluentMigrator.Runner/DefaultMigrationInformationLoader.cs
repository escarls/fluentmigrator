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

using System;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Exceptions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Default implementation of a <see cref="IMigrationInformationLoader"/>
    /// </summary>
    public class DefaultMigrationInformationLoader : IMigrationInformationLoader
    {
        [NotNull, ItemNotNull]
        private readonly IReadOnlyCollection<string> _tagsToMatch;

        [NotNull]
        private readonly IMigrationSource _source;

        [CanBeNull]
        private SortedList<long, IMigrationInfo> _migrationInfos;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMigrationInformationLoader"/> class.
        /// </summary>
        /// <param name="source">The configured source of <see cref="IMigration"/> instances</param>
        /// <param name="filterOptions">The filter options</param>
        /// <param name="conventions">The conventions used to validate tags and creating <see cref="IMigrationInfo"/> instances</param>
        /// <param name="runnerOptions">The runner options</param>
        public DefaultMigrationInformationLoader(
            [NotNull] IMigrationSource source,
            [NotNull] IOptionsSnapshot<TypeFilterOptions> filterOptions,
            [NotNull] IMigrationRunnerConventions conventions,
            [NotNull] IOptions<RunnerOptions> runnerOptions)
        {
            _source = source;
            Namespace = filterOptions.Value.Namespace;
            LoadNestedNamespaces = filterOptions.Value.NestedNamespaces;
            Conventions = conventions;
            _tagsToMatch = runnerOptions.Value.Tags ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets the migration runner conventions
        /// </summary>
        [NotNull]
        public IMigrationRunnerConventions Conventions { get; }

        /// <summary>
        /// Gets the namespace used to filter the <see cref="IMigration"/> instances
        /// </summary>
        [CanBeNull]
        public string Namespace { get; }

        /// <summary>
        /// Gets a value indicating whether all <see cref="IMigration"/> instances that
        /// are in a child namespace of <see cref="Namespace"/> should be used
        /// </summary>
        public bool LoadNestedNamespaces { get; }

        /// <inheritdoc />
        public SortedList<long, IMigrationInfo> LoadMigrations()
        {
            if (_migrationInfos != null)
            {
                if (_migrationInfos.Count == 0)
                    throw new MissingMigrationsException();
                return _migrationInfos;
            }

            _migrationInfos = new SortedList<long, IMigrationInfo>();
            var migrationInfos = FindMigrations(_source, Conventions, Namespace, LoadNestedNamespaces, _tagsToMatch);
            foreach (var migrationInfo in migrationInfos)
            {
                if (_migrationInfos.ContainsKey(migrationInfo.Version))
                {
                    throw new DuplicateMigrationException($"Duplicate migration version {migrationInfo.Version}.");
                }

                _migrationInfos.Add(migrationInfo.Version, migrationInfo);
            }

            if (_migrationInfos.Count == 0)
                throw new MissingMigrationsException();

            return _migrationInfos;
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<IMigrationInfo> FindMigrations(
            [NotNull] IMigrationSource source,
            [NotNull] IMigrationRunnerConventions conventions,
            [CanBeNull] string @namespace,
            bool loadNestedNamespaces,
            [NotNull, ItemNotNull] IReadOnlyCollection<string> tagsToMatch)
        {
            var migrations = source.GetMigrations().ToList();
            if (migrations.Count == 0)
            {
                throw new MissingMigrationsException("No migrations found");
            }

            var migrationInfos =
                (from migration in migrations
                 let type = migration.GetType()
                 where type.IsInNamespace(@namespace, loadNestedNamespaces)
                 where conventions.TypeHasMatchingTags(type, tagsToMatch) || (tagsToMatch.Count == 0 && !conventions.TypeHasTags(type)) || !conventions.TypeHasTags(type)
                 select conventions.GetMigrationInfoForMigration(migration));

            return migrationInfos.ToList();
        }
    }
}

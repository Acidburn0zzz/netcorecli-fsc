using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.DotNet.Tools.Test.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using FluentAssertions;
using static System.Environment;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace NetcoreCliFsc.Tests
{
    public static class TestSuite
    {
        private static IEnumerable<string> RepoNugetConfigFeeds()
        {
            Func<string,string> fixPath = path => {
                if (path.ToLower().StartsWith("http"))
                    return path;
                else
                    return Path.Combine(TestBase.RepoRoot, path);
            };
            var doc = XDocument.Load(Path.Combine(TestBase.RepoRoot, "NuGet.Config"));
            return doc.Element("configuration")
                      .Element("packageSources")
                      .Elements("add")
                      .Attributes("value")
                      .Select(a => a.Value)
                      .Select(fixPath)
                      .ToList();
        }

        public static IEnumerable<string> NugetConfigSources
        {
            get 
            {
                foreach (var feed in RepoNugetConfigFeeds())
                    yield return feed;

                var pkgsDir = Path.Combine(TestBase.RepoRoot, "test", "packagesToTest");
                if (Directory.Exists(pkgsDir))
                    yield return pkgsDir;
                var pkgs2Dir = Path.Combine(TestBase.RepoRoot, "test", "externalPackages");
                if (Directory.Exists(pkgs2Dir))
                    yield return pkgs2Dir;
            }
        }

        public static string NugetPackagesDir => Path.Combine(TestBase.RepoRoot, "test", "packages");

        public static string RestoreSourcesArgs(IEnumerable<string> sources)
        {
            return string.Join(" ", sources.Select(x => $"--source \"{x}\""));
        }

        public static string RestoreDefaultArgs => $"--no-cache {LogArgs} --packages \"{NugetPackagesDir}\"";

        public static string LogArgs => "-v n";

        public static MSBuildHostTypesOnly MSBuildHostTypesOnly
        {
            get 
            {
                var msbuildHost = 
                    (GetEnvironmentVariable("TEST_SUITE_MSBUILD_HOST_TYPES") ?? "")
                    .ToUpper()
                    .Split(';');
                
                var result = MSBuildHostTypesOnly.Core;
                if (msbuildHost.Contains("MSBUILD"))
                    result = result | MSBuildHostTypesOnly.MSBuild;
                if (msbuildHost.Contains("MONO"))
                    result = result | MSBuildHostTypesOnly.Mono;
                return result;
            }
        } 
    }
}

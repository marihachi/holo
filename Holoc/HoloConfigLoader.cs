using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Holoc;

public static class HoloConfigLoader
{
    private const string ConfigFileName = "holo-config.yml";

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(HoloConfig))]
    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    [UnconditionalSuppressMessage("AOT", "IL3050")]
    public static HoloConfig Load()
    {
        var exeDir = Path.GetDirectoryName(Environment.ProcessPath) ?? Directory.GetCurrentDirectory();
        var configPath = Path.Combine(exeDir, ConfigFileName);

        if (!File.Exists(configPath))
        {
            var config = new HoloConfig();
            Save(config, configPath);
            return config;
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var yaml = File.ReadAllText(configPath);
        return deserializer.Deserialize<HoloConfig>(yaml) ?? new HoloConfig();
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(HoloConfig))]
    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    [UnconditionalSuppressMessage("AOT", "IL3050")]
    private static void Save(HoloConfig config, string path)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(config);
        File.WriteAllText(path, yaml);
    }
}

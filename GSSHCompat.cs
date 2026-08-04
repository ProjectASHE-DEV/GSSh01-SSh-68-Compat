using System.Collections.Generic;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Servers;
using Range = SemanticVersioning.Range;

namespace Ssh68ConflictPatch;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.mrlahey.ssh68conflictpatch";
    public override string Name { get; init; } = "SSh-68 Helmet Conflict Patch";
    public override string Author { get; init; } = "MrLahey";
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override Range SptVersion { get; init; } = new("4.0.13");
    public override Dictionary<string, Range>? ModDependencies { get; init; } = null;
    public override List<string>? Contributors { get; init; }
    public override List<string>? Incompatibilities { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; } = false;
    public override string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class Ssh68ConflictPatchMod(
    ISptLogger<Ssh68ConflictPatchMod> logger,
    DatabaseServer databaseServer)
    : IOnLoad
{
    private const string SSH68_HELMET_ID = "5c06c6a80db834001b735491";
    private const string GSSH_HEADSET_ID = "5b432b965acfc47a8774094e";

    public async Task OnLoad()
    {
        RemoveBidirectionalConflict();
        await Task.CompletedTask;
    }

    private void RemoveBidirectionalConflict()
    {
        var tables = databaseServer.GetTables();
        var items = tables.Templates.Items;

        int removedCount = 0;

        if (items.TryGetValue(SSH68_HELMET_ID, out var helmetItem))
        {
            var helmetConflicts = helmetItem.Properties?.ConflictingItems;
            if (helmetConflicts != null && helmetConflicts.Contains(GSSH_HEADSET_ID))
            {
                helmetConflicts.Remove(GSSH_HEADSET_ID);
                removedCount++;
            }
        }
        else
        {
            logger.Error($"[SSh-68 Patch] Could not find SSh-68 helmet ID '{SSH68_HELMET_ID}' in database.");
        }
        if (items.TryGetValue(GSSH_HEADSET_ID, out var headsetItem))
        {
            var headsetConflicts = headsetItem.Properties?.ConflictingItems;
            if (headsetConflicts != null && headsetConflicts.Contains(SSH68_HELMET_ID))
            {
                headsetConflicts.Remove(SSH68_HELMET_ID);
                removedCount++;
            }
        }
        else
        {
            logger.Error($"[SSh-68 Patch] Could not find GSSh-01 headset ID '{GSSH_HEADSET_ID}' in database.");
        }
        if (removedCount > 0)
        {
            logger.Success($"[SSh-68 Patch] Successfully removed {removedCount}/2 conflict entries between the SSh-68 and GSSh-01 headset");
        }
        else
        {
            logger.Warning("[SSh-68 Patch] No conflict entries were changed because no conflicts were found.");
        }
    }
}

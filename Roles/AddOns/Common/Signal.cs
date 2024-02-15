using Hazel;
using System.Collections.Generic;
using UnityEngine;

namespace TOHE.Roles.AddOns.Common;
public static class Signal
{
    private static readonly int Id = 27800;
    private static List<byte> playerIdList = [];
    public static Dictionary<byte, Vector2> Signalbacktrack = new();
    public static bool IsEnable = false;
    public static OptionItem CanBeOnCrew;
    public static OptionItem CanBeOnImp;
    public static OptionItem CanBeOnNeutral;
    public static void SetupCustomOption()
    {
        Options.SetupAdtRoleOptions(Id, CustomRoles.Signal, canSetNum: true, tab: TabGroup.Addons);
        CanBeOnImp = BooleanOptionItem.Create(Id + 11, "ImpCanBeSignal", true, TabGroup.Addons, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Signal]);
        CanBeOnCrew = BooleanOptionItem.Create(Id + 12, "CrewCanBeSignal", true, TabGroup.Addons, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Signal]);
        CanBeOnNeutral = BooleanOptionItem.Create(Id + 13, "NeutralCanBeSignal", true, TabGroup.Addons, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Signal]);
    }
    public static void Init()
    {
        playerIdList = [];
        Signalbacktrack = new();
        IsEnable = false;
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        IsEnable = false;
    }
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);
    public static void AddPosi(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || !player.IsAlive() || !player.Is(CustomRoles.Signal) || !GameStates.IsInTask) return;
        Signalbacktrack.Add(player.PlayerId, player.GetTruePosition());
        SendRPC();
    }
    public static void AfterMeetingTasks()
    {
        foreach (var pc in Main.AllAlivePlayerControls)
        {
            if (pc.Is(CustomRoles.Signal))
            {
                pc.RpcTeleport(Signalbacktrack[pc.PlayerId]);
            }
        }
        Signalbacktrack = new();
    }
    public static void SendRPC()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SignalPosition, SendOption.Reliable, -1);
        foreach (var pc in Main.AllAlivePlayerControls)
        {
            if (Signalbacktrack.ContainsKey(pc.PlayerId))
            {
                writer.Write(pc.PlayerId);
                writer.Write(Signalbacktrack[pc.PlayerId].x);
                writer.Write(Signalbacktrack[pc.PlayerId].y);
            }
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC(MessageReader reader)
    {
        var pc = reader.ReadByte();
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        Signalbacktrack.Add(pc,new(x, y));
    }
}

﻿using System;
using Unity.Netcode;
using UnityEngine;

namespace LethalMenu.Types
{
    public enum Unlockable : int
    {
        /**USELESS
        OrangeSuit = 0,
        Cupboard = 7,
        FileCabinet = 8,
        LightSwitch = 11,
        Bunkbeds = 15,
        Terminal = 16,
        **/

        Terminal = 16,
        OrangeSuit = 0,
        GreenSuit = 1,
        HazardSuit = 2,
        PajamaSuit = 3,
        CozyLights = 4,
        Teleporter = 5,
        Television = 6,
        Toilet = 9,
        Shower = 10,
        RecordPlayer = 12,
        Table = 13,
        RomanticTable = 14,
        SignalTranslator = 17,
        LoudHorn = 18,
        InverseYeleporter = 19,
        JackOLantern = 20,
        WelcomeMat = 21,
        Goldfish = 22,
        PlushiePajamaMan = 23,
        PurpleSuit = 24,
        BeeSuit = 25,
        BunnySuit = 26,
        DiscoBall = 27,
    }

    public static class UnlockableExtensions
    {
        public static UnlockableItem GetItem(this Unlockable unlockable)
        {
            return StartOfRound.Instance.unlockablesList.unlockables[(int)unlockable];
        }

        public static void SetLocked(this Unlockable unlockable, bool locked)
        {
            UnlockableItem item = unlockable.GetItem();
            item.alreadyUnlocked = locked;
            item.hasBeenUnlockedByPlayer = locked;
        }

        public static void Buy(this Unlockable unlockable, int credits)
        {
            unlockable.SetLocked(false);
            StartOfRound.Instance.BuyShipUnlockableServerRpc((int)unlockable, credits);
            StartOfRound.Instance.SyncShipUnlockablesServerRpc();
        }

        public static void Return(this Unlockable unlockable)
        {
            StartOfRound.Instance.ReturnUnlockableFromStorageServerRpc((int)unlockable);
        }

        public static void Store(this Unlockable unlockable, bool clone)
        {
            if (GameObject.Find(unlockable + (clone ? "(Clone)" : ""))?.GetComponent<NetworkObject>() is NetworkObject networkObject) ShipBuildModeManager.Instance.StoreObjectServerRpc(new NetworkObjectReference(networkObject), (int)LethalMenu.localPlayer.actualClientId);
        }

        public static void Move(this Unlockable unlockable, Vector3 Position, Vector3 Rotation, bool clone)
        {
            if (GameObject.Find(unlockable + (clone ? "(Clone)" : ""))?.GetComponent<NetworkObject>() is NetworkObject networkObject) ShipBuildModeManager.Instance.PlaceShipObjectServerRpc(Position, Rotation, new NetworkObjectReference(networkObject), (int)LethalMenu.localPlayer.actualClientId);
        }

        public static Vector3 GetPosition(this Unlockable unlockable, bool clone)
        {
            if (GameObject.Find(unlockable + (clone ? "(Clone)" : ""))?.GetComponent<NetworkObject>() is NetworkObject networkObject) return networkObject.transform.position;
            return Vector3.zero;
        }

        public static Vector3 GetRotation(this Unlockable unlockable, bool clone)
        {
            if (GameObject.Find(unlockable + (clone ? "(Clone)" : ""))?.GetComponent<NetworkObject>() is NetworkObject networkObject) return networkObject.transform.rotation.eulerAngles;
            return Vector3.zero;
        }
    }
}

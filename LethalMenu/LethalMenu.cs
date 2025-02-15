using GameNetcodeStuff;
using HarmonyLib;
using LethalMenu.Language;
using LethalMenu.Menu.Core;
using LethalMenu.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;
using LethalMenu.Themes;
using LethalMenu.Menu.Popup;

namespace LethalMenu
{
    public class LethalMenu : MonoBehaviour
    {
        private List<Cheat> cheats;
        public static List<GrabbableObject> items = new List<GrabbableObject>();
        public static List<Landmine> landmines = new List<Landmine>();
        public static List<Turret> turrets = new List<Turret>();
        public static List<EntranceTeleport> doors = new List<EntranceTeleport>();
        public static List<PlayerControllerB> players = new List<PlayerControllerB>();
        public static List<EnemyAI> enemies = new List<EnemyAI>();
        public static List<SteamValveHazard> steamValves = new List<SteamValveHazard>();
        public static List<TerminalAccessibleObject> bigDoors = new List<TerminalAccessibleObject>();
        public static List<TerminalAccessibleObject> allTerminalObjects = new List<TerminalAccessibleObject>();
        public static List<DoorLock> doorLocks = new List<DoorLock>();
        public static List<ShipTeleporter> teleporters = new List<ShipTeleporter>();
        public static List<InteractTrigger> interactTriggers = new List<InteractTrigger>();
        public static List<SpikeRoofTrap> spikeRoofTraps = new List<SpikeRoofTrap>();
        public static List<MoldSpore> vainShrouds = new List<MoldSpore>();
        public static List<AnimatedObjectTrigger> animatedTriggers = new List<AnimatedObjectTrigger>();
        public static HangarShipDoor shipDoor;
        public static BreakerBox breaker;
        public static MineshaftElevatorController mineshaftElevator;
        public static PlayerControllerB localPlayer;
        public static VehicleController vehicle;
        public static Volume volume;
        public static PatcherTool ZapGun;
        public static int selectedPlayer = -1;
        public int fps;
        public Dictionary<string, string> LMUsers { get; set; } = [];


        public static Harmony harmony;
        private HackMenu menu;
        private static LethalMenu instance;
        public static LethalMenu Instance
        {
            get
            {
                if (instance == null)
                    instance = new LethalMenu();
                return instance;
            }
        }

        public void Start()
        {
            instance = this;
            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Settings.debugMessage = (e.Message + "\n" + e.StackTrace);
            }
        }

        private void Initialize()
        {
            Localization.Initialize();
            Theme.Initialize();
            HarmonyPatching();
            LoadCheats();
            MenuUtil.LMUser();
            this.StartCoroutine(this.CollectObjects());
            this.StartCoroutine(this.FPSCounter());
        }

        private void HarmonyPatching()
        {
            harmony = new Harmony("LethalMenu");
            Harmony.DEBUG = false;
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void LoadCheats()
        {
            try
            {
                Settings.Changelog.ReadChanges();
                cheats = new List<Cheat>();
                menu = new HackMenu();
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => String.Equals(t.Namespace, "LethalMenu.Cheats", StringComparison.Ordinal) && t.IsSubclassOf(typeof(Cheat))))
                {
                    cheats.Add((Cheat)Activator.CreateInstance(type));
                }
                Settings.Config.SaveDefaultConfig();
                Settings.Config.LoadConfig();
            }
            catch (Exception e)
            {
                Settings.debugMessage = (e.Message);
            }
        }

        public void FixedUpdate()
        {
            try
            {
                if ((bool)StartOfRound.Instance) cheats.ForEach(cheat => cheat.FixedUpdate());
            }
            catch (Exception e)
            {
                Settings.debugMessage = ("Msg: " + e.Message + "\nSrc: " + e.Source + "\n" + e.StackTrace);
            }
        }

        public void Update()
        {
            if (!(bool)StartOfRound.Instance || StartOfRound.Instance.inShipPhase) Settings.v_savedLocation = Vector3.zero;
            try
            {
                foreach (Hack hack in Enum.GetValues(typeof(Hack)))
                {
                    if ((bool)StartOfRound.Instance && localPlayer != null && (localPlayer.isTypingChat || localPlayer.quickMenuManager.isMenuOpen || localPlayer.inTerminalMenu)) continue;
                    if (hack.HasKeyBind() && hack.GetKeyBind().wasPressedThisFrame && !hack.IsAnyHackWaiting()) hack.Execute();
                }
                if (!(bool)StartOfRound.Instance) return;
                cheats.ForEach(cheat => cheat.Update());
            }
            catch (Exception e)
            {
                Settings.debugMessage = ("Msg: " + e.Message + "\nSrc: " + e.Source + "\n" + e.StackTrace);
            }
        }
        
        public IEnumerator FPSCounter()
        {
            while(true)
            {
                fps = (int)(1.0f / Time.deltaTime);
                yield return new WaitForSeconds(1f);
            }
        }

        public void OnGUI()
        {
            try
            {
                if (Event.current.type == EventType.Repaint)
                {
                    string LethalMenuTitle = $"Lethal Menu {Settings.version} By IcyRelic, and Dustin | Menu Toggle: {FirstSetupManagerWindow.kb}";
                    LethalMenuTitle += Settings.b_FPSCounter ? $" | FPS: {fps}" : "";
                    VisualUtil.DrawString(new Vector2(5f, 2f), LethalMenuTitle, Settings.c_primary, centered: false, bold: true, fontSize: 14);
                    if (MenuUtil.resizing)
                    {
                        VisualUtil.DrawString(new Vector2(Screen.width / 2, 35f), Localization.Localize(["SettingsTab.ResizeTitle", "SettingsTab.ResizeConfirm", $"{HackMenu.Instance.windowRect.width}x{HackMenu.Instance.windowRect.height}"], true), Settings.c_playerESP, true, true, true, true, 22);
                        MenuUtil.ResizeMenu();
                    }
                    if (Settings.DebugMode)
                    {
                        VisualUtil.DrawString(new Vector2(5f, 20f), "[DEBUG MODE]", new RGBAColor(50, 205, 50, 1f), false, false, false, true, 10);
                        VisualUtil.DrawString(new Vector2(10f, 65f), new RGBAColor(255, 195, 0, 1f).AsString(Settings.debugMessage), false, false, false, false, 22);
                    }
                    if ((bool)StartOfRound.Instance) cheats.ForEach(cheat => cheat.OnGui());
                }
                menu.Draw();
            }
            catch (Exception e)
            {
                Settings.debugMessage = ("Msg: " + e.Message + "\nSrc: " + e.Source + "\n" + e.StackTrace);
            }
        }

        public IEnumerator CollectObjects()
        {
            float totalDuration = 3f;
            int totalTasks = 17;
            float interval = totalDuration / totalTasks;

            while (true)
            {
                var tasks = new List<IEnumerator>
        {
            CollectWithBatch(items),
            CollectWithBatch(landmines),
            CollectWithBatch(turrets),
            CollectWithBatch(doors),
            CollectWithBatch(players, obj => !obj.playerUsername.StartsWith("Player #") && !obj.disconnectedMidGame),
            CollectWithBatch(enemies),
            CollectWithBatch(steamValves),
            CollectWithBatch(allTerminalObjects),
            CollectWithBatch(teleporters),
            CollectWithBatch(interactTriggers),
            CollectWithBatch(bigDoors, obj => obj.isBigDoor),
            CollectWithBatch(doorLocks),
            CollectWithBatch(spikeRoofTraps),
            CollectWithBatch(animatedTriggers),
            FindObjectWithDelay<HangarShipDoor>(obj => shipDoor = obj),
            FindObjectWithDelay<BreakerBox>(obj => breaker = obj),
            FindObjectWithDelay<MineshaftElevatorController>(obj => mineshaftElevator = obj)
        };

                foreach (var task in tasks)
                {
                    yield return StartCoroutine(task);
                    yield return new WaitForSeconds(interval);
                }
                localPlayer = GameNetworkManager.Instance?.localPlayerController;

                yield return new WaitForSeconds(0.1f);

            }
        }


        private IEnumerator CollectWithBatch<T>(List<T> list, Func<T, bool> filter = null) where T : MonoBehaviour
        {
            list.Clear();
            var foundObjects = Object.FindObjectsOfType<T>();
            int batchSize = 20;

            for (int i = 0; i < foundObjects.Length; i++)
            {
                if (filter == null || filter(foundObjects[i]))
                    list.Add(foundObjects[i]);

                if ((i + 1) % batchSize == 0) 
                    yield return null;
            }
        }

        private IEnumerator FindObjectWithDelay<T>(Action<T> assignAction) where T : MonoBehaviour
        {
            assignAction(Object.FindObjectOfType<T>());
            yield return null;
        }




        public void Unload()
        {
            this.StopAllCoroutines();
            Loader.Unload();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using UnityEngine;
using MEC;
using Exiled.API.Features.Core;
using Mirror;
using PlayerRoles;
using MapEditorReborn.API.Features.Objects;
using Exiled.API.Features.Doors;

namespace DAONPowerOutage
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance;

        public List<Transform> ClubLights = new List<Transform>();
        public List<Transform> Pads = new List<Transform>();

        public bool ButtonPressed = false;
        public bool LastOneMode = false;
        public bool BlockBlackout = true;

        ReferenceHub dj;

        // -------------------------------------------------------------------------------------------------- // [OnEnabled & OnDisabled]

        public override void OnEnabled()
        {
           Instance = this;

            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawningTeam;

            Exiled.Events.Handlers.Cassie.SendingCassieMessage += OnSendingCassieMessage;

            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.SearchingPickup += OnSearchingPickup;
            // Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;

            Exiled.Events.Handlers.Scp330.InteractingScp330 += OnInteractingScp330;

            if (UnityEngine.Random.Range(1, 5) == 1)
                Config.IsLovelyArloEnabled = true;

            else
                Config.IsLovelyArloEnabled = false;

            base.OnEnabled();
        }

        public override void OnDisabled() 
        { 
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawningTeam;

            Exiled.Events.Handlers.Cassie.SendingCassieMessage -= OnSendingCassieMessage;

            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.SearchingPickup -= OnSearchingPickup;
            // Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;

            Exiled.Events.Handlers.Scp330.InteractingScp330 -= OnInteractingScp330;

            base.OnEnabled();
            Instance = null;
        }

        // -------------------------------------------------------------------------------------------------- // [Exiled.Events.EventArgs]

        public async void OnWaitingForPlayers()
        {
            if (Config.IsLovelyArloEnabled)
            {
                Server.ExecuteCommand("/mp load POplus");

                await Task.Delay(1000);

                ClubLights = GameObject.FindObjectsOfType<Transform>().Where(t => t.name == "ClubLight").ToList();
                Pads = GameObject.FindObjectsOfType<Transform>().Where(t => t.name == "Pad").ToList();
            }

            else
                Server.ExecuteCommand("/mp load PO");
        }

        public async void OnRoundStarted()
        {
            if (Config.IsLovelyArloEnabled)
            {
                dj = GGUtils.Gtool.Spawn(RoleTypeId.Tutorial, new Vector3(136.45f, 998.8373f, -63.06765f));

                Dictionary<ReferenceHub, string> register = new Dictionary<ReferenceHub, string>()
                {
                    { dj, "dj" }
                };

                foreach (var reg in register)
                {
                    try
                    {
                        GGUtils.Gtool.Register(reg.Key, reg.Value);
                    }
                    catch (Exception e)
                    {
                    }
                }

                GGUtils.Gtool.PlayerGet("dj").DisplayNickname = "DJ";
                GGUtils.Gtool.PlaySound("dj", "tothemoon", VoiceChat.VoiceChatChannel.Proximity, 20, true);

                Timing.RunCoroutine(DJHeadBanging());
                Timing.RunCoroutine(DJ());
                Timing.RunCoroutine(LastOne());
            }
            Timing.RunCoroutine(Blackout());
            Timing.RunCoroutine(LastScpBlackout());
            Timing.RunCoroutine(GSounds());
            Timing.RunCoroutine(AutoTesla());
            Timing.RunCoroutine(CustomRoomColor());

            while (true)
            {
                while (!ButtonPressed)
                {
                    foreach (var player in Player.List.Where(x => !x.IsNPC))
                    {
                        if (Physics.Raycast(player.Position, Vector3.down, out RaycastHit hit, 1f, (LayerMask)1))
                        {
                            if (hit.transform.name == "Red")
                            {
                                if (!ButtonPressed)
                                    ButtonPressed = true;
                            }
                        }
                    }
                    await Task.Delay(100);
                }

                Map.ChangeLightsColor(Color.HSVToRGB(1f, 1f, 1f));
                await Task.Delay(200);
                Map.ChangeLightsColor(Color.HSVToRGB(0.1f, 0.1f, 0.1f));
                ButtonPressed = false;
            }
        }

        public void OnSendingCassieMessage(Exiled.Events.EventArgs.Cassie.SendingCassieMessageEventArgs ev)
        {
            if (ev.Words.Contains("SCP") && !BlockBlackout)
                Map.TurnOffAllLights(180);
        }

        public void OnRespawningTeam(Exiled.Events.EventArgs.Server.RespawningTeamEventArgs ev)
        {
            Timing.CallDelayed(120f, () => { Map.TurnOffAllLights(180); });
        }

        public async void OnSpawned(Exiled.Events.EventArgs.Player.SpawnedEventArgs ev)
        {
            if (ev.Reason == Exiled.API.Enums.SpawnReason.RoundStart && ev.Player.IsScp)
            {
                if (UnityEngine.Random.Range(1, 8) == 1)
                    ev.Player.Role.Set(RoleTypeId.Scp3114);
            }

            if (UnityEngine.Random.Range(1, 9) != 1)
                ev.Player.AddItem(ItemType.Flashlight);
            else
                ev.Player.AddItem(ItemType.Lantern);

            // ev.Player.EnableEffect(Exiled.API.Enums.EffectType.FogControl, 7);
            ev.Player.EnableEffect(Exiled.API.Enums.EffectType.Scanned);
            await Task.Delay(7000);
            ev.Player.DisableEffect(Exiled.API.Enums.EffectType.Scanned);
        }

        public void OnInteractingScp330(Exiled.Events.EventArgs.Scp330.InteractingScp330EventArgs ev)
        {
            if (UnityEngine.Random.Range(1, 7) == 1)
            {
                ev.IsAllowed = false;
                ev.Player.TryAddCandy(InventorySystem.Items.Usables.Scp330.CandyKindID.Pink);
            }
        }

        public async void OnInteractingDoor(Exiled.Events.EventArgs.Player.InteractingDoorEventArgs ev)
        {
            await Task.Delay(10000);

            ev.Door.IsOpen = false;
        }

        public void OnHurting(Exiled.Events.EventArgs.Player.HurtingEventArgs ev)
        {
            if (Config.IsLovelyArloEnabled && ev.Player.IsNPC)
                ev.IsAllowed = false;
        }

        public void OnSearchingPickup(Exiled.Events.EventArgs.Player.SearchingPickupEventArgs ev)
        {
            if (Config.IsLovelyArloEnabled && ev.Pickup.Type == ItemType.Coin && ev.Pickup.Rotation == new Quaternion(0, 0, 90, 0))
            {
                if (!LastOneMode)
                {
                    LastOneMode = true;
                    ev.Player.ShowHint($"<color=red><i><size=20>라스트 원 모드가 활성화되었습니다.</size></i></color>", 3f);
                }
            }
        }

        // -------------------------------------------------------------------------------------------------- // [IEnumerator]

        public IEnumerator<float> Blackout()
        {
            yield return Timing.WaitForSeconds(180f);

            BlockBlackout = false;

            while (true)
            {
                if (Player.List.Where(x => !x.IsScp && x.IsAlive).Count() >= 10)
                    Map.TurnOffAllLights(90f);

                yield return Timing.WaitForSeconds(180f);
            }
        }

        public IEnumerator<float> LastScpBlackout()
        {
            yield return Timing.WaitForSeconds(1f);

            GameObject Prefab = PrefabHelper.Spawn(Exiled.API.Enums.PrefabType.RegularKeycardPickup, new Vector3(1, 1, 1));
            Prefab.GetComponent<DoorObject>().Scale = new

            while (true)
            {
                if (!BlockBlackout && Player.List.Where(x => x.IsScp && x.IsAlive).Count() == 1)
                    Map.TurnOffAllLights(99999f);

                yield return Timing.WaitForSeconds(1f);
            }
        }

        public IEnumerator<float> GSounds()
        {
           while (true)
           {
                yield return Timing.WaitForSeconds(300);

                Server.ExecuteCommand($"/cassie_sl .G{UnityEngine.Random.Range(1, 8)}");
            }
        }

        public IEnumerator<float> AutoTesla()
        {
            while (true)
            {
                List<Exiled.API.Features.TeslaGate> Teslas = Exiled.API.Features.TeslaGate.List.ToList();
                Teslas[UnityEngine.Random.Range(0, Teslas.Count)].Trigger(isInstantBurst: UnityEngine.Random.Range(1, 10) == 1 ? true : false);

               yield return Timing.WaitForSeconds(UnityEngine.Random.Range(1, 100));
            }
        }

        public IEnumerator<float> CustomRoomColor()
        {
            List<Room> room = Room.List.ToList();

            while (true)
            {
                List<Room> crs = new List<Room>();

                for (int i = 0; i < 10; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, room.Count);
                    Room randomRoom = room[randomIndex];
                    crs.Add(randomRoom);
                }

                crs.ForEach(x => x.Color = new Color(UnityEngine.Random.Range(0.1f, 25f), UnityEngine.Random.Range(0.1f, 25f), UnityEngine.Random.Range(0.1f, 25f)));
                yield return Timing.WaitForSeconds(5);

                crs.ForEach(x => x.Color = new Color(1f, 1f, 1f));
                yield return Timing.WaitForSeconds(1f);
            }
        }

        public IEnumerator<float> DJHeadBanging()
        {
            yield return Timing.WaitForSeconds(1f);

            bool HeadUp = true;

            while (true)
            {
                if (HeadUp)
                {
                    GGUtils.Gtool.Rotate(dj, new Vector3(0, -1f, 0));

                    HeadUp = false;

                    yield return Timing.WaitForSeconds(0.2f);
                }
                else
                {
                    GGUtils.Gtool.Rotate(dj, new Vector3(0, 1f, 0));

                    HeadUp = true;

                    yield return Timing.WaitForSeconds(0.15f);
                }
            }
        }

        public IEnumerator<float> DJ()
        {
            yield return Timing.WaitForSeconds(1f);

            while (true)
            {
                foreach (var Pad in Pads)
                {
                    Pad.GetComponent<PrimitiveObject>().Primitive.Color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                }

                foreach (var ClubLight in ClubLights)
                {
                    ClubLight.GetComponent<Light>().color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                }

                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        public IEnumerator<float> LastOne()
        {
            yield return Timing.WaitForSeconds(1f);

            while (true)
            {
                if (LastOneMode && Player.List.Where(x => x.IsAlive && !x.IsScp && !x.IsTutorial).Count() == 1)
                { 
                    GGUtils.Gtool.PlaySound("dj", "lastone", VoiceChat.VoiceChatChannel.Intercom, 30, false);
                    break;
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}

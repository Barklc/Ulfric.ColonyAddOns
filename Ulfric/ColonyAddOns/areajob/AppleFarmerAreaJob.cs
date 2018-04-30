using BlockTypes.Builtin;
using NPC;
using System.IO;
using System.Threading;
using Pipliz.Mods.APIProvider.AreaJobs;
using Pipliz;
using Pipliz.JSON;
using Pipliz.Threading;
using Server.NPCs;

namespace Ulfric.ColonyAddOns.AreaJobs
{
    [ModLoader.ModManager]
    [AreaJobDefinitionAutoLoader]
    public class AppleFarmerDefinition : AreaJobDefinitionDefault<AppleFarmerDefinition>
    {
        static NPCTypeStandardSettings MilitiaNPCSettings = new NPCTypeStandardSettings()
        {
            type = NPCTypeID.GetNextID(),
            keyName = GameLoader.NAMESPACE + ".AreaJobs.AppleFarmer",
            printName = "Apple Farmer",
            maskColor0 = UnityEngine.Color.red,
            maskColor1 = UnityEngine.Color.magenta
        };
        public static NPCType AppleFarmerNPCType;

        private string logName = "logtemperate";
        private string saplingName = "Ulfric.ColonyAddOns.Blocks.AppleSapling";
        private string leaveName = "Ulfric.ColonyAddOns.Blocks.LeavesApples";
        private string fruitName = "Ulfric.ColonyAddOns.Blocks.Apple";
        private ushort logIndex = 0;
        private ushort saplingIndex = 0;
        private ushort leaveIndex = 0;
        private ushort fruitIndex = 0;
        private string name = "applefarms";
        private string id = "Ulfric.ColonyAddOns.AreaJobs.AppleFarm";
        private string npctype = "Ulfric.ColonyAddOns.AreaJobs.AppleFarmer";

        public AppleFarmerDefinition()
        {
            logIndex = ItemTypes.IndexLookup.GetIndex(logName);
            saplingIndex = ItemTypes.IndexLookup.GetIndex(saplingName);
            leaveIndex = ItemTypes.IndexLookup.GetIndex(leaveName);
            fruitIndex = ItemTypes.IndexLookup.GetIndex(fruitName);

            Logger.Log("Loading {0} AreaJobDefinitionAutoLoader", id);

            identifier = id;
            fileName = name;
            NPCType.AddSettings(MilitiaNPCSettings);
            npcType = Server.NPCs.NPCType.GetByKeyNameOrDefault(npctype);
            areaType = Shared.EAreaType.BerryFarm;
        }

        public override IAreaJob CreateAreaJob(Players.Player owner, Vector3Int min, Vector3Int max, int npcID = 0)
        {
            Logger.Log("Create area job");
            return new AppleFarmerJob(owner, min, max, npcID);
        }

        public override void CalculateSubPosition(IAreaJob rawJob, ref Vector3Int positionSub)
        {

            if (rawJob != null)
            {
                AppleFarmerJob job = (AppleFarmerJob)rawJob;

                Vector3Int min = job.Minimum;
                Vector3Int max = job.Maximum;

                Logger.Log("Min/Max {0}/{1}", job.Minimum, job.Maximum);
                Logger.Log("Saplings available {0}", job.NPC.Colony.UsedStockpile.Contains(saplingIndex));
                if (job.checkMissingBushes && job.NPC.Colony.UsedStockpile.Contains(saplingIndex))
                {

                    // remove legacy positions
                    for (int x = min.x + 1; x <= max.x; x += 3)
                    {
                        for (int z = min.z; z <= max.z; z += 3)
                        {
                            ushort type;
                            Vector3Int possiblePositionSub = new Vector3Int(x, min.y, z);
                            if (!World.TryGetTypeAt(possiblePositionSub, out type))
                            {
                                return;
                            }
                            if (type == leaveIndex)
                            {
                                job.removingOldBush = true;
                                job.bushLocation = possiblePositionSub;
                                positionSub = Server.AI.AIManager.ClosestPosition(job.bushLocation, job.NPC.Position);
                                return;
                            }
                        }
                    }
                    // place new positions
                    for (int x = min.x; x <= max.x; x += 3)
                    {
                        for (int z = min.z; z <= max.z; z += 3)
                        {
                            ushort type;
                            Vector3Int possiblePositionSub = new Vector3Int(x, min.y, z);
                            if (!World.TryGetTypeAt(possiblePositionSub, out type))
                            {
                                return;
                            }
                            if (type == 0)
                            {
                                job.placingMissingBush = true;
                                job.bushLocation = possiblePositionSub;
                                positionSub = Server.AI.AIManager.ClosestPositionNotAt(job.bushLocation, job.NPC.Position);
                                return;
                            }
                        }
                    }
                    job.checkMissingBushes = false;
                }

                positionSub = min;
                positionSub.x += Random.Next(0, (max.x - min.x) / 3 + 1) * 3;
                positionSub.z += Random.Next(0, (max.z - min.z) / 3 + 1) * 3;
            }
            else
                Logger.Log("rawJob equals null");
        }

        static System.Collections.Generic.List<ItemTypes.ItemTypeDrops> GatherResults = new System.Collections.Generic.List<ItemTypes.ItemTypeDrops>();

        public override void OnNPCAtJob(IAreaJob rawJob, ref Vector3Int positionSub, ref NPCBase.NPCState state, ref bool shouldDumpInventory)
        {
            AppleFarmerJob job = (AppleFarmerJob)rawJob;

            state.JobIsDone = true;
            if (positionSub.IsValid)
            {
                ushort type;
                if (job.placingMissingBush)
                {
                    if (job.NPC.Colony.UsedStockpile.TryRemove(saplingIndex))
                    {
                        job.placingMissingBush = false;
                        ServerManager.TryChangeBlock(job.bushLocation, saplingIndex, rawJob.Owner, ServerManager.SetBlockFlags.DefaultAudio);
                        state.SetCooldown(2.0);
                    }
                    else
                    {
                        state.SetIndicator(new Shared.IndicatorState(Random.NextFloat(8f, 14f), saplingIndex, true, false));
                    }
                }
                else if (job.removingOldBush)
                {
                    if (ServerManager.TryChangeBlock(job.bushLocation, 0, rawJob.Owner, ServerManager.SetBlockFlags.DefaultAudio))
                    {
                        job.NPC.Colony.UsedStockpile.Add(saplingIndex);
                        job.removingOldBush = false;
                    }
                    state.SetCooldown(2.0);
                }
                else if (World.TryGetTypeAt(positionSub, out type))
                {
                    if (type == 0)
                    {
                        job.checkMissingBushes = true;
                        state.SetCooldown(1.0, 4.0);
                    }
                    else if (World.TryGetTypeAt(positionSub.Add(0, 3, 0), out type) && type == leaveIndex)
                    {
                        GatherResults.Clear();
                        GatherResults.Add(new ItemTypes.ItemTypeDrops(fruitIndex, 1, 1.0));
                        GatherResults.Add(new ItemTypes.ItemTypeDrops(saplingIndex, 1, 0.1));

                        ModLoader.TriggerCallbacks(ModLoader.EModCallbackType.OnNPCGathered, rawJob as IJob, positionSub, GatherResults);

                        InventoryItem toShow = ItemTypes.ItemTypeDrops.GetWeightedRandom(GatherResults);
                        if (toShow.Amount > 0)
                        {
                            state.SetIndicator(new Shared.IndicatorState(8.5f, toShow.Type));
                        }
                        else
                        {
                            state.SetCooldown(8.5);
                        }

                        job.NPC.Inventory.Add(GatherResults);
                    }
                    else
                    {
                        state.SetIndicator(new Shared.IndicatorState(Random.NextFloat(8f, 14f), BuiltinBlocks.ErrorMissing));
                    }
                }
                else
                {
                    state.SetCooldown(Random.NextFloat(3f, 6f));
                }
                positionSub = Vector3Int.invalidPos;
            }
            else
            {
                state.SetCooldown(10.0);
            }
        }

       
        /// <summary>
        /// Simple wrapper to have some per-job data
        /// </summary>
        class AppleFarmerJob : DefaultFarmerAreaJob<AppleFarmerDefinition>
        {
            public Vector3Int bushLocation = Vector3Int.invalidPos;
            public bool checkMissingBushes = true;
            public bool placingMissingBush = false;
            public bool removingOldBush = false;

            public AppleFarmerJob(Players.Player owner, Vector3Int min, Vector3Int max, int npcID = 0) : base(owner, min, max, npcID)
            {
                Logger.Log("New AppleFarmerJob NPCID= {0}", npcID);
            }

            public override Vector3Int GetJobLocation()
            {
                Logger.Log("AppleFarmerJob.GetJobLocation {0}", positionSub.IsValid);
                if (!positionSub.IsValid)
                {
                    CalculateSubPosition();
                }
                return positionSub;
            }
            public override void CalculateSubPosition()
            {
                Logger.Log("CalculateSubPosition in AppleFarmerJob {0}", this.GetType() == null ? "" : this.GetType().ToString());
                Logger.Log("Definition {0}", Definition.GetType() == null ? "" : Definition.GetType().ToString());
                Logger.Log("Min/Max {0}/{1}", positionMin, positionMax);
                Definition.CalculateSubPosition(this, ref positionSub);
            }
            public override NPCBase NPC
            {
                get
                {
                    return usedNPC;
                }
                set
                {
                    Logger.Log("AppleFarmer NPC");
                    if (usedNPC != value)
                    {
                        if (usedNPC != null)
                        {
                            usedNPC.ClearJob();
                        }

                        usedNPC = value;
                        if (usedNPC == null)
                        {
                            JobTracker.Add(this);

                        }
                        else
                        {
                            usedNPC.TakeJob(this);
                            Logger.Log("NPC = {0}",usedNPC.ID);
                        }
                    }
                    else if (value == null)
                    {
                        JobTracker.Add(this);
                    }
                }
            }
        }

        #region LOAD_LEGACY_BLOCKS_WORKAROUND
        /// <summary>
        /// This #region code is to load the legacy json data for upgrading from the old area jobs to this
        /// from before v0.5.0 to v0.5.0 and later
        /// </summary>
        JSONNode legacyJSON;

        public override void StartLoading()
        {
            // do custom things before base.AsyncLoad so FinishLoading also waits for this to complete
            ThreadPool.QueueUserWorkItem(delegate (object obj)
            {
                try
                {
                    string path = string.Format("gamedata/savegames/{0}/blocktypes/AppleFarmerAreaJob.json", ServerManager.WorldName);
                    if (File.Exists(path))
                    {
                        Log.Write("Loading legacy json from {0}", path);
                        JSON.Deserialize(path, out legacyJSON, false);
                        File.Delete(path);
                    }
                }
                catch (System.Exception e)
                {
                    Log.WriteException(e);
                }
                finally
                {
                    AsyncLoad(obj);
                }
            });
        }

        public override void FinishLoading()
        {
            base.FinishLoading();
            if (legacyJSON != null)
            {
                foreach (var pair in legacyJSON.LoopObject())
                {
                    try
                    {
                        Players.Player player = Players.GetPlayer(NetworkID.Parse(pair.Key));

                        for (int i = 0; i < pair.Value.ChildCount; i++)
                        {
                            JSONNode jobNode = pair.Value[i];

                            int npcID = jobNode.GetAsOrDefault("npcID", 0);
                            Vector3Int min = (Vector3Int)jobNode["positionMin"];
                            Vector3Int max = (Vector3Int)jobNode["positionMax"];

                            var job = new DefaultFarmerAreaJob<AppleFarmerDefinition>(player, min, max, npcID);
                            if (!AreaJobTracker.RegisterAreaJob(job))
                            {
                                job.OnRemove();
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Log.WriteException("Exception loading legacy area job data", e);
                    }
                }
                legacyJSON = null;
            }
        }

        #endregion LOAD_LEGACY_BLOCKS_WORKAROUND


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".AppleFarmer.trychangeblock")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        {
            if (userData.TypeNew == ItemTypes.IndexLookup.GetIndex("Ulfric.ColonyAddOns.Blocks.AppleBasket") && userData.TypeOld == BuiltinBlocks.Air)
            {
                Logger.Log("Check if area is clear for AppleFarmer");
                Vector3Int position = userData.Position;
                int xlen = 7;
                int zlen = 7;
                int radius = 3;

                //set NW corner
                Vector3Int nwcorner = new Vector3Int(position.x - radius, position.y, position.z - radius);
                Vector3Int secorner = new Vector3Int(position.x + radius, position.y, position.z + radius);

                bool blocked = false;
                for (int x = 0; x <= xlen; x++)
                {
                    for (int z = 0; z <= zlen; z++)
                    {
                        if (World.TryGetTypeAt(nwcorner.Add(x, 0, z), out ushort val) && val != BuiltinBlocks.Air)
                        {
                            blocked = true;
                        }
                    }
                }

                if (blocked)
                {
                    Chat.Send(userData.RequestedByPlayer, "Apple Farmer 9 x 9 area is blocked.");
                }
                else
                {

                    var job = new DefaultFarmerAreaJob<AppleFarmerDefinition>(userData.RequestedByPlayer, nwcorner, secorner);
                    if (!AreaJobTracker.RegisterAreaJob(job))
                    {
                        job.OnRemove();
                    }
                    job.Definition.CreateAreaJob(userData.RequestedByPlayer, nwcorner, secorner);
                    Logger.Log("Currently assigned NPC {0}", job.NPC == null ? 0 : job.NPC.ID);
                    Logger.Log("Job Type {0}", job.GetType() == null ? "" : job.GetType().ToString());
                    Logger.Log("Max {0}    Min {1}", job.Maximum, job.Minimum);
                    Logger.Log("AppleFarmer {0}", job.NPCType.ToString());


                    ThreadManager.InvokeOnMainThread(delegate ()
                    {
                        ServerManager.TryChangeBlock(position, userData.TypeNew);
                    }, 0.1f);
                }

            }
            if (userData.TypeOld == ItemTypes.IndexLookup.GetIndex("Ulfric.ColonyAddOns.Blocks.AppleBasket") && userData.TypeNew == BuiltinBlocks.Air)
            {
                Logger.Log("Remove job");
                Vector3Int position = userData.Position;
                int xlen = 7;
                int zlen = 7;
                int radius = 3;

                //set NW corner
                Vector3Int nwcorner = new Vector3Int(position.x - radius, position.y, position.z - radius);
                Vector3Int secorner = new Vector3Int(position.x + radius, position.y, position.z + radius);
                AreaJobTracker.RemoveJobAt(nwcorner, secorner);
            }

        }
    }
}
using BlockTypes.Builtin;
using NPC;
using System.IO;
using System.Threading;
using Pipliz.Mods.APIProvider.AreaJobs;
using Pipliz;
using Pipliz.JSON;
using Pipliz.Threading;

namespace Ulfric.ColonyAddOns.AreaJobs
{
	[AreaJobDefinitionAutoLoader]
	public class AppleForesterDefinition : AreaJobDefinitionDefault<AppleForesterDefinition>
	{
        private string logName = "logtemperate";
        private string saplingName = "Ulfric.ColonyAddOns.Blocks.AppleSapling";
        private string leaveName = "leavestemperate";
        private string fruitName = "Ulfric.ColonyAddOns.Blocks.Apple";
        private ushort logIndex = 0;
        private ushort saplingIndex = 0;
        private ushort leaveIndex = 0;
        private ushort fruitIndex = 0;
        private string name = "appleforester";
        private string id = "Ulfric.ColonyAddOns.AreaJobs.appleforest";

        public AppleForesterDefinition ()
		{
            logIndex = ItemTypes.IndexLookup.GetIndex(logName);
            saplingIndex = ItemTypes.IndexLookup.GetIndex(saplingName);
            leaveIndex = ItemTypes.IndexLookup.GetIndex(leaveName);
            fruitIndex = ItemTypes.IndexLookup.GetIndex(fruitName);

            Logger.Log("Loading {0} AreaJobDefinitionAutoLoader", id);

			identifier = id;
            fileName = name;
			npcType = Server.NPCs.NPCType.GetByKeyNameOrDefault("pipliz.forester");
			areaType = Shared.EAreaType.Forestry;
		}

		public override IAreaJob CreateAreaJob (Players.Player owner, Vector3Int min, Vector3Int max, int npcID = 0)
		{
			SetLayer(min, max, BuiltinBlocks.LumberArea, -1, owner);
			return base.CreateAreaJob(owner, min, max, npcID);
		}

		public override void OnRemove (IAreaJob job)
		{
			SetLayer(job.Minimum, job.Maximum, BuiltinBlocks.GrassTemperate, -1, job.Owner);
		}

		public override void CalculateSubPosition (IAreaJob job, ref Vector3Int positionSub)
		{
			bool hasSeeds = job.NPC.Colony.UsedStockpile.Contains(new InventoryItem(saplingIndex));
			Vector3Int firstPlanting = Vector3Int.invalidPos;
			Vector3Int min = job.Minimum;
			Vector3Int max = job.Maximum;

			for (int x = min.x + 1; x < max.x; x += 3) {
				for (int z = min.z + 1; z < max.z; z += 3) {
					ushort type;
					Vector3Int possiblePositionSub = new Vector3Int(x, min.y, z);
					if (!World.TryGetTypeAt(possiblePositionSub, out type)) {
						positionSub = min;
						return;
					}
					if (type == 0) {
						if (hasSeeds) {
							positionSub = possiblePositionSub; // can plant here
							return;
						} else if (!firstPlanting.IsValid) {
							firstPlanting = possiblePositionSub; // no seeds but should plant here. If no location found, go here
						}
					} else if (type == BuiltinBlocks.LogTemperate) {
						positionSub = possiblePositionSub; // should cut here
						return;
					}
				}
			}

			if (firstPlanting.IsValid) {
				positionSub = firstPlanting;
				return;
			}

			int xOffset = max.x - min.x;
			int zOffset = max.z - min.z;
			int xRandom = Random.Next(0, xOffset / 3) * 3 + min.x;
			int zRandom = Random.Next(0, zOffset / 3) * 3 + min.z;
			positionSub = new Vector3Int(xRandom, min.y, zRandom);
		}

		static System.Collections.Generic.List<ItemTypes.ItemTypeDrops> GatherResults = new System.Collections.Generic.List<ItemTypes.ItemTypeDrops>();

		public override void OnNPCAtJob (IAreaJob job, ref Vector3Int positionSub, ref NPCBase.NPCState state, ref bool shouldDumpInventory)
		{
			state.JobIsDone = true;
			Vector3Int min = job.Minimum;
			Vector3Int max = job.Maximum;
			if (positionSub.x == min.x || positionSub.x == max.x
				|| positionSub.z == min.z || positionSub.z == max.z
				|| (positionSub.x - (min.x + 1)) % 3 != 0
				|| (positionSub.z - (min.z + 1)) % 3 != 0)
			{
				ushort type;
				if (World.TryGetTypeAt(positionSub.Add(1, 0, 1), out type)) {
					if (type == saplingIndex) {
						Server.GrowableBlocks.IGrowableBlock block;
						if (Server.GrowableBlocks.GrowableBlockManager.TryGetGrowableBlock(positionSub.Add(1, 0, 1), out block)) {
							state.SetCooldown(5.0);
						} else {
							ItemTypesServer.OnChange(positionSub.Add(1, 0, 1), 0, saplingIndex, null);
							state.SetIndicator(new Shared.IndicatorState(2f, saplingIndex));
							state.SetCooldown(0.2);
						}
					}
				}
			} else if (positionSub.IsValid) {
				ushort type;
				if (World.TryGetTypeAt(positionSub, out type)) {
					if (type == 0) {
						if (job.NPC.Inventory.TryGetOneItem(saplingIndex)
							|| job.NPC.Colony.UsedStockpile.TryRemove(saplingIndex)) {
							ServerManager.TryChangeBlock(positionSub, saplingIndex, job.Owner, ServerManager.SetBlockFlags.DefaultAudio);
							state.SetCooldown(2.0);
						} else {
							state.SetIndicator(new Shared.IndicatorState(2f, saplingIndex));
						}
					} else if (type == logIndex) {
						if (ChopTree(positionSub, job.Owner)) {
							state.SetIndicator(new Shared.IndicatorState(10f, logIndex));
							ServerManager.SendAudio(positionSub.Vector, "woodDeleteHeavy");

							GatherResults.Clear();
							GatherResults.Add(new ItemTypes.ItemTypeDrops(logIndex, 3, 1.0));
							GatherResults.Add(new ItemTypes.ItemTypeDrops(leaveIndex, 9, 1.0));
							GatherResults.Add(new ItemTypes.ItemTypeDrops(saplingIndex, 1, 1.0));
							GatherResults.Add(new ItemTypes.ItemTypeDrops(saplingIndex, 1, 0.25));
                            GatherResults.Add(new ItemTypes.ItemTypeDrops(fruitIndex, 2, 0.5));

                            ModLoader.TriggerCallbacks(ModLoader.EModCallbackType.OnNPCGathered, job as IJob, positionSub, GatherResults);

							job.NPC.Inventory.Add(GatherResults);
						} else {
							state.SetCooldown(Random.NextFloat(3f, 6f));
						}
					} else {
						state.SetCooldown(Random.NextFloat(8f, 16f));
					}
				} else {
					state.SetCooldown(Random.NextFloat(3f, 6f));
				}
			} else {
				state.SetCooldown(10.0);
			}
			positionSub = Vector3Int.invalidPos;
		}

		static bool ChopTree (Vector3Int p, Players.Player owner)
		{
			return ServerManager.TryChangeBlock(p, 0, owner)
				&& ServerManager.TryChangeBlock(p.Add(0, 1, 0), 0, owner)
				&& ServerManager.TryChangeBlock(p.Add(0, 2, 0), 0, owner);
		}

		#region LOAD_LEGACY_BLOCKS_WORKAROUND
		/// <summary>
		/// This #region code is to load the legacy json data for upgrading from the old area jobs to this
		/// from before v0.5.0 to v0.5.0 and later
		/// </summary>
		JSONNode legacyJSON;

		public override void StartLoading ()
		{
			// do custom things before base.AsyncLoad so FinishLoading also waits for this to complete
			ThreadPool.QueueUserWorkItem(delegate (object obj)
			{
				try {
					string path = string.Format("gamedata/savegames/{0}/blocktypes/AppleForesterAreaJob.json", ServerManager.WorldName);
					if (File.Exists(path)) {
						Log.Write("Loading legacy json from {0}", path);
						JSON.Deserialize(path, out legacyJSON, false);
						File.Delete(path);
					}
				} catch (System.Exception e) {
					Log.WriteException(e);
				} finally {
					AsyncLoad(obj);
				}
			});
		}

		public override void FinishLoading ()
		{
			base.FinishLoading();
			if (legacyJSON != null) {
				foreach (var pair in legacyJSON.LoopObject()) {
					try {
						Players.Player player = Players.GetPlayer(NetworkID.Parse(pair.Key));

						for (int i = 0; i < pair.Value.ChildCount; i++) {
							JSONNode jobNode = pair.Value[i];

							int npcID = jobNode.GetAsOrDefault("npcID", 0);
							Vector3Int min = (Vector3Int)jobNode["positionMin"];
							Vector3Int max = (Vector3Int)jobNode["positionMax"];

							var job = new DefaultFarmerAreaJob<AppleForesterDefinition>(player, min, max, npcID);
							if (!AreaJobTracker.RegisterAreaJob(job)) {
								job.OnRemove();
							}
						}
					} catch (System.Exception e) {
						Log.WriteException("Exception loading legacy area job data", e);
					}
				}
				legacyJSON = null;
			}
		}

        #endregion LOAD_LEGACY_BLOCKS_WORKAROUND

        //[ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".AppleForester.trychangeblock")]
        //public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        //{
        //    if (userData.TypeNew == ItemTypes.IndexLookup.GetIndex("Ulfric.ColonyAddOns.Blocks.AppleBasket") && userData.TypeOld == BuiltinBlocks.Air)
        //    {
        //        Vector3Int position = userData.Position;
        //        int xlen = 9;
        //        int zlen = 9;
        //        int radius = 4;

        //        //set NW corner
        //        Vector3Int nwcorner = new Vector3Int(position.x - radius, position.y, position.z - radius);
        //        Vector3Int secorner = new Vector3Int(position.x + radius, position.y, position.z + radius);

        //        bool blocked = false;
        //        for (int x = 0; x <= xlen; x++)
        //        {
        //            for (int z = 0; z <= zlen; z++)
        //            {
        //                if (World.TryGetTypeAt(nwcorner.Add(x, 0, z), out ushort val) && val != BuiltinBlocks.Air)
        //                {
        //                    blocked = true;
        //                }
        //            }
        //        }

        //        if (blocked)
        //        {
        //            Chat.Send(userData.RequestedByPlayer, "Apple Farmer 9 x 9 area is blocked.");
        //        }
        //        else
        //        {
        //            var job = new DefaultFarmerAreaJob<AreaJobs.AppleForesterDefinition>(userData.RequestedByPlayer, nwcorner, secorner);
        //            AreaJobTracker.RegisterAreaJob(job);

        //            ThreadManager.InvokeOnMainThread(delegate ()
        //            {
        //                ServerManager.TryChangeBlock(position, userData.TypeNew);
        //            }, 0.1f);
        //        }

        //    }
        //}
    }
}
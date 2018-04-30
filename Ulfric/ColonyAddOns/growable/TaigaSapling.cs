using BlockTypes.Builtin;
using Server.GrowableBlocks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Pipliz.Mods.APIProvider.GrowableBlocks;
using Pipliz;
using Pipliz.JSON;

namespace Ulfric.ColonyAddOns.GrowableBlocks
{

    [GrowableBlockDefinitionAutoLoader]
	public class TaigaSapling : GrowableBlockDefinition<TaigaSapling>
	{
        private string logName = "logtaiga";
        private string saplingName = "Ulfric.ColonyAddOns.Blocks.TaigaSapling";
        private string leavesName = "leavestaiga";
        private ushort logIndex = 0;
        private ushort saplingIndex = 0;
        private ushort leavesIndex = 0;
        private string filename = "TaigaSapling";

        public TaigaSapling()
		{
            logIndex = ItemTypes.IndexLookup.GetIndex(logName);
            saplingIndex = ItemTypes.IndexLookup.GetIndex(saplingName);
            leavesIndex = ItemTypes.IndexLookup.GetIndex(leavesName);

            Logger.Log("Autoload {0} Definition",filename);

            FileName = filename;
			GrowthType = EGrowthType.Always;
			RandomStartGrowthMax = 9f / 21f; // to maintain random 9-21 hour growth from < 0.5.0
			Stages = new List<IGrowableStage>()
			{
				new GrowableStage(saplingName, 21f),
				new GrowableStage()
			};
		}

		static List<Vector3Int> logs = new List<Vector3Int>()
		{
			new Vector3Int(0,0,0),
			new Vector3Int(0,1,0),
			new Vector3Int(0,2,0),
            new Vector3Int(0,3,0),
            new Vector3Int(0,4,0),
            new Vector3Int(0,5,0),
            new Vector3Int(0,6,0),
            new Vector3Int(0,7,0),
            new Vector3Int(0,8,0),
            new Vector3Int(0,9,0),
            new Vector3Int(0,10,0)
        };

		static List<Vector3Int> leaves = new List<Vector3Int>()
        {
            new Vector3Int(-3, 4,0),
            new Vector3Int(-3,5,-1),
            new Vector3Int(-3,5,0),
            new Vector3Int(-3,5,1),
            new Vector3Int(-3,6,0),
            new Vector3Int(-2,3,0),
            new Vector3Int(-2,4,-1),
            new Vector3Int(-2,4,0),
            new Vector3Int(-2,4,1),
            new Vector3Int(-2,5,-2),
            new Vector3Int(-2,5,-1),
            new Vector3Int(-2,5,0),
            new Vector3Int(-2,5,1),
            new Vector3Int(-2,5,2),
            new Vector3Int(-2,6,-1),
            new Vector3Int(-2,6,0),
            new Vector3Int(-2,6,1),
            new Vector3Int(-2,7,-1),
            new Vector3Int(-2,7,0),
            new Vector3Int(-2,7,1),
            new Vector3Int(-2,8,0),
            new Vector3Int(-2,9,0),
            new Vector3Int(-1,3,0),
            new Vector3Int(-1,4,-2),
            new Vector3Int(-1,4,-1),
            new Vector3Int(-1,4,0),
            new Vector3Int(-1,4,1),
            new Vector3Int(-1,4,2),
            new Vector3Int(-1,5,-3),
            new Vector3Int(-1,5,-2),
            new Vector3Int(-1,5,-1),
            new Vector3Int(-1,5,0),
            new Vector3Int(-1,5,1),
            new Vector3Int(-1,5,2),
            new Vector3Int(-1,5,3),
            new Vector3Int(-1,6,-2),
            new Vector3Int(-1,6,-1),
            new Vector3Int(-1,6,0),
            new Vector3Int(-1,6,1),
            new Vector3Int(-1,6,2),
            new Vector3Int(-1,7,-2),
            new Vector3Int(-1,7,-1),
            new Vector3Int(-1,7,0),
            new Vector3Int(-1,7,1),
            new Vector3Int(-1,7,2),
            new Vector3Int(-1,8,-1),
            new Vector3Int(-1,8,0),
            new Vector3Int(-1,8,1),
            new Vector3Int(-1,9,-1),
            new Vector3Int(-1,9,0),
            new Vector3Int(-1,9,1),
            new Vector3Int(-1,10,-1),
            new Vector3Int(-1,10,0),
            new Vector3Int(-1,10,1),
            new Vector3Int(-1,11,0),
            new Vector3Int(-1,12,0),
            new Vector3Int(0,3,-2),
            new Vector3Int(0,3,-1),
            new Vector3Int(0,3,1),
            new Vector3Int(0,3,2),
            new Vector3Int(0,4,-3),
            new Vector3Int(0,4,-2),
            new Vector3Int(0,4,-1),
            new Vector3Int(0,4,1),
            new Vector3Int(0,4,2),
            new Vector3Int(0,4,3),
            new Vector3Int(0,5,-3),
            new Vector3Int(0,5,2),
            new Vector3Int(0,5,-1),
            new Vector3Int(0,5,1),
            new Vector3Int(0,5,2),
            new Vector3Int(0,5,3),
            new Vector3Int(0,6,-3),
            new Vector3Int(0,6,-2),
            new Vector3Int(0,6,-1),
            new Vector3Int(0,6,1),
            new Vector3Int(0,6,2),
            new Vector3Int(0,6,3),
            new Vector3Int(0,7,-2),
            new Vector3Int(0,7,-1),
            new Vector3Int(0,7,1),
            new Vector3Int(0,7,2),
            new Vector3Int(0,8,-2),
            new Vector3Int(0,8,-1),
            new Vector3Int(0,8,1),
            new Vector3Int(0,8,2),
            new Vector3Int(0,9,-2),
            new Vector3Int(0,9,-1),
            new Vector3Int(0,9,1),
            new Vector3Int(0,9,2),
            new Vector3Int(0,10,-1),
            new Vector3Int(0,10,1),
            new Vector3Int(0,11,-1),
            new Vector3Int(0,11,1),
            new Vector3Int(0,12,-1),
            new Vector3Int(0,12,0),
            new Vector3Int(0,12,1),
            new Vector3Int(0,13,0),
            new Vector3Int(0,14,0),
            new Vector3Int(1,3,0),
            new Vector3Int(1,4,-2),
            new Vector3Int(1,4,-1),
            new Vector3Int(1,4,0),
            new Vector3Int(1,4,1),
            new Vector3Int(1,4,2),
            new Vector3Int(1,5,-3),
            new Vector3Int(1,5,-2),
            new Vector3Int(1,5,-1),
            new Vector3Int(1,5,0),
            new Vector3Int(1,5,1),
            new Vector3Int(1,5,2),
            new Vector3Int(1,5,3),
            new Vector3Int(1,6,-2),
            new Vector3Int(1,6,-1),
            new Vector3Int(1,6,0),
            new Vector3Int(1,6,1),
            new Vector3Int(1,6,2),
            new Vector3Int(1,7,-2),
            new Vector3Int(1,7,-1),
            new Vector3Int(1,7,0),
            new Vector3Int(1,7,1),
            new Vector3Int(1,7,2),
            new Vector3Int(1,8,-1),
            new Vector3Int(1,8,0),
            new Vector3Int(1,8,1),
            new Vector3Int(1,9,-1),
            new Vector3Int(1,9,0),
            new Vector3Int(1,9,1),
            new Vector3Int(1,10,-1),
            new Vector3Int(1,10,0),
            new Vector3Int(1,10,1),
            new Vector3Int(1,11,0),
            new Vector3Int(1,12,0),
            new Vector3Int(2,3,0),
            new Vector3Int(2,4,-1),
            new Vector3Int(2,4,0),
            new Vector3Int(2,4,1),
            new Vector3Int(2,5,-2),
            new Vector3Int(2,5,-1),
            new Vector3Int(2,5,0),
            new Vector3Int(2,5,1),
            new Vector3Int(2,5,2),
            new Vector3Int(2,6,-1),
            new Vector3Int(2,6,0),
            new Vector3Int(2,6,1),
            new Vector3Int(2,7,-1),
            new Vector3Int(2,7,0),
            new Vector3Int(2,7,1),
            new Vector3Int(2,8,0),
            new Vector3Int(2,9,0),
            new Vector3Int(3,4,0),
            new Vector3Int(3,5,-1),
            new Vector3Int(3,5,0),
            new Vector3Int(3,5,1),
            new Vector3Int(3,6,0)
		};

		#region LOAD_LEGACY_BLOCKS_WORKAROUND
		/// <summary>
		/// This #region code is to load the legacy updatableblocks.json data for upgrading from IUpdatableBlocks to Server.Growables
		/// from before v0.5.0 to v0.5.0 and later
		/// </summary>
		JSONNode updatableBlocks;

		public override void StartLoading ()
		{
			// do custom things before base.AsyncLoad so FinishLoading also waits for this to complete
			ThreadPool.QueueUserWorkItem(delegate (object obj)
			{
				try {
					string path = string.Format("gamedata/savegames/{0}/updatableblocks.json", ServerManager.WorldName);
					if (File.Exists(path)) {
						JSON.Deserialize(path, out updatableBlocks, false);
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
			if (updatableBlocks != null) {
				JSONNode array;
				if (updatableBlocks.TryGetChild("sappling", out array)) {
					Log.Write("Loading {0} legacy blocks to type temperate sapling", array.ChildCount);
					for (int i = 0; i < array.ChildCount; i++) {
						GrowableBlockManager.RegisterGrowableBlock(MakeGrowableBlockLegacy(array[i]));
					}
				}

				General.Application.StartAsyncQuitToComplete(delegate ()
				{
					Log.Write("Queueing delete of legacy updatableblocks.json");
					Thread.Sleep(15000);
					Log.Write("Deleting legacy updatableblocks.json");
					File.Delete(string.Format("gamedata/savegames/{0}/updatableblocks.json", ServerManager.WorldName));
				});
			}
		}

		#endregion LOAD_LEGACY_BLOCKS_WORKAROUND

		public override bool TryAdvanceStage (IGrowableBlock block, byte currentStageIndex)
		{
            Logger.Log("{0} TryAdvanceStage",filename);
			Vector3Int pos = block.Position;
			if (currentStageIndex == 0 && pos.IsValid) {
				for (int i = 0; i < logs.Count; i++) {
					ushort currentType;
					if (World.TryGetTypeAt(pos + logs[i], out currentType)) {
						if (currentType == 0 || currentType == saplingIndex) {
							if (!ServerManager.TryChangeBlock(pos + logs[i], logIndex)) {
								return false; // not loaded
							}
						}
					} else {
						return false; // not loaded
					}
				}
				for (int i = 0; i < leaves.Count; i++) {
					ushort currentType;
					if (World.TryGetTypeAt(pos + leaves[i], out currentType)) {
						if (currentType == 0) {
							if (!ServerManager.TryChangeBlock(pos + leaves[i], leavesIndex)) {
								return false; // not loaded
							}
						}
					} else {
						return false; // not loaded
					}
				}
			}
			// succesfully grew, or invalid stage index. Either case, done.
			block.SetInvalid();
			return true;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Monsters;
using NPC;

namespace Ulfric.ColonyAddOns
{
    [ModLoader.ModManager]
    public static class Disease
    {
        //Set class variables and constants
        public const string NAMESPACE = "Ulfric.ColonyAddOns";
        private const string MOD_NAMESPACE = NAMESPACE + ".Disease";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, Disease.MOD_NAMESPACE + ".OnUpdate")]
        public static void OnUpdate()
        {
            if (Configuration.EnableDisease)
            {
                List<Colony> colonylist = Colony.collection.ValuesAsList;

                foreach (Colony c in colonylist)
                {
                    PlayerState ps = PlayerState.GetPlayerState(c.Owner);
                    if (c.InSiegeMode)
                    {

                        if (ps.StartOfSiege == 0)
                            ps.StartOfSiege = TimeCycle.TotalTime;
                        if (ps.DiseaseOutbreak && ps.LastDiseaseCheck + 1 < Calendar.DaysSinceWorldCreated)
                        {
                            string sickNPC = InfectNPC(c);
                            if (sickNPC != null)
                            {
                                Chat.Send(ps.Player, "One of your {0} has fallen sick!", ChatColor.red, sickNPC);
                            }
                            ps.LastDiseaseCheck = Calendar.DaysSinceWorldCreated;

                        }
                        else if (ps.StartOfSiege > TimeCycle.TotalTime + Configuration.DiseaseStartThreshold)
                        {
                            Chat.Send(ps.Player, "A disease has broken out in the colony due to the siege!", ChatColor.red);
                            Chat.Send(ps.Player, "Workers who are sick can not work!", ChatColor.red);
                            ps.DiseaseOutbreak = true;
                            ps.LastDiseaseCheck = Calendar.DaysSinceWorldCreated;
                        }
                    }
                    else
                    {
                        ps.DiseaseOutbreak = false;
                        ps.StartOfSiege = 0;
                        ps.LastDiseaseCheck = 0;
                    }
                }
            }
        }

        private static string InfectNPC(Colony c)
        {
            string sickNPC = null;

            if (Pipliz.Random.NextFloat(0.0f, 1.0f) <= Configuration.ChanceOfDiseaseSpread)
            {
                foreach (NPCBase follower in c.Followers)
                {
                    if (!follower.Job.NPCType.Equals(SickJob.SickNPCType))
                    {
                        if (follower.Job != null)
                        {
                            follower.Job.NPC = null;
                            follower.ClearJob();
                        }

                        //Create the sickjob, save the old job so it can be reset with the NPC is health again and if it is available.  Init job and assign the NPC to the sickjob. 
                        SickJob sickjob = new SickJob();
                        sickjob.OldJob = (Job)follower.Job;
                        sickjob.InitializeJob(follower.Colony.Owner, follower.Position, follower.ID);
                        sickjob.OnAssignedNPC(follower);
                        follower.TakeJob(sickjob);

                        //add job so it will be saved and loaded with server restarts
                        ColonyManager.AddJobs(sickjob);

                        //Make old job available
                        JobTracker.Add(sickjob.OldJob);
                        
                        //Set so the name of the old job can be returned
                        sickNPC = follower.Job.NPCType.ToString();

                        break;
                    }
                }
            }
            return sickNPC;
        }
    }
}

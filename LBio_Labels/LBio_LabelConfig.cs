using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LittleBiologist.LBio_Const;

namespace LittleBiologist
{
    public static class LBio_LabelConfig
    {
        public static Dictionary<CreatureTemplate.Type, LBio_CreatureConfig> LBio_Configs = new Dictionary<CreatureTemplate.Type, LBio_CreatureConfig>();
        public static void SetupConfig()
        {
            foreach(CreatureTemplate.Type type in Enum.GetValues(typeof(CreatureTemplate.Type)))
            {
                if(type != CreatureTemplate.Type.LizardTemplate && type != CreatureTemplate.Type.StandardGroundCreature)
                {
                    LBio_CreatureConfig lBio_CreatureConfig = new LBio_CreatureConfig();
                    Log(type.ToString());

                    lBio_CreatureConfig.IOwnPersonality = type == CreatureTemplate.Type.BigSpider ||
                                                          type == CreatureTemplate.Type.Deer ||
                                                          type == CreatureTemplate.Type.DropBug ||
                                                          type == CreatureTemplate.Type.EggBug ||
                                                          type == CreatureTemplate.Type.MirosBird ||
                                                          type == CreatureTemplate.Type.PoleMimic ||
                                                          type == CreatureTemplate.Type.TentaclePlant ||
                                                          type == CreatureTemplate.Type.Scavenger ||
                                                          type == CreatureTemplate.Type.TempleGuard ||
                                                          type.ToString().Contains("Lizard") ||
                                                          type.ToString().Contains("Cicada");

                    if (lBio_CreatureConfig.IOwnPersonality)
                    {
                        lBio_CreatureConfig.UsingAggression = type == CreatureTemplate.Type.Scavenger ||
                                                              type == CreatureTemplate.Type.PoleMimic ||
                                                              type == CreatureTemplate.Type.TentaclePlant;
                        lBio_CreatureConfig.UsingBravery = type.ToString().Contains("Cicada") || type == CreatureTemplate.Type.Scavenger;
                        lBio_CreatureConfig.UsingEnergy = type.ToString().Contains("Lizard") || type == CreatureTemplate.Type.Scavenger;
                        lBio_CreatureConfig.UsingNervous = type == CreatureTemplate.Type.Scavenger;
                        lBio_CreatureConfig.UsingSympathy = type == CreatureTemplate.Type.Scavenger;
                        lBio_CreatureConfig.UsingDominance = !type.ToString().Contains("Cicada");

                        lBio_CreatureConfig.IOwnSkills = type == CreatureTemplate.Type.Scavenger;
                    }


                    lBio_CreatureConfig.IOwnRelationship = type == CreatureTemplate.Type.Scavenger || 
                                                           type == CreatureTemplate.Type.JetFish ||
                                                           type == CreatureTemplate.Type.Vulture ||
                                                           type == CreatureTemplate.Type.KingVulture ||
                                                           type.ToString().Contains("Lizard") ||
                                                           type.ToString().Contains("NeedleWorm") ||
                                                           type.ToString().Contains("Cicada");
                    lBio_CreatureConfig.IOwnSquad = type == CreatureTemplate.Type.Scavenger;

                    if (lBio_CreatureConfig.IOwnRelationship)
                    {
                        lBio_CreatureConfig.UsingLike = type == CreatureTemplate.Type.Scavenger ||
                                                        type == CreatureTemplate.Type.JetFish ||
                                                        type == CreatureTemplate.Type.Vulture ||
                                                        type == CreatureTemplate.Type.KingVulture;
                        lBio_CreatureConfig.UsingTempLike = type == CreatureTemplate.Type.BigNeedleWorm ||
                                                            type == CreatureTemplate.Type.Scavenger ||
                                                            type.ToString().Contains("Cicada") ||
                                                            type.ToString().Contains("Lizard");
                        lBio_CreatureConfig.UsingKnow = type == CreatureTemplate.Type.Scavenger;
                    }

                    LBio_Configs.Add(type, lBio_CreatureConfig);
                }
            }
        }
        public class LBio_CreatureConfig
        {
            #region Page_1
            public bool IOwnPersonality = false;
            #region personality
            public bool UsingAggression = false;
            public bool UsingBravery = false;
            public bool UsingEnergy = false;
            public bool UsingNervous = false;
            public bool UsingSympathy = false;
            public bool UsingDominance = false;
            #endregion
            #region skills
            public bool IOwnSkills = false;
            #endregion
            #endregion
            #region Page_2
            public bool IOwnRelationship = false;
            public bool IOwnSquad = false;
            #region relationship
            public bool UsingKnow = false;
            public bool UsingLike = false;
            public bool UsingTempLike = false;
            #endregion
            #endregion
        }

        public static LBio_CreatureConfig GetConfig(Creature creature)
        {
            if (LBio_Configs.ContainsKey(creature.abstractCreature.creatureTemplate.type))
            {
                return LBio_Configs[creature.abstractCreature.creatureTemplate.type];
            }
            else
            {
                return new LBio_CreatureConfig();
            }
        }
    }
}

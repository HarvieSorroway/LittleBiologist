using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LittleBiologist
{
    public class LBioPage_ShowID : LBio_CreatureLabel.LBio_LabelPage
    {
        public LBioPage_ShowID(LBio_CreatureLabel owner) : base(owner)
        {
        }
    }


    public class LBioPage_ShowPersonality : LBio_CreatureLabel.LBio_LabelPage
    {
        public LBioPage_ShowPersonality(LBio_CreatureLabel owner) : base(owner)
        {
            AbstractCreature.Personality personality = owner.creature.abstractCreature.personality;
            LBio_LabelConfig.LBio_CreatureConfig config = LBio_LabelConfig.GetConfig(owner.creature);
            max = 1;

            text += owner.creature.abstractCreature.ID.number.ToString() + "\n";
            if (config.IOwnPersonality)
            {
                text += config.UsingAggression ? String.Format("Aggression : {0:F2}\n", personality.aggression) : "";
                text += config.UsingBravery ? String.Format("Bravery : {0:F2}\n", personality.bravery) : "";
                text += config.UsingEnergy ? String.Format("Energy : {0:F2}\n", personality.energy) : "";
                text += config.UsingNervous ? String.Format("Nervous : {0:F2}\n", personality.nervous) : "";
                text += config.UsingSympathy ? String.Format("Sympathy : {0:F2}\n", personality.sympathy) : "";
                text += config.UsingDominance ? String.Format("Dominance : {0:F2}\n", personality.dominance) : "";
            }
            else
            {
                text += "I don't need personality";
            }

            if(config.IOwnSkills)
            {

                Scavenger scavenger = owner.creature as Scavenger;
                
                text2 += owner.creature.abstractCreature.ID.number.ToString() +" Skills" + "\n";
                
                text2 += String.Format("Blocking : {0:F2}\n", scavenger.blockingSkill);
                text2 += String.Format("Dodge : {0:F2}\n", scavenger.dodgeSkill);
                text2 += String.Format("Melee : {0:F2}\n", scavenger.meleeSkill);
                text2 += String.Format("MidRange : {0:F2}\n", scavenger.midRangeSkill);
                text2 += String.Format("Reaction : {0:F2}\n", scavenger.reactionSkill);
            }
            else
            {
                text2 += owner.creature.abstractCreature.ID.number.ToString() + "\n";
                text2 += "I don't have any skill";
            }
        }

        public override void SwitchLocalPage()
        {
            base.SwitchLocalPage();
        }

        public override string GetText()
        {
            if (localPageIndex == 0) { return text; }
            else { return text2; }
        }

        public override Color GetColor()
        {
            return currentColor;
        }

        Color currentColor = new Color(0.4f, 0f, 0.4f, 0.8f);
        string text = "";
        string text2 = "";
        int max = 0;
        public override int maxLocalPageIndex => max;
    }


    public class LBioPage_ShowRelationship : LBio_CreatureLabel.LBio_LabelPage
    {
        public LBioPage_ShowRelationship(LBio_CreatureLabel owner) : base(owner)
        {
            socialMemory = owner.creature.abstractCreature.state.socialMemory;

            title = owner.creature.abstractCreature.ID.number.ToString();
            config = LBio_LabelConfig.GetConfig(owner.creature);
        }

        public override void UpdateText()
        {
            if(localPageIndex == 0)
            {
                if (owner.creature.room.game.Players.Count > 0 )
                {
                    if (config.IOwnRelationship)
                    {
                        haveSlugcat = true;
                        var social = socialMemory.GetOrInitiateRelationship(owner.creature.room.game.Players[0].ID);
                        if (social != null)
                        {
                            haveSocial = true;

                            bool flashing = social.tempLike != tempLike || social.like != like || social.know != know;

                            know = social.know;
                            like = social.like;
                            tempLike = social.tempLike;

                            if (flashing)
                            {
                                owner.Flash();
                            }
                        }
                        else
                        {
                            haveSocial = false;
                        }
                    }
                    else
                    {
                        haveSocial = false;
                    }
                }
                else
                {
                    haveSlugcat = false;
                }
            }
        }

        public override string GetText()
        {
            string text = title;
            if (localPageIndex == 0)
            {
                if (haveSlugcat)
                {
                    if (haveSocial && config.IOwnRelationship)
                    {
                        text += config.UsingLike ? String.Format("\nLike:{0:f2}", like) : "";
                        text += config.UsingTempLike ? String.Format("\nTemplike:{0:f2}", tempLike) : "";
                        text += config.UsingKnow ? String.Format("\nKnow:{0:f2}", know) : "";
                    }
                    else
                    {
                        text += "\nI don't care slugcat :<";
                    }
                }
                else
                {
                    if (owner.creature.abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
                    {
                        text += "\nNo slugcat found :s";
                    }
                    else
                    {
                        text += "\nSlugcat is me :-";
                    }
                }
            }
            else
            {
                if (config.IOwnSquad)
                {
                    if (owner.creature != null)
                    {
                        Scavenger scavenger = owner.creature as Scavenger;

                        if ((scavenger.abstractCreature.abstractAI as ScavengerAbstractAI).squad != null)
                        {
                            ScavengerAbstractAI abstractAI = (scavenger.abstractCreature.abstractAI as ScavengerAbstractAI);

                            text += abstractAI.squad.Active ? "\nSquard activate" : "\nSquad disactivate";

                            if (abstractAI.squad.leader != null)
                            {
                                if (abstractAI.squad.leader == scavenger.abstractCreature)
                                {
                                    text += String.Format("\nSqaudLeader is me");
                                    owner.Flash();
                                }
                                else
                                {
                                    text += String.Format("\nSqaudLeader is {0}", abstractAI.squad.leader.ID.number.ToString());
                                    if (scavenger.AI.seenSquadLeaderInRoom == -1)
                                    {
                                        text += "\nCan't find leader :S";
                                    }
                                    else
                                    {
                                        text += String.Format("\nSee leader in {0}", scavenger.abstractCreature.world.GetAbstractRoom(scavenger.AI.seenSquadLeaderInRoom).name);
                                    }

                                }

                                if (abstractAI.squad.HasAMission)
                                {
                                    text += "\nMission is " + abstractAI.squad.missionType.ToString();
                                    if (abstractAI.squad.MissionRoom == scavenger.abstractCreature.Room.index)
                                    {
                                        text += "\nArrived at mission room";
                                    }
                                    else
                                    {
                                        text += "\nMission room is " + scavenger.abstractCreature.world.GetAbstractRoom(abstractAI.squad.MissionRoom).name;
                                    }

                                    if (abstractAI.squad.targetCreature != null)
                                    {
                                        text += "\nTarget creature is " + abstractAI.squad.targetCreature.ToString();
                                    }
                                }
                                else
                                {
                                    text += "\nMy squad has no mission :)";
                                }

                                if (abstractAI.longTermMigration != null)
                                {
                                    if (abstractAI.squad.HasAMission && abstractAI.longTermMigration.room == abstractAI.squad.MissionRoom)
                                    {
                                        text += "\nMigration destination is mission room";
                                    }
                                    else if (scavenger.abstractCreature.Room.index == abstractAI.longTermMigration.room)
                                    {
                                        text += "\nArrived at migration destination";
                                    }
                                    else
                                    {
                                        if (scavenger.abstractCreature.world.GetAbstractRoom(abstractAI.longTermMigration.room) != null)
                                        {
                                            text += "\nMigration destination:" + scavenger.abstractCreature.world.GetAbstractRoom(abstractAI.longTermMigration.room).name;
                                        }
                                        else
                                        {
                                            text += "\nNo migration destination :S";
                                        }
                                    }
                                }

                                squadColor = abstractAI.squad.color;
                            }
                            else
                            {
                                text += "\nMy squad has no lead :o";
                                squadColor = Color.gray * 0.7f + Color.red * 0.3f;
                            }
                        }
                        else
                        {
                            text += "\nFree scav is me! XD";
                            squadColor = Color.gray;
                        }
                    }
                }
                else
                {
                    text += "\nI'm not scav :<";
                }
            }

            return text;
        }

        public override Color GetColor()
        {
            return currentColor;
        }

        Color squadColor = Color.gray;
        Color currentColor => (config.IOwnSquad && localPageIndex == 1) ? squadColor :  Color.cyan * 0.8f;
        SocialMemory socialMemory;

        string title = "";

        bool haveSocial = false;
        bool haveSlugcat = false;

        float know = 0f;
        float like = 0f;
        float tempLike = 0f;

        public override int maxLocalPageIndex => 1;

        LBio_LabelConfig.LBio_CreatureConfig config;
    }

}

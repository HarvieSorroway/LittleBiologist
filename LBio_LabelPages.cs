using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

            text += owner.creature.abstractCreature.ID.number.ToString() + "\n";
            text += String.Format("Aggression : {0:F2}\n", personality.aggression);
            text += String.Format("Bravery : {0:F2}\n", personality.bravery);
            text += String.Format("Energy : {0:F2}\n", personality.energy);
            text += String.Format("Nervous : {0:F2}\n", personality.nervous);
            text += String.Format("Sympathy : {0:F2}\n", personality.sympathy);
            text += String.Format("Dominance : {0:F2}\n", personality.dominance);

            if(owner.creature.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
            {
                max = 1;
                Scavenger scavenger = owner.creature as Scavenger;
                
                text2 += owner.creature.abstractCreature.ID.number.ToString() +" Skills" + "\n";
                
                text2 += String.Format("Blocking : {0:F2}\n", scavenger.blockingSkill);
                text2 += String.Format("Dodge : {0:F2}\n", scavenger.dodgeSkill);
                text2 += String.Format("Melee : {0:F2}\n", scavenger.meleeSkill);
                text2 += String.Format("MidRange : {0:F2}\n", scavenger.midRangeSkill);
                text2 += String.Format("Reaction : {0:F2}\n", scavenger.reactionSkill);
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
}
